using UnityEngine;
using System.Collections.Generic;

public class Edge
{
    public List<Triangle> triangles = new List<Triangle>();
    public bool enabled = false;

    public Point right;

    public Point left;

    public Edge(Point left, Point right)
    {
        this.left = left;
        this.right = right;
        this.left.edges.Add(this);
        this.right.edges.Add(this);
    }

    private bool ccw(Vector3 A, Vector3 B, Vector3 C)
    {
        return (C.y - A.y) * (B.x - A.x) > (B.y - A.y) * (C.x - A.x);
    }

    public bool CheckIntersection(Edge edge)
    {
        if (edge.left == left || edge.left == right)
            return false;
        else if (edge.right == left || edge.right == right)
            return false;

        return
            ccw(left.Position, right.Position, edge.left.Position) != ccw(left.Position, right.Position, edge.right.Position) &&
            ccw(edge.left.Position, edge.right.Position, left.Position) != ccw(edge.left.Position, edge.right.Position, right.Position);
    }

    public Point FindCandidatePoint(List<Point> points, bool rightSide)
    {
        var sidePoint = (rightSide) ? right : left;

        foreach (var target in points)
        {
            if (target == sidePoint)
                continue;

            var angle = Vector3.SignedAngle(
                (rightSide ? left : right).Position - sidePoint.Position,
                target.Position - sidePoint.Position,
                Vector3.forward);

            if ((rightSide) ? angle > 0 : angle < 0)
                continue;

            bool inCircle = false;

            foreach (var p in points)
            {
                if (p == sidePoint || p == target)
                    continue;

                if (DelaunayTriangulations.IsPInCircle(target, right, left, p))
                {
                    inCircle = true;
                    break;
                }
            }

            if (!inCircle)
            {
                return target;
            }
        }

        return sidePoint;
    }

    public bool IsPointOfEdge(Point point)
    {
        return left == point || right == point;
    }

    public Triangle FindTriangle(Point point)
    {
        if (point == left || point == right)
            return null;

        foreach (var triangle in triangles)
        {
            if (triangle.IsPointOfTriangle(point))
                return triangle;
        }

        return null;
    }
}