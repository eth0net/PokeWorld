using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

public static class PokemonDamageUtility
{
    public static int GetNonPokemonPawnDefense(Pawn pawn)
    {
        var armorSharp = GetOverallArmor(pawn, StatDefOf.ArmorRating_Sharp);
        var armorBlunt = GetOverallArmor(pawn, StatDefOf.ArmorRating_Blunt);
        var armorHeat = GetOverallArmor(pawn, StatDefOf.ArmorRating_Heat);
        var totalAverage = armorSharp * 0.4f + armorBlunt * 0.4f + armorHeat * 0.2f;
        return 15 + (int)(totalAverage * 100);
    }

    private static float GetOverallArmor(Pawn pawn, StatDef stat)
    {
        var num = 0f;
        var num2 = Mathf.Clamp01(pawn.GetStatValue(stat) / 2f);
        var allParts = pawn.RaceProps.body.AllParts;
        var list = pawn.apparel?.WornApparel;
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

        return Mathf.Clamp(num * 2f, 0f, 2f);
    }
}
