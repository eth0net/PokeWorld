using UnityEngine;
using Verse;

namespace PokeWorld;

[StaticConstructorOnStartup]
public class PawnKindColumnWorker_PokedexIcon : PawnKindColumnWorker_Icon
{
    protected override Texture2D GetIconFor(PawnKindDef pawnKind)
    {
        if (Find.World.GetComponent<PokedexManager>().IsPokemonSeen(pawnKind))
        {
            var Icon = ContentFinder<Texture2D>.Get(pawnKind.lifeStages[0].bodyGraphicData.texPath + "_east");
            return Icon;
        }

        return null;
    }

    protected override string GetIconTip(PawnKindDef pawnKind)
    {
        if (Find.World.GetComponent<PokedexManager>().IsPokemonSeen(pawnKind)) return pawnKind.label;
        return null;
    }
}
