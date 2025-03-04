public class BuildWonder : VictoryCondition {

    public override string GetDescription() => "Building Wonder";

    public override bool PlayerMeetsConditions(Player player) {
        var wonder = player.GetComponentInChildren<Wonder>();
        return player && !player.IsDead() && wonder && !wonder.UnderConstruction();
    }
}
