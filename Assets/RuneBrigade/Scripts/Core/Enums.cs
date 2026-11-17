namespace RuneBrigade.Core
{
    /// <summary>The four roles a player can take. See design/game-concept.md, "4つの役割".</summary>
    public enum RoleId
    {
        /// <summary>守護者: hammer and shield, gathers and launches enemies.</summary>
        Warden = 0,
        /// <summary>術士: staff with fire, ice and lightning.</summary>
        Arcanist = 1,
        /// <summary>狩人: longbow, weak points and binding.</summary>
        Hunter = 2,
        /// <summary>祈祷師: healing beam, barriers and amplification.</summary>
        Oracle = 3,
    }

    /// <summary>Statuses an enemy can carry. Fusion spells consume them.</summary>
    public enum StatusType
    {
        /// <summary>集結: pulled together by the Warden's roar.</summary>
        Gather = 0,
        /// <summary>打ち上げ: knocked into the air.</summary>
        Launch = 1,
        /// <summary>炎上: burning.</summary>
        Burn = 2,
        /// <summary>凍結: frozen solid.</summary>
        Freeze = 3,
        /// <summary>帯電: charged with lightning.</summary>
        Shock = 4,
        /// <summary>弱点露出: weak point revealed.</summary>
        Expose = 5,
        /// <summary>拘束: bound by vines.</summary>
        Bind = 6,
        /// <summary>聖化: sanctified by a barrier.</summary>
        Sanctify = 7,
        /// <summary>増幅: amplified by the Oracle.</summary>
        Amplify = 8,
    }

    /// <summary>
    /// What kind of attack landed. Fusion rules trigger on these tags, so a new
    /// attack only needs a tag, not new fusion code.
    /// </summary>
    public enum AttackTag
    {
        /// <summary>Any attack with no fusion role.</summary>
        Generic = 0,
        /// <summary>Warden hammer hits 1 and 2.</summary>
        WardenCombo = 1,
        /// <summary>Warden hammer hit 3 (launches).</summary>
        WardenFinisher = 2,
        /// <summary>Damage reflected through the Warden's guard.</summary>
        ShieldGuard = 3,
        /// <summary>Warden shield charge.</summary>
        ShieldCharge = 4,
        /// <summary>Arcanist basic element bolt.</summary>
        ElementBolt = 5,
        /// <summary>Arcanist area spell.</summary>
        ElementBlast = 6,
        /// <summary>Arcanist ultimate.</summary>
        Meteor = 7,
        /// <summary>Hunter charged arrow.</summary>
        ChargedShot = 8,
        /// <summary>Hunter spread arrows.</summary>
        SpreadArrow = 9,
        /// <summary>Any ultimate ability.</summary>
        Ultimate = 10,
    }

    /// <summary>Which pool of attack tokens an enemy draws from.</summary>
    public enum AttackKind
    {
        /// <summary>Close-range attackers.</summary>
        Melee = 0,
        /// <summary>Ranged attackers.</summary>
        Ranged = 1,
    }
}
