using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

public class CompDittoEggLayer : ThingComp
{
    private ThingDef eggFertilizedDef;
    private float eggLayIntervalDays;
    private float eggProgress;
    private int fertilizationCount;
    private Pawn fertilizedBy;

    private bool Active => true;

    public bool CanLayNow
    {
        get
        {
            if (!Active) return false;
            return eggProgress >= 1f;
        }
    }

    public bool FullyFertilized => fertilizationCount >= Props.eggFertilizationCountMax;

    private bool ProgressStoppedBecauseUnfertilized
    {
        get
        {
            if (Props.eggProgressUnfertilizedMax < 1f && fertilizationCount == 0)
                return eggProgress >= Props.eggProgressUnfertilizedMax;
            return false;
        }
    }

    public CompProperties_DittoEggLayer Props => (CompProperties_DittoEggLayer)props;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        eggLayIntervalDays = Props.eggLayIntervalDays;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref eggProgress, "eggProgress");
        Scribe_Values.Look(ref fertilizationCount, "fertilizationCount");
        Scribe_Values.Look(ref eggLayIntervalDays, "eggLayIntervalDays", 1);
        Scribe_Defs.Look(ref eggFertilizedDef, "eggFertilizedDef");
        Scribe_References.Look(ref fertilizedBy, "fertilizedBy", true);
    }

    public override void CompTick()
    {
        if (Active)
        {
            var num = 1f / (eggLayIntervalDays * 60000f);
            if (parent is Pawn pawn) num *= PawnUtility.BodyResourceGrowthSpeed(pawn);
            eggProgress += num;
            if (eggProgress > 1f) eggProgress = 1f;
            if (ProgressStoppedBecauseUnfertilized) eggProgress = Props.eggProgressUnfertilizedMax;
        }
    }

    public void Fertilize(Pawn male)
    {
        fertilizationCount = Props.eggFertilizationCountMax;
        fertilizedBy = male;
        eggFertilizedDef = male.TryGetComp<CompEggLayer>().Props.eggFertilizedDef;
        eggLayIntervalDays = male.TryGetComp<CompEggLayer>().Props.eggLayIntervalDays;
    }

    public virtual Thing ProduceEgg()
    {
        if (!Active) Log.Error("LayEgg while not Active: " + parent);
        eggProgress = 0f;
        var randomInRange = Props.eggCountRange.RandomInRange;
        if (randomInRange == 0) return null;
        Thing thing = null;
        if (fertilizationCount > 0 && eggFertilizedDef != null)
            thing = ThingMaker.MakeThing(eggFertilizedDef);
        else if (Props.eggUnfertilizedDef != null) thing = ThingMaker.MakeThing(Props.eggUnfertilizedDef);
        fertilizationCount = Mathf.Max(0, fertilizationCount - randomInRange);
        if (thing == null) return thing;
        thing.stackCount = randomInRange;
        var compHatcher = thing.TryGetComp<CompHatcher>();
        if (compHatcher != null)
        {
            compHatcher.hatcheeFaction = parent.Faction;
            if (parent is Pawn pawn) compHatcher.hatcheeParent = pawn;
            if (fertilizedBy != null) compHatcher.otherParent = fertilizedBy;
        }

        eggLayIntervalDays = Props.eggLayIntervalDays;
        return thing;
    }

    public override string CompInspectStringExtra()
    {
        if (!Active) return null;
        string text = "EggProgress".Translate() + ": " + eggProgress.ToStringPercent();
        if (fertilizationCount > 0)
            text += "\n" + "Fertilized".Translate();
        else if (ProgressStoppedBecauseUnfertilized) text += "\n" + "ProgressStoppedUntilFertilized".Translate();
        return text;
    }
}

public class CompProperties_DittoEggLayer : CompProperties
{
    public IntRange eggCountRange = IntRange.one;

    public int eggFertilizationCountMax = 1;
    public float eggLayIntervalDays = 1f;

    public float eggProgressUnfertilizedMax = 1f;

    public ThingDef eggUnfertilizedDef;

    public CompProperties_DittoEggLayer()
    {
        compClass = typeof(CompDittoEggLayer);
    }
}
