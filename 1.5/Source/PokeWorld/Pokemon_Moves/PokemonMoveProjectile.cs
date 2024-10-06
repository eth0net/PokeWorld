using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PokeWorld;

public class PokemonMoveProjectile : Projectile
{
    protected MoveDef moveDef;

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        var map = Map;
        var position = Position;
        base.Impact(hitThing);
        moveDef = ((Pawn)launcher).TryGetComp<CompPokemon>().moveTracker.lastUsedMove;
        var battleLogEntry_RangedImpact = new BattleLogEntry_PokemonRangedMoveImpact(
            launcher, hitThing, intendedTarget.Thing, moveDef, def, targetCoverDef
        );
        Find.BattleLog.Add(battleLogEntry_RangedImpact);
        NotifyImpact(hitThing, map, position);
        if (hitThing != null)
        {
            var dinfo = new DamageInfo(
                def.projectile.damageDef, base.DamageAmount, base.ArmorPenetration, ExactRotation.eulerAngles.y,
                launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing
            );
            hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
            var pawn = hitThing as Pawn;
            if (pawn != null && pawn.stances != null && pawn.BodySize <= def.projectile.StoppingPower + 0.001f)
                pawn.stances.stagger.StaggerFor(95);
            if (def.projectile.extraDamages == null) return;
            foreach (var extraDamage in def.projectile.extraDamages)
                if (Rand.Chance(extraDamage.chance))
                {
                    var dinfo2 = new DamageInfo(
                        extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(),
                        ExactRotation.eulerAngles.y, launcher, null, equipmentDef,
                        DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing
                    );
                    hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
                }
        }
        else
        {
            SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(Position, map));
            if (Position.GetTerrain(map).takeSplashes)
                FleckMaker.WaterSplash(ExactPosition, map, Mathf.Sqrt(base.DamageAmount) * 1f, 4f);
            else
                FleckMaker.Static(ExactPosition, map, FleckDefOf.ShotHit_Dirt);
        }
    }

    private void NotifyImpact(Thing hitThing, Map map, IntVec3 position)
    {
        var bulletImpactData = default(BulletImpactData);
        bulletImpactData.bullet = this as Projectile as Bullet;
        bulletImpactData.hitThing = hitThing;
        bulletImpactData.impactPosition = position;
        var impactData = bulletImpactData;
        hitThing?.Notify_BulletImpactNearby(impactData);
        var num = 9;
        for (var i = 0; i < num; i++)
        {
            var c = position + GenRadial.RadialPattern[i];
            if (!c.InBounds(map)) continue;
            var thingList = c.GetThingList(map);
            for (var j = 0; j < thingList.Count; j++)
                if (thingList[j] != hitThing)
                    thingList[j].Notify_BulletImpactNearby(impactData);
        }
    }
}
