using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RimWorld;
using Verse;

namespace PokeWorld;

public class FishingRateDef : Def
{
    private List<FishingRateBiomeRecord> biomes = new();

    private Dictionary<PawnKindDef, float> rateTable;

    public PawnKindDef GetRandomFish(BiomeDef biome, TerrainDef terrain, ThingDef rod)
    {
        var rates = biomes.Where(b => b.biome == biome).First().terrains.Where(t => t.terrain == terrain).First().rods
            .Where(r => r.rod == rod).First().pokemons;

        rateTable = new Dictionary<PawnKindDef, float>();

        for (var i = 0; i < rates.Count; i++)
            if (rates[i].pokemon != null)
                rateTable.Add(rates[i].pokemon, rates[i].rate);

        return rateTable.Keys.RandomElementByWeight(def => rateTable[def]);
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        defName = xmlRoot.SelectSingleNode("defName")?.InnerText;
        label = xmlRoot.SelectSingleNode("label")?.InnerText;
        biomes = DirectXmlToObject.ObjectFromXml<List<FishingRateBiomeRecord>>(
            xmlRoot.SelectSingleNode("biomes"), false
        );
    }

    public class FishingRateBiomeRecord
    {
        public BiomeDef biome;

        public List<FishingRateTerrainRecord> terrains = new();

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "biome", xmlRoot.Name);
            terrains = DirectXmlToObject.ObjectFromXml<List<FishingRateTerrainRecord>>(xmlRoot, false);
        }
    }

    public class FishingRateTerrainRecord
    {
        public List<FishingRateRodRecord> rods = new();
        public TerrainDef terrain;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "terrain", xmlRoot.Name);
            rods = DirectXmlToObject.ObjectFromXml<List<FishingRateRodRecord>>(xmlRoot, false);
        }
    }

    public class FishingRateRodRecord
    {
        public List<FishingRatePokemonRecord> pokemons = new();
        public ThingDef rod;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "rod", xmlRoot.Name);
            pokemons = DirectXmlToObject.ObjectFromXml<List<FishingRatePokemonRecord>>(xmlRoot, false);
        }
    }

    public class FishingRatePokemonRecord
    {
        public PawnKindDef pokemon;

        public float rate;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "pokemon", xmlRoot);
            rate = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
        }
    }
}
