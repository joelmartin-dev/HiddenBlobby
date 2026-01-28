using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[System.Flags] public enum ConeFlags
{
    NEAR_CONE = 0x1,
    MID_CONE = 0x2,
    FAR_CONE = 0x4,
    VERY_FAR_CONE = 0x8
};

public class EnemyController : MonoBehaviour
{
    private Transform player_transform;
    private NavMeshAgent agent;
    [SerializeField] private Vector3 point_a;
    [SerializeField] private Vector3 point_b;
    [SerializeField] public bool is_chasing;
    private bool to_point_a;
    [SerializeField] public ConeFlags cone_flags;

    // Start is called before the first frame update
    void Start()
    {
        player_transform = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        is_chasing = false;

        point_a = transform.position;
        to_point_a = false;
        // Get all the destinations. Whoever's destination has this transform as a parent is our destination
        var destinations = GameObject.FindGameObjectsWithTag("Destination");
        foreach (var dest in destinations)
        {
            if (dest.transform.parent == transform)
            {
                point_b = dest.transform.position;
                break;
            }
        }

        cone_flags = 0;
    }

    private void FixedUpdate()
    {
        if (is_chasing) agent.destination = player_transform.position;
        else if (agent.remainingDistance < 0.2f)
        {
            agent.destination = to_point_a ? point_b : point_a;
            to_point_a = !to_point_a;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
