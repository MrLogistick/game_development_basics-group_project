using UnityEngine;

public class EnemyShooting : TankShooting {

    public float shotDelay = 1f;
    public Transform player;

    bool canShoot;
    float shotTimer;

    void Awake() {
        canShoot = false;
        shotTimer = 0f;
    }

    void OnEnable() {
        canShoot = false;
    }

    void Update() {
        if (canShoot) {
            shotTimer -= Time.deltaTime;
            if (shotTimer <= 0) {
                shotTimer = shotDelay;
                Fire("Enemy");
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.transform != player) return;
        canShoot = true;
    }

    void OnTriggerExit(Collider other) {
        if (other.transform != player) return;
        canShoot = false;
    }
}