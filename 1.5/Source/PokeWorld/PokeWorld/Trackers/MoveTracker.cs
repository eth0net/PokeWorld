using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PokeWorld;

public class MoveTracker : IExposable
{
    public CompPokemon comp;
    private List<Verb> initializedVerbs;
    public MoveDef lastUsedMove;
    public Pawn pokemonHolder;
    public Dictionary<MoveDef, int> unlockableMoves;
    public Dictionary<MoveDef, bool> wantedMoves;

    public MoveTracker(CompPokemon comp)
    {
        this.comp = comp;
        pokemonHolder = (Pawn)comp.parent;
        unlockableMoves = new Dictionary<MoveDef, int>();
        wantedMoves = new Dictionary<MoveDef, bool>();
        initializedVerbs = new List<Verb>();
        foreach (var move in comp.Moves)
        {
            unlockableMoves.Add(move.moveDef, move.unlockLevel);
            wantedMoves.Add(move.moveDef, true);
        }

        OrderMoves();
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref unlockableMoves, "unlockableMoves", LookMode.Def, LookMode.Value);
        Scribe_Collections.Look(ref wantedMoves, "wantedMoves", LookMode.Def, LookMode.Value);
        Scribe_Collections.Look(ref initializedVerbs, "initializedVerbs", LookMode.Deep);
        Scribe_Defs.Look(ref lastUsedMove, "lastUsedMove");
    }

    public IEnumerable<Gizmo> GetGizmos()
    {
        if (PokemonMasterUtility.IsPokemonMasterDrafted(pokemonHolder))
            foreach (var attackGizmo in PokemonAttackGizmoUtility.GetAttackGizmos(pokemonHolder))
                yield return attackGizmo;
    }

    public bool HasUnlocked(MoveDef moveDef)
    {
        if (unlockableMoves.TryGetValue(moveDef, out var unlockLevel))
            if (unlockLevel <= comp.levelTracker.level)
                return true;
        return false;
    }

    public bool GetWanted(MoveDef moveDef)
    {
        if (wantedMoves.ContainsKey(moveDef)) return wantedMoves[moveDef];
        return false;
    }

    public void SetWanted(MoveDef moveDef, bool wanted)
    {
        if (wantedMoves.ContainsKey(moveDef)) wantedMoves[moveDef] = wanted;
    }

    public void GetUnlockedMovesFromPreEvolution(CompPokemon preEvoComp)
    {
        foreach (var kvp in preEvoComp.moveTracker.unlockableMoves)
        {
            if (kvp.Key == DefDatabase<MoveDef>.GetNamed("Struggle")) continue;
            if (preEvoComp.moveTracker.HasUnlocked(kvp.Key))
            {
                if (unlockableMoves.Keys.Contains(kvp.Key))
                {
                    unlockableMoves.Remove(kvp.Key);
                    wantedMoves.Remove(kvp.Key);
                }

                unlockableMoves.Add(kvp.Key, kvp.Value);
                wantedMoves.Add(kvp.Key, preEvoComp.moveTracker.wantedMoves[kvp.Key]);
            }
        }

        OrderMoves();
    }

    private void OrderMoves()
    {
        var myList = unlockableMoves.ToList();
        myList.Sort(
            delegate(
                KeyValuePair<MoveDef, int> pair1,
                KeyValuePair<MoveDef, int> pair2
            )
            {
                return pair1.Value.CompareTo(pair2.Value);
            }
        );
        unlockableMoves = myList.ToDictionary(x => x.Key, x => x.Value);
    }
}
