using RimWorld;
using Verse;

namespace PokeWorld;

public class StatTracker : IExposable
{
    private int attackEV;
    private int attackIV;
    private int attackSpEV;
    private int attackSpIV;
    public int attackSpStat;
    public int attackStat;
    public CompPokemon comp;
    private int defenseEV;
    private int defenseIV;
    private int defenseSpEV;
    private int defenseSpIV;
    public int defenseSpStat;
    public int defenseStat;
    private float healthScaleMult = 1;

    private int hpEV;

    private int hpIV;

    public int hpStat;

    public NatureDef nature;
    public Pawn pokemonHolder;
    private int speedEV;
    private int speedIV;
    public int speedStat;

    public StatTracker(CompPokemon comp)
    {
        this.comp = comp;
        pokemonHolder = comp.Pokemon;
        nature = DefDatabase<NatureDef>.AllDefs.RandomElement();
        RandomizeIV();
        SetAllEVZero();
    }

    public float HealthScaleMult => healthScaleMult;

    private int TotalEV => hpEV + attackEV + defenseEV + attackSpEV + defenseSpEV + speedEV;

    public void ExposeData()
    {
        Scribe_Values.Look(ref hpStat, "PW_hpStat", 1);
        Scribe_Values.Look(ref attackStat, "PW_attackStat", 1);
        Scribe_Values.Look(ref defenseStat, "PW_defenseStat", 1);
        Scribe_Values.Look(ref attackSpStat, "PW_attackSpStat", 1);
        Scribe_Values.Look(ref defenseSpStat, "PW_defenseSpStat", 1);
        Scribe_Values.Look(ref speedStat, "PW_speedStat", 1);

        Scribe_Values.Look(ref hpIV, "PW_hpIV");
        Scribe_Values.Look(ref attackIV, "PW_attackIV");
        Scribe_Values.Look(ref defenseIV, "PW_defenseIV");
        Scribe_Values.Look(ref attackSpIV, "PW_attackSpIV");
        Scribe_Values.Look(ref defenseSpIV, "PW_defenseSpIV");
        Scribe_Values.Look(ref speedIV, "PW_speedIV");

        Scribe_Values.Look(ref hpEV, "PW_hpEV");
        Scribe_Values.Look(ref attackEV, "PW_attackEV");
        Scribe_Values.Look(ref defenseEV, "PW_defenseEV");
        Scribe_Values.Look(ref attackSpEV, "PW_attackSpEV");
        Scribe_Values.Look(ref defenseSpEV, "PW_defenseSpEV");
        Scribe_Values.Look(ref speedEV, "PW_speedEV");

        Scribe_Values.Look(ref healthScaleMult, "PW_healthScaleMult", 1);
        Scribe_Defs.Look(ref nature, "PW_nature");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            if (nature == null)
                nature = DefDatabase<NatureDef>.AllDefs.RandomElement();
    }

    public void RandomizeIV()
    {
        hpIV = Rand.RangeInclusive(0, 31);
        attackIV = Rand.RangeInclusive(0, 31);
        defenseIV = Rand.RangeInclusive(0, 31);
        attackSpIV = Rand.RangeInclusive(0, 31);
        defenseSpIV = Rand.RangeInclusive(0, 31);
        speedIV = Rand.RangeInclusive(0, 31);
    }

    public void SetAllEVZero()
    {
        hpEV = 0;
        attackEV = 0;
        defenseEV = 0;
        attackSpEV = 0;
        defenseSpEV = 0;
        speedEV = 0;
    }

    private void Increase(ref int ev, int amount)
    {
        if (ev + amount > 252)
            ev = 252;
        else
            ev += amount;
    }

    public void IncreaseEV(StatDef stat, int amount)
    {
        if (TotalEV >= 510)
            return;
        if (TotalEV + amount > 510) amount = 510 - TotalEV;
        switch (stat.defName)
        {
            case "PW_HP":
                Increase(ref hpEV, amount);
                break;
            case "PW_Attack":
                Increase(ref attackEV, amount);
                break;
            case "PW_Defense":
                Increase(ref defenseEV, amount);
                break;
            case "PW_SpecialAttack":
                Increase(ref attackSpEV, amount);
                break;
            case "PW_SpecialDefense":
                Increase(ref defenseSpEV, amount);
                break;
            case "PW_Speed":
                Increase(ref speedEV, amount);
                break;
        }
    }

    private void CopyPreEvoEV(CompPokemon preEvoComp)
    {
        var tracker = preEvoComp.statTracker;
        hpEV = tracker.GetEV(DefDatabase<StatDef>.GetNamed("PW_HP"));
        attackEV = tracker.GetEV(DefDatabase<StatDef>.GetNamed("PW_Attack"));
        defenseEV = tracker.GetEV(DefDatabase<StatDef>.GetNamed("PW_Defense"));
        attackSpEV = tracker.GetEV(DefDatabase<StatDef>.GetNamed("PW_SpecialAttack"));
        defenseSpEV = tracker.GetEV(DefDatabase<StatDef>.GetNamed("PW_SpecialDefense"));
        speedEV = tracker.GetEV(DefDatabase<StatDef>.GetNamed("PW_Speed"));
    }

    private void CopyPreEvoNature(CompPokemon preEvoComp)
    {
        nature = preEvoComp.statTracker.nature;
    }

    public void CopyPreEvoStat(CompPokemon preEvoComp)
    {
        CopyPreEvoNature(preEvoComp);
        CopyPreEvoIV(preEvoComp);
        CopyPreEvoEV(preEvoComp);
    }

    public int GetIV(StatDef stat)
    {
        int value;
        switch (stat.defName)
        {
            case "PW_HP":
                value = hpIV;
                break;
            case "PW_Attack":
                value = attackIV;
                break;
            case "PW_Defense":
                value = defenseIV;
                break;
            case "PW_SpecialAttack":
                value = attackSpIV;
                break;
            case "PW_SpecialDefense":
                value = defenseSpIV;
                break;
            case "PW_Speed":
                value = speedIV;
                break;
            default:
                value = 0;
                break;
        }

        return value;
    }

    private void CopyPreEvoIV(CompPokemon preEvoComp)
    {
        var tracker = preEvoComp.statTracker;
        hpIV = tracker.GetIV(DefDatabase<StatDef>.GetNamed("PW_HP"));
        attackIV = tracker.GetIV(DefDatabase<StatDef>.GetNamed("PW_Attack"));
        defenseIV = tracker.GetIV(DefDatabase<StatDef>.GetNamed("PW_Defense"));
        attackSpIV = tracker.GetIV(DefDatabase<StatDef>.GetNamed("PW_SpecialAttack"));
        defenseSpIV = tracker.GetIV(DefDatabase<StatDef>.GetNamed("PW_SpecialDefense"));
        speedIV = tracker.GetIV(DefDatabase<StatDef>.GetNamed("PW_Speed"));
    }

    public int GetEV(StatDef stat)
    {
        int value;
        switch (stat.defName)
        {
            case "PW_HP":
                value = hpEV;
                break;
            case "PW_Attack":
                value = attackEV;
                break;
            case "PW_Defense":
                value = defenseEV;
                break;
            case "PW_SpecialAttack":
                value = attackSpEV;
                break;
            case "PW_SpecialDefense":
                value = defenseSpEV;
                break;
            case "PW_Speed":
                value = speedEV;
                break;
            default:
                value = 0;
                break;
        }

        return value;
    }

    public void UpdateStats()
    {
        hpStat = (int)pokemonHolder.GetStatValue(DefDatabase<StatDef>.GetNamed("PW_HP"));
        attackStat = (int)pokemonHolder.GetStatValue(DefDatabase<StatDef>.GetNamed("PW_Attack"));
        defenseStat = (int)pokemonHolder.GetStatValue(DefDatabase<StatDef>.GetNamed("PW_Defense"));
        attackSpStat = (int)pokemonHolder.GetStatValue(DefDatabase<StatDef>.GetNamed("PW_SpecialAttack"));
        defenseSpStat = (int)pokemonHolder.GetStatValue(DefDatabase<StatDef>.GetNamed("PW_SpecialDefense"));
        speedStat = (int)pokemonHolder.GetStatValue(DefDatabase<StatDef>.GetNamed("PW_Speed"));
        healthScaleMult = hpStat / 50f;
    }
}
