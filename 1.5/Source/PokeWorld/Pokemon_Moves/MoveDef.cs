using Verse;

namespace PokeWorld;

public class MoveDef : Def
{
    public float accuracy;
    public MoveCategory category;
    public Tool tool;
    public TypeDef type;
    public VerbProperties verb;

    public bool IsStab(Pawn pawn)
    {
        var comp = pawn.TryGetComp<CompPokemon>();
        if (comp != null)
            if (comp.Types.Contains(type))
                return true;
        return false;
    }
}
