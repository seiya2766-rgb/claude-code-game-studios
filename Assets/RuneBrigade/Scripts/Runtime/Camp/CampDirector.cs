using System.Collections.Generic;
using RuneBrigade.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RuneBrigade
{
    /// <summary>
    /// Runs the camp scene: loads progress, shows who and what has returned, fades
    /// colour back, and has Garo speak (wake-up lines on the first visit, return
    /// announcements afterwards). Debug keys: F2 = area 1 boss defeated,
    /// F3 = area 2 boss defeated, F9 = erase all progress.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CampDirector : MonoBehaviour
    {
        [SerializeField] private CampProgressStore store;
        [SerializeField] private CampLines lines;
        [SerializeField] private DialogueHud dialogue;
        [Tooltip("Every feature object in the camp. Empty = found under this object.")]
        [SerializeField] private List<CampFeatureView> views = new List<CampFeatureView>();
        [Tooltip("Every colour target in the camp. Empty = found under this object.")]
        [SerializeField] private List<CampColorTarget> colorTargets = new List<CampColorTarget>();
        [Tooltip("Allow F2 / F3 / F9 for testing.")]
        [SerializeField] private bool debugKeys = true;

        private void Start()
        {
            if (views.Count == 0) views.AddRange(GetComponentsInChildren<CampFeatureView>(true));
            if (colorTargets.Count == 0) colorTargets.AddRange(GetComponentsInChildren<CampColorTarget>(true));

            store.Load();
            ApplyAll(instant: true);

            List<CampFeature> returned = store.TakePendingAnnouncements();
            if (store.Progress.IsFirstRun) dialogue.SayAll(lines.wakeLines);
            else if (returned.Count == 0) dialogue.Say(lines.welcomeBack);
            else Announce(returned);
        }

        private void Update()
        {
            if (!debugKeys) return;
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.f2Key.wasPressedThisFrame) RecordAndAnnounce(CampMilestone.Area1BossDefeated);
            if (keyboard.f3Key.wasPressedThisFrame) RecordAndAnnounce(CampMilestone.Area2BossDefeated);
            if (keyboard.f9Key.wasPressedThisFrame)
            {
                store.ResetAll();
                ApplyAll(instant: true);
                dialogue.SayAll(lines.wakeLines);
            }
        }

        private void RecordAndAnnounce(CampMilestone milestone)
        {
            store.Record(milestone);
            Announce(store.TakePendingAnnouncements());
            ApplyAll(instant: false);
        }

        private void Announce(List<CampFeature> features)
        {
            foreach (CampFeature feature in features) dialogue.Say(lines.For(feature));
        }

        private void ApplyAll(bool instant)
        {
            CampProgress progress = store.Progress;
            foreach (CampFeatureView view in views) if (view) view.Apply(progress);
            foreach (CampColorTarget target in colorTargets) if (target) target.Apply(progress, instant);
        }
    }
}
