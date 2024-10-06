using System.Linq;
using RimWorld;
using Verse;

namespace PokeWorld;

public static class FishingUtility
{
    private static readonly TerrainDef[] fishingTerrains =
    {
        TerrainDefOf.WaterShallow,
        TerrainDefOf.WaterDeep,
        TerrainDefOf.WaterMovingShallow,
        TerrainDefOf.WaterMovingChestDeep,
        TerrainDefOf.WaterOceanShallow,
        TerrainDefOf.WaterOceanDeep,
        TerrainDef.Named("Marsh")
    };

    public static int ComputeFishingTicks(Pawn pawn, Thing fishingRod)
    {
        var ticks = (int)(1200f * (1f / pawn.GetStatValue(StatDef.Named("PW_FishingSpeed"))) *
                          (1f / fishingRod.GetStatValue(StatDef.Named("PW_FishingSpeedMultipler"))));
        return ticks;
    }

    public static float ComputeLineBreakChance(Pawn pawn, Thing fishingRod)
    {
        var pawnBreakProb = pawn.GetStatValue(StatDef.Named("PW_FishingLineBreak"));
        var rodStrenght = fishingRod.GetStatValue(StatDef.Named("PW_FishingLineStrenghtMultiplier"));
        if (rodStrenght >= 1) return pawnBreakProb * (1 / rodStrenght);
        return 1 - (1 - pawnBreakProb) * rodStrenght;
    }

    public static bool TryFish(Pawn pawn, Thing fishingRod, IntVec3 cell)
    {
        if (Rand.Chance(ComputeLineBreakChance(pawn, fishingRod)))
        {
            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "Line broke");
            return false;
        }

        var kind = DefDatabase<FishingRateDef>.GetRandom().GetRandomFish(
            pawn.Map.Biome, cell.GetTerrain(pawn.Map), fishingRod.def
        );
        var pokemon = PokemonGeneratorUtility.GenerateAndSpawnNewPokemon(kind, null, cell, pawn.Map);
        pokemon.caller.DoCall();
        return true;
    }

    public static bool IsFishingTerrain(TerrainDef terrain)
    {
        return fishingTerrains.Contains(terrain);
    }
}
