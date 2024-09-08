using RimWorld;
using Verse;

namespace PokeWorld;

internal class Projectile_LightningBullet : Bullet
{
    #region Overrides

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        var map = Map;
        var position = Position;

        base.Impact(hitThing);

        map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(map, position));

        Messages.Message("Lightning_hit", MessageTypeDefOf.NeutralEvent);
    }

    #endregion Overrides
}
