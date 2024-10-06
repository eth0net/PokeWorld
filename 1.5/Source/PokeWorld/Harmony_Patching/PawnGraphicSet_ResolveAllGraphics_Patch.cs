using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace PokeWorld;
//[HarmonyPatch(typeof(PawnGraphicSet))]
//[HarmonyPatch("ResolveAllGraphics")]
//class PawnGraphicSet_ResolveAllGraphics_Patch
//{
//    static void Postfix(ref PawnGraphicSet __instance)
//    {
//        CompPokemon compPokemon = __instance.pawn.TryGetComp<CompPokemon>();
//        if (compPokemon != null)
//        {
//            bool flag = false;
//            string texPath = "";
//            if (compPokemon.formTracker != null)
//            {
//                texPath += compPokemon.formTracker.GetCurrentFormKey();
//                flag = true;
//            }
//            if (compPokemon.shinyTracker != null && compPokemon.shinyTracker.isShiny)
//            {
//                texPath += "Shiny";
//                flag = true;
//            }
//            if (flag)
//            {
//                GraphicData graphicData = new GraphicData();
//                graphicData.CopyFrom(__instance.nakedGraphic.data);
//                graphicData.texPath += texPath;
//                __instance.nakedGraphic = graphicData.Graphic;
//            }
//        }
//    }
//}

[HarmonyPatch(typeof(PawnRenderNode_AnimalPart))]
[HarmonyPatch(nameof(PawnRenderNode_AnimalPart.GraphicFor))]
internal class PawnRenderNode_AnimalPart_GraphicFor_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var index = codes.FindIndex(code => code.Calls(AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Dead))));

        codes.InsertRange(
            index, new List<CodeInstruction>
            {
                new(OpCodes.Ldloca_S, 1),
                CodeInstruction.Call(typeof(PawnRenderNode_AnimalPart_GraphicFor_Patch), nameof(TryApplyShinyForPawn)),
                new(OpCodes.Ldarg_1)
            }
        );

        return codes.AsEnumerable();
    }

    private static void TryApplyShinyForPawn(Pawn pawn, ref Graphic graphic)
    {
        var compPokemon = pawn.TryGetComp<CompPokemon>();
        if (compPokemon != null && compPokemon.shinyTracker != null && compPokemon.shinyTracker.isShiny)
        {
            var graphicData = new GraphicData();
            graphicData.CopyFrom(graphic.data);
            graphicData.texPath += "Shiny";
            graphic = graphicData.Graphic;
        }
    }
}
