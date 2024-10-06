using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PokeWorld;

public class ScenPart_StartingPokemon : ScenPart
{
    private static readonly List<Pair<int, float>> PetCountChances = new()
    {
        new Pair<int, float>(1, 20f),
        new Pair<int, float>(2, 10f),
        new Pair<int, float>(3, 5f),
        new Pair<int, float>(4, 3f),
        new Pair<int, float>(5, 1f),
        new Pair<int, float>(6, 1f),
        new Pair<int, float>(7, 1f),
        new Pair<int, float>(8, 1f),
        new Pair<int, float>(9, 1f),
        new Pair<int, float>(10, 0.1f),
        new Pair<int, float>(11, 0.1f),
        new Pair<int, float>(12, 0.1f),
        new Pair<int, float>(13, 0.1f),
        new Pair<int, float>(14, 0.1f)
    };

    private PawnKindDef animalKind;

    private float bondToRandomPlayerPawnChance = 0.5f;

    private int count = 1;

    private string countBuf;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref animalKind, "animalKind");
        Scribe_Values.Look(ref count, "count");
        Scribe_Values.Look(ref bondToRandomPlayerPawnChance, "bondToRandomPlayerPawnChance");
    }

    public override void DoEditInterface(Listing_ScenEdit listing)
    {
        var scenPartRect = listing.GetScenPartRect(this, RowHeight * 2f);
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(scenPartRect.TopHalf());
        listing_Standard.ColumnWidth = scenPartRect.width;
        listing_Standard.TextFieldNumeric(ref count, ref countBuf, 1f);
        listing_Standard.End();
        if (!Widgets.ButtonText(scenPartRect.BottomHalf(), CurrentAnimalLabel().CapitalizeFirst())) return;
        var list = new List<FloatMenuOption>
        {
            new("RandomPet".Translate().CapitalizeFirst(), delegate { animalKind = null; })
        };
        foreach (var item in PossibleAnimals(false))
        {
            var localKind = item;
            list.Add(new FloatMenuOption(localKind.LabelCap, delegate { animalKind = localKind; }));
        }

        Find.WindowStack.Add(new FloatMenu(list));
    }

    private IEnumerable<PawnKindDef> PossibleAnimals(bool checkForTamer = true)
    {
        if (PokeWorldSettings.MinSelected())
            return DefDatabase<PawnKindDef>.AllDefs.Where(
                td => td.RaceProps.Animal && !td.race.HasComp(typeof(CompPokemon)) &&
                      (!checkForTamer || CanKeepPetTame(td))
            );
        return DefDatabase<PawnKindDef>.AllDefs.Where(
            td => td.RaceProps.Animal && td.race.HasComp(typeof(CompPokemon)) &&
                  PokeWorldSettings.GenerationAllowed(td.race.GetCompProperties<CompProperties_Pokemon>().generation) &&
                  td.race.GetCompProperties<CompProperties_Pokemon>().starter && (!checkForTamer || CanKeepPetTame(td))
        );
    }

    private static bool CanKeepPetTame(PawnKindDef def)
    {
        var level = Find.GameInitData.startingAndOptionalPawns.Take(Find.GameInitData.startingPawnCount)
            .MaxBy(c => c.skills.GetSkill(SkillDefOf.Animals).Level).skills.GetSkill(SkillDefOf.Animals).Level;
        var statValueAbstract = def.race.GetStatValueAbstract(StatDefOf.MinimumHandlingSkill);
        return level >= statValueAbstract;
    }

    private IEnumerable<PawnKindDef> RandomPets()
    {
        return from td in PossibleAnimals()
            where td.RaceProps.petness > 0f
            select td;
    }

    private string CurrentAnimalLabel()
    {
        if (animalKind == null) return "RandomPet".TranslateSimple();
        return animalKind.label;
    }

    public override string Summary(Scenario scen)
    {
        return ScenSummaryList.SummaryWithList(
            scen, "PlayerStartsWith", ScenPart_StartingThing_Defined.PlayerStartWithIntro
        );
    }

    public override IEnumerable<string> GetSummaryListEntries(string tag)
    {
        if (tag == "PlayerStartsWith") yield return CurrentAnimalLabel().CapitalizeFirst() + " x" + count;
    }

    public override void Randomize()
    {
        if (Rand.Value < 0.5f)
            animalKind = null;
        else
            animalKind = PossibleAnimals(false).RandomElement();
        count = PetCountChances.RandomElementByWeight(pa => pa.Second).First;
        bondToRandomPlayerPawnChance = 0f;
    }

    public override bool TryMerge(ScenPart other)
    {
        if (other is ScenPart_StartingPokemon scenPart_StartingPokemon &&
            scenPart_StartingPokemon.animalKind == animalKind)
        {
            count += scenPart_StartingPokemon.count;
            return true;
        }

        return false;
    }

    public override IEnumerable<Thing> PlayerStartingThings()
    {
        for (var i = 0; i < count; i++)
        {
            var kindDef = animalKind ?? RandomPets().RandomElementByWeight(td => td.RaceProps.petness);
            var animal = PawnGenerator.GeneratePawn(kindDef, Faction.OfPlayer);
            Find.World.GetComponent<PokedexManager>().AddPokemonKindCaught(animal.kindDef);
            if (animal.Name == null || animal.Name.Numerical)
                animal.Name = PawnBioAndNameGenerator.GeneratePawnName(animal);
            if (Rand.Value < bondToRandomPlayerPawnChance &&
                animal.training.CanAssignToTrain(TrainableDefOf.Obedience).Accepted)
            {
                var pawn = (from p in Find.GameInitData.startingAndOptionalPawns.Take(
                        Find.GameInitData.startingPawnCount
                    )
                    where TrainableUtility.CanBeMaster(p, animal, false) &&
                          !p.story.traits.HasTrait(TraitDefOf.Psychopath)
                    select p).RandomElementWithFallback();
                if (pawn != null)
                {
                    animal.training.Train(TrainableDefOf.Obedience, null, true);
                    animal.training.SetWantedRecursive(TrainableDefOf.Obedience, true);
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Bond, animal);
                    animal.playerSettings.Master = pawn;
                }
            }

            yield return animal;
        }
    }
}
