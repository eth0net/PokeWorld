using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PokeWorld;

internal class IncidentWorker_PokemonHerdMigration : IncidentWorker
{
    private const float MinTotalBodySize = 4f;
    private static readonly IntRange AnimalsCount = new(3, 5);

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        var map = (Map)parms.target;
        IntVec3 start;
        IntVec3 end;
        if (TryFindAnimalKind(map.Tile, out var _)) return TryFindStartAndEndCells(map, out start, out end);
        return false;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (PokeWorldSettings.OkforPokemon())
        {
            var map = (Map)parms.target;
            if (!TryFindAnimalKind(map.Tile, out var animalKind)) return false;
            if (!TryFindStartAndEndCells(map, out var start, out var end)) return false;
            var rot = Rot4.FromAngleFlat((map.Center - start).AngleFlat);
            var list = GenerateAnimals(animalKind, map.Tile);
            for (var i = 0; i < list.Count; i++)
            {
                var newThing = list[i];
                var loc = CellFinder.RandomClosewalkCellNear(start, map, 10);
                GenSpawn.Spawn(newThing, loc, map, rot);
            }

            LordMaker.MakeNewLord(null, new LordJob_ExitMapNear(end, LocomotionUrgency.Walk), map, list);
            var str = string.Format(def.letterText, animalKind[0].GetLabelPlural()).CapitalizeFirst();
            var str2 = string.Format(def.letterLabel, animalKind[0].GetLabelPlural().CapitalizeFirst());
            SendStandardLetter(str2, str, def.letterDef, parms, list[0]);
            return true;
        }

        return false;
    }

    private bool TryFindAnimalKind(int tile, out List<PawnKindDef> allPokemonKind)
    {
        var source = DefDatabase<PawnKindDef>.AllDefs.Where(
            k => k.RaceProps.Animal && k.race.HasComp(typeof(CompPokemon)) &&
                 PokeWorldSettings.GenerationAllowed(k.race.GetCompProperties<CompProperties_Pokemon>().generation) &&
                 k.RaceProps.CanDoHerdMigration && (tile == -1 ||
                                                    Find.World.tileTemperatures
                                                        .SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race))
        );
        PawnKindDef singlePokemonKind = null;
        if (source.Any())
        {
            if (source.TryRandomElementByWeight(x => Mathf.Lerp(0.2f, 1f, x.RaceProps.wildness), out singlePokemonKind))
            {
            }
            else
            {
                allPokemonKind = null;
                return false;
            }

            var evolutionLine =
                (singlePokemonKind.race.comps.Find(y => y.compClass == typeof(CompPokemon)) as CompProperties_Pokemon)
                .evolutionLine;
            allPokemonKind = DefDatabase<PawnKindDef>.AllDefs.Where(
                x => x.RaceProps.Animal && x.RaceProps.CanDoHerdMigration && x.race.HasComp(typeof(CompPokemon)) &&
                     (x.race.comps.Find(y => y.compClass == typeof(CompPokemon)) as CompProperties_Pokemon)
                     .evolutionLine == evolutionLine &&
                     (tile == -1 || Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, x.race))
            ).ToList();
            return true;
        }

        allPokemonKind = null;
        return false;
    }

    private bool TryFindStartAndEndCells(Map map, out IntVec3 start, out IntVec3 end)
    {
        if (!RCellFinder.TryFindRandomPawnEntryCell(out start, map, CellFinder.EdgeRoadChance_Animal))
        {
            end = IntVec3.Invalid;
            return false;
        }

        end = IntVec3.Invalid;
        for (var i = 0; i < 8; i++)
        {
            var startLocal = start;
            if (!CellFinder.TryFindRandomEdgeCellWith(
                    x => map.reachability.CanReach(
                        startLocal, x, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly
                    ), map, CellFinder.EdgeRoadChance_Ignore, out var result
                )) break;
            if (!end.IsValid || result.DistanceToSquared(start) > end.DistanceToSquared(start)) end = result;
        }

        return end.IsValid;
    }

    private List<Pawn> GenerateAnimals(List<PawnKindDef> allPokemonKind, int tile)
    {
        var list = new List<Pawn>();
        for (var i = 0; i < allPokemonKind.Count; i++)
        {
            var randomInRange = AnimalsCount.RandomInRange;
            randomInRange = Mathf.Max(
                randomInRange, Mathf.CeilToInt(4f / (allPokemonKind[i].RaceProps.baseBodySize * allPokemonKind.Count))
            );

            for (var j = 0; j < randomInRange; j++)
            {
                var item = PawnGenerator.GeneratePawn(
                    new PawnGenerationRequest(allPokemonKind[i], null, PawnGenerationContext.NonPlayer, tile)
                );
                list.Add(item);
            }
        }

        return list;
    }
}
