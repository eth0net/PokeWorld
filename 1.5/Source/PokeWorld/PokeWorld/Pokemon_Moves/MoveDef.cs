using Verse;

namespace PokeWorld.Pokemon_Moves;

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
        return comp != null && comp.types.Contains(type);
    }
}
