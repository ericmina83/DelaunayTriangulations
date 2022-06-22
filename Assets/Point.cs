using UnityEngine;
using System.Collections.Generic;

public class Point : MonoBehaviour
{
    private Material material;

    public Vector3 position
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