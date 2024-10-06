using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PokeWorld;

[HarmonyPatch(typeof(FloatMenuMakerMap))]
[HarmonyPatch(nameof(FloatMenuMakerMap.ChoicesAtFor))]
internal class FloatMenuMakerMap_ChoicesAtFor_Patch
{
    //private static readonly FloatMenuOption[] equivalenceGroupTempStorage = null;

    public static bool Prefix(Vector3 __0, Pawn __1, ref List<FloatMenuOption> __result)
    {
        var comp = __1.TryGetComp<CompPokemon>();
        if (comp != null)
        {
            var list = new List<FloatMenuOption>();
            if (__1.MentalStateDef == null && __1.playerSettings != null && __1.playerSettings.Master != null &&
                __1.playerSettings.Master.Drafted)
            {
                var intVec = IntVec3.FromVector3(__0);
                if (!intVec.InBounds(__1.Map)) return false;
                if (__1.Map != Find.CurrentMap) return false;
                if (intVec.Fogged(__1.Map))
                {
                    var floatMenuOption = GotoLocationOption(intVec, __1);
                    if (floatMenuOption != null)
                    {
                        if (!floatMenuOption.Disabled)
                        {
                            list.Add(floatMenuOption);
                            __result = list;
                            return false;
                        }

                        __result = list;
                        return false;
                    }

                    __result = list;
                    return false;
                }

                AddDraftedOrders(__0, __1, list);
                foreach (var item in __1.GetExtraFloatMenuOptionsFor(intVec)) list.Add(item);
                __result = list;
                return false;
            }

            __result = list;
            return false;
        }

        return true;
    }

    private static FloatMenuOption GotoLocationOption(IntVec3 clickCell, Pawn pawn)
    {
        var num = GenRadial.NumCellsInRadius(2.9f);
        IntVec3 curLoc;
        for (var i = 0; i < num; i++)
        {
            curLoc = GenRadial.RadialPattern[i] + clickCell;
            if (!curLoc.Standable(pawn.Map)) continue;
            if (curLoc != pawn.Position)
            {
                if (!PokemonMasterUtility.IsPokemonMasterDrafted(pawn))
                    return new FloatMenuOption("PW_CannotGoNoMaster".Translate(), null);
                if (!pawn.CanReach(curLoc, PathEndMode.OnCell, Danger.Deadly))
                    return new FloatMenuOption("CannotGoNoPath".Translate(), null);
                if (clickCell.DistanceTo(pawn.playerSettings.Master.Position) >
                    PokemonMasterUtility.GetMasterObedienceRadius(pawn))
                    return new FloatMenuOption("PW_CannotGoTooFarFromMaster".Translate(), null);

                void action()
                {
                    var intVec = PokemonRCellFinder.BestOrderedGotoDestNear(curLoc, pawn);
                    var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("PW_PokemonGotoForced"), intVec);
                    job.playerForced = true;
                    if (pawn.Map.exitMapGrid.IsExitCell(UI.MouseCell()))
                    {
                        job.exitMapOnArrival = true;
                    }
                    else if (!pawn.Map.IsPlayerHome && !pawn.Map.exitMapGrid.MapUsesExitGrid &&
                             CellRect.WholeMap(pawn.Map).IsOnEdge(UI.MouseCell(), 3) &&
                             pawn.Map.Parent.GetComponent<FormCaravanComp>() != null &&
                             MessagesRepeatAvoider.MessageShowAllowed(
                                 "MessagePlayerTriedToLeaveMapViaExitGrid-" + pawn.Map.uniqueID, 60f
                             ))
                    {
                        if (pawn.Map.Parent.GetComponent<FormCaravanComp>().CanFormOrReformCaravanNow)
                            Messages.Message(
                                "MessagePlayerTriedToLeaveMapViaExitGrid_CanReform".Translate(), pawn.Map.Parent,
                                MessageTypeDefOf.RejectInput, false
                            );
                        else
                            Messages.Message(
                                "MessagePlayerTriedToLeaveMapViaExitGrid_CantReform".Translate(), pawn.Map.Parent,
                                MessageTypeDefOf.RejectInput, false
                            );
                    }

                    if (pawn.jobs.TryTakeOrderedJob(job)) FleckMaker.Static(intVec, pawn.Map, FleckDefOf.FeedbackGoto);
                }

                return new FloatMenuOption("GoHere".Translate(), action, MenuOptionPriority.GoHere)
                {
                    autoTakeable = true,
                    autoTakeablePriority = 10f
                };
            }

            return null;
        }

        return null;
    }

    private static void AddDraftedOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
    {
        var clickCell = IntVec3.FromVector3(clickPos);
        foreach (var item in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackHostile(), true))
        {
            var attackTarg = item;
            var comp = pawn.TryGetComp<CompPokemon>();
            if (comp != null && comp.moveTracker != null && PokemonAttackGizmoUtility.CanUseAnyRangedVerb(pawn))
            {
                var rangedAct = PokemonFloatMenuUtility.GetRangedAttackAction(pawn, attackTarg, out var failStr);
                string text = "FireAt".Translate(attackTarg.Thing.Label, attackTarg.Thing);
                var floatMenuOption = new FloatMenuOption("", null, MenuOptionPriority.High, null, item.Thing);
                if (rangedAct == null)
                {
                    text = text + ": " + failStr;
                }
                else if (PokemonMasterUtility.IsPokemonMasterDrafted(pawn))
                {
                    text = "PW_CannotGoNoMaster".Translate();
                }
                else if (clickCell.DistanceTo(pawn.playerSettings.Master.Position) >
                         PokemonMasterUtility.GetMasterObedienceRadius(pawn))
                {
                    text = "PW_CannotGoTooFarFromMaster".Translate();
                }
                else
                {
                    floatMenuOption.autoTakeable = !attackTarg.HasThing || attackTarg.Thing.HostileTo(Faction.OfPlayer);
                    floatMenuOption.autoTakeablePriority = 40f;
                    floatMenuOption.action = delegate
                    {
                        FleckMaker.Static(attackTarg.Thing.DrawPos, attackTarg.Thing.Map, FleckDefOf.FeedbackShoot);
                        rangedAct();
                    };
                }

                floatMenuOption.Label = text;
                opts.Add(floatMenuOption);
            }

            var meleeAct = PokemonFloatMenuUtility.GetMeleeAttackAction(pawn, attackTarg, out var failStr2);
            string text2 = !(attackTarg.Thing is Pawn pawn2) || !pawn2.Downed
                ? "MeleeAttack".Translate(attackTarg.Thing.Label, attackTarg.Thing)
                : (string)"MeleeAttackToDeath".Translate(attackTarg.Thing.Label, attackTarg.Thing);
            var priority = !attackTarg.HasThing || !pawn.HostileTo(attackTarg.Thing)
                ? MenuOptionPriority.VeryLow
                : MenuOptionPriority.AttackEnemy;
            var floatMenuOption2 = new FloatMenuOption("", null, priority, null, attackTarg.Thing);
            if (meleeAct == null)
            {
                text2 = text2 + ": " + failStr2.CapitalizeFirst();
            }
            else if (pawn.playerSettings == null || pawn.playerSettings.Master == null ||
                     !pawn.playerSettings.Master.Spawned || pawn.playerSettings.Master.Map != pawn.Map)
            {
                text2 = "PW_CannotAttackNoMaster".Translate();
            }
            else if (clickCell.DistanceTo(pawn.playerSettings.Master.Position) >
                     PokemonMasterUtility.GetMasterObedienceRadius(pawn))
            {
                text2 = "PW_CannotAttackTooFarFromMaster".Translate();
            }
            else
            {
                floatMenuOption2.autoTakeable = !attackTarg.HasThing || attackTarg.Thing.HostileTo(Faction.OfPlayer);
                floatMenuOption2.autoTakeablePriority = 30f;
                floatMenuOption2.action = delegate
                {
                    FleckMaker.Static(attackTarg.Thing.DrawPos, attackTarg.Thing.Map, FleckDefOf.FeedbackMelee);
                    meleeAct();
                };
            }

            floatMenuOption2.Label = text2;
            opts.Add(floatMenuOption2);
        }

        var floatMenuOption3 = GotoLocationOption(clickCell, pawn);
        if (floatMenuOption3 != null) opts.Add(floatMenuOption3);
    }
}
