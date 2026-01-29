using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class WallPlacer : MonoBehaviour
{
    public SplineContainer splineContainer;
    public float density_mod;
    public float max_yaw;
    public GameObject[] prefabs;
    // Start is called before the first frame update
    void Start()
    {
        splineContainer = GetComponent<SplineContainer>();
        int upper_bound = prefabs.Length;
        foreach (var spline in splineContainer.Splines)
        {
            int density = (int)(spline.GetLength() * density_mod + 1);
            for (int i = 0; i < density; i++)
            {
                float t = (float)i / (density - 1);
                Vector3 position = spline.EvaluatePosition(t);
                Vector3 tangent = spline.EvaluateTangent(t);
                float yaw = Random.Range(-max_yaw, max_yaw);
                int random_index = Random.Range(0, upper_bound);
                if (i == 0 || i == density - 1) random_index = 1;
                Quaternion rotation = Quaternion.LookRotation(tangent);
                rotation.eulerAngles += new Vector3(0.0f, Random.Range(-max_yaw, max_yaw), 0.0f);
                if (random_index == 0) i++;
                Instantiate(prefabs[random_index], transform.position + position, rotation, transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
