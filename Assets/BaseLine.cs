using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class BaseLine : Edge
{
    public Point Right => this.to;
    public Point Left => this.from;

    public BaseLine(Point left, Point right, DrawHandler.Line line) : base(left, right, line)
    {

    }

    public Point FindCandidatePoint(List<Point> points, bool rightSide)
    {
        var sidePoint = (rightSide) ? Right : Left;

        foreach (var target in points)
        {
            if (target == sidePoint)
                continue;

            var angle = Vector3.SignedAngle(
                (rightSide ? Left : Right).Position - sidePoint.Position,
                target.Position - sidePoint.Position,
                Vector3.forward);

            if ((rightSide) ? angle > 0 : angle < 0)
                continue;

            bool inCircle = false;

            foreach (var p in points)
            {
                if (p == sidePoint || p == target)
                    continue;

                if (DelaunayTriangulations.IsPInCircle(target, Right, Left, p))
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
}