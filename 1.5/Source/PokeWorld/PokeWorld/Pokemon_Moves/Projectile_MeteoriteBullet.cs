using RimWorld;
using Verse;

namespace PokeWorld;

internal class Projectile_MeteoriteBullet : Bullet
{
    #region Overrides

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        var map = Map;
        var position = Position;

        base.Impact(hitThing);

        SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, position, map);

        Messages.Message("Meteorite_hit", MessageTypeDefOf.NeutralEvent);
    }

    #endregion Overrides
}
