

using System.Collections.Generic;

public class WarFactory : Building {
    protected override void Start() {
        base.Start();
        List<string> actionlist = new();
        for (int i = 0; i < 11; i++) actionlist.Add("Tank");
        actions = actionlist.ToArray();
    }

    public override void PerformAction(string actionToPerform) {
        base.PerformAction(actionToPerform);
        CreateUnit(actionToPerform);
    }
}
