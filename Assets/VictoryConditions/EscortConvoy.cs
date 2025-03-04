using UnityEngine;

public class EscortConvoy : VictoryCondition {
    public Vector3 destination = new(0.0f, 0.0f, 0.0f);
    public Texture2D highlight;

    void Start() {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Ground";
        cube.transform.localScale = new(3, 0.01f, 3);
        cube.transform.position = new(destination.x, 0.005f, destination.z);
        if (highlight) cube.GetComponent<Renderer>().material.mainTexture = highlight;
        cube.transform.parent = transform;
    }

    public override string GetDescription() => "Escort Convoy Truck";

    public override bool PlayerMeetsConditions(Player player) {
        var truck = player.GetComponentInChildren<ConvoyTruck>();
        return player && !player.IsDead() && TruckInPosition(truck);
    }

    private bool TruckInPosition(ConvoyTruck truck) {
        if (!truck) return false;
        float closeEnough = 3.0f;
        Vector3 truckPos = truck.transform.position;
        bool xInPos = truckPos.x > destination.x - closeEnough && truckPos.x < destination.x + closeEnough;
        bool zInPos = truckPos.z > destination.z - closeEnough && truckPos.z < destination.z + closeEnough;
        return xInPos && zInPos;
    }

}
