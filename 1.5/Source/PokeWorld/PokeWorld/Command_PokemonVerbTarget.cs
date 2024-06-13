using Verse;

namespace PokeWorld
{
    public class Command_PokemonVerbTarget : Command_Target
    {
        public Verb verb;

        public bool drawRadius = true;

        public override void GizmoUpdateOnMouseover()
        {
            if (!drawRadius)
            {
                return;
            }
            verb.verbProps.DrawRadiusRing(verb.caster.Position);
        }
    }
}
