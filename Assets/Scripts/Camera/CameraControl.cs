using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float dampTime = 0.2f;
    public float scrollSpeed = 10f;
    public float maxSize = 35f;

    Transform target;

    Vector3 moveVelocity;
    Vector3 targetPos;

    void Awake() {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update() {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        float size = Camera.main.orthographicSize - scroll * scrollSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(size, 5f, maxSize);
    }

    void FixedUpdate() {
        Move();
    }

    void Move() {
        targetPos = target.position;
        transform.position = Vector3.SmoothDamp(transform.position,
        targetPos, ref moveVelocity, dampTime);
    }
}
