using HarmonyLib;
using Verse;

namespace PokeWorld.Harmony_Patching;

[StaticConstructorOnStartup]
internal class Main
{
    static Main()
    {
#if DEBUG
        Harmony.DEBUG = true;
#endif
        var harmony = new Harmony("com.Rimworld.PokeWorld");
        harmony.PatchAll();
    }
}
