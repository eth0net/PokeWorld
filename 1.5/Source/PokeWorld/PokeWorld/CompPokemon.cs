using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PokeWorld
{
    public class CompPokemon : ThingComp
    {

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            PokemonComponentsUtility.CreateInitialComponents(this);
            statTracker.UpdateStats();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!Pokemon.Position.Fogged(Pokemon.Map))
            {
                PokedexManager pokedex = Find.World.GetComponent<PokedexManager>();
                if (!pokedex.IsPokemonSeen(Pokemon.kindDef))
                {
                    pokedex.AddPokemonKindSeen(Pokemon.kindDef);
                }
            }
            statTracker?.UpdateStats();
        }

        public override void CompTick()
        {
            base.CompTick();
            levelTracker?.LevelTick();
            friendshipTracker?.FriendshipTick();
            formTracker?.FormTick();
            if (tryCatchCooldown > 0)
            {
                tryCatchCooldown -= 1;
            }
            if (inBall == true && Pokemon.Spawned)
            {
                inBall = false;
            }
            if (flagIsPokedexSeen == null)
            {
                flagIsPokedexSeen = Find.World.GetComponent<PokedexManager>().IsPokemonSeen(Pokemon.kindDef);
            }
            else if (!(bool)flagIsPokedexSeen && Pokemon.Spawned && !Pokemon.Position.Fogged(Pokemon.Map))
            {
                Find.World.GetComponent<PokedexManager>().AddPokemonKindSeen(Pokemon.kindDef);
                flagIsPokedexSeen = true;
            }
            if (flagIsPokedexCaught == null)
            {
                flagIsPokedexCaught = Find.World.GetComponent<PokedexManager>().IsPokemonCaught(Pokemon.kindDef);
            }
            else if (!(bool)flagIsPokedexCaught && Pokemon.Faction == Faction.OfPlayer)
            {
                Find.World.GetComponent<PokedexManager>().AddPokemonKindCaught(Pokemon.kindDef);
                flagIsPokedexCaught = true;
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            shinyTracker?.ShinyTickRare();
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            if (parent.Faction != null && parent.Faction == Faction.OfPlayer && statTracker.nature != null)
            {
                NatureDef nature = statTracker.nature;
                string natureLabel = nature.label;
                string natureDescription = "PW_StatNatureDesc".Translate() + "\n\n";
                if (nature.increasedStat != nature.decreasedStat)
                {
                    natureDescription += "PW_StatNatureChangeDesc".Translate(nature.increasedStat.label, nature.decreasedStat.label);
                }
                else
                {
                    natureDescription += "PW_StatNatureNoChangeDesc".Translate();
                }
                yield return new StatDrawEntry(DefDatabase<StatCategoryDef>.GetNamed("PW_PokeWorldStat"), "PW_Nature".Translate(), natureLabel, natureDescription, 4050);
            }

            string eggGroupValue = "";
            string eggGroupDescription = "PW_StatEggGroupDesc".Translate() + "\n";
            string typeValue = "";
            string typeDescription = "PW_StatTypeDesc".Translate() + "\n";
            string evolutionValue = "";
            string evolutionDescription = "PW_StatEvolutionDesc".Translate() + "\n";
            if (Find.World.GetComponent<PokedexManager>().IsPokemonCaught(parent.def.race.AnyPawnKind))
            {
                foreach (EggGroupDef eggGroupDef in EggGroups)
                {
                    if (eggGroupValue.Length > 0)
                    {
                        eggGroupValue += ", ";
                    }
                    eggGroupValue += eggGroupDef.LabelCap;
                    eggGroupDescription += "\n" + "PW_EggGroupLabelAndDesc".Translate(eggGroupDef.LabelCap, eggGroupDef.description);
                }
                for (int x = 0; x < Types.Count(); x++)
                {
                    if (typeValue.Length > 0)
                    {
                        typeValue += ", ";
                    }
                    string text;
                    if (x == 0)
                    {
                        text = "PW_TypePrimary".Translate();
                    }
                    else if (x == 1)
                    {
                        text = "PW_TypeSecondary".Translate();
                    }
                    else
                    {
                        text = "PW_Type".Translate();
                    }
                    typeValue += Types[x].LabelCap;
                    typeDescription += "\n" + "PW_TypeName".Translate(text, Types[x].LabelCap);
                }
                if (CanEvolve)
                {
                    foreach (Evolution evolution in Evolutions)
                    {
                        if (evolutionValue.Length > 0)
                        {
                            evolutionValue += ", ";
                        }
                        string evolutionLabel;
                        if (Find.World.GetComponent<PokedexManager>().IsPokemonCaught(evolution.pawnKind))
                        {
                            evolutionLabel = evolution.pawnKind.LabelCap;
                        }
                        else
                        {
                            evolutionLabel = "PW_StatUnknownEvolution".Translate();
                        }
                        evolutionValue += evolutionLabel;
                        if (evolution.requirement == EvolutionRequirement.level)
                        {
                            string text2;
                            if (evolution.friendship > 0)
                            {
                                switch (evolution.gender)
                                {
                                    case Gender.Male:
                                        text2 = "PW_HappyMalePokemon".Translate(Pokemon.KindLabel).CapitalizeFirst();
                                        break;
                                    case Gender.Female:
                                        text2 = "PW_HappyFemalePokemon".Translate(Pokemon.KindLabel).CapitalizeFirst();
                                        break;
                                    default:
                                        text2 = "PW_HappyPokemon".Translate(Pokemon.KindLabel).CapitalizeFirst();
                                        break;
                                }
                            }
                            else
                            {
                                switch (evolution.gender)
                                {
                                    case Gender.Male:
                                        text2 = "PW_MalePokemon".Translate(Pokemon.KindLabel).CapitalizeFirst();
                                        break;
                                    case Gender.Female:
                                        text2 = "PW_FemalePokemon".Translate(Pokemon.KindLabel).CapitalizeFirst();
                                        break;
                                    default:
                                        text2 = Pokemon.KindLabel;
                                        break;
                                }
                            }
                            if (evolution.level > 1)
                            {
                                switch (evolution.timeOfDay)
                                {
                                    case TimeOfDay.Day:
                                        evolutionDescription += "\n" + "PW_StatEvolutionNeedFixedLevelDay".Translate(text2, evolution.level, evolutionLabel);
                                        break;
                                    case TimeOfDay.Night:
                                        evolutionDescription += "\n" + "PW_StatEvolutionNeedFixedLevelNight".Translate(text2, evolution.level, evolutionLabel);
                                        break;
                                    default:
                                        evolutionDescription += "\n" + "PW_StatEvolutionNeedFixedLevel".Translate(text2, evolution.level, evolutionLabel);
                                        break;
                                }
                            }
                            else
                            {
                                switch (evolution.timeOfDay)
                                {
                                    case TimeOfDay.Day:
                                        evolutionDescription += "\n" + "PW_StatEvolutionNeedLevelUpDay".Translate(text2, evolutionLabel);
                                        break;
                                    case TimeOfDay.Night:
                                        evolutionDescription += "\n" + "PW_StatEvolutionNeedLevelUpNight".Translate(text2, evolutionLabel);
                                        break;
                                    default:
                                        evolutionDescription += "\n" + "PW_StatEvolutionNeedLevelUp".Translate(text2, evolutionLabel);
                                        break;
                                }
                            }
                        }
                        else if (evolution.requirement == EvolutionRequirement.item)
                        {
                            string itemLabel;
                            if (Find.World.GetComponent<PokedexManager>().IsPokemonCaught(evolution.pawnKind))
                            {
                                itemLabel = evolution.item.LabelCap;
                            }
                            else
                            {
                                itemLabel = "PW_CertainItem".Translate();
                            }
                            evolutionDescription += "\n" + "PW_StatEvolutionNeedItem".Translate(Pokemon.KindLabel, itemLabel, evolutionLabel);
                        }
                    }
                }
                else
                {
                    evolutionValue = "PW_StatNoEvolution".Translate();
                    evolutionDescription += "\n" + "PW_CannotEvolve".Translate();
                }
            }
            else
            {
                eggGroupValue = "PW_StatUnknownEggGroup".Translate();
                typeValue = "PW_StatUnknownType".Translate();
                evolutionValue = "PW_StatUnknownEvolution".Translate();
                eggGroupDescription += "\n" + "PW_StatCatchPokemonToLearnMore".Translate();
                typeDescription += "\n" + "PW_StatCatchPokemonToLearnMore".Translate();
                evolutionDescription += "\n" + "PW_StatCatchPokemonToLearnMore".Translate();
            }
            yield return new StatDrawEntry(DefDatabase<StatCategoryDef>.GetNamed("PW_PokeWorldStat"), "PW_EggGroup".Translate(), eggGroupValue, eggGroupDescription, 4049);
            yield return new StatDrawEntry(DefDatabase<StatCategoryDef>.GetNamed("PW_PokeWorldStat"), "PW_Type".Translate(), typeValue, typeDescription, 4054);
            yield return new StatDrawEntry(DefDatabase<StatCategoryDef>.GetNamed("PW_PokeWorldStat"), "PW_Evolution".Translate(), evolutionValue, evolutionDescription, 4050);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (Pokemon.Faction == Faction.OfPlayer)
            {
                if (Pokemon.Map.designationManager.DesignationOn(Pokemon, DefDatabase<DesignationDef>.GetNamed("PW_PutInBall")) == null)
                {
                    Command_Action command_PutInBall = new Command_Action
                    {
                        defaultLabel = "PW_DesignationPutInPokeball".Translate(),
                        defaultDesc = "PW_DesignationPutInPokeballDesc".Translate(),
                        hotKey = KeyBindingDefOf.Misc2,
                        icon = ContentFinder<Texture2D>.Get("Things/Item/Utility/Balls/PokeBall"),
                        action = delegate ()
                        {
                            PutInBallUtility.UpdatePutInBallDesignation(Pokemon);
                        }
                    };
                    yield return command_PutInBall;
                }
            }
            if (levelTracker != null)
            {
                foreach (Gizmo item in levelTracker.GetGizmos())
                {
                    yield return item;
                }
            }
            if (formTracker != null)
            {
                foreach (Gizmo item in formTracker.GetGizmos())
                {
                    yield return item;
                }
            }
            if (moveTracker != null)
            {
                foreach (Gizmo item in moveTracker.GetGizmos())
                {
                    yield return item;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            PokemonComponentsUtility.ExposeData(this);
            Scribe_Values.Look(ref wantPutInBall, "wantPutInBall", defaultValue: false);
            Scribe_Values.Look(ref inBall, "inBall", defaultValue: false);
            Scribe_Defs.Look(ref ballDef, "ballDef");
            Scribe_Values.Look(ref tryCatchCooldown, "tryCatchCooldown", defaultValue: 0);
            Scribe_Values.Look(ref flagIsPokedexSeen, "flagIsPokedexSeen", defaultValue: false);
            Scribe_Values.Look(ref flagIsPokedexCaught, "flagIsPokedexCaught", defaultValue: false);
            Scribe_Values.Look(ref tryCatchKillChanceIfDown, "tryCatchKillChanceIfDown", defaultValue: 0);
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            if (PokemonMasterUtility.IsPokemonMasterDrafted(Pokemon))
            {
                PokemonMasterUtility.DrawObedienceRadiusRingAroundMaster(Pokemon);
                Pokemon.pather.curPath?.DrawPath(Pokemon);
                Pokemon.jobs.DrawLinesBetweenTargets();
            }
        }
        public override string CompInspectStringExtra()
        {
            string text;
            if (Pokemon.Faction != null && Pokemon.Faction.IsPlayer)
            {
                text = "PW_InspectLevelExperiencePlayerPokemon".Translate(levelTracker.level, levelTracker.experience, levelTracker.totalExpForNextLevel);
                text += '\n' + friendshipTracker.GetStatement();
            }
            else
            {
                text = "PW_InspectLevelExperienceNonPlayerPokemon".Translate(levelTracker.level);
            }
            return text;
        }
        public override void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
        {
            base.PrePreTraded(action, playerNegotiator, trader);
            if (levelTracker.flagIsEvolving)
            {
                levelTracker.CancelEvolution();
            }
            if (action == TradeAction.PlayerBuys)
            {
                Pokemon.SetFaction(Faction.OfPlayer, playerNegotiator);
                Pokemon.training.Train(DefDatabase<TrainableDef>.GetNamed("Obedience"), playerNegotiator, true);
                Pokemon.training.SetWantedRecursive(DefDatabase<TrainableDef>.GetNamed("Obedience"), true);
                Find.World.GetComponent<PokedexManager>().AddPokemonKindCaught(Pokemon.kindDef);
            }
            else if (action == TradeAction.PlayerSells)
            {
                Pokemon.SetFaction(trader.Faction);
            }
        }

        public CompProperties_Pokemon Props => (CompProperties_Pokemon)this.props;
        public int PokedexNumber => Props.pokedexNumber;
        public int Generation => Props.generation;
        public List<TypeDef> Types => Props.types;
        public bool Starter => Props.starter;
        public int Rarity => Props.rarity;
        public bool CanEvolve => Props.canEvolve;
        public int EvolutionLine => Props.evolutionLine;
        public List<Evolution> Evolutions => Props.evolutions;
        public List<Move> Moves => Props.moves;
        public List<PokemonAttribute> Attributes => Props.attributes;
        public string ExpCategory => Props.expCategory;
        public int WildLevelMin => Props.wildLevelMin;
        public int WildLevelMax => Props.wildLevelMax;
        public List<EggGroupDef> EggGroups => Props.eggGroups;
        public int BaseHP => Props.baseHP;
        public int BaseAttack => Props.baseAttack;
        public int BaseDefense => Props.baseDefense;
        public int BaseSpAttack => Props.baseSpAttack;
        public int BaseSpDefense => Props.baseSpDefense;
        public int BaseSpeed => Props.baseSpeed;
        public int BaseFriendship => Props.baseFriendship;
        public List<EVYield> EVYields => Props.EVYields;
        public int CatchRate => Props.catchRate;
        public float FemaleRatio => Props.femaleRatio;
        public float ShinyChance => Props.shinyChance;
        public FormChangerCondition FormChangerCondition => Props.formChangerCondition;
        public bool ShowFormLabel => Props.showFormLabel;
        public List<PokemonForm> Forms => Props.forms;

        public ShinyTracker shinyTracker;
        public LevelTracker levelTracker;
        public FriendshipTracker friendshipTracker;
        public StatTracker statTracker;
        public MoveTracker moveTracker;
        public FormTracker formTracker;

        public Pawn Pokemon => (Pawn)parent;
        public bool wantPutInBall = false;
        public bool inBall;
        public ThingDef ballDef = DefDatabase<ThingDef>.GetNamed("PW_CryptosleepPokeBall");
        public bool? flagIsPokedexSeen;
        public bool? flagIsPokedexCaught;

        public int tryCatchCooldown = 0;
        public float tryCatchKillChanceIfDown = 0;
    }

    public class CompProperties_Pokemon : CompProperties
    {
        public int pokedexNumber;
        public int generation;
        public List<TypeDef> types;
        public bool starter;
        public int rarity;
        public bool canEvolve;
        public int evolutionLine;
        public List<Evolution> evolutions;
        public List<Move> moves;
        public List<PokemonAttribute> attributes = new List<PokemonAttribute>();
        public string expCategory;
        public int wildLevelMin;
        public int wildLevelMax;
        public List<EggGroupDef> eggGroups;
        public int baseFriendship;
        public float femaleRatio;
        public float shinyChance;
        public FormChangerCondition formChangerCondition;
        public bool showFormLabel = false;
        public List<PokemonForm> forms;
        public int baseHP;
        public int baseAttack;
        public int baseDefense;
        public int baseSpAttack;
        public int baseSpDefense;
        public int baseSpeed;
        public List<EVYield> EVYields;
        public int catchRate;

        public CompProperties_Pokemon()
        {
            this.compClass = typeof(CompPokemon);
        }

        public CompProperties_Pokemon(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}
