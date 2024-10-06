using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PokeWorld;

public class Recipe_HealingItem : Recipe_Surgery
{
    //Surgery recipe, not crafting recipe
    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        if (billDoer != null)
        {
            var hpLeftToHeal = ingredients[0].TryGetComp<CompHealingItem>().HealingAmount;
            for (var i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                var hediff = FindMostBleedingHediff(pawn);
                if (hediff != null)
                {
                    var heddifSev = hediff.Severity;
                    if (hediff.Severity <= hpLeftToHeal)
                    {
                        hediff.Heal(heddifSev);
                        hpLeftToHeal -= heddifSev;
                        continue;
                    }

                    hediff.Heal(hpLeftToHeal);
                    break;
                }

                var hediff_Injury = FindInjury(pawn);
                if (hediff_Injury != null)
                {
                    var heddifSev = hediff_Injury.Severity;
                    if (hediff_Injury.Severity <= hpLeftToHeal)
                    {
                        hediff_Injury.Heal(heddifSev);
                        hpLeftToHeal -= heddifSev;
                        continue;
                    }

                    hediff_Injury.Heal(hpLeftToHeal);
                }

                break;
            }
        }
    }

    private Hediff FindMostBleedingHediff(Pawn pawn)
    {
        var num = 0f;
        Hediff hediff = null;
        var hediffs = pawn.health.hediffSet.hediffs;
        for (var i = 0; i < hediffs.Count; i++)
            if (hediffs[i].Visible && hediffs[i].BleedRate > 0f)
            {
                var bleedRate = hediffs[i].BleedRate;
                if (bleedRate > num || hediff == null)
                {
                    num = bleedRate;
                    hediff = hediffs[i];
                }
            }

        return hediff;
    }

    private Hediff_Injury FindInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
    {
        Hediff_Injury hediff_Injury = null;
        var hediffs = pawn.health.hediffSet.hediffs;
        for (var i = 0; i < hediffs.Count; i++)
            if (hediffs[i] is Hediff_Injury hediff_Injury2 && hediff_Injury2.Visible && hediff_Injury2.def.tendable &&
                !hediff_Injury2.IsPermanent() &&
                (allowedBodyParts == null || allowedBodyParts.Contains(hediff_Injury2.Part)) &&
                (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity))
                hediff_Injury = hediff_Injury2;
        return hediff_Injury;
    }
}
