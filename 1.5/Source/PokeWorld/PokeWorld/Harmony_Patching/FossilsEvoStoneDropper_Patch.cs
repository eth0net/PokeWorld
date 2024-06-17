using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PokeWorld
{
    class FossilsEvoStoneDropper_Patch
    {
        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch(nameof(Thing.ButcherProducts))]
        public class Thing_ButcherProducts_Patch
        {
            public static void Postfix(Thing __instance, Pawn __0, ref IEnumerable<Thing> __result)
            {
                CompPokFossilsEvoStoneDropper comp = __instance.TryGetComp<CompPokFossilsEvoStoneDropper>();
                if (comp != null)
                {
                    ThingDef def = FossilEvoStoneDropperUtility.TryGetItem(comp.StoneDropRate, comp.FossilDropRate);
                    if (def != null)
                    {
                        Thing thing = ThingMaker.MakeThing(def);
                        thing.stackCount = 1;
                        List<Thing> list = __result.ToList();
                        list.Add(thing);
                        __result = list.AsEnumerable();
                        if (__0 != null && __0.Faction == Faction.OfPlayer)
                        {
                            Messages.Message("PW_FoundFossilOrEvoStone".Translate(__0.LabelShortCap, thing.Label), thing, MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Mineable))]
    [HarmonyPatch("TrySpawnYield")]
    [HarmonyPatch(new Type[] { typeof(Map), typeof(bool), typeof(Pawn) })]
    public class Mineable_TrySpawnYield_Patch
    {
        public static void Postfix(Mineable __instance, Map __0, Pawn __2)
        {
            CompPokFossilsEvoStoneDropper comp = __instance.TryGetComp<CompPokFossilsEvoStoneDropper>();
            if (comp != null)
            {
                ThingDef def = FossilEvoStoneDropperUtility.TryGetItem(comp.StoneDropRate, comp.FossilDropRate);
                if (def != null)
                {
                    Thing thing = ThingMaker.MakeThing(def);
                    thing.stackCount = 1;
                    GenPlace.TryPlaceThing(thing, __instance.Position, __0, ThingPlaceMode.Near, ForbidIfNecessary);
                    if (__2 != null && __2.Faction == Faction.OfPlayer)
                    {
                        Messages.Message("PW_FoundFossilOrEvoStone".Translate(__2.LabelShortCap, thing.Label), thing, MessageTypeDefOf.PositiveEvent);
                    }
                }
            }
            void ForbidIfNecessary(Thing thing, int count)
            {
                if ((__2 == null || !__2.IsColonist) && thing.def.EverHaulable && !thing.def.designateHaulable)
                {
                    thing.SetForbidden(value: true, warnOnFail: false);
                }
            }
        }
    }

    [HarmonyPatch(typeof(CompDeepDrill))]
    [HarmonyPatch("TryProducePortion")]
    public class CompDeepDrill_TryProducePortion_Patch
    {
        public static void Postfix(CompDeepDrill __instance)
        {
            CompPokFossilsEvoStoneDropper comp = __instance.parent.TryGetComp<CompPokFossilsEvoStoneDropper>();
            if (comp != null)
            {
                ThingDef def = FossilEvoStoneDropperUtility.TryGetItem(comp.StoneDropRate, comp.FossilDropRate);
                if (def != null)
                {
                    Thing thing = ThingMaker.MakeThing(def);
                    thing.stackCount = 1;
                    GenPlace.TryPlaceThing(thing, __instance.parent.Position, __instance.parent.Map, ThingPlaceMode.Near);
                    Building building = __instance.parent as Building;
                    Pawn user = building.InteractionCell.GetFirstPawn(building.Map);
                    if (user != null && user.Faction == Faction.OfPlayer)
                    {
                        Messages.Message("PW_FoundFossilOrEvoStone".Translate(user.LabelShortCap, thing.Label), thing, MessageTypeDefOf.PositiveEvent);
                    }
                }
            }
        }
    }

    public static class FossilEvoStoneDropperUtility
    {

        public static ThingDef TryGetItem(float stoneDropRate, float fossilDropRate)
        {
            if (Rand.Value <= stoneDropRate)
            {
                IEnumerable<ThingDef> evoStones = DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.thingCategories != null && x.thingCategories.Contains(DefDatabase<ThingCategoryDef>.GetNamed("PW_EvolutionStone")));
                return evoStones.RandomElement();
            }
            if (Rand.Value <= fossilDropRate)
            {
                List<ThingDef> fossils = DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.thingCategories != null && x.thingCategories.Contains(DefDatabase<ThingCategoryDef>.GetNamed("PW_Fossils"))).ToList();
                if (!PokeWorldSettings.allowGen1)
                {
                    fossils.Remove(DefDatabase<ThingDef>.GetNamed("PW_HelixFossil"));
                    fossils.Remove(DefDatabase<ThingDef>.GetNamed("PW_DomeFossil"));
                    fossils.Remove(DefDatabase<ThingDef>.GetNamed("PW_OldAmber"));
                }
                if (!PokeWorldSettings.allowGen3)
                {
                    fossils.Remove(DefDatabase<ThingDef>.GetNamed("PW_RootFossil"));
                    fossils.Remove(DefDatabase<ThingDef>.GetNamed("PW_ClawFossil"));
                }
                if (!PokeWorldSettings.allowGen4)
                {
                    fossils.Remove(DefDatabase<ThingDef>.GetNamed("PW_SkullFossil"));
                    fossils.Remove(DefDatabase<ThingDef>.GetNamed("PW_ArmorFossil"));
                }
                if (fossils.Any())
                {
                    return fossils.RandomElement();
                }
            }
            return null;
        }
    }
}

