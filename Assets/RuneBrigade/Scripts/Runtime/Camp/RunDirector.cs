using System.Collections;
using RuneBrigade.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneBrigade
{
    /// <summary>
    /// Ties a run scene to the camp: records each new enemy type and fusion for the
    /// archive, shows the first-run shatter hint once, and returns to camp when the
    /// waves are cleared or the player falls (both count as coming back).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RunDirector : MonoBehaviour
    {
        [SerializeField] private CampProgressStore store;
        [SerializeField] private CampLines lines;
        [SerializeField] private DialogueHud dialogue;
        [SerializeField] private CombatDirector director;
        [SerializeField] private WaveSpawner spawner;
        [SerializeField] private PlayerHealth player;
        [SerializeField] private PlayerRoleController roles;
        [Tooltip("Camp scene to load (must be in Build Profiles' scene list).")]
        [SerializeField] private string campSceneName = "Camp";
        [Tooltip("Seconds (real time) between the end of the run and the camp loading.")]
        [Min(0f)] [SerializeField] private float returnDelay = 3f;

        private const string ShatterId = "shatter";
        private bool _hintShown;
        private bool _ending;

        private void Awake()
        {
            store.Load();
        }

        private void OnEnable()
        {
            director.HitLanded += OnHitLanded;
            director.ComboTriggered += OnCombo;
            spawner.Cleared += EndRun;
            player.Defeated += EndRun;
        }

        private void OnDisable()
        {
            director.HitLanded -= OnHitLanded;
            director.ComboTriggered -= OnCombo;
            spawner.Cleared -= EndRun;
            player.Defeated -= EndRun;
        }

        private void OnHitLanded(EnemyHealth target, HitInfo hit, HitResult result)
        {
            if (!target) return;
            if (result.Killed)
            {
                EnemyBrain brain = target.GetComponent<EnemyBrain>();
                if (brain && brain.Definition) store.RecordEnemy(brain.Definition.name);
                return;
            }

            if (!_hintShown && store.Progress.IsFirstRun && !store.Combos.Contains(ShatterId)
                && target.Statuses.Has(StatusType.Freeze) && roles.Switcher.Current != RoleId.Warden)
            {
                _hintShown = true;
                dialogue.Say(lines.shatterHint);
            }
        }

        private void OnCombo(ComboRule rule, ComboTable.Entry entry, Vector3 position)
        {
            store.DiscoverCombo(rule.Id);
        }

        private void EndRun()
        {
            if (_ending) return;
            _ending = true;
            store.Record(CampMilestone.RunReturned);
            StartCoroutine(ReturnToCamp());
        }

        private IEnumerator ReturnToCamp()
        {
            yield return new WaitForSecondsRealtime(returnDelay);
            Time.timeScale = 1f;
            SceneManager.LoadScene(campSceneName);
        }
    }
}
