using RimWorld;
using Verse;

namespace PokeWorld;

internal class Projectile_FireBullet : Bullet
{
    #region Overrides

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        var map = Map;
        var position = Position;

        base.Impact(hitThing);

        GenExplosion.DoExplosion(position, map, 1.9f, DamageDefOf.Flame, null);

        Messages.Message("Lightning_hit", MessageTypeDefOf.NeutralEvent);
    }

    #endregion Overrides
}
