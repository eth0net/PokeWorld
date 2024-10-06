using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PokeWorld;

internal class DamageWorker_PokemonScratch : DamageWorker_PokemonMeleeMove
{
    protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
    {
        return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
    }

    protected override void ApplySpecialEffectsToPart(
        Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result
    )
    {
        if (dinfo.HitPart.depth == BodyPartDepth.Inside)
        {
            var list = new List<BodyPartRecord>();
            for (var bodyPartRecord = dinfo.HitPart; bodyPartRecord != null; bodyPartRecord = bodyPartRecord.parent)
            {
                list.Add(bodyPartRecord);
                if (bodyPartRecord.depth == BodyPartDepth.Outside) break;
            }

            float num = list.Count;
            for (var i = 0; i < list.Count; i++)
            {
                var dinfo2 = dinfo;
                dinfo2.SetHitPart(list[i]);
                FinalizeAndAddInjury(pawn, totalDamage / num, dinfo2, result);
            }

            return;
        }

        var enumerable = dinfo.HitPart.GetDirectChildParts();
        if (dinfo.HitPart.parent != null)
        {
            enumerable = enumerable.Concat(dinfo.HitPart.parent);
            if (dinfo.HitPart.parent.parent != null)
                enumerable = enumerable.Concat(dinfo.HitPart.parent.GetDirectChildParts());
        }

        enumerable = enumerable.Where(
            target => target != dinfo.HitPart && !target.def.conceptual && target.depth == BodyPartDepth.Outside &&
                      !pawn.health.hediffSet.PartIsMissing(target)
        );
        var bodyPartRecord2 = enumerable.RandomElementWithFallback();
        if (bodyPartRecord2 == null)
        {
            FinalizeAndAddInjury(pawn, ReduceDamageToPreserveOutsideParts(totalDamage, dinfo, pawn), dinfo, result);
            return;
        }

        FinalizeAndAddInjury(
            pawn, ReduceDamageToPreserveOutsideParts(totalDamage * def.scratchSplitPercentage, dinfo, pawn), dinfo,
            result
        );
        var dinfo3 = dinfo;
        dinfo3.SetHitPart(bodyPartRecord2);
        FinalizeAndAddInjury(
            pawn, ReduceDamageToPreserveOutsideParts(totalDamage * def.scratchSplitPercentage, dinfo3, pawn), dinfo3,
            result
        );
    }
}
