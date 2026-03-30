using UnityEngine;

public class TankAim : MonoBehaviour {
    public Transform turret;
    LayerMask layerMask;

    void Awake() {
        layerMask = LayerMask.GetMask("Ground");
    }

    void Update() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) {
            Vector3 point = new Vector3(hit.point.x, turret.position.y, hit.point.z);
            turret.LookAt(point);
        }
    }
}