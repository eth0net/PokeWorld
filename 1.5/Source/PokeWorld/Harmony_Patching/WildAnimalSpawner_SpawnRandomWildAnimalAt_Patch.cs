using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(WildAnimalSpawner))]
[HarmonyPatch("SpawnRandomWildAnimalAt")]
internal class WildAnimalSpawner_SpawnRandomWildAnimalAt_Patch
{
    public static bool Prefix(WildAnimalSpawner __instance, IntVec3 __0, ref bool __result)
    {
        var foo = __instance.GetType().GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
        var map = (Map)foo.GetValue(__instance);

        PawnKindDef pawnKindDef = null;
        if (PokeWorldSettings.OkforPokemon())
        {
            pawnKindDef = map.Biome.AllWildAnimals
                .Where(
                    a => map.mapTemperature.SeasonAcceptableFor(a.race) && a.race.HasComp(typeof(CompPokemon)) &&
                         PokeWorldSettings.GenerationAllowed(
                             a.race.GetCompProperties<CompProperties_Pokemon>().generation
                         )
                ).RandomElementByWeight(def => map.Biome.CommonalityOfAnimal(def) / def.wildGroupSize.Average);
            if (pawnKindDef == null)
            {
                Log.Error("No spawnable Pokémon right now, defaulting to other animals");
                return true;
            }
        }
        else
        {
            pawnKindDef = map.Biome.AllWildAnimals
                .Where(a => map.mapTemperature.SeasonAcceptableFor(a.race) && !a.race.HasComp(typeof(CompPokemon)))
                .RandomElementByWeight(def => map.Biome.CommonalityOfAnimal(def) / def.wildGroupSize.Average);
            if (pawnKindDef == null) return true;
        }

        var randomInRange = pawnKindDef.wildGroupSize.RandomInRange;
        var radius = Mathf.CeilToInt(Mathf.Sqrt(pawnKindDef.wildGroupSize.max));
        for (var i = 0; i < randomInRange; i++)
        {
            var loc2 = CellFinder.RandomClosewalkCellNear(__0, map, radius);
            GenSpawn.Spawn(PawnGenerator.GeneratePawn(pawnKindDef), loc2, map);
        }

        __result = true;
        return false;
    }
}
