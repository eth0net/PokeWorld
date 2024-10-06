using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(ThingSetMaker_MapGen_AncientPodContents))]
[HarmonyPatch("GenerateScarabs")]
public class ThingSetMaker_MapGen_AncientPodContents_GenerateScarabs_Patch
{
    public static bool Prefix(ref List<Thing> __result)
    {
        if (PokeWorldSettings.OkforPokemon())
        {
            __result = GenerateUnownAndBronzor();
            if (__result.Count > 0)
                return false;
            return true;
        }

        return true;
    }

    private static List<Thing> GenerateUnownAndBronzor()
    {
        var list = new List<Thing>();
        if (PokeWorldSettings.allowGen2)
        {
            var num = Rand.Range(3, 5);
            for (var i = 0; i < num; i++)
            {
                var unown = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.GetNamed("PW_Unown"));
                list.Add(unown);
            }
        }

        if (PokeWorldSettings.allowGen4)
        {
            var bronzor = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.GetNamed("PW_Bronzor"));
            list.Add(bronzor);
        }

        return list;
    }
}
