using UnityEngine;
using System.Collections.Generic;

public class Point
{
    public List<Edge> edges = new List<Edge>();
    private Material material;

    private Vector3 position;
    public Vector3 Position => position;

    public Point(Vector3 position)
    {
        this.position = position;
    }

    public Edge FindEdge(Point target)
    {
        if (target == this)
            return null;

        foreach (var edge in edges)
        {
            if (edge.IsPointOfEdge(target))
                return edge;
        }

        return null;
    }
}