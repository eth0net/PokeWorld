using HarmonyLib;
using RimWorld;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(ResearchManager))]
[HarmonyPatch("FinishProject")]
internal class ResearchManager_FinishProject_Patch
{
    private static void Postfix(ResearchProjectDef __0, Pawn __2)
    {
        if (__0.defName == "PW_CyberspaceProgramming" || __0.defName == "PW_PlanetaryDevelopment" ||
            __0.defName == "PW_ExtraDimensionalActivity")
        {
            var pawn = __2;
            if (pawn != null)
            {
                var pos = pawn.Position;
                var map = pawn.Map;
                switch (__0.defName)
                {
                    case "PW_CyberspaceProgramming":
                        PokemonGeneratorUtility.GenerateAndSpawnNewPokemon(
                            DefDatabase<PawnKindDef>.GetNamed("PW_Porygon"), Faction.OfPlayer, pos, map, pawn, true
                        );
                        Find.World.GetComponent<PokedexManager>()
                            .AddPokemonKindCaught(DefDatabase<PawnKindDef>.GetNamed("PW_Porygon"));
                        break;

                    case "PW_PlanetaryDevelopment":
                        var item1 = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("PW_Upgrade"));
                        GenSpawn.Spawn(item1, pos, map);
                        break;

                    case "PW_ExtraDimensionalActivity":
                        var item2 = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("PW_DubiousDisc"));
                        GenSpawn.Spawn(item2, pos, map);
                        break;
                }
            }
            else
            {
                Log.Error("Researched without having an active pawn researcher.");
            }
        }
    }
}
