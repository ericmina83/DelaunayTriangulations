using UnityEngine;


public class Edge
{
    public Point from;
    public Point to;
    public DrawHandler.Line line;

    public Edge(Point from, Point to, DrawHandler.Line line)
    {
        this.from = from;
        this.to = to;
        this.line = line;
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
            ccw(from.position, to.position, edge.from.position) != ccw(from.position, to.position, edge.to.position) &&
            ccw(edge.from.position, edge.to.position, from.position) != ccw(edge.from.position, edge.to.position, to.position);
    }
}