using HarmonyLib;
using RimWorld;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(JobDriver_WatchTelevision))]
[HarmonyPatch("WatchTickAction")]
internal class JobDriver_WatchTelevision_WatchTickAction_Patch
{
    public static void Prefix(JobDriver_WatchTelevision __instance)
    {
        var thing = __instance.job.targetA.Thing;
        var comp = thing.TryGetComp<CompPokemonSpawner>();
        if (comp != null) comp.TickAction(__instance);
    }
}
