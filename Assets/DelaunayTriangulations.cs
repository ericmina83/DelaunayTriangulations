using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DelaunayTriangulations : MonoBehaviour
{
    Mesh mesh;
    public GameObject ball;
    private List<Vector3> vertices = new List<Vector3>();
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

                Instantiate(ball, newPoint, Quaternion.identity, gameObject.transform);

                vertices.Add(newPoint);
                vertices = (from vertex in vertices orderby vertex.x select vertex).ToList(); //vertices.OrderBy(vertex => vertex.x).ToList();

                lineHandler.ClearAllLines();

                Divide(0, vertices.Count() - 1);
            }
        }
    }

    bool IsPInCircle(Vector3 c1, Vector3 c2, Vector3 c3, Vector3 p)
    {
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


    List<Vector3> Divide(int l, int r)
    {
        List<Vector3> result = new List<Vector3>();

        if (l > r)
            return result;
        else if (r - l <= 1) // 1 or 2 points
        {
            if (r > l)
            {
                lineHandler.DrawLine(vertices[l], vertices[r], linesParent, Color.red);
            }

            result.AddRange(vertices.GetRange(l, r - l + 1));
        }
        else // over 2 points
        {
            int mid = (l + r) / 2;

            var lPoints = Divide(l, mid);
            var rPoints = Divide(mid + 1, r);

            result.AddRange(lPoints);
            result.AddRange(rPoints);

            Vector3 lPoint, rPoint;

            GetBaseLRLine(lPoints, rPoints, out lPoint, out rPoint);

            while (true)
            {
                var oldRPoint = rPoint;
                var oldLPoint = lPoint;

                foreach (var tmpRPoint in rPoints)
                {
                    if (tmpRPoint == oldRPoint)
                        continue;
                    else if (Vector3.SignedAngle(oldLPoint - oldRPoint, tmpRPoint - oldRPoint, Vector3.forward) > 0)
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
                    else if (Vector3.SignedAngle(oldRPoint - oldLPoint, tmpLPoint - oldLPoint, Vector3.forward) < 0)
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
                    break;

                // if (IsPInCircle(oldLPoint, oldRPoint, lPoint, rPoint))
                // {
                //     lPoint = oldLPoint;
                //     rPoints.Remove(rPoint);
                // }
                // else
                // {
                //     rPoint = oldRPoint;
                //     lPoints.Remove(lPoint);
                // }
                if (IsPInCircle(oldLPoint, oldRPoint, lPoint, rPoint))
                    lineHandler.DrawLine(oldLPoint, rPoint, linesParent, Color.red);
                else
                    lineHandler.DrawLine(oldRPoint, lPoint, linesParent, Color.red);
                lineHandler.DrawLine(lPoint, rPoint, linesParent, Color.red);
            }

        }

        return result;
    }

    void GetBaseLRLine(List<Vector3> lPoints, List<Vector3> rPoints, out Vector3 lPoint, out Vector3 rPoint)
    {
        lPoints = (from vertex in lPoints orderby vertex.z select vertex).ToList();
        rPoints = (from vertex in rPoints orderby vertex.z select vertex).ToList();

        lPoint = lPoints.First();
        rPoint = rPoints.First();

        foreach (var tmpRPoint in rPoints)
        {
            if (rPoint == tmpRPoint)
                continue;

            if (Vector3.SignedAngle(rPoint - lPoint, tmpRPoint - lPoint, Vector3.forward) < 0)
                rPoint = tmpRPoint;
        }

        foreach (var tmpLPoint in lPoints)
        {
            if (lPoint == tmpLPoint)
                continue;

            if (Vector3.SignedAngle(lPoint - rPoint, tmpLPoint - rPoint, Vector3.forward) > 0)
                lPoint = tmpLPoint;
        }

        lineHandler.DrawLine(lPoint, rPoint, linesParent, Color.red);
    }
}
