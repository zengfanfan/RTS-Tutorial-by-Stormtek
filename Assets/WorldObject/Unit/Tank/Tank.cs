using Newtonsoft.Json;
using RTS;
using UnityEngine;

public class Tank : Unit {
    public float weaponAimSpeed = 1;
    private Quaternion aimRotation;

    protected override void Start() => base.Start();

    protected override void Update() {
        base.Update();
        if (aiming) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, aimRotation, weaponAimSpeed);
            CalculateBounds();
            //sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
            Quaternion inverseAimRotation = new(-aimRotation.x, -aimRotation.y, -aimRotation.z, -aimRotation.w);
            if (transform.rotation == aimRotation || transform.rotation == inverseAimRotation) {
                aiming = false;
            }
        }
    }

    public override bool CanAttack() => true;

    protected override void AimAtTarget() {
        base.AimAtTarget();
        aimRotation = Quaternion.LookRotation(target.transform.position - transform.position);
    }

    protected override void UseWeapon() {
        base.UseWeapon();
        Vector3 spawnPoint = transform.position;
        spawnPoint.x += 2.1f * transform.forward.x;
        spawnPoint.y += 1.4f;
        spawnPoint.z += 2.1f * transform.forward.z;
        GameObject gameObject = Instantiate(ResourceManager.GetWorldObject("TankProjectile"), spawnPoint, transform.rotation);
        Projectile projectile = gameObject.GetComponentInChildren<Projectile>();
        projectile.SetRange(0.9f * weaponRange);
        projectile.SetTarget(target);
    }

    public override void SaveDetails(JsonWriter writer) {
        base.SaveDetails(writer);
        SaveManager.WriteQuaternion(writer, "AimRotation", aimRotation);
    }

    protected override void HandleLoadedProperty(JsonTextReader reader, string propertyName, object readValue) {
        base.HandleLoadedProperty(reader, propertyName, readValue);
        switch (propertyName) {
        case "AimRotation": aimRotation = LoadManager.LoadQuaternion(reader); break;
        default: break;
        }
    }

}
