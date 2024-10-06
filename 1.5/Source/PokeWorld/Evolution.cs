using Verse;

namespace PokeWorld;

public class Evolution
{
    public int friendship;

    public Gender gender;

    public ThingDef item;

    public int level;

    public OtherEvolutionRequirement otherRequirement;
    public PawnKindDef pawnKind;

    public EvolutionRequirement requirement;

    public TimeOfDay timeOfDay = TimeOfDay.Any;
}

public enum EvolutionRequirement
{
    level = 0,
    item = 1
}

public enum OtherEvolutionRequirement
{
    none = 0,
    attack = 1,
    defense = 2,
    balanced = 3
}
