using UnityEngine;

public class Shell : MonoBehaviour {
    public float lifetime = 2f;

    public float maxDmg = 34f;
    public float explosionRadius = 5;
    public float explosionForce = 100f;

    public ParticleSystem explosion;

    public string safeTag = "";

    void Start() {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision other) {
        Rigidbody target = other.gameObject.GetComponent<Rigidbody>();

        if (target && target.gameObject.tag != safeTag) {
            target.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            TankHealth targetHealth = target.GetComponent<TankHealth>();

            if (targetHealth) {
                float dmg = CalculateDamage(target.position);
                targetHealth.TakeDamage(dmg);
            }
        }
        else if (other.gameObject.CompareTag("Destructable")) {
            other.gameObject.SetActive(false);
        }

        explosion.transform.parent = null;
        explosion.Play();

        Destroy(gameObject);
    }

    float CalculateDamage(Vector3 target) {
        float dist = (target - transform.position).magnitude;
        float relativeDist = (explosionRadius - dist) / explosionRadius;

        float dmg = relativeDist * maxDmg;
        dmg = Mathf.Max(0f, dmg);

        return dmg;
    }
}