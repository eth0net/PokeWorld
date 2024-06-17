using HarmonyLib;
using Verse;

namespace PokeWorld
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            Harmony.DEBUG = true;
            var harmony = new Harmony("com.Rimworld.PokeWorld");
            harmony.PatchAll();
        }
    }
}
