using Newtonsoft.Json;
using RTS;

public class Resource : WorldObject {

    //Public variables
    public float capacity;

    //Variables accessible by subclass
    protected float amountLeft;
    protected ResourceType resourceType;

    /*** Game Engine methods, all can be overridden by subclass ***/

    protected override void Start() {
        base.Start();
        amountLeft = capacity;
        resourceType = ResourceType.Unknown;
    }

    protected override void CalculateCurrentHealth(float lowSplit, float highSplit) {
        healthPercentage = amountLeft / capacity;
        healthStyle.normal.background = ResourceManager.GetResourceHealthBar(resourceType);
    }

    protected override void HandleLoadedProperty(JsonTextReader reader, string propertyName, object readValue) {
        base.HandleLoadedProperty(reader, propertyName, readValue);
        switch (propertyName) {
        case "AmountLeft": amountLeft = (float)(double)readValue; break;
        default: break;
        }
    }

    /*** Public methods ***/

    public void Remove(float amount) {
        amountLeft -= amount;
        if (amountLeft < 0) amountLeft = 0;
    }

    public bool IsEmpty() => amountLeft <= 0;

    public ResourceType GetResourceType() => resourceType;

    public override void SaveDetails(JsonWriter writer) {
        base.SaveDetails(writer);
        SaveManager.WriteFloat(writer, "AmountLeft", amountLeft);
    }
}
