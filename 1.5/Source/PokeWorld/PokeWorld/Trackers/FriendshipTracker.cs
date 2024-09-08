using RimWorld;
using Verse;

namespace PokeWorld;

public class FriendshipTracker : IExposable
{
    private const int minFriendship = 0;
    private const int maxFriendship = 255;
    private const int mateFriendshipThreshold = 150;
    public int baseFriendship;
    public CompPokemon comp;
    public bool flagMaxFriendshipMessage;

    public int friendship;
    public Pawn pokemonHolder;

    public FriendshipTracker(CompPokemon comp)
    {
        this.comp = comp;
        pokemonHolder = (Pawn)comp.parent;
        baseFriendship = comp.BaseFriendship;
        friendship = baseFriendship;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref friendship, "PW_friendship");
        Scribe_Values.Look(ref flagMaxFriendshipMessage, "PW_flagMaxFriendshipMessage");
    }

    public void FriendshipTick()
    {
        if (pokemonHolder.Spawned)
        {
            if (!pokemonHolder.Downed && GenTicks.TicksAbs % 30000 == 0)
                ChangeFriendship(1);
            else if (pokemonHolder.Downed && GenTicks.TicksAbs % 10000 == 0) ChangeFriendship(-1);
        }
    }

    public void IncreaseFriendshipLevelUp()
    {
        if (friendship >= 200)
            ChangeFriendship(2);
        else if (friendship >= 100)
            ChangeFriendship(3);
        else
            ChangeFriendship(5);
    }

    public void ChangeFriendship(int amount)
    {
        if (pokemonHolder.Faction == Faction.OfPlayer)
        {
            friendship += amount;
            if (friendship >= maxFriendship)
            {
                friendship = maxFriendship;
                if (!flagMaxFriendshipMessage)
                {
                    flagMaxFriendshipMessage = true;
                    Messages.Message(
                        "PW_PokemonLovesYou".Translate(pokemonHolder.Label), pokemonHolder,
                        MessageTypeDefOf.PositiveEvent
                    );
                }
            }
            else if (friendship <= minFriendship)
            {
                friendship = minFriendship;
            }
        }
    }

    public bool EvolutionAllowed(int minimumFriendshipForEvolution)
    {
        if (friendship >= minimumFriendshipForEvolution)
            return true;
        return false;
    }

    public bool CanMate()
    {
        return friendship >= mateFriendshipThreshold;
    }

    public string GetStatement()
    {
        string text;
        if (friendship >= 250)
            text = "PW_FriendshipStatementReallyHappy".Translate(pokemonHolder.gender.GetPronoun().CapitalizeFirst());
        else if (friendship >= 200)
            text = "PW_FriendshipStatementTrusts".Translate(pokemonHolder.gender.GetPronoun().CapitalizeFirst());
        else if (friendship >= 150)
            text = "PW_FriendshipStatementSortHappy".Translate(pokemonHolder.gender.GetPronoun().CapitalizeFirst());
        else if (friendship >= 100)
            text = "PW_FriendshipStatementCute".Translate(pokemonHolder.gender.GetPronoun().CapitalizeFirst());
        else if (friendship >= 50)
            text = "PW_FriendshipStatementNotUsed".Translate(pokemonHolder.gender.GetPronoun().CapitalizeFirst());
        else
            text = "PW_FriendshipStatementMean".Translate(pokemonHolder.gender.GetPronoun().CapitalizeFirst());
        return text;
    }
}
