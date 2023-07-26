using UnityEngine;
using System.Collections.Generic;


public class Edge
{
    public List<Triangle> triangles = new List<Triangle>();
    public Point from;
    public Point to;
    public DrawHandler.Line line;

    public Edge(Point from, Point to, DrawHandler.Line line)
    {
        this.from = from;
        this.to = to;
        this.line = line;
        this.from.edges.Add(this);
        this.to.edges.Add(this);
    }

    private bool ccw(Vector3 A, Vector3 B, Vector3 C)
    {
        return (C.y - A.y) * (B.x - A.x) > (B.y - A.y) * (C.x - A.x);
    }

    public bool CheckIntersection(Edge edge)
    {
        if (edge.from == from || edge.from == to)
            return false;
        else if (edge.to == from || edge.to == to)
            return false;

        return
            ccw(from.Position, to.Position, edge.from.Position) != ccw(from.Position, to.Position, edge.to.Position) &&
            ccw(edge.from.Position, edge.to.Position, from.Position) != ccw(edge.from.Position, edge.to.Position, to.Position);
    }
}