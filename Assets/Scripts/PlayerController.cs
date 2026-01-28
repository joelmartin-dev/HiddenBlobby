using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Transform model_transform;
    private Animator animator;
    private int is_moving_param_index;
    private Camera cam;
    private Vector3 velocity;
    [SerializeField]
    private float move_speed;
    private float move_speed_mod;
    [SerializeField]
    private float look_speed;
    public bool masked;
    [SerializeField] public List<EnemyController> chasing_enemies;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        var view_model = GameObject.FindGameObjectWithTag("PlayerViewModel");
        model_transform = view_model.transform;
        animator = view_model.GetComponent<Animator>();
        is_moving_param_index = Animator.StringToHash("IsMoving");
        cam = Camera.main;
        move_speed_mod = 1.0f;

        // Disable controller, teleport, re-enable controller
        controller.enabled = false;
        transform.position = GameObject.FindGameObjectWithTag("PlayerStart").transform.position;
        controller.enabled = true;
        // Start the player from the movable height, avoid jitter on first action
        controller.Move(new Vector3(0.01f, 0.0f, 0.0f));
        controller.Move(new Vector3(-0.01f, 0.0f, 0.0f));
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var input_value = context.ReadValue<Vector2>();
        velocity = new Vector3(input_value.x, 0.0f, input_value.y);
        if (velocity.magnitude > 1.0f) velocity = velocity.normalized;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        var input_value = context.ReadValueAsButton();
        if (input_value == true)
        {
            masked = true;
            foreach (var enemy in chasing_enemies)
            {
                if (enemy.can_see_player) enemy.knows_player_masked = true;
            }
            move_speed_mod = 0.1f;
        }
        else
        {
            masked = false;
            move_speed_mod = 1.0f;
        }
        Debug.Log("Interact: " + input_value);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.CompareTag("Enemy"))
        {
            RaycastHit hit;
            var enemy_controller = other.GetComponentInParent<EnemyController>();
            if (Physics.Raycast(enemy_controller.transform.position, transform.position - enemy_controller.transform.position, out hit, 1000.0f))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    enemy_controller.DetectedPlayer();
                    if (!chasing_enemies.Contains(enemy_controller)) chasing_enemies.Add(enemy_controller);
                    Debug.DrawLine(enemy_controller.transform.position, transform.position, Color.green, 10.0f);
                }
                else
                {
                    Debug.DrawLine(enemy_controller.transform.position, transform.position, Color.red, 10.0f);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        controller.Move(velocity * move_speed * move_speed_mod * Time.deltaTime); 
        if (velocity != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(velocity, Vector3.up);
            model_transform.rotation = Quaternion.Slerp(model_transform.rotation, toRotation, look_speed * Time.deltaTime);
            animator.SetBool(is_moving_param_index, true);
        }
        else
        {
            animator.SetBool(is_moving_param_index, false);
        }
    }
}
