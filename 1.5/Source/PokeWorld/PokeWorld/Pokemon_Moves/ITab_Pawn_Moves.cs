using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

public class ITab_Pawn_Moves : ITab
{
    public ITab_Pawn_Moves()
    {
        labelKey = "PW_Moves";
    }

    public override bool IsVisible
    {
        get
        {
            if (base.SelPawn.TryGetComp<CompPokemon>() != null) return base.SelPawn.Faction == Faction.OfPlayer;
            return false;
        }
    }

    protected override void FillTab()
    {
        var rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(17f);
        rect.yMin += 10f;
        MoveCardUtility.DrawMoveCard(rect, base.SelPawn);
    }

    protected override void UpdateSize()
    {
        base.UpdateSize();
        size = new Vector2(400f, MoveCardUtility.TotalHeightForPawn(base.SelPawn));
    }
}
