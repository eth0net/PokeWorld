using System.Text;
using RimWorld;
using Verse;

namespace PokeWorld;

internal class StatWorker_PokemonFriendship : StatWorker
{
    public override bool ShouldShowFor(StatRequest req)
    {
        if (!base.ShouldShowFor(req)) return false;
        if (req.Def is ThingDef thingDef && thingDef.HasComp(typeof(CompPokemon)) && req.HasThing &&
            req.Thing.Faction == Faction.OfPlayer) return true;
        return false;
    }

    public override string ValueToString(
        float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute
    )
    {
        string text;
        if (val >= 250)
            text = "PW_StatFriendshipMaximum".Translate();
        else if (val >= 200)
            text = "PW_StatFriendshipVeryHigh".Translate();
        else if (val >= 150)
            text = "PW_StatFriendshipHigh".Translate();
        else if (val >= 100)
            text = "PW_StatFriendshipAverage".Translate();
        else if (val >= 50)
            text = "PW_StatFriendshipPoor".Translate();
        else if (val > 5)
            text = "PW_StatFriendshipVeryPoor".Translate();
        else
            text = "PW_StatFriendshipTerrible".Translate();

        return text;
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        return ValueFromReq(req);
    }

    private float ValueFromReq(StatRequest req)
    {
        if (req.Thing != null && req.Thing is Pawn pawn && pawn.TryGetComp<CompPokemon>() != null)
            return pawn.TryGetComp<CompPokemon>().friendshipTracker.friendship;
        return 0;
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        return "";
    }

    public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
    {
        if (req.Thing != null)
        {
            var pawn = req.Thing as Pawn;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(pawn.TryGetComp<CompPokemon>().friendshipTracker.GetStatement());
            return stringBuilder.ToString();
        }

        return "";
    }
}
