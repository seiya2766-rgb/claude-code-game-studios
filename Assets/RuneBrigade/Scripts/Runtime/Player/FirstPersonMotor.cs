using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// First-person movement on a CharacterController: walk, mouse look, jump and the
    /// two-charge dash with brief invulnerability. Abilities use <see cref="Lunge"/>
    /// and <see cref="Teleport"/> for the shield charge and blink.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonMotor : MonoBehaviour
    {
        [SerializeField] private CombatTuning tuning;
        [SerializeField] private PlayerInputReader input;
        [Tooltip("Pitch pivot; the camera sits under it.")]
        [SerializeField] private Transform cameraPivot;

        [Header("Movement")]
        [Tooltip("Base walk speed in m/s. Design: movement is on the fast side.")]
        [Min(0f)] [SerializeField] private float moveSpeed = 7.5f;
        [Min(0f)] [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -24f;
        [Tooltip("Degrees per pixel of mouse movement.")]
        [Min(0f)] [SerializeField] private float lookSensitivity = 0.12f;

        [Header("Dash")]
        [Min(0f)] [SerializeField] private float dashDistance = 6f;
        [Min(0.01f)] [SerializeField] private float dashTime = 0.18f;
        [Tooltip("Camera FOV added during a dash for a sense of speed.")]
        [Min(0f)] [SerializeField] private float dashFovKick = 8f;

        private CharacterController _controller;
        private Camera _camera;
        private float _baseFov;
        private float _pitch;
        private float _verticalVelocity;
        private Vector3 _lungeVelocity;
        private float _lungeTimer;
        private bool _lungeIsDash;
        private DashCharges _dash;

        /// <summary>Multiplier from the current role (Warden 0.9).</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        /// <summary>Set by abilities that slow the player (guarding).</summary>
        public float AbilitySpeedMultiplier { get; set; } = 1f;

        /// <summary>The dash stock (HUD reads it).</summary>
        public DashCharges Dash => _dash;

        /// <summary>True during dash invulnerability.</summary>
        public bool IsInvulnerable => _dash != null && _dash.IsInvulnerable;

        /// <summary>The view camera's transform (aim origin and direction).</summary>
        public Transform Aim => cameraPivot;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _camera = cameraPivot ? cameraPivot.GetComponentInChildren<Camera>() : null;
            if (_camera) _baseFov = _camera.fieldOfView;
            _dash = new DashCharges(tuning.dashCharges, tuning.dashRechargeSeconds, tuning.dashInvulnerableSeconds);
        }

        /// <summary>Pushes the player along <paramref name="direction"/> for <paramref name="distance"/> metres.</summary>
        public void Lunge(Vector3 direction, float distance, float seconds)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 1e-4f || seconds <= 0f) return;
            _lungeVelocity = direction.normalized * (distance / seconds);
            _lungeTimer = seconds;
            _lungeIsDash = false;
        }

        /// <summary>Moves the player up to <paramref name="distance"/> forward, stopping short of walls.</summary>
        public void Teleport(Vector3 direction, float distance)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 1e-4f) return;
            direction.Normalize();
            Vector3 origin = transform.position + Vector3.up * (_controller.height * 0.5f);
            float allowed = distance;
            if (Physics.SphereCast(origin, _controller.radius, direction, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                allowed = Mathf.Max(0f, hit.distance - _controller.radius);
            }
            _controller.enabled = false;
            transform.position += direction * allowed;
            _controller.enabled = true;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _dash.Tick(dt);
            Look();

            Vector3 wish = transform.right * input.Move.x + transform.forward * input.Move.y;
            Vector3 velocity = wish * (moveSpeed * SpeedMultiplier * AbilitySpeedMultiplier);

            if (input.DashPressed && _dash.TryDash())
            {
                Vector3 dir = wish.sqrMagnitude > 0.01f ? wish : transform.forward;
                _lungeVelocity = dir.normalized * (dashDistance / dashTime);
                _lungeTimer = dashTime;
                _lungeIsDash = true;
            }

            if (_lungeTimer > 0f)
            {
                velocity = _lungeVelocity;
                _lungeTimer -= dt;
            }

            if (_controller.isGrounded)
            {
                if (_verticalVelocity < 0f) _verticalVelocity = -2f;
                if (input.JumpPressed) _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            _verticalVelocity += gravity * dt;
            velocity.y = _verticalVelocity;

            _controller.Move(velocity * dt);
            UpdateFov();
        }

        private void Look()
        {
            Vector2 look = input.Look * lookSensitivity;
            transform.Rotate(0f, look.x, 0f, Space.Self);
            _pitch = Mathf.Clamp(_pitch - look.y, -85f, 85f);
            if (cameraPivot) cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void UpdateFov()
        {
            if (!_camera) return;
            float target = _baseFov + (_lungeTimer > 0f && _lungeIsDash ? dashFovKick : 0f);
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, target, 1f - Mathf.Exp(-20f * Time.unscaledDeltaTime));
        }
    }
}
