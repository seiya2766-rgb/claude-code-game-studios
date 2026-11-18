using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RuneBrigade
{
    /// <summary>
    /// Shows spoken lines one at a time at the bottom of the screen. F or Enter
    /// advances; each line also moves on by itself. Prototype IMGUI, like DebugHud;
    /// IMGUI is not recommended for runtime UI in Unity 6 (docs/engine-reference/unity/modules/ui.md),
    /// so replace it with UI Toolkit before production.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DialogueHud : MonoBehaviour
    {
        [Tooltip("Seconds (real time) before a line moves on by itself.")]
        [Min(0.5f)] [SerializeField] private float secondsPerLine = 5f;

        private readonly Queue<CampLines.Line> _queue = new Queue<CampLines.Line>();
        private CampLines.Line _current;
        private float _until;
        private GUIStyle _speaker;
        private GUIStyle _text;

        /// <summary>True while a line is on screen or waiting.</summary>
        public bool IsTalking => _current != null || _queue.Count > 0;

        /// <summary>Queues a line. Null or empty lines are ignored.</summary>
        public void Say(CampLines.Line line)
        {
            if (line == null || string.IsNullOrEmpty(line.text)) return;
            _queue.Enqueue(line);
            if (_current == null) Next();
        }

        /// <summary>Queues several lines in order.</summary>
        public void SayAll(IEnumerable<CampLines.Line> lines)
        {
            if (lines == null) return;
            foreach (CampLines.Line line in lines) Say(line);
        }

        private void Next()
        {
            _current = _queue.Count > 0 ? _queue.Dequeue() : null;
            _until = Time.unscaledTime + secondsPerLine;
        }

        private void Update()
        {
            if (_current == null) return;
            Keyboard keyboard = Keyboard.current;
            bool advance = keyboard != null && (keyboard.fKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame);
            if (advance || Time.unscaledTime >= _until) Next();
        }

        private void OnGUI()
        {
            if (_current == null) return;
            if (_speaker == null)
            {
                _speaker = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
                _text = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true };
            }
            float width = Mathf.Min(900f, Screen.width - 64f);
            var box = new Rect((Screen.width - width) / 2f, Screen.height - 170f, width, 120f);
            GUI.Box(box, "");
            GUI.Label(new Rect(box.x + 20f, box.y + 12f, width - 40f, 24f), _current.speaker, _speaker);
            GUI.Label(new Rect(box.x + 20f, box.y + 40f, width - 40f, 60f), _current.text, _text);
            GUI.Label(new Rect(box.x + width - 160f, box.y + 96f, 150f, 20f), "F / Enter で次へ");
        }
    }
}
