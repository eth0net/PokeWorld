using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PokeWorld;

internal class DamageWorker_PokemonSlash : DamageWorker_PokemonMeleeMove
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

            var num = list.Count - 1 + 0.5f;
            for (var i = 0; i < list.Count; i++)
            {
                var dinfo2 = dinfo;
                dinfo2.SetHitPart(list[i]);
                FinalizeAndAddInjury(pawn, totalDamage / num * (i == 0 ? 0.5f : 1f), dinfo2, result);
            }

            return;
        }

        var num2 = def.cutExtraTargetsCurve != null
            ? GenMath.RoundRandom(def.cutExtraTargetsCurve.Evaluate(Rand.Value))
            : 0;
        List<BodyPartRecord> list2 = null;
        if (num2 != 0)
        {
            var enumerable = dinfo.HitPart.GetDirectChildParts();
            if (dinfo.HitPart.parent != null)
            {
                enumerable = enumerable.Concat(dinfo.HitPart.parent);
                if (dinfo.HitPart.parent.parent != null)
                    enumerable = enumerable.Concat(dinfo.HitPart.parent.GetDirectChildParts());
            }

            list2 = (from x in enumerable.Except(dinfo.HitPart).InRandomOrder().Take(num2)
                where !x.def.conceptual
                select x).ToList();
        }
        else
        {
            list2 = new List<BodyPartRecord>();
        }

        list2.Add(dinfo.HitPart);
        var num3 = totalDamage * (1f + def.cutCleaveBonus) / (list2.Count + def.cutCleaveBonus);
        if (num2 == 0) num3 = ReduceDamageToPreserveOutsideParts(num3, dinfo, pawn);
        for (var j = 0; j < list2.Count; j++)
        {
            var dinfo3 = dinfo;
            dinfo3.SetHitPart(list2[j]);
            FinalizeAndAddInjury(pawn, num3, dinfo3, result);
        }
    }
}
