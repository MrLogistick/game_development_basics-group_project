using UnityEngine;

public class TankHealth : MonoBehaviour {
    public float initialHealth = 100f;
    public bool scratchless = true;

    public ParticleSystem explosion;

    float health;
    bool dead;

    void OnEnable() {
        health = initialHealth;
        scratchless = true;
        dead = false;
    }

    public void TakeDamage(float value) {
        health -= value;
        scratchless = false;

        if (health <= 0 && !dead) {
            OnDeath();
        }
    }

    void OnDeath() {
        dead = true;

        var instance = Instantiate(explosion, transform.position, Quaternion.identity);
        instance.Play();

        gameObject.SetActive(false);
    }
}