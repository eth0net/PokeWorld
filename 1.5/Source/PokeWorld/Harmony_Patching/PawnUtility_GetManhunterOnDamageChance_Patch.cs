using HarmonyLib;
using RimWorld;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(PawnUtility), "GetManhunterOnDamageChance", typeof(Pawn), typeof(float), typeof(Thing))]
internal class PawnUtility_GetManhunterOnDamageChance_Patch
{
    public static void Postfix(Pawn __0, float __1, Thing __2, ref float __result)
    {
        if (__2 != null)
        {
            var instigator = __2 as Pawn;
            var instigatorComp = instigator.TryGetComp<CompPokemon>();
            if (instigatorComp != null)
            {
                var targetComp = __0.TryGetComp<CompPokemon>();
                if (targetComp != null)
                    __result *= GenMath.LerpDoubleClamped(
                        -10f, 10f, 1f, 3f, targetComp.levelTracker.level - instigatorComp.levelTracker.level
                    );
            }
        }
    }
}
