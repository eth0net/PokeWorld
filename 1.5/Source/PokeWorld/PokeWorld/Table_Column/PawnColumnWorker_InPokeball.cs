using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld.Table_Column;

public class PawnColumnWorker_InPokeball : PawnColumnWorker_Icon
{
    public override Texture2D GetIconFor(Pawn pawn)
    {
        if (pawn.TryGetComp<CompPokemon>() != null && pawn.TryGetComp<CompPokemon>().inBall)
            return ContentFinder<Texture2D>.Get("Things/Item/Utility/Balls/PokeBall");
        return null;
    }

    public override string GetIconTip(Pawn pawn)
    {
        if (pawn.TryGetComp<CompPokemon>() != null && pawn.TryGetComp<CompPokemon>().inBall)
            return "PW_TipInPokeball".Translate();
        return null;
    }

    public override int Compare(Pawn a, Pawn b)
    {
        return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
    }

    private new static int GetValueToCompare(Pawn pawn)
    {
        var comp = pawn.TryGetComp<CompPokemon>();
        if (comp == null) return 0;
        return comp.inBall ? 1 : 0;
    }
}
