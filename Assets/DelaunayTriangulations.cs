using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DelaunayTriangulations
{
    public List<Point> points = new List<Point>();
    public List<Edge> edges = new List<Edge>();
    public List<Triangle> triangles = new List<Triangle>();

    public static Mesh Triangulate(List<Vector3> positions)
    {
        var result = new DelaunayTriangulations(positions);
        return result.Algorithm();
    }

    public DelaunayTriangulations(List<Vector3> positions)
    {
        points = positions.Select((position) => new Point(position)).OrderBy((point => point.Position.x)).ToList();

        for (int i = 0; i < points.Count; i++)
        {
            var point1 = points[i];

            for (int j = i + 1; j < points.Count; j++)
            {
                var point2 = points[j];
                var edge = new Edge(point1, point2);
                edges.Add(edge);
            }
        }

        for (int i = 0; i < points.Count; i++)
        {
            var point1 = points[i];
            for (int j = i + 1; j < points.Count; j++)
            {
                var point2 = points[j];
                for (int k = j + 1; k < points.Count; k++)
                {
                    var point3 = points[k];
                    var triangle = new Triangle(point1, point2, point3);
                    triangles.Add(triangle);
                }
            }
        }

        Algorithm();
    }

    private Triangle FindTriangle(Point p1, Point p2, Point p3)
    {
        var triangle = FindEdge(p1, p2)?.FindTriangle(p3);
        triangle.points[0] = p1;
        triangle.points[1] = p2;
        triangle.points[2] = p3;
        return triangle;
    }

    private Edge FindEdge(Point p1, Point p2)
    {
        return p1.FindEdge(p2);
    }

    private Mesh Algorithm()
    {
        Divide(0, points.Count - 1);

        if (triangles.Count == 0)
        {
            return null;
        }

        var mesh = new Mesh();

        mesh.Clear();

        var vertices = points
            .Select((point) => point.Position)
            .ToArray();
        var indices = triangles
             .Where(triangle => triangle.enabled == true)
             .SelectMany(triangle => triangle.points.Select(point => points.IndexOf(point)))
             .ToArray();

        mesh.vertices = vertices;
        mesh.triangles = indices.ToArray();

        return mesh;
    }

    static public bool IsPInCircle(Point c1p, Point c2p, Point c3p, Point pp)
    {
        Vector3 c1 = c1p.Position;
        Vector3 c2 = c2p.Position;
        Vector3 c3 = c3p.Position;
        Vector3 p = pp.Position;
        // use elipse to solve, project 3 point to a elipse will get a 3D face
        // if p under the face means in circle, otherwise means out the circle
        c1.z = c1.x * c1.x + c1.y * c1.y;
        c2.z = c2.x * c2.x + c2.y * c2.y;
        c3.z = c3.x * c3.x + c3.y * c3.y;

        var normal = Vector3.Cross(c1 - c2, c3 - c2);

        if (Vector3.Dot(normal, Vector3.forward) < 0)
            normal = -normal;

        p.z = p.x * p.x + p.y * p.y;

        return Vector3.Dot(normal, p - c2) < 0; // < mean under the c1 c2 c3 face
    }

    private List<Point> Divide(int l, int r)
    {
        List<Point> result = new List<Point>();

        if (l > r)
            return result;
        else if (r - l <= 1) // 1 or 2 points
        {
            if (r > l)
            {
                FindEdge(points[l], points[r]).enabled = true;
            }

            result.AddRange(points.GetRange(l, r - l + 1));
        }
        else // over 2 points
        {
            int mid = (l + r) / 2;

            var lPoints = Divide(l, mid);
            var rPoints = Divide(mid + 1, r);

            result.AddRange(lPoints);
            result.AddRange(rPoints);

            var baseLine = GetBaseLRLine(ref lPoints, ref rPoints);
            baseLine.enabled = true;

            while (true)
            {
                var lPoint = baseLine.FindCandidatePoint(lPoints, false);
                var rPoint = baseLine.FindCandidatePoint(rPoints, true);

                Point thirdPoint = null;

                if (lPoint == baseLine.left && rPoint == baseLine.right)
                {
                    break;
                }
                else if (lPoint == baseLine.left || rPoint == baseLine.right)
                {
                    if (lPoint == baseLine.left)
                    {
                        thirdPoint = rPoint;
                    }
                    else
                    {
                        thirdPoint = lPoint;
                    }
                }
                else
                {
                    if (IsPInCircle(baseLine.right, baseLine.left, lPoint, rPoint))
                    {
                        lPoint = baseLine.left;
                        thirdPoint = rPoint;
                    }
                    else
                    {
                        rPoint = baseLine.right;
                        thirdPoint = lPoint;
                    }
                }

                FindTriangle(baseLine.right, baseLine.left, thirdPoint).enabled = true;
                baseLine = FindEdge(lPoint, rPoint);

                foreach (var edge in edges.Where(edge => edge.enabled == true).ToList())
                {
                    if (baseLine.CheckIntersection(edge))
                    {
                        edge.enabled = false;
                        foreach (var triangle in edge.triangles)
                        {
                            triangle.enabled = false;
                        }
                    }
                }

                baseLine.enabled = true;
            }
        }

        return result;
    }

    private Edge GetBaseLRLine(ref List<Point> lPoints, ref List<Point> rPoints)
    {
        lPoints = (from vertex in lPoints orderby vertex.Position.y select vertex).ToList();
        rPoints = (from vertex in rPoints orderby vertex.Position.y select vertex).ToList();

        var lPoint = lPoints.First();
        var rPoint = rPoints.First();

        // if (lPoint.position.y < rPoint.position.y)
        {
            foreach (var tmpRPoint in rPoints)
            {
                if (rPoint == tmpRPoint)
                    continue;

                if (Vector3.SignedAngle(rPoint.Position - lPoint.Position, tmpRPoint.Position - lPoint.Position, Vector3.forward) < 0)
                    rPoint = tmpRPoint;
            }
        }
        // else
        {
            foreach (var tmpLPoint in lPoints)
            {
                if (lPoint == tmpLPoint)
                    continue;

                if (Vector3.SignedAngle(lPoint.Position - rPoint.Position, tmpLPoint.Position - rPoint.Position, Vector3.forward) > 0)
                    lPoint = tmpLPoint;
            }
        }

        return FindEdge(lPoint, rPoint);
    }
}
