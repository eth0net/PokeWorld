using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(GenStep_CaveHives))]
[HarmonyPatch("TrySpawnHive")]
public class GenStep_CaveHives_TrySpawnHive_Patch
{
    public static bool Prefix(GenStep_CaveHives __instance, Map __0)
    {
        if (PokeWorldSettings.OkforPokemon() && PokeWorldSettings.allowPokemonInfestation)
        {
            var field1 = __instance.GetType().GetField(
                "possibleSpawnCells", BindingFlags.NonPublic | BindingFlags.Instance
            );
            var possibleSpawnCells = (List<IntVec3>)field1.GetValue(__instance);

            var field2 = __instance.GetType().GetField("spawnedHives", BindingFlags.NonPublic | BindingFlags.Instance);
            var spawnedHives = (List<Hive>)field2.GetValue(__instance);

            var method = __instance.GetType().GetMethod(
                "TryFindHiveSpawnCell", BindingFlags.NonPublic | BindingFlags.Instance
            );
            object[] parameters = { __0, null };
            var result = method.Invoke(__instance, parameters);
            var blResult = (bool)result;
            if (blResult)
            {
                var spawnCell = (IntVec3)parameters[1];
                possibleSpawnCells.Remove(spawnCell);
                var hiveDef = PokemonInfestationUtility.GetNaturalPokemonHiveDef();
                var hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(hiveDef), spawnCell, __0);
                hive.SetFaction(
                    Find.FactionManager.AllFactions.Where(f => f.def.defName == "PW_HostilePokemon").First()
                );
                hive.PawnSpawner.aggressive = false;
                (from x in hive.GetComps<CompSpawner>()
                    where x.PropsSpawner.thingToSpawn == ThingDefOf.GlowPod
                    select x).First().TryDoSpawn();
                hive.PawnSpawner.SpawnPawnsUntilPoints(Rand.Range(200f, 500f));
                hive.PawnSpawner.canSpawnPawns = false;
                hive.GetComp<CompSpawnerHives>().canSpawnHives = false;
                spawnedHives.Add(hive);
            }

            return false;
        }

        return true;
    }
}
