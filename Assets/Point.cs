using UnityEngine;
using System.Collections.Generic;

public class Point : MonoBehaviour
{
    public List<Edge> edges = new List<Edge>();
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

    public Edge FindEdge(Point target)
    {
        if (target == this)
            return null;

        foreach (var edge in edges)
        {
            if (edge.from == target || edge.to == target)
                return edge;
        }

        return null;
    }
}