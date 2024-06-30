using Verse;

namespace PokeWorld.Pokemon_Moves;

internal class DamageWorker_PokemonBite : DamageWorker_PokemonMeleeMove
{
    public override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
    {
        return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
    }
}
