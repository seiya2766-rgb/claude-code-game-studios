using RuneBrigade.Core;
using UnityEngine;
using UnityEngine.AI;

namespace RuneBrigade
{
    /// <summary>
    /// Enemy behaviour: chase → telegraph → attack → recover, on a NavMeshAgent.
    /// An enemy must hold an attack token to start an attack; without one it waits
    /// just outside range. Frozen or bound enemies stop. Abilities can launch, pull,
    /// knock back and stagger it. Rules: design/game-concept.md, "敵とボス".
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent), typeof(EnemyHealth))]
    public sealed class EnemyBrain : MonoBehaviour
    {
        private enum State { Chase, Telegraph, Recover, Staggered, Airborne, Dead }

        [Tooltip("Visual child that is lifted when launched. Optional.")]
        [SerializeField] private Transform body;
        [Tooltip("Renderers tinted during the telegraph. Empty = all child renderers.")]
        [SerializeField] private Renderer[] tintRenderers;
        [Tooltip("Seconds between path updates while chasing.")]
        [Min(0.05f)] [SerializeField] private float repathInterval = 0.2f;
        [Tooltip("Seconds before a dead enemy is removed.")]
        [Min(0f)] [SerializeField] private float despawnDelay = 0.4f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private NavMeshAgent _agent;
        private EnemyHealth _health;
        private CombatDirector _director;
        private EnemyDefinition _definition;
        private PlayerHealth _target;
        private MaterialPropertyBlock _block;

        private State _state = State.Chase;
        private float _timer;
        private float _repathTimer;
        private float _airTime;
        private float _airDuration;
        private float _airHeight;
        private Vector3 _forcedVelocity;
        private float _forcedTimer;

        /// <summary>The enemy's type data.</summary>
        public EnemyDefinition Definition => _definition;

        /// <summary>True while it is warning of an attack.</summary>
        public bool IsTelegraphing => _state == State.Telegraph;

        private int OwnerId => GetInstanceID();

        /// <summary>Sets the enemy up. Called by the spawner right after instantiation.</summary>
        public void Initialize(CombatDirector director, EnemyDefinition definition, PlayerHealth target, float maxHealth)
        {
            _director = director;
            _definition = definition;
            _target = target;
            _agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<EnemyHealth>();
            _health.Initialize(director, maxHealth);
            _health.Died += OnDied;
            _agent.speed = definition.moveSpeed;
            _agent.stoppingDistance = definition.attackRange * 0.8f;
            if (tintRenderers == null || tintRenderers.Length == 0) tintRenderers = GetComponentsInChildren<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        /// <summary>Knocks it into the air; it cannot act until it lands.</summary>
        public void Launch(float height)
        {
            if (_state == State.Dead || height <= 0f) return;
            CancelAttack();
            _state = State.Airborne;
            _airHeight = height;
            _airDuration = Mathf.Sqrt(height) * 0.55f;
            _airTime = 0f;
        }

        /// <summary>Drags it toward <paramref name="point"/> (the Warden's roar).</summary>
        public void PullTowards(Vector3 point, float speed, float seconds)
        {
            if (_state == State.Dead) return;
            Vector3 to = point - transform.position;
            to.y = 0f;
            if (to.magnitude < 0.5f) return;
            _forcedVelocity = to.normalized * Mathf.Min(speed, to.magnitude / Mathf.Max(seconds, 0.01f));
            _forcedTimer = seconds;
        }

        /// <summary>Pushes it along <paramref name="direction"/> by <paramref name="distance"/>.</summary>
        public void Knockback(Vector3 direction, float distance)
        {
            if (_state == State.Dead) return;
            direction.y = 0f;
            if (direction.sqrMagnitude < 1e-4f) return;
            const float seconds = 0.2f;
            _forcedVelocity = direction.normalized * (distance / seconds);
            _forcedTimer = seconds;
        }

        /// <summary>Interrupts it for <paramref name="seconds"/> (just guard).</summary>
        public void Stagger(float seconds)
        {
            if (_state == State.Dead || _state == State.Airborne) return;
            CancelAttack();
            _state = State.Staggered;
            _timer = seconds;
        }

        private void Update()
        {
            if (_definition == null || _state == State.Dead) return;
            float dt = Time.deltaTime;

            if (_forcedTimer > 0f)
            {
                _forcedTimer -= dt;
                if (_agent.isOnNavMesh) _agent.Move(_forcedVelocity * dt);
            }

            if (_state == State.Airborne)
            {
                UpdateAirborne(dt);
                return;
            }

            if (IsHeld())
            {
                CancelAttack();
                Stop();
                return;
            }

            switch (_state)
            {
                case State.Chase: UpdateChase(dt); break;
                case State.Telegraph: UpdateTelegraph(dt); break;
                case State.Recover:
                case State.Staggered: UpdateRecover(dt); break;
            }
        }

        private bool IsHeld()
        {
            StatusContainer statuses = _health.Statuses;
            return statuses != null && (statuses.Has(StatusType.Freeze) || statuses.Has(StatusType.Bind));
        }

        private void UpdateChase(float dt)
        {
            if (!_target) return;
            Vector3 targetPos = _target.transform.position;
            float distance = FlatDistance(targetPos);

            if (distance <= _definition.attackRange && _director.Tokens.TryAcquire(OwnerId, _definition.attackKind))
            {
                Stop();
                _state = State.Telegraph;
                _timer = _definition.telegraphSeconds;
                SetTint(true);
                return;
            }

            _repathTimer -= dt;
            if (_repathTimer > 0f || !_agent.isOnNavMesh) return;
            _repathTimer = repathInterval;
            _agent.isStopped = false;

            if (distance <= _definition.attackRange * 1.8f && !_director.Tokens.Holds(OwnerId))
            {
                // No token: hold a ring just outside range instead of crowding in.
                Vector3 away = (transform.position - targetPos);
                away.y = 0f;
                if (away.sqrMagnitude < 1e-4f) away = transform.forward;
                _agent.SetDestination(targetPos + away.normalized * (_definition.attackRange * 1.6f));
            }
            else
            {
                _agent.SetDestination(targetPos);
            }
        }

        private void UpdateTelegraph(float dt)
        {
            Face(_target.transform.position, dt);
            _timer -= dt;
            if (_timer > 0f) return;

            SetTint(false);
            if (FlatDistance(_target.transform.position) <= _definition.attackRange * 1.25f)
            {
                _target.ReceiveEnemyAttack(_definition.attackDamage, this);
            }
            _state = State.Recover;
            _timer = _definition.recoverSeconds;
        }

        private void UpdateRecover(float dt)
        {
            _timer -= dt;
            if (_timer > 0f) return;
            _director.Tokens.Release(OwnerId);
            _state = State.Chase;
        }

        private void UpdateAirborne(float dt)
        {
            _airTime += dt;
            float t = Mathf.Clamp01(_airTime / _airDuration);
            if (body) body.localPosition = new Vector3(0f, _airHeight * 4f * t * (1f - t), 0f);
            if (t < 1f) return;
            if (body) body.localPosition = Vector3.zero;
            _state = State.Recover;
            _timer = 0.3f;
        }

        private void CancelAttack()
        {
            if (_state == State.Telegraph) SetTint(false);
            if (_director != null) _director.Tokens.Release(OwnerId);
        }

        private void Stop()
        {
            if (_agent.isOnNavMesh) _agent.isStopped = true;
        }

        private void Face(Vector3 point, float dt)
        {
            Vector3 to = point - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 1e-4f) return;
            Quaternion look = Quaternion.LookRotation(to);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, 540f * dt);
        }

        private float FlatDistance(Vector3 point)
        {
            Vector3 d = point - transform.position;
            d.y = 0f;
            return d.magnitude;
        }

        private void SetTint(bool on)
        {
            if (tintRenderers == null) return;
            foreach (Renderer r in tintRenderers)
            {
                if (!r) continue;
                if (on)
                {
                    r.GetPropertyBlock(_block);
                    _block.SetColor(BaseColorId, _definition.telegraphColor);
                    _block.SetColor(ColorId, _definition.telegraphColor);
                    r.SetPropertyBlock(_block);
                }
                else
                {
                    r.SetPropertyBlock(null);
                }
            }
        }

        private void OnDied(EnemyHealth _)
        {
            CancelAttack();
            _state = State.Dead;
            Stop();
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
            Destroy(gameObject, despawnDelay);
        }

        private void OnDestroy()
        {
            if (_director != null) _director.Tokens.Release(OwnerId);
        }
    }
}
