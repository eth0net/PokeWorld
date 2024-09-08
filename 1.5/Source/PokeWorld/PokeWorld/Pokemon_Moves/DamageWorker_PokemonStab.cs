using System.Collections.Generic;
using Verse;

namespace PokeWorld;

internal class DamageWorker_PokemonStab : DamageWorker_PokemonMeleeMove
{
    private const float DamageFractionOnOuterParts = 0.75f;

    private const float DamageFractionOnInnerParts = 0.4f;

    protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
    {
        var randomNotMissingPart = pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, dinfo.Depth);
        if (randomNotMissingPart.depth != BodyPartDepth.Inside && Rand.Chance(def.stabChanceOfForcedInternal))
        {
            var randomNotMissingPart2 = pawn.health.hediffSet.GetRandomNotMissingPart(
                dinfo.Def, BodyPartHeight.Undefined, BodyPartDepth.Inside, randomNotMissingPart
            );
            if (randomNotMissingPart2 != null) return randomNotMissingPart2;
        }

        return randomNotMissingPart;
    }

    protected override void ApplySpecialEffectsToPart(
        Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result
    )
    {
        totalDamage = ReduceDamageToPreserveOutsideParts(totalDamage, dinfo, pawn);
        var list = new List<BodyPartRecord>();
        for (var bodyPartRecord = dinfo.HitPart; bodyPartRecord != null; bodyPartRecord = bodyPartRecord.parent)
        {
            list.Add(bodyPartRecord);
            if (bodyPartRecord.depth == BodyPartDepth.Outside) break;
        }

        for (var i = 0; i < list.Count; i++)
        {
            var bodyPartRecord2 = list[i];
            var totalDamage2 = list.Count != 1
                ? bodyPartRecord2.depth == BodyPartDepth.Outside ? totalDamage * 0.75f : totalDamage * 0.4f
                : totalDamage;
            var dinfo2 = dinfo;
            dinfo2.SetHitPart(bodyPartRecord2);
            FinalizeAndAddInjury(pawn, totalDamage2, dinfo2, result);
        }
    }
}
