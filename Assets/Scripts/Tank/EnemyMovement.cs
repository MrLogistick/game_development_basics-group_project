using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour {
    public Transform[] patrolNodes;
    public float maxError = 1f;
    int targetNode = 0;

    public float staticTime = 2f;
    float timeElapsed;
    
    public float closeDist = 8f;
    public Transform turret;

    public float turnSpeed;

    Transform player;
    NavMeshAgent navAgent;
    Rigidbody rb;
    SphereCollider col;

    bool follow;
    bool fresh = true;

    Vector3 initialPos;
    Quaternion initialRot;

    void Awake() {
        navAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();
        follow = false;

        initialPos = transform.position;
        initialRot = transform.rotation;

        navAgent.updateRotation = true;
    }

    void OnEnable() {
        try {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        catch {Debug.LogError($"{gameObject.name} couldn't find the player tank!"); }

        rb.isKinematic = false;
        navAgent.isStopped = false;

        follow = false;
        fresh = true;
    }

    void OnDisable()
    {
        navAgent.Warp(initialPos);
        transform.position = initialPos;
        transform.rotation = initialRot;
        print(initialPos);

        rb.isKinematic = true;
        if (navAgent.isActiveAndEnabled) navAgent.isStopped = true;
    }

    void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        follow = true;
    }

    void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return;
        follow = false;
    }

    void Update() {
        if (follow) {
            Follow();

            fresh = true;
            timeElapsed = staticTime;
        }
        else if (patrolNodes.Length > 0) {
            Patrol();
        }

        if (turret) {
            turret.LookAt(new Vector3(player.position.x, turret.position.y, player.position.z));
        }
    }

    void Follow() {
        float dist = (player.position - transform.position).magnitude;
        if (dist > closeDist && dist < col.radius) {
            Move(player.position, (player.position - transform.position).normalized);
        }
        else {
            navAgent.isStopped = true;
        }
    }

    void Patrol() {
        if (fresh) {
            fresh = false;

            float shortestDist = -1;
            for (int i = 0; i < patrolNodes.Length; i++) {
                float d = (patrolNodes[i].position - transform.position).magnitude;

                if (d < shortestDist || shortestDist == -1) {
                    shortestDist = d;
                    targetNode = i;
                }
            }
        }
        
        float dist = (patrolNodes[targetNode].position - transform.position).magnitude;
        if (dist > maxError) {
            Move(patrolNodes[targetNode].position, (patrolNodes[targetNode].position - transform.position).normalized);
            timeElapsed = staticTime;
        }
        else {
            timeElapsed -= Time.deltaTime;
            navAgent.isStopped = true;
        }

        if (timeElapsed <= 0) {
            if (targetNode == patrolNodes.Length - 1) targetNode = 0;
            else targetNode++;
        }
    }

    void Move(Vector3 target, Vector3 dir) {
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        Quaternion flatTargetRot = Quaternion.Euler(0f, targetRot.eulerAngles.y, 0f);

        if (Quaternion.Angle(transform.rotation, flatTargetRot) > 1f) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, flatTargetRot, turnSpeed * Time.deltaTime);
        }
        else {
            navAgent.isStopped = false;   
            navAgent.SetDestination(target);
        }
    }

    void OnDrawGizmos() {
        if (patrolNodes.Length == 0 && patrolNodes == null) return;

        Gizmos.color = Color.red;

        for (int i = 0; i < patrolNodes.Length; i++) {
            Gizmos.DrawWireSphere(patrolNodes[i].position, maxError);

            if (i < patrolNodes.Length - 1) Gizmos.DrawLine(patrolNodes[i].position, patrolNodes[i + 1].position);
            else Gizmos.DrawLine(patrolNodes[i].position, patrolNodes[0].position);
        }
    }
}