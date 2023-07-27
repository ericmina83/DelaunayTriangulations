using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Test : MonoBehaviour
{
    private List<PointPrefab> points = new List<PointPrefab>();
    public PointPrefab point;
    public DrawHandler.LineHandler lineHandler;
    public MeshFilter meshFilter;

    static readonly string linesParent = "EdgeLines";

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        lineHandler = GetComponent<DrawHandler.LineHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                points.Add(Instantiate(point, hit.point, Quaternion.identity, gameObject.transform));
                points = (from vertex in points orderby vertex.Position.x select vertex).ToList();
                //vertices.OrderBy(vertex => vertex.x).ToList();

                lineHandler.ClearAllLines();

                meshFilter.mesh = DelaunayTriangulations.Triangulate(points.Select(point => point.Position).ToList());
            }
        }
    }
}