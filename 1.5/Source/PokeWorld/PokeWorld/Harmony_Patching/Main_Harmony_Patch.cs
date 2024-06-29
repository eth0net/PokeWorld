using HarmonyLib;
using Verse;

namespace PokeWorld;

[StaticConstructorOnStartup]
internal class Main
{
    static Main()
    {
        var harmony = new Harmony("com.Rimworld.PokeWorld");
        harmony.PatchAll();
    }
}
