using RimWorld;
using Verse;

namespace PokeWorld;

internal class StatWorker_PokeBallAimingDelayFactor : StatWorker
{
    public override bool ShouldShowFor(StatRequest req)
    {
        if (!base.ShouldShowFor(req)) return false;
        if (req.Def is ThingDef thingDef && thingDef.category == ThingCategory.Pawn) return true;
        return false;
    }
}
