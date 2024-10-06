using Verse;

namespace PokeWorld;

public class Command_PokemonVerbTarget : Command_Target
{
    public bool drawRadius = true;
    public Verb verb;

    public override void GizmoUpdateOnMouseover()
    {
        if (!drawRadius) return;
        verb.verbProps.DrawRadiusRing(verb.caster.Position);
    }
}
