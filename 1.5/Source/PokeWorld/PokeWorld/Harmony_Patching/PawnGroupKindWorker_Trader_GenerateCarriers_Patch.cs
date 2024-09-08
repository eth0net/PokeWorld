using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(PawnGroupKindWorker_Trader))]
[HarmonyPatch("GenerateCarriers")]
internal class PawnGroupKindWorker_Trader_GenerateCarriers_Patch
{
    public static bool Prefix(PawnGroupMakerParms __0, PawnGroupMaker __1, Pawn __2, List<Thing> __3, List<Pawn> __4)
    {
        PawnKindDef kind = null;
        IEnumerable<PawnGenOption> kinds = null;
        if (!PokeWorldSettings.OkforPokemon() || PokeWorldSettings.allowNPCPokemonPack == false)
            kind = __1.carriers
                .Where(
                    x => __0.tile == -1 || (Find.WorldGrid[__0.tile].biome.IsPackAnimalAllowed(x.kind.race) &&
                                            !x.kind.race.HasComp(typeof(CompPokemon)))
                ).RandomElementByWeight(x => x.selectionWeight).kind;
        else
            kinds = __1.carriers.Where(
                x => __0.tile == -1 || (Find.WorldGrid[__0.tile].biome.IsPackAnimalAllowed(x.kind.race) &&
                                        x.kind.race.HasComp(typeof(CompPokemon)) && PokeWorldSettings.GenerationAllowed(
                                            x.kind.race.GetCompProperties<CompProperties_Pokemon>().generation
                                        ))
            );
        var list = __3.Where(x => !(x is Pawn)).ToList();
        var i = 0;
        var num = Mathf.CeilToInt(list.Count / 8f);
        var list2 = new List<Pawn>();
        for (var j = 0; j < num; j++)
        {
            Pawn pawn;
            if (kind != null)
                pawn = PawnGenerator.GeneratePawn(
                    new PawnGenerationRequest(
                        kind, __0.faction, 
                        PawnGenerationContext.NonPlayer, __0.tile,
                        allowFood: true, allowAddictions: true, inhabitant: __0.inhabitants
                    )
                );
            else
                pawn = PawnGenerator.GeneratePawn(
                    new PawnGenerationRequest(
                        kinds.RandomElementByWeight(x => x.selectionWeight).kind, __0.faction,
                        PawnGenerationContext.NonPlayer, __0.tile,
                        allowFood: true, allowAddictions: true, inhabitant: __0.inhabitants
                    )
                );
            if (i < list.Count)
            {
                pawn.inventory.innerContainer.TryAdd(list[i]);
                i++;
            }

            list2.Add(pawn);
            __4.Add(pawn);
        }

        for (; i < list.Count; i++) list2.RandomElement().inventory.innerContainer.TryAdd(list[i]);
        return false;
    }
}
