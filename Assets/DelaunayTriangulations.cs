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
                Instantiate(ball, hit.point, Quaternion.identity, gameObject.transform);
                vertices.Add(hit.point);
                vertices = (from vertex in vertices orderby vertex.x select vertex).ToList(); //vertices.OrderBy(vertex => vertex.x).ToList();

                lineHandler.ClearAllLines();

                Divide(0, vertices.Count() - 1);
            }
        }
    }


    List<Vector3> Divide(int l, int r)
    {
        if(l + 1 >= r)
        {
            if (r > l)
            {
                lineHandler.DrawLine(vertices[l], vertices[r], linesParent, Color.red);
            }

            return vertices.GetRange(l, r - l + 1);
        }

        int mid = (l + r) / 2;
        var lPoints = Divide(l, mid);
        var rPoints = Divide(mid + 1, r);


        GetBaseLRLine(lPoints, rPoints);

        lPoints.AddRange(rPoints);

        return lPoints;
    }

    void GetBaseLRLine(List<Vector3> lPoints, List<Vector3> rPoints)
    {
        var lPoint = (from vertex in lPoints orderby vertex.z select vertex).First();
        var rPoint = (from vertex in rPoints orderby vertex.z select vertex).First();

        lineHandler.DrawLine(lPoint, rPoint, linesParent, Color.red);
    }
}
