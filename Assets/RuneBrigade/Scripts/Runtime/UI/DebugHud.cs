using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// Prototype HUD drawn with IMGUI: role, health, dash, ultimate, cooldowns,
    /// wave, and the fusion name in the centre. Replace with UI Toolkit later
    /// (layout: design/art-bible.md, "UI"). F1 fills the ultimate gauge for testing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DebugHud : MonoBehaviour
    {
        [SerializeField] private PlayerRoleController roles;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private FirstPersonMotor motor;
        [SerializeField] private CombatDirector director;
        [SerializeField] private WaveSpawner spawner;
        [Tooltip("Seconds (real time) a fusion name stays on screen.")]
        [Min(0f)] [SerializeField] private float comboNameSeconds = 1.2f;

        private string _comboName;
        private Color _comboColor = Color.white;
        private float _comboUntil;
        private GUIStyle _big;
        private GUIStyle _box;

        private void OnEnable()
        {
            if (director) director.ComboTriggered += OnCombo;
        }

        private void OnDisable()
        {
            if (director) director.ComboTriggered -= OnCombo;
        }

        private void Update()
        {
            UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.f1Key.wasPressedThisFrame && roles) roles.Gauge.Add(roles.Gauge.Max);
        }

        private void OnCombo(ComboRule rule, ComboTable.Entry entry, Vector3 position)
        {
            _comboName = rule.DisplayName;
            _comboColor = entry != null ? entry.colorA : Color.white;
            _comboUntil = Time.unscaledTime + comboNameSeconds;
        }

        private void OnGUI()
        {
            if (_big == null)
            {
                _big = new GUIStyle(GUI.skin.label) { fontSize = 56, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                _box = new GUIStyle(GUI.skin.box) { fontSize = 15, alignment = TextAnchor.UpperLeft, wordWrap = true };
            }

            // Crosshair
            float cx = Screen.width / 2f, cy = Screen.height / 2f;
            GUI.Label(new Rect(cx - 10f, cy - 12f, 20f, 24f), "+");

            if (roles && health && motor)
            {
                RoleAbilities kit = roles.ActiveKit;
                RoleSwitcher switcher = roles.Switcher;
                string lines =
                    "役割: " + kit.Definition.displayName + (switcher.IsSwitching ? "（切替中）" : "") +
                    (switcher.IsStrikeReady ? "  スイッチストライク!" : "") + "\n" +
                    "体力: " + Mathf.CeilToInt(health.Pool.Current) + " / " + Mathf.CeilToInt(health.Pool.Max) +
                    (health.ReviveUsed ? "" : "  （加護あり）") + "\n" +
                    "ダッシュ: " + motor.Dash.Charges + " / " + motor.Dash.MaxCharges + "\n" +
                    "奥義: " + Mathf.FloorToInt(roles.Gauge.Fraction * 100f) + "%（F1で満タン）\n" +
                    kit.DebugStatus() + "\n" +
                    "1〜" + switcher.Available.Count + ": 役割切替";
                GUI.Box(new Rect(16f, Screen.height - 230f, 360f, 214f), lines, _box);
            }

            if (spawner)
            {
                GUI.Box(new Rect(Screen.width - 236f, 16f, 220f, 52f), spawner.CurrentLabel + "\n残り: " + spawner.AliveCount, _box);
            }

            if (!string.IsNullOrEmpty(_comboName) && Time.unscaledTime < _comboUntil)
            {
                Color old = GUI.color;
                GUI.color = _comboColor;
                GUI.Label(new Rect(0f, Screen.height * 0.22f, Screen.width, 80f), _comboName, _big);
                GUI.color = old;
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                GUI.Label(new Rect(cx - 120f, cy + 30f, 240f, 24f), "クリックで操作開始 / Escで解除");
            }
        }
    }
}
