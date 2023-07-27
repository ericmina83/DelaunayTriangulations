using UnityEngine;
using System.Collections.Generic;

public class PointPrefab : MonoBehaviour
{
    private Material material;

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    public void SetColor(Color color)
    {
        material = GetComponent<MeshRenderer>().material;
        material.color = color;
    }
}
