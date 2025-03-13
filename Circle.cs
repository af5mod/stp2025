using System;
using System.Collections.Generic;
using System.Composition;
using System.Drawing;

namespace GraphicEditor
{
    [Export(typeof(IFigure))]
    [ExportMetadata("Name", nameof(Circle))]
    public class Circle : IFigure
    {
        public PointF Center { get; set; }
        public PointF PointOnCircle { get; set; }
        public string Name => "Circle";

        public Circle(PointF center, PointF pointOnCircle)
        {
            Center = center;
            PointOnCircle = pointOnCircle;
        }

        public void Move(PointF vector)
        {
            Center = new PointF(Center.X + vector.X, Center.Y + vector.Y);
            PointOnCircle = new PointF(PointOnCircle.X + vector.X, PointOnCircle.Y + vector.Y);
        }

        public void Rotate(PointF rotationCenter, float angle)
        {
            double rad = angle * Math.PI / 180;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);
            Center = RotatePoint(Center, rotationCenter, cosA, sinA);
            PointOnCircle = RotatePoint(PointOnCircle, rotationCenter, cosA, sinA);
        }

        private PointF RotatePoint(PointF pt, PointF rotationCenter, double cosA, double sinA)
        {
            float dx = pt.X - rotationCenter.X;
            float dy = pt.Y - rotationCenter.Y;
            return new PointF(
                (float)(rotationCenter.X + dx * cosA - dy * sinA),
                (float)(rotationCenter.Y + dx * sinA + dy * cosA)
            );
        }

        public void Scale(float dx, float dy)
        {
            Center = new PointF(Center.X * dx, Center.Y * dy);
            PointOnCircle = new PointF(PointOnCircle.X * dx, PointOnCircle.Y * dy);
        }

        public void Scale(PointF scaleCenter, float dr)
        {
            Center = new PointF(scaleCenter.X + (Center.X - scaleCenter.X) * dr,
                                scaleCenter.Y + (Center.Y - scaleCenter.Y) * dr);
            PointOnCircle = new PointF(scaleCenter.X + (PointOnCircle.X - scaleCenter.X) * dr,
                                       scaleCenter.Y + (PointOnCircle.Y - scaleCenter.Y) * dr);
        }

        public void Reflection(PointF a, PointF b)
        {
            // Implement reflection logic if needed
        }

        public IFigure Clone()
        {
            return new Circle(new PointF(Center.X, Center.Y), new PointF(PointOnCircle.X, PointOnCircle.Y));
        }

        public IEnumerable<IDrawingFigure> GetAsDrawable() => throw new NotImplementedException();

        public bool IsIn(PointF point, float eps)
        {
            float radius = Distance(Center, PointOnCircle);
            float distToCenter = Distance(Center, point);
            return Math.Abs(distToCenter - radius) <= eps;
        }

        private float Distance(PointF p1, PointF p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();
        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, PointF> pointParams) => throw new NotImplementedException();
        public void Draw(IDrawing drawing) => throw new NotImplementedException();
        public void SetParameters(IDictionary<string, float> doubleParams, IDictionary<string, PointF> pointParams) => throw new NotImplementedException();
    }
}
