using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PokeWorld;

public class JobDriver_LayDittoEgg : JobDriver
{
    private const int LayEgg = 500;

    private const TargetIndex LaySpotInd = TargetIndex.A;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Goto.GotoCell(LaySpotInd, PathEndMode.OnCell);
        yield return Toils_General.Wait(LayEgg);
        yield return Toils_General.Do(
            delegate
            {
                var thing = pawn.GetComp<CompDittoEggLayer>().ProduceEgg();
                if (thing != null) GenSpawn.Spawn(thing, pawn.Position, pawn.Map).SetForbiddenIfOutsideHomeArea();
            }
        );
    }
}
