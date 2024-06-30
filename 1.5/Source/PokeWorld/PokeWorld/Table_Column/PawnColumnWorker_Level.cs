using RimWorld;
using Verse;

namespace PokeWorld.Table_Column;

public class PawnColumnWorker_Level : PawnColumnWorker_Text
{
    public override string GetTextFor(Pawn pawn)
    {
        var comp = pawn.TryGetComp<CompPokemon>();
        if (comp != null) return "PW_LevelShort".Translate(comp.levelTracker.level);
        return "";
    }

    public override string GetTip(Pawn pawn)
    {
        var comp = pawn.TryGetComp<CompPokemon>();
        return comp != null ? "PW_LevelLong".Translate(comp.levelTracker.level) : null;
    }

    public override int Compare(Pawn a, Pawn b)
    {
        return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
    }

    private int GetValueToCompare(Pawn pawn)
    {
        var comp = pawn.TryGetComp<CompPokemon>();
        return comp != null ? comp.levelTracker.level : 0;
    }
}
