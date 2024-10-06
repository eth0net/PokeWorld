using Verse;

namespace PokeWorld;

public static class LegendaryPokemonQuestUtility
{
    public static Pawn GenerateLegendaryPokemon(int tile, PawnKindDef pawnKind = null)
    {
        var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, tile: tile));
        pawn.health.Reset();
        return pawn;
    }
}
