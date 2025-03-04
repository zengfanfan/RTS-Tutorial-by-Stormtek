using UnityEngine;
using System.Collections;

public class Survival : VictoryCondition {
    public int minutes = 10;
    private float timeLeft = 0.0f;

    void Awake() => timeLeft = minutes * 60;

    void Update() => timeLeft -= Time.deltaTime;

    public override string GetDescription() => "Survival";

    public override bool GameFinished() {
        foreach (Player player in players) {
            if (player && player.human && player.IsDead()) return true;
        }
        return timeLeft < 0;
    }

    public override bool PlayerMeetsConditions(Player player) => player && player.human && !player.IsDead();
}
