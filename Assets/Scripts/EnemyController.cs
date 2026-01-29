using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    private PlayerController player;
    private NavMeshAgent agent;
    [SerializeField] private Vector3 point_a;
    [SerializeField] private Vector3 point_b;
    [SerializeField] public bool is_chasing;
    [SerializeField] public bool can_see_player;
    [SerializeField] public bool knows_player_masked;
    private bool to_point_a;
    private int turn;
    private int frame;
    private int frame_modulo;
    [SerializeField] private float max_wait_time;
    [SerializeField] private float wait_time;
    [SerializeField] public float max_held_time;
    [SerializeField] public float held_time;
    private GameObject alert;
    private GameObject question; 

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        agent = GetComponent<NavMeshAgent>();
        is_chasing = false;
        can_see_player = false;
        wait_time = max_wait_time;

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
        frame = 0;

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        frame_modulo = enemies.Length;
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].name.CompareTo(name) == 0)
            {
                turn = i;
                break;
            }
        }

        held_time = max_held_time;
        var alerts = GameObject.FindGameObjectsWithTag("Alert");
        foreach (var candidate in alerts)
        {
            if (candidate.transform.parent == transform)
            {
                alert = candidate; break;
            }
        }

        var questions = GameObject.FindGameObjectsWithTag("Question");
        foreach (var candidate in questions)
        {
            if (candidate.transform.parent == transform)
            {
                question = candidate; break;
            }
        }
    }

    public void DetectedPlayer()
    {
        is_chasing = true;
        can_see_player = true;
        wait_time = max_wait_time;
    }

    private void FixedUpdate()
    {
        frame = (frame + 1) % frame_modulo;
        if (frame != turn) return;
        if (is_chasing)
        {
            if (Physics.Raycast(transform.position, player.transform.position - transform.position, out RaycastHit hit, 1000.0f))
            {
                if (!hit.collider.CompareTag("Player"))
                {
                    can_see_player = false;
                    knows_player_masked = false;
                    held_time = max_held_time;
                    Debug.DrawLine(transform.position, player.transform.position, Color.yellow, 10.0f);
                }
                else if (!(!knows_player_masked && player.masked))
                {
                    can_see_player = true;
                    Debug.DrawLine(transform.position, player.transform.position, Color.green, 10.0f);
                }
            }
            if (can_see_player)
            {
                alert.transform.localScale = Vector3.Lerp(alert.transform.localScale, Vector3.one, 0.2f);
                question.transform.localScale = Vector3.Lerp(question.transform.localScale, Vector3.zero, 0.2f);
                agent.destination = player.transform.position;
                wait_time = max_wait_time;
                if ((player.transform.position - transform.position).magnitude < 4.0f)
                {
                    held_time -= Time.fixedDeltaTime * frame_modulo;
                    if (held_time < 0.0f) player.Lose();
                }
                else held_time = max_held_time;
            }
            else
            {
                alert.transform.localScale = Vector3.Lerp(alert.transform.localScale, Vector3.zero, 0.2f);
                question.transform.localScale = Vector3.Lerp(question.transform.localScale, Vector3.one, 0.2f);
                wait_time -= Time.fixedDeltaTime * frame_modulo;
                if (wait_time < 0.0f)
                {
                    is_chasing = false;
                    player.chasing_enemies.Remove(this);
                }
                held_time = max_held_time;
            }

            
        }
        else
        {
            alert.transform.localScale = Vector3.Lerp(alert.transform.localScale, Vector3.zero, 0.2f);
            question.transform.localScale = Vector3.Lerp(question.transform.localScale, Vector3.zero, 0.2f);
            held_time = max_held_time;
            if (agent.remainingDistance < 0.2f)
            {
                agent.destination = to_point_a ? point_b : point_a;
                to_point_a = !to_point_a;
            }
        }
    }
}
