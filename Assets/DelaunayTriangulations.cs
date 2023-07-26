using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DelaunayTriangulations : MonoBehaviour
{
    Mesh mesh;
    public Point point;
    private List<Point> points = new List<Point>();
    private List<Edge> edges = new List<Edge>();
    private List<Triangle> triangles = new List<Triangle>();
    public DrawHandler.LineHandler lineHandler;

    static readonly string linesParent = "EdgeLines";

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh = new Mesh();
        lineHandler = GetComponent<DrawHandler.LineHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 newPoint = hit.point;

                points.Add(Instantiate(point, newPoint, Quaternion.identity, gameObject.transform));
                points = (from vertex in points orderby vertex.Position.x select vertex).ToList(); //vertices.OrderBy(vertex => vertex.x).ToList();

                lineHandler.ClearAllLines();
                edges.Clear();
                triangles.Clear();

                Divide(0, points.Count() - 1);


                List<int> indices = new List<int>();


                mesh.Clear();

                if (triangles.Count > 0)
                {
                    var vertices = points.Select((point) => point.Position).ToArray();
                    mesh.vertices = vertices;
                    foreach (var triangle in triangles)
                    {
                        foreach (var point in triangle.points)
                        {
                            indices.Add(points.IndexOf(point));
                        }
                    }
                    mesh.triangles = indices.ToArray();
                    // mesh.colors = points.Select((point) => Color.white).ToArray();

                    // mesh.RecalculateNormals();
                    // mesh.RecalculateTangents();
                }
            }
        }
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

    List<Point> Divide(int l, int r)
    {
        List<Point> result = new List<Point>();

        if (l > r)
            return result;
        else if (r - l <= 1) // 1 or 2 points
        {
            if (r > l)
            {
                edges.Add
                (
                    new Edge
                    (
                        points[l],
                        points[r],
                        lineHandler.DrawLine
                        (
                            points[l].Position,
                            points[r].Position,
                            linesParent,
                            Color.red
                        )
                    )
                );
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
            edges.Add(baseLine);

            while (true)
            {
                var lPoint = baseLine.FindCandidatePoint(lPoints, false);
                var rPoint = baseLine.FindCandidatePoint(rPoints, true);

                Point thirdPoint = null;

                if (lPoint == baseLine.Left && rPoint == baseLine.Right)
                {
                    break;
                }
                else if (lPoint == baseLine.Left || rPoint == baseLine.Right)
                {
                    if (lPoint == baseLine.Left)
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
                    if (IsPInCircle(baseLine.Right, baseLine.Left, lPoint, rPoint))
                    {
                        lPoint = baseLine.Left;
                        thirdPoint = rPoint;
                    }
                    else
                    {
                        rPoint = baseLine.Right;
                        thirdPoint = lPoint;
                    }
                }

                triangles.Add(new Triangle(baseLine.Right, baseLine.Left, thirdPoint));
                baseLine = new BaseLine(lPoint, rPoint, lineHandler.DrawLine(lPoint.Position, rPoint.Position, linesParent, Color.red));

                foreach (var edge in new List<Edge>(edges))
                // foreach (var edge in  edges)
                {
                    if (baseLine.CheckIntersection(edge))
                    {
                        // edge.line.SetColor(Color.black);
                        edges.Remove(edge);
                        foreach (var triangle in edge.triangles)
                        {
                            triangles.Remove(triangle);
                        }
                        edge.from.edges.Remove(edge);
                        edge.to.edges.Remove(edge);
                        edge.triangles.Clear();
                        edge.line.DeleteSelf();
                    }
                }

                edges.Add(baseLine);
            }
        }

        return result;
    }

    private BaseLine GetBaseLRLine(ref List<Point> lPoints, ref List<Point> rPoints)
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

        return new BaseLine(lPoint, rPoint, lineHandler.DrawLine(lPoint.Position, rPoint.Position, linesParent, Color.red));
    }
}
