using System.Collections;
using System.Collections.Generic;
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
    [SerializeField]
    private float look_speed;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        var view_model = GameObject.FindGameObjectWithTag("PlayerViewModel");
        model_transform = view_model.transform;
        animator = view_model.GetComponent<Animator>();
        is_moving_param_index = Animator.StringToHash("IsMoving");
        cam = Camera.main;

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
        velocity = new Vector3(input_value.x, 0.0f, input_value.y) * move_speed;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        var input_value = context.ReadValueAsButton();
        Debug.Log("Interact: " + input_value);
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemy_controller = other.GetComponentInParent<EnemyController>();
        if (other.CompareTag("Near_Cone"))
        {
            enemy_controller.is_chasing = true;
        }
        else if (other.CompareTag("Mid_Cone"))
        {
            // if Near_Cone is unobstructed
            if ((enemy_controller.cone_flags & ConeFlags.NEAR_CONE) != ConeFlags.NEAR_CONE)
            {
                enemy_controller.is_chasing = true;
            }
        }
        else if (other.CompareTag("Far_Cone"))
        {
            // if Near_Cone and Mid_Cone are unobstructed
            if ((enemy_controller.cone_flags & (ConeFlags.NEAR_CONE | ConeFlags.MID_CONE)) != (ConeFlags.NEAR_CONE | ConeFlags.MID_CONE))
            {
                enemy_controller.is_chasing = true;
            }
        }
        else if (other.CompareTag("Very_Far_Cone"))
        {
            // if Near_Cone and Mid_Cone are unobstructed
            if ((enemy_controller.cone_flags & (ConeFlags.NEAR_CONE | ConeFlags.MID_CONE | ConeFlags.FAR_CONE)) != (ConeFlags.NEAR_CONE | ConeFlags.MID_CONE | ConeFlags.FAR_CONE))
            {
                enemy_controller.is_chasing = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        controller.Move(velocity * Time.deltaTime); 
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
