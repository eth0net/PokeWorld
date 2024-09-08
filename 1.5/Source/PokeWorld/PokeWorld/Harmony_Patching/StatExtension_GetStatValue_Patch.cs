using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(StatExtension))]
[HarmonyPatch("GetStatValue")]
internal class StatExtension_GetStatValue_Patch
{
    public static void Postfix(Thing __0, StatDef __1, ref float __result)
    {
        var pawn = __0 as Pawn;
        var comp = pawn.TryGetComp<CompPokemon>();
        if (comp != null)
        {
            if (__1 == StatDefOf.MeleeHitChance)
            {
                __result += comp.levelTracker.level / 100f;
            }
            else if (__1 == StatDefOf.ArmorRating_Sharp)
            {
                __result += Mathf.Clamp(
                    (0.4f * comp.statTracker.defenseStat + 0.6f * comp.statTracker.defenseSpStat) / 100, 0, 1.5f
                );
                __result = Mathf.Clamp(__result, 0, 2.0f);
            }
            else if (__1 == StatDefOf.ArmorRating_Blunt)
            {
                __result += Mathf.Clamp(
                    (0.6f * comp.statTracker.defenseStat + 0.4f * comp.statTracker.defenseSpStat) / 100, 0, 1.5f
                );
                __result = Mathf.Clamp(__result, 0, 2.0f);
            }
            else if (__1 == StatDefOf.ArmorRating_Heat)
            {
                if (comp.Types.Contains(DefDatabase<TypeDef>.GetNamed("Fire")))
                {
                    __result += 1.0f;
                    __result = Mathf.Clamp(__result, 0, 2.0f);
                }
                else
                {
                    __result += Mathf.Clamp(comp.statTracker.defenseSpStat / 200.0f, 0, 1.0f);
                    __result = Mathf.Clamp(__result, 0, 2.0f);
                }
            }
        }
    }
}
