using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

public class IncidentWorker_PokemonDeepDrillInfestation : IncidentWorker
{
    private const float MinPointsFactor = 0.3f;

    private const float MaxPointsFactor = 0.6f;

    private const float MinPoints = 200f;

    private const float MaxPoints = 1000f;
    private static readonly List<Thing> tmpDrills = new();

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!base.CanFireNowSub(parms)) return false;
        var map = (Map)parms.target;
        tmpDrills.Clear();
        DeepDrillInfestationIncidentUtility.GetUsableDeepDrills(map, tmpDrills);
        return tmpDrills.Any();
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (PokeWorldSettings.OkforPokemon() && PokeWorldSettings.allowPokemonInfestation)
        {
            var map = (Map)parms.target;
            tmpDrills.Clear();
            DeepDrillInfestationIncidentUtility.GetUsableDeepDrills(map, tmpDrills);
            if (!tmpDrills.TryRandomElement(out var deepDrill)) return false;
            var hiveDef = PokemonInfestationUtility.GetInfestationPokemonHiveDef();
            var intVec = CellFinder.FindNoWipeSpawnLocNear(
                deepDrill.Position, map, DefDatabase<ThingDef>.GetNamed("PW_TunnelPokemonHiveSpawner"), Rot4.North, 2,
                x => x.Walkable(map) && x.GetFirstThing(map, deepDrill.def) == null &&
                     x.GetFirstThingWithComp<CompCreatesInfestations>(map) == null &&
                     x.GetFirstThing(map, hiveDef) == null && x.GetFirstThing(
                         map, DefDatabase<ThingDef>.GetNamed("PW_TunnelPokemonHiveSpawner")
                     ) == null
            );
            if (intVec == deepDrill.Position) return false;
            var tunnelHiveSpawner =
                (TunnelPokemonHiveSpawner)ThingMaker.MakeThing(
                    DefDatabase<ThingDef>.GetNamed("PW_TunnelPokemonHiveSpawner")
                );
            tunnelHiveSpawner.pokemonHiveDef = hiveDef;
            tunnelHiveSpawner.spawnHive = false;
            tunnelHiveSpawner.insectsPoints = Mathf.Clamp(parms.points * Rand.Range(0.3f, 0.6f), 200f, 1000f);
            tunnelHiveSpawner.spawnedByInfestationThingComp = true;
            GenSpawn.Spawn(tunnelHiveSpawner, intVec, map, WipeMode.FullRefund);
            deepDrill.TryGetComp<CompCreatesInfestations>().Notify_CreatedInfestation();
            SendStandardLetter(parms, new TargetInfo(tunnelHiveSpawner.Position, map));
            return true;
        }

        return false;
    }
}
