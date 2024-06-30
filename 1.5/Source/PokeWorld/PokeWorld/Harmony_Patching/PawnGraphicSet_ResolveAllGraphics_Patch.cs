using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace PokeWorld.Harmony_Patching;

[HarmonyPatch(typeof(PawnRenderNode_AnimalPart))]
[HarmonyPatch(nameof(PawnRenderNode_AnimalPart.GraphicFor))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class PawnRenderNode_AnimalPart_GraphicFor_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var index = codes.FindIndex(code => code.Calls(AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Dead))));

        codes.InsertRange(index, new List<CodeInstruction>
        {
            new(OpCodes.Ldloca_S, 1),
            CodeInstruction.Call(typeof(PawnRenderNode_AnimalPart_GraphicFor_Patch), nameof(TryApplyShinyForPawn)),
            new(OpCodes.Ldarg_1)
        });

        return codes.AsEnumerable();
    }

    private static void TryApplyShinyForPawn(Pawn pawn, ref Graphic graphic)
    {
        var texPath = "";
        var compPokemon = pawn.TryGetComp<CompPokemon>();

        if (compPokemon.formTracker != null) texPath += compPokemon.formTracker.GetCurrentFormKey();
        if (compPokemon.shinyTracker is { isShiny: true }) texPath += "Shiny";

        if (texPath == "") return;

        var graphicData = new GraphicData();
        graphicData.CopyFrom(graphic.data);
        graphicData.texPath += "Shiny";
        graphic = graphicData.Graphic;
    }
}
