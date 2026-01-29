using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Transform model_transform;
    private Animator animator;
    private int is_moving_param_index;
    private int speed_mod_param_index;
    private Camera cam;
    private Vector3 velocity;
    [SerializeField]
    private float move_speed;
    private float move_speed_mod;
    [SerializeField]
    private float look_speed;
    public bool masked;
    private Material player_mat;
    [SerializeField] public List<EnemyController> chasing_enemies;
    private Material grass_mat;
    [SerializeField] private Vector2 peephole_origin;
    private Vector2 peephole_target;
    [SerializeField] private float peephole_min_size;
    [SerializeField] private float max_lookahead;

    private Renderer map_renderer;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        var view_model = GameObject.FindGameObjectWithTag("PlayerViewModel");
        model_transform = view_model.transform;
        animator = view_model.GetComponent<Animator>();
        is_moving_param_index = Animator.StringToHash("IsMoving");
        speed_mod_param_index = Animator.StringToHash("speed_mod");
        cam = Camera.main;
        move_speed_mod = 1.0f;

        // Disable controller, teleport, re-enable controller
        controller.enabled = false;
        transform.position = GameObject.FindGameObjectWithTag("PlayerStart").transform.position;
        controller.enabled = true;
        // Start the player from the movable height, avoid jitter on first action
        controller.Move(new Vector3(0.01f, 0.0f, 0.0f));
        controller.Move(new Vector3(-0.01f, 0.0f, 0.0f));

        grass_mat = GameObject.FindGameObjectWithTag("Hedge").GetComponent<Renderer>().sharedMaterial;
        peephole_target = peephole_origin;
        grass_mat.SetFloat("_Size", peephole_min_size);

        map_renderer = GameObject.FindGameObjectWithTag("Map").GetComponent<Renderer>();

        player_mat = GetComponentInChildren<Renderer>().material;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var input_value = context.ReadValue<Vector2>();
        velocity = new Vector3(input_value.x, 0.0f, input_value.y);
        if (velocity.magnitude > 1.0f) velocity = velocity.normalized;
    }

    public void OnPeepHole(InputAction.CallbackContext context)
    {
        var input_value = context.ReadValue<Vector2>();
        if (input_value.magnitude > 1.0f) input_value = input_value.normalized;
        peephole_target = peephole_origin + input_value * max_lookahead;
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
            move_speed_mod = 0.12f;
        }
        else
        {
            masked = false;
            move_speed_mod = 1.0f;
        }
        Debug.Log("Interact: " + input_value);
    }

    public void OnCheat(InputAction.CallbackContext context)
    {
        var input_value = context.ReadValueAsButton();
        map_renderer.enabled = input_value;
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        var input_value = context.ReadValueAsButton();
        if (input_value) Application.Quit();
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

    public void Lose()
    {
        Debug.Log("Player loses");
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
            animator.SetFloat(speed_mod_param_index, move_speed_mod);
        }
        else
        {
            animator.SetBool(is_moving_param_index, false);
        }

        grass_mat.SetVector("_PlayerScreenPos", Vector2.Lerp(grass_mat.GetVector("_PlayerScreenPos"), peephole_target, 0.5f));
        if (peephole_target - peephole_origin != Vector2.zero)
        {
            grass_mat.SetFloat("_Size", Mathf.Lerp(grass_mat.GetFloat("_Size"), Mathf.Max((peephole_target - peephole_origin).magnitude * 3.0f, peephole_min_size), 0.2f));
        }
        else
        {
            grass_mat.SetFloat("_Size", Mathf.Lerp(grass_mat.GetFloat("_Size"), peephole_min_size, 0.2f));
        }

        if (masked)
        {
            grass_mat.SetFloat("_Size", Mathf.Lerp(grass_mat.GetFloat("_Size"), 2.5f, 0.2f));
            player_mat.SetFloat("_Mask", Mathf.Lerp(player_mat.GetFloat("_Mask"), 1.0f, 0.2f));
        }
        else player_mat.SetFloat("_Mask", Mathf.Lerp(player_mat.GetFloat("_Mask"), 0.0f, 0.2f));
    }

}
