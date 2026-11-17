using System;

namespace RuneBrigade.Core
{
    /// <summary>
    /// Dash stock: up to <see cref="MaxCharges"/>, refilled one at a time every
    /// <see cref="RechargeSeconds"/>. Each dash grants <see cref="InvulnerableSeconds"/>
    /// of invulnerability. Values: design/game-concept.md, "基本操作".
    /// </summary>
    public sealed class DashCharges
    {
        private float _rechargeTimer;
        private float _invulnerableTimer;

        /// <summary>Creates a full stock.</summary>
        public DashCharges(int maxCharges, float rechargeSeconds, float invulnerableSeconds)
        {
            if (maxCharges < 1) throw new ArgumentOutOfRangeException(nameof(maxCharges));
            if (rechargeSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(rechargeSeconds));
            if (invulnerableSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(invulnerableSeconds));
            MaxCharges = maxCharges;
            RechargeSeconds = rechargeSeconds;
            InvulnerableSeconds = invulnerableSeconds;
            Charges = maxCharges;
        }

        /// <summary>Stock size.</summary>
        public int MaxCharges { get; }

        /// <summary>Seconds to refill one charge.</summary>
        public float RechargeSeconds { get; }

        /// <summary>Invulnerability per dash.</summary>
        public float InvulnerableSeconds { get; }

        /// <summary>Charges available now.</summary>
        public int Charges { get; private set; }

        /// <summary>True during a dash's invulnerability.</summary>
        public bool IsInvulnerable => _invulnerableTimer > 0f;

        /// <summary>Progress of the charge being refilled, 0 to 1 (1 when full).</summary>
        public float RechargeProgress => Charges >= MaxCharges ? 1f : _rechargeTimer / RechargeSeconds;

        /// <summary>Spends a charge if one is available.</summary>
        /// <returns>True when the dash happens.</returns>
        public bool TryDash()
        {
            if (Charges <= 0) return false;
            Charges--;
            _invulnerableTimer = InvulnerableSeconds;
            return true;
        }

        /// <summary>Advances recharge and invulnerability timers.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            if (_invulnerableTimer > 0f) _invulnerableTimer = Math.Max(0f, _invulnerableTimer - deltaTime);
            if (Charges >= MaxCharges)
            {
                _rechargeTimer = 0f;
                return;
            }

            _rechargeTimer += deltaTime;
            while (_rechargeTimer >= RechargeSeconds && Charges < MaxCharges)
            {
                _rechargeTimer -= RechargeSeconds;
                Charges++;
            }
            if (Charges >= MaxCharges) _rechargeTimer = 0f;
        }
    }
}
