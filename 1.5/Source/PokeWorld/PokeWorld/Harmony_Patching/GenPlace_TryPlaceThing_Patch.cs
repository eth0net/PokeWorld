using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PokeWorld;

[HarmonyPatch(
    typeof(GenPlace), nameof(GenPlace.TryPlaceThing), typeof(Thing), typeof(IntVec3), typeof(Map),
    typeof(ThingPlaceMode), typeof(Action<Thing, int>), typeof(Predicate<IntVec3>), typeof(Rot4)
)]
internal class GenPlace_TryPlaceThing_Patch
{
    public static bool Prefix(Thing __0, IntVec3 __1, Map __2, ref bool __result)
    {
        if (__0.def == DefDatabase<ThingDef>.GetNamed("PW_Porygon"))
        {
            __result = true;
            //Pawn revivedPokemon = PokemonGeneratorUtility.GenerateAndSpawnNewPokemon(DefDatabase<PawnKindDef>.GetNamed("PW_Porygon"), Faction.OfPlayer, __1, __2, null, true, false);
            PokemonGeneratorUtility.GenerateAndSpawnNewPokemon(
                DefDatabase<PawnKindDef>.GetNamed("PW_Porygon"), Faction.OfPlayer, __1, __2, null, true
            );
            Find.World.GetComponent<PokedexManager>()
                .AddPokemonKindCaught(DefDatabase<PawnKindDef>.GetNamed("PW_Porygon"));
            //Messages.Message("PW_CraftedPokemon".Translate(actor.LabelShortCap, revivedPokemon.KindLabel), revivedPokemon, MessageTypeDefOf.PositiveEvent);
            return false;
        }

        return true;
    }
}
