using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeLogic : MonoBehaviour
{
    private EnemyController parent_controller;
    // Start is called before the first frame update
    void Start()
    {
        parent_controller = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit something!");
        if (other.CompareTag("Wall"))
        {
            if (CompareTag("Near_Cone"))
            {
                parent_controller.cone_flags |= ConeFlags.NEAR_CONE;
            }
            else if (CompareTag("Mid_Cone"))
            {
                parent_controller.cone_flags |= ConeFlags.MID_CONE;
            }
            else if (CompareTag("Far_Cone"))
            {
                parent_controller.cone_flags |= ConeFlags.FAR_CONE;
            }
            else if (CompareTag("Very_Far_Cone"))
            {
                parent_controller.cone_flags |= ConeFlags.VERY_FAR_CONE;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            if (CompareTag("Near_Cone"))
            {
                parent_controller.cone_flags &= ~ConeFlags.NEAR_CONE;
            }
            else if (CompareTag("Mid_Cone"))
            {
                parent_controller.cone_flags &= ~ConeFlags.MID_CONE;
            }
            else if (CompareTag("Far_Cone"))
            {
                parent_controller.cone_flags &= ~ConeFlags.FAR_CONE;
            }
            else if (CompareTag("Very_Far_Cone"))
            {
                parent_controller.cone_flags &= ~ConeFlags.VERY_FAR_CONE;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
