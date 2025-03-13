using System;
using System.Collections.Generic;
using System.Drawing;

namespace GraphicEditor
{
    public class Rectangle : IFigure
    {
        private List<PointF> corners;

        public Rectangle(PointF topLeft, float width, float height)
        {
            corners = new List<PointF>
            {
                topLeft,
                new PointF(topLeft.X + width, topLeft.Y),
                new PointF(topLeft.X + width, topLeft.Y + height),
                new PointF(topLeft.X, topLeft.Y + height)
            };
        }

        public Rectangle(List<PointF> corners)
        {
            if (corners == null || corners.Count != 4)
                throw new ArgumentException("Для прямоугольника необходимо задать 4 вершины.");
            this.corners = new List<PointF>(corners);
        }

        public string Name => "Rectangle";

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
            return new Rectangle(new List<PointF>(corners));
        }

        public void Draw(IDrawing drawing) => throw new NotImplementedException();

        public bool IsIn(PointF point, float eps)
        {
            float minX = Math.Min(corners[0].X, corners[2].X);
            float maxX = Math.Max(corners[0].X, corners[2].X);
            float minY = Math.Min(corners[0].Y, corners[2].Y);
            float maxY = Math.Max(corners[0].Y, corners[2].Y);

            return point.X >= minX - eps && point.X <= maxX + eps &&
                   point.Y >= minY - eps && point.Y <= maxY + eps;
        }

        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();

        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, PointF> pointParams)
        {
            if (pointParams != null &&
                pointParams.ContainsKey("TopLeft") &&
                pointParams.ContainsKey("TopRight") &&
                pointParams.ContainsKey("BottomRight") &&
                pointParams.ContainsKey("BottomLeft"))
            {
                corners[0] = pointParams["TopLeft"];
                corners[1] = pointParams["TopRight"];
                corners[2] = pointParams["BottomRight"];
                corners[3] = pointParams["BottomLeft"];
            }
            else
            {
                throw new ArgumentException("Для Rectangle необходимы параметры 'TopLeft', 'TopRight', 'BottomRight' и 'BottomLeft'.");
            }
        }

        void IFigure.RandomizeParameters() => throw new NotImplementedException();
    }
}
