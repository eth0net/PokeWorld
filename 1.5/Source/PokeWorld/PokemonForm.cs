using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PokeWorld;

public enum FormChangerCondition
{
    Fixed,
    Selectable,
    Environment
}

public class PokemonForm
{
    public int baseAttack;
    public int baseDefense;
    public int baseHP;
    public int baseSpAttack;
    public int baseSpDefense;
    public int baseSpeed;
    public List<BiomeDef> excludeBiomes;
    public List<WeatherDef> excludeWeathers;
    public List<BiomeDef> includeBiomes;
    public List<WeatherDef> includeWeathers;
    public bool isDefault = false;
    public string label;

    [NoTranslate]
    public string texPathKey;

    public TimeOfDay timeOfDay = TimeOfDay.Any;
    public TypeDef type1;
    public TypeDef type2;
    public float weight = 1;
}
