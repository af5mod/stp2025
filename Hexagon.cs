using System;
using System.Collections.Generic;
using System.Drawing;
using System.Composition;

namespace GraphicEditor
{
    public class Hexagon : IFigure
    {
        private List<PointF> vertices;
        public string Name => "Hexagon";

        public PointF Center
        {
            get
            {
                float sumX = 0, sumY = 0;
                foreach (var pt in vertices)
                {
                    sumX += pt.X;
                    sumY += pt.Y;
                }
                return new PointF(sumX / vertices.Count, sumY / vertices.Count);
            }
        }

        bool IFigure.IsSelected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        PointF IFigure.Center { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IFigure.Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        string IDrawingFigure.DrawingGeometry => throw new NotImplementedException();

        public Hexagon(List<PointF> points)
        {
            if (points == null || points.Count != 6)
                throw new ArgumentException("Hexagon requires exactly 6 vertices.");
            vertices = new List<PointF>(points);
        }

        public void Move(PointF vector)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = new PointF(vertices[i].X + vector.X, vertices[i].Y + vector.Y);
            }
        }

        public void Rotate(PointF rotationCenter, float angle)
        {
            double rad = angle * Math.PI / 180;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);

            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = RotatePoint(vertices[i], rotationCenter, cosA, sinA);
            }
        }

        private PointF RotatePoint(PointF pt, PointF center, double cosA, double sinA)
        {
            float dx = pt.X - center.X;
            float dy = pt.Y - center.Y;
            return new PointF(
                (float)(center.X + dx * cosA - dy * sinA),
                (float)(center.Y + dx * sinA + dy * cosA)
            );
        }

        public void Scale(float dx, float dy)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = new PointF(vertices[i].X * dx, vertices[i].Y * dy);
            }
        }

        public void Scale(PointF scaleCenter, float dr)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = new PointF(
                    scaleCenter.X + (vertices[i].X - scaleCenter.X) * dr,
                    scaleCenter.Y + (vertices[i].Y - scaleCenter.Y) * dr
                );
            }
        }

        public void Reflection(PointF a, PointF b)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = ReflectPoint(vertices[i], a, b);
            }
        }

        private PointF ReflectPoint(PointF p, PointF a, PointF b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float mag2 = dx * dx + dy * dy;
            if (mag2 == 0) return p;
            float t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / mag2;
            PointF proj = new PointF(a.X + t * dx, a.Y + t * dy);
            return new PointF(2 * proj.X - p.X, 2 * proj.Y - p.Y);
        }

        public IFigure Clone()
        {
            return new Hexagon(new List<PointF>(vertices));
        }

        public void Draw(IDrawing drawing) => throw new NotImplementedException();

        public bool IsIn(PointF point, float eps)
        {
            bool inside = false;
            int j = vertices.Count - 1;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (((vertices[i].Y > point.Y) != (vertices[j].Y > point.Y)) &&
                    (point.X < (vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) + vertices[i].X))
                    inside = !inside;
                j = i;
            }
            return inside;
        }

        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();

        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, PointF> pointParams)
        {
            if (pointParams != null && pointParams.Count == 6)
            {
                vertices = new List<PointF>(pointParams.Values);
            }
            else
            {
                throw new ArgumentException("Hexagon requires 6 point parameters.");
            }
        }

        void IFigure.RandomizeParameters() => throw new NotImplementedException();
    }
}
