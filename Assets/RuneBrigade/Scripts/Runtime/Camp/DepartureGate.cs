using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneBrigade
{
    /// <summary>
    /// 出撃門: walking into this trigger starts a run. Put it at the top of the path
    /// down to the village, on a trigger collider.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class DepartureGate : MonoBehaviour
    {
        [Tooltip("Run scene to load (must be in Build Profiles' scene list).")]
        [SerializeField] private string runSceneName = "Arena";
        [Tooltip("Optional: the gate waits while this camp director is still talking.")]
        [SerializeField] private DialogueHud dialogue;

        private bool _leaving;

        private void OnTriggerStay(Collider other)
        {
            if (_leaving || !other.GetComponentInParent<FirstPersonMotor>()) return;
            if (dialogue && dialogue.IsTalking) return;
            _leaving = true;
            SceneManager.LoadScene(runSceneName);
        }
    }
}
