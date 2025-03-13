using System;
using System.Collections.Generic;
using System.Drawing;

namespace GraphicEditor
{
    public class Triangle: IFigure
    {
        private List<PointF> corners;

        public Triangle(PointF vertex1, PointF vertex2, PointF vertex3)
        {
            corners = new List<PointF> {vertex1, vertex2, vertex3};
        }

        public Triangle(List<PointF> corners)
        {
            if (corners == null || corners.Count != 3)
                throw new ArgumentException("Для треугольника необходимо задать 3 вершины.");
            this.corners = new List<PointF>(corners);
        }

        public string Name => "Triangle";

        public PointF Center
        {
            get
            {
                float sumX = 0, sumY = 0;
                foreach (var pt in corners)
                {
                    sumX += pt.X;
                    sumY += pt.Y;
                }
                return new PointF(sumX / corners.Count, sumY / corners.Count);
            }
        }

        bool IFigure.IsSelected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        PointF IFigure.Center { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IFigure.Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        string IDrawingFigure.DrawingGeometry => throw new NotImplementedException();

        public void Move(PointF vector)
        {
            for (int i = 0; i < corners.Count; i++)
            {
                corners[i] = new PointF(corners[i].X + vector.X, corners[i].Y + vector.Y);
            }
        }
        
        public void Rotate(PointF rotationCenter, float angle)
        {
            double rad = angle * Math.PI / 180;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);

            for (int i = 0; i < corners.Count; i++)
            {
                corners[i] = RotatePoint(corners[i], rotationCenter, cosA, sinA);
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
            for (int i = 0; i < corners.Count; i++)
            {
                corners[i] = new PointF(corners[i].X * dx, corners[i].Y * dy);
            }
        }
        
        public void Scale(PointF scaleCenter, float dr)
        {
            for (int i = 0; i < corners.Count; i++)
            {
                float newX = scaleCenter.X + (corners[i].X - scaleCenter.X) * dr;
                float newY = scaleCenter.Y + (corners[i].Y - scaleCenter.Y) * dr;
                corners[i] = new PointF(newX, newY);
            }
        }

        public void Reflection(PointF a, PointF b)
        {
            for (int i = 0; i < corners.Count; i++)
            {
                corners[i] = ReflectPoint(corners[i], a, b);
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
            return new Triangle(new List<PointF>(corners));
        }

        public void Draw(IDrawing drawing) => throw new NotImplementedException();

        public bool IsIn(PointF point, float eps)
        {
            bool inside = false;
            int j = corners.Count - 1;
            for (int i = 0; i < corners.Count; i++)
            {
                if (((corners[i].Y > point.Y) != (corners[j].Y > point.Y)) &&
                    (point.X < (corners[j].X - corners[i].X) * (point.Y - corners[i].Y) / (corners[j].Y - corners[i].Y) + corners[i].X))
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
            if (pointParams != null &&
                pointParams.ContainsKey("Vertex1") &&
                pointParams.ContainsKey("Vertex2") &&
                pointParams.ContainsKey("Vertex3"))
            {
                corners[0] = pointParams["Vertex1"];
                corners[1] = pointParams["Vertex2"];
                corners[2] = pointParams["Vertex3"];
            }
            else
            {
                throw new ArgumentException("Для Triangle необходимы параметры 'Vertex1', 'Vertex2' и 'Vertex3'.");
            }
        }

        void IFigure.RandomizeParameters() => throw new NotImplementedException();
    }
}
