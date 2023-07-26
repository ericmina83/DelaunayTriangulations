public class Triangle
{
    public Point[] points = new Point[3];

    public Triangle(Point p1, Point p2, Point p3)
    {
        points[0] = p1;
        points[1] = p2;
        points[2] = p3;
        p1.FindEdge(p2)?.triangles.Add(this);
        p2.FindEdge(p3)?.triangles.Add(this);
        p3.FindEdge(p1)?.triangles.Add(this);
    }
}
