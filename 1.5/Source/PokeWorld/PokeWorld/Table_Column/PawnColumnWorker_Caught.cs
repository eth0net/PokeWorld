using PokeWorld.Pokedex;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld.Table_Column;

internal class PawnColumnWorker_Caught : PawnColumnWorker_Icon
{
    public override Texture2D GetIconFor(Pawn pawn)
    {
        if (pawn.TryGetComp<CompPokemon>() != null &&
            Find.World.GetComponent<PokedexManager>().IsPokemonCaught(pawn.kindDef))
            return ContentFinder<Texture2D>.Get("Things/Item/Utility/Balls/PokeBall");
        return null;
    }

    public override string GetIconTip(Pawn pawn)
    {
        if (pawn.TryGetComp<CompPokemon>() != null &&
            Find.World.GetComponent<PokedexManager>().IsPokemonCaught(pawn.kindDef))
            return "PW_TipAlreadyCaught".Translate();
        return null;
    }

    public override int Compare(Pawn a, Pawn b)
    {
        return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
    }

    private new int GetValueToCompare(Pawn pawn)
    {
        var comp = pawn.TryGetComp<CompPokemon>();
        if (comp == null) return 0;
        return Find.World.GetComponent<PokedexManager>().IsPokemonCaught(pawn.kindDef) ? 1 : 0;
    }
}
