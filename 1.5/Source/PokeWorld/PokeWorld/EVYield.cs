using System.Xml;
using RimWorld;
using Verse;

namespace PokeWorld;

public class EVYield
{
    public StatDef stat;

    public int value;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stat", xmlRoot.Name);
        value = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
    }
}
