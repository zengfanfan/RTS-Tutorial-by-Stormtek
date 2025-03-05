public class WarFactory : Building {
    protected override void Start() {
        base.Start();
        actions = new[] { "Tank", "ConvoyTruck" };
    }

    public override void PerformAction(string actionToPerform) {
        base.PerformAction(actionToPerform);
        CreateUnit(actionToPerform);
    }

    protected override bool ShouldMakeDecision() => false;

}
