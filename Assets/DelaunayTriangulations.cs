using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DelaunayTriangulations : MonoBehaviour
{
    Mesh mesh;
    public Point point;
    private List<Point> points = new List<Point>();
    private List<Edge> edges = new List<Edge>();
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
                points = (from vertex in points orderby vertex.position.x select vertex).ToList(); //vertices.OrderBy(vertex => vertex.x).ToList();

                lineHandler.ClearAllLines();
                edges.Clear();

                Divide(0, points.Count() - 1);
            }
        }
    }

    bool IsPInCircle(Point c1p, Point c2p, Point c3p, Point pp)
    {
        Vector3 c1 = c1p.position;
        Vector3 c2 = c2p.position;
        Vector3 c3 = c3p.position;
        Vector3 p = pp.position;
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
                edges.Add(new Edge(points[l], points[r], lineHandler.DrawLine(points[l].position, points[r].position, linesParent, Color.red)));
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

            GetBaseLRLine(lPoints, rPoints, out Point lPoint, out Point rPoint);

            int roundCount = 0;

            while (true)
            {
                var oldRPoint = rPoint;
                var oldLPoint = lPoint;

                foreach (var tmpRPoint in rPoints)
                {
                    if (tmpRPoint == oldRPoint)
                        continue;
                    else if (Vector3.SignedAngle(
                        oldLPoint.position - oldRPoint.position,
                        tmpRPoint.position - oldRPoint.position,
                        Vector3.forward) > 0)
                        continue;


                    bool inCircle = false;
                    foreach (var p in rPoints)
                    {
                        if (p == oldRPoint || p == tmpRPoint)
                            continue;

                        if (IsPInCircle(tmpRPoint, oldRPoint, oldLPoint, p))
                        {
                            inCircle = true;
                            break;
                        }
                    }

                    if (!inCircle)
                    {
                        rPoint = tmpRPoint;
                        break;
                    }
                }

                foreach (var tmpLPoint in lPoints)
                {
                    if (tmpLPoint == oldLPoint)
                        continue;
                    else if (Vector3.SignedAngle(
                        oldRPoint.position - oldLPoint.position,
                        tmpLPoint.position - oldLPoint.position,
                        Vector3.forward) < 0)
                        continue;

                    bool inCircle = false;
                    foreach (var p in lPoints)
                    {
                        if (p == oldLPoint || p == tmpLPoint)
                            continue;

                        if (IsPInCircle(tmpLPoint, oldRPoint, oldLPoint, p))
                        {

                            inCircle = true;
                            break;
                        }
                    }

                    if (!inCircle)
                    {
                        lPoint = tmpLPoint;
                        break;
                    }
                }

                if (lPoint == oldLPoint && rPoint == oldRPoint)
                {
                    break;
                }
                else if (lPoint == oldLPoint || rPoint == oldRPoint)
                {

                }
                else
                {
                    if (IsPInCircle(oldLPoint, oldRPoint, lPoint, rPoint))
                    {
                        lPoint = oldLPoint;
                    }
                    else
                    {
                        rPoint = oldRPoint;
                    }
                }
                var newEdge = new Edge(lPoint, rPoint, lineHandler.DrawLine(lPoint.position, rPoint.position, linesParent, Color.red));
                foreach (var edge in new List<Edge>(edges))
                // foreach (var edge in  edges)
                {
                    if (newEdge.CheckIntersection(edge))
                    {
                        // edge.line.SetColor(Color.black);
                        edges.Remove(edge);
                        edge.line.DeleteSelf();
                    }
                }

                edges.Add(newEdge);
            }

            Debug.Log("end in round: " + roundCount);

        }

        return result;
    }

    void GetBaseLRLine(List<Point> lPoints, List<Point> rPoints, out Point lPoint, out Point rPoint)
    {
        lPoints = (from vertex in lPoints orderby vertex.position.y select vertex).ToList();
        rPoints = (from vertex in rPoints orderby vertex.position.y select vertex).ToList();

        lPoint = lPoints.First();
        rPoint = rPoints.First();

        // if (lPoint.position.y < rPoint.position.y)
        {
            foreach (var tmpRPoint in rPoints)
            {
                if (rPoint == tmpRPoint)
                    continue;

                if (Vector3.SignedAngle(rPoint.position - lPoint.position, tmpRPoint.position - lPoint.position, Vector3.forward) < 0)
                    rPoint = tmpRPoint;
            }
        }
        // else
        {
            foreach (var tmpLPoint in lPoints)
            {
                if (lPoint == tmpLPoint)
                    continue;

                if (Vector3.SignedAngle(lPoint.position - rPoint.position, tmpLPoint.position - rPoint.position, Vector3.forward) > 0)
                    lPoint = tmpLPoint;
            }
        }

        edges.Add(new Edge(lPoint, rPoint, lineHandler.DrawLine(lPoint.position, rPoint.position, linesParent, Color.red)));
    }
}
