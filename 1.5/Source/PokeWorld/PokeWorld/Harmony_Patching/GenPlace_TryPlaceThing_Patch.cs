using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using PokeWorld.Pokedex;
using RimWorld;
using Verse;

namespace PokeWorld.Harmony_Patching;

[HarmonyPatch(typeof(GenPlace))]
[HarmonyPatch(nameof(GenPlace.TryPlaceThing))]
[HarmonyPatch([
    typeof(Thing), typeof(IntVec3), typeof(Map),
    typeof(ThingPlaceMode), typeof(Action<Thing, int>), typeof(Predicate<IntVec3>), typeof(Rot4)
])]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class GenPlace_TryPlaceThing_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static bool Prefix(Thing __0, IntVec3 __1, Map __2, ref bool __result)
    {
        if (__0.def != DefDatabase<ThingDef>.GetNamed("PW_Porygon")) return true;
        __result = true;
        PokemonGeneratorUtility.GenerateAndSpawnNewPokemon(
            DefDatabase<PawnKindDef>.GetNamed("PW_Porygon"), Faction.OfPlayer, __1, __2, null, true);
        Find.World.GetComponent<PokedexManager>()
            .AddPokemonKindCaught(DefDatabase<PawnKindDef>.GetNamed("PW_Porygon"));
        return false;
    }
}
