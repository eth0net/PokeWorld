using HarmonyLib;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(PawnGraphicSet))]
[HarmonyPatch("ResolveAllGraphics")]
internal class PawnGraphicSet_ResolveAllGraphics_Patch
{
    private static void Postfix(ref PawnGraphicSet __instance)
    {
        CompPokemon compPokemon = __instance.pawn.TryGetComp<CompPokemon>();
        if (compPokemon != null)
        {
            var flag = false;
            var texPath = "";
            if (compPokemon.formTracker != null)
            {
                texPath += compPokemon.formTracker.GetCurrentFormKey();
                flag = true;
            }

            if (compPokemon.shinyTracker != null && compPokemon.shinyTracker.isShiny)
            {
                texPath += "Shiny";
                flag = true;
            }

            if (flag)
            {
                var graphicData = new GraphicData();
                graphicData.CopyFrom(__instance.nakedGraphic.data);
                graphicData.texPath += texPath;
                __instance.nakedGraphic = graphicData.Graphic;
            }
        }
    }
}
