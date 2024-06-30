using RimWorld;
using Verse;

namespace PokeWorld.Stats;

internal class StatWorker_PokeBallAimingDelayFactor : StatWorker
{
    public override bool ShouldShowFor(StatRequest req)
    {
        if (!base.ShouldShowFor(req)) return false;
        return req.Def is ThingDef { category: ThingCategory.Pawn };
    }
}
