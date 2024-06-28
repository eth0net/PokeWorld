using System.Linq;
using LudeonTK;
using RimWorld;
using Verse;

namespace PokeWorld
{
    public static class DebugToolsPokemon
    {
        [DebugAction("Pokeworld", "+1 level", actionType = DebugActionType.ToolMap,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Add1Level()
        {
            foreach (var item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
                if (item is Pawn pawn)
                {
                    var comp = pawn.TryGetComp<CompPokemon>();
                    comp?.levelTracker.IncreaseExperience(comp.levelTracker.totalExpForNextLevel -
                                                          comp.levelTracker.experience);
                }
        }

        [DebugAction("Pokeworld", "Max level", actionType = DebugActionType.ToolMap,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void MakeMaxLevel()
        {
            foreach (var item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
                if (item is Pawn pawn)
                {
                    var comp = pawn.TryGetComp<CompPokemon>();
                    comp?.levelTracker.IncreaseExperience(2000000);
                }
        }

        [DebugAction("Pokeworld", "+ 20.000 Xp", actionType = DebugActionType.ToolMap,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Give20000Xp()
        {
            foreach (var item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
                if (item is Pawn pawn)
                {
                    var comp = pawn.TryGetComp<CompPokemon>();
                    comp?.levelTracker.IncreaseExperience(20000);
                }
        }

        [DebugAction("Pokeworld", "Make Shiny", actionType = DebugActionType.ToolMap,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void MakeShiny()
        {
            foreach (var item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
                if (item is Pawn pawn)
                {
                    var comp = pawn.TryGetComp<CompPokemon>();
                    if (comp != null && comp.shinyTracker != null)
                    {
                        comp.shinyTracker.MakeShiny();
                        pawn.Drawer.renderer.SetAllGraphicsDirty();
                    }
                }
        }

        [DebugAction("Pokeworld", "+ 20 friendship", actionType = DebugActionType.ToolMap,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Give20Friendship()
        {
            foreach (var item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
                if (item is Pawn pawn)
                {
                    var comp = pawn.TryGetComp<CompPokemon>();
                    if (comp != null && comp.friendshipTracker != null) comp.friendshipTracker.ChangeFriendship(20);
                }
        }

        [DebugAction("Pokeworld", "- 20 friendship", actionType = DebugActionType.ToolMap,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Remove20Friendship()
        {
            foreach (var item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
                if (item is Pawn pawn)
                {
                    var comp = pawn.TryGetComp<CompPokemon>();
                    if (comp != null && comp.friendshipTracker != null) comp.friendshipTracker.ChangeFriendship(-20);
                }
        }

        [DebugAction("Pokeworld", "Hatch Pokémon Egg", actionType = DebugActionType.ToolMap,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void HatchEgg()
        {
            foreach (var item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
                if (item is ThingWithComps thingWithComps && thingWithComps.TryGetComp<CompPokemonEggHatcher>() != null)
                {
                    var comp = thingWithComps.TryGetComp<CompPokemonEggHatcher>();
                    comp.HatchPokemon();
                }
        }

        [DebugAction("Pokeworld", "Fill Pokédex", actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.Playing)]
        public static void FillPokedex()
        {
            Find.World.GetComponent<PokedexManager>().DebugFillPokedex();
        }

        [DebugAction("Pokeworld", "Fill Pokédex no legendary", actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.Playing)]
        public static void FillPokedexNoLegendary()
        {
            Find.World.GetComponent<PokedexManager>().DebugFillPokedexNoLegendary();
        }

        [DebugAction("Pokeworld", "Show hidden stats", actionType = DebugActionType.ToolMap,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ShowHiddenStat()
        {
            foreach (var item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
                if (item is Pawn pawn)
                {
                    var comp = pawn.TryGetComp<CompPokemon>();
                    if (comp != null && comp.statTracker != null)
                    {
                        var text = "";
                        var hpStatDef = DefDatabase<StatDef>.GetNamed("PW_HP");
                        var attackStatDef = DefDatabase<StatDef>.GetNamed("PW_Attack");
                        var defenseStatDef = DefDatabase<StatDef>.GetNamed("PW_Defense");
                        var spAttackStatDef = DefDatabase<StatDef>.GetNamed("PW_SpecialAttack");
                        var spDefenseStatDef = DefDatabase<StatDef>.GetNamed("PW_SpecialDefense");
                        var speedStatDef = DefDatabase<StatDef>.GetNamed("PW_Speed");
                        var hpIV = comp.statTracker.GetIV(hpStatDef);
                        var atIV = comp.statTracker.GetIV(attackStatDef);
                        var defIV = comp.statTracker.GetIV(defenseStatDef);
                        var spatIV = comp.statTracker.GetIV(spAttackStatDef);
                        var spdefIV = comp.statTracker.GetIV(spDefenseStatDef);
                        var spdIV = comp.statTracker.GetIV(speedStatDef);
                        var hpEV = comp.statTracker.GetEV(hpStatDef);
                        var atEV = comp.statTracker.GetEV(attackStatDef);
                        var defEV = comp.statTracker.GetEV(defenseStatDef);
                        var spatEV = comp.statTracker.GetEV(spAttackStatDef);
                        var spdefEV = comp.statTracker.GetEV(spDefenseStatDef);
                        var spdEV = comp.statTracker.GetEV(speedStatDef);
                        text +=
                            $"{pawn.ThingID} stats:\nIV: hp:{hpIV} att:{atIV} def:{defIV} spAt:{spatIV} spDef:{spdefIV} spd:{spdIV}\nEV: hp:{hpEV} att:{atEV} def:{defEV} spAt:{spatEV} spDef:{spdefEV} spd:{spdEV}";
                        Log.Message(text);
                    }
                }
        }
    }
}
