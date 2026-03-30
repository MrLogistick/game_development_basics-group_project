using UnityEngine;

public class TankShooting : MonoBehaviour {
    public Rigidbody shell;
    public Transform shotPoint;
    public float launchForce = 30f;

    void Update() {
        if (Input.GetButtonUp("Fire1")) {
            Fire("Player");
        }
    }

    public void Fire(string safeTag) {
        Rigidbody shellInstance = Instantiate(shell, shotPoint.position, shotPoint.rotation);
        shellInstance.velocity = launchForce * shotPoint.forward;
        shellInstance.GetComponent<Shell>().safeTag = safeTag;
    }
}