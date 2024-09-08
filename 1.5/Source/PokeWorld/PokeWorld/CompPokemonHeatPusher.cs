using UnityEngine;
using Verse;

namespace PokeWorld;

internal class CompPokemonHeatPusher : CompHeatPusher
{
    public override void CompTickRare()
    {
        if (parent != null && parent is Pawn pawn && pawn.Spawned && !pawn.Dead && ShouldPushHeatNow)
        {
            var comp = pawn.TryGetComp<CompPokemon>();
            if (comp != null)
                GenTemperature.PushHeat(
                    pawn.PositionHeld, pawn.MapHeld,
                    Props.heatPerSecond * 4.16666651f * Mathf.Sqrt(comp.levelTracker.level) / 2
                );
        }
    }
}
