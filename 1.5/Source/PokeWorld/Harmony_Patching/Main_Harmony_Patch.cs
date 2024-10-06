using HarmonyLib;
using Verse;

namespace PokeWorld;

[StaticConstructorOnStartup]
internal class Main
{
    static Main()
    {
        Harmony.DEBUG = true;
        var harmony = new Harmony("com.Rimworld.PokeWorld");
        harmony.PatchAll();
    }
}
