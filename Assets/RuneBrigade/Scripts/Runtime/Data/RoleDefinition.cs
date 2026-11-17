using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>Per-role numbers shared by every ability of that role.</summary>
    [CreateAssetMenu(menuName = "Rune Brigade/Role Definition", fileName = "Role_")]
    public sealed class RoleDefinition : ScriptableObject
    {
        [Tooltip("Which role this asset describes.")]
        public RoleId role;
        [Tooltip("Name shown in the HUD.")]
        public string displayName = "守護者";
        [Tooltip("Role colour (art bible). Warden #F2B33D, Arcanist #FF5A36, Hunter #5BEF8A, Oracle #FFF6D8.")]
        public Color themeColor = new Color(0.949f, 0.702f, 0.239f);
        [Tooltip("Max health multiplier. Design: Warden 1.5, Arcanist 0.8, Hunter 1, Oracle 1.")]
        [Min(0.1f)] public float healthMultiplier = 1f;
        [Tooltip("Move speed multiplier. Design: Warden 0.9.")]
        [Min(0.1f)] public float moveSpeedMultiplier = 1f;
    }
}
