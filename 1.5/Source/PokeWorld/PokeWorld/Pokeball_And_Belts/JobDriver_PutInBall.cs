using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PokeWorld;

internal class JobDriver_PutInBall : JobDriver
{
    protected Pawn pokemon => (Pawn)job.targetA.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(pokemon, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOnAggroMentalState(TargetIndex.A);
        this.FailOnThingMissingDesignation(TargetIndex.A, DefDatabase<DesignationDef>.GetNamed("PW_PutInBall"));
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.WaitWith(TargetIndex.A, 100, true);
        var enter = new Toil();
        enter.initAction = delegate { PutInBallUtility.PutPokemonInBall(pokemon); };
        enter.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return enter;
    }
}
