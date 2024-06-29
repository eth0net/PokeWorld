using Verse;

namespace PokeWorld
{
    public class Evolution
    {
        public int friendship = 0;
        public Gender gender = Gender.None;

        public ThingDef item;

        public int level = 1;
        public OtherEvolutionRequirement otherRequirement = OtherEvolutionRequirement.none;
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
}
