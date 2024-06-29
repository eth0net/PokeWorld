namespace PokeWorld;

public static class PokemonComponentsUtility
{
    public static void CreateInitialComponents(CompPokemon comp)
    {
        if (comp.shinyTracker == null) comp.shinyTracker = new ShinyTracker(comp);
        if (comp.levelTracker == null) comp.levelTracker = new LevelTracker(comp);
        if (comp.friendshipTracker == null) comp.friendshipTracker = new FriendshipTracker(comp);
        if (comp.statTracker == null) comp.statTracker = new StatTracker(comp);
        if (comp.moveTracker == null) comp.moveTracker = new MoveTracker(comp);
        if (comp.formTracker == null && comp.Props.forms != null) comp.formTracker = new FormTracker(comp);
    }

    public static void ExposeData(CompPokemon comp)
    {
        comp.shinyTracker?.ExposeData();
        comp.levelTracker?.ExposeData();
        comp.friendshipTracker?.ExposeData();
        comp.statTracker?.ExposeData();
        comp.moveTracker?.ExposeData();
        comp.formTracker?.ExposeData();
    }
}
