using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

internal class StatWorker_XPYield : StatWorker
{
    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        float value = 0;
        if (req.HasThing && req.Thing is Pawn)
        {
            var pawn = (Pawn)req.Thing;
            var comp = pawn.TryGetComp<CompPokemon>();
            var valueUnfinalized = base.GetValueUnfinalized(
                StatRequest.For(req.BuildableDef, req.StuffDef), applyPostProcess
            );
            if (comp != null)
            {
                value = valueUnfinalized * comp.levelTracker.level / 7f;
            }
            else if (pawn.AnimalOrWildMan() && pawn.kindDef != PawnKindDefOf.WildMan)
            {
                value = 2 * pawn.kindDef.combatPower;
            }
            else
            {
                value = valueUnfinalized * PriceUtility.PawnQualityPriceFactor(pawn);
                value += (int)(300 * (GetOverallArmor(pawn, StatDefOf.ArmorRating_Blunt) +
                                      GetOverallArmor(pawn, StatDefOf.ArmorRating_Heat) + GetOverallArmor(
                                          pawn, StatDefOf.ArmorRating_Sharp
                                      )));
                if (pawn.equipment != null && pawn.equipment.Primary != null &&
                    pawn.equipment.Primary.def.IsRangedWeapon)
                    value += Mathf.Min((int)(pawn.equipment.Primary.MarketValue * 0.8f), 1000);
                else
                    value += Mathf.Min((int)(pawn.GetStatValue(StatDefOf.MeleeDPS) * 40), 1000);
            }
        }
        else if (req.Def != null && req.Def is ThingDef def)
        {
            if (def.race.Animal && !def.HasComp(typeof(CompPokemon)))
                value = 2 * def.race.AnyPawnKind.combatPower;
            else
                value = base.GetValueUnfinalized(StatRequest.For(def, req.StuffDef), applyPostProcess);
        }

        return Mathf.RoundToInt(value);
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        var stringBuilder = new StringBuilder(base.GetExplanationUnfinalized(req, numberSense));
        if (req.HasThing && req.Thing is Pawn)
        {
            var pawn = (Pawn)req.Thing;
            var comp = req.Thing.TryGetComp<CompPokemon>();
            if (comp != null)
            {
                stringBuilder.AppendLine(
                    "PW_StatLevel".Translate(
                        comp.levelTracker.level.ToString(), (comp.levelTracker.level / 7f).ToStringPercent()
                    ).ToLower().CapitalizeFirst()
                );
            }
            else if (pawn.AnimalOrWildMan() && pawn.kindDef != PawnKindDefOf.WildMan)
            {
                stringBuilder.AppendLine(
                    "PW_StatXPYieldCombatPower".Translate(
                        (2 * pawn.kindDef.combatPower /
                         base.GetValueUnfinalized(StatRequest.For(req.BuildableDef, req.StuffDef))).ToStringPercent()
                    )
                );
            }
            else
            {
                PriceUtility.PawnQualityPriceFactor(pawn, stringBuilder);
                stringBuilder.AppendLine(
                    "PW_StatXPYieldApparel".Translate(
                        ((int)(300 * (GetOverallArmor(pawn, StatDefOf.ArmorRating_Blunt) +
                                      GetOverallArmor(pawn, StatDefOf.ArmorRating_Heat) +
                                      GetOverallArmor(pawn, StatDefOf.ArmorRating_Sharp)))).ToString()
                    )
                );
                if (pawn.equipment != null && pawn.equipment.Primary != null &&
                    pawn.equipment.Primary.def.IsRangedWeapon)
                    stringBuilder.AppendLine(
                        "PW_StatXPYieldWeapon".Translate(
                            Mathf.Min((int)(pawn.equipment.Primary.MarketValue * 0.8f), 1000).ToString()
                        )
                    );
                else
                    stringBuilder.AppendLine(
                        "PW_StatXPYieldMeleeDPS".Translate(
                            Mathf.Min((int)(pawn.GetStatValue(StatDefOf.MeleeDPS) * 40), 1000).ToString()
                        )
                    );
            }
        }

        return stringBuilder.ToString();
    }

    public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
    {
        if (!req.HasThing) return "";
        return base.GetExplanationFinalizePart(req, numberSense, finalVal);
    }

    private float GetOverallArmor(Pawn pawn, StatDef stat)
    {
        var num = 0f;
        var num2 = Mathf.Clamp01(pawn.GetStatValue(stat) / 2f);
        var allParts = pawn.RaceProps.body.AllParts;
        var list = pawn.apparel != null ? pawn.apparel.WornApparel : null;
        for (var i = 0; i < allParts.Count; i++)
        {
            var num3 = 1f - num2;
            if (list != null)
                for (var j = 0; j < list.Count; j++)
                    if (list[j].def.apparel.CoversBodyPart(allParts[i]))
                    {
                        var num4 = Mathf.Clamp01(list[j].GetStatValue(stat) / 2f);
                        num3 *= 1f - num4;
                    }

            num += allParts[i].coverageAbs * (1f - num3);
        }

        num = Mathf.Clamp(num * 2f, 0f, 2f);
        return num;
    }
}
