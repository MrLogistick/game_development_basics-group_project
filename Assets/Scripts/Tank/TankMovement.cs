using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public float speed = 20f;
    public float rotSpeed = 100f;

    Rigidbody rb;

    float movementInput;
    float turnInput;

    Vector3 initialPos;
    Quaternion initialRot;

    void Awake() {
        rb = GetComponent<Rigidbody>();

        initialPos = rb.position;
        initialRot = rb.rotation;
    }

    void OnEnable() {
        rb.isKinematic = false;

        movementInput = 0;
        turnInput = 0;
    }

    void OnDisable()  {
        rb.position = initialPos;
        rb.rotation = initialRot;

        rb.isKinematic = true;
    }

    void Update() {
        movementInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate() {
        Move();
        Turn();
    }

    void Move() {
        Vector3 targetVelocity = transform.forward * movementInput * speed;
        rb.AddForce(targetVelocity - rb.velocity, ForceMode.VelocityChange);
    }

    void Turn() {
        float turnValue = turnInput * rotSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turnValue, 0f);
        rb.MoveRotation(transform.rotation * turnRotation);
    }
}
