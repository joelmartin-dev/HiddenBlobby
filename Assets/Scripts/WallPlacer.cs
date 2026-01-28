using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class WallPlacer : MonoBehaviour
{
    public SplineContainer splineContainer;
    public int totalPlacedObjects;
    public GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {
        splineContainer = GetComponent<SplineContainer>();
        totalPlacedObjects = 10;
        var spline = splineContainer.Splines[1];
        for (int i = 0; i < totalPlacedObjects; i++)
        {
            float t = (float) i / (totalPlacedObjects - 1);
            Vector3 position = spline.EvaluatePosition(t);
            Vector3 tangent = spline.EvaluateTangent(t);
            Quaternion rotation = Quaternion.LookRotation(tangent);
            Instantiate(prefab, position, rotation, transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
