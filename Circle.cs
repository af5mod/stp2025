using System;
using System.Collections.Generic;
using System.Composition;
using System.Drawing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GraphicEditor.Models
{
    [Export(typeof(IFigure))]
    [ExportMetadata(nameof(FigureMetadata.Name), "Circle")]
    [ExportMetadata(nameof(FigureMetadata.IconPath), "/Assets/Circle.svg")]
    [ExportMetadata(nameof(FigureMetadata.NumberOfPointParameters), 2)]
    [ExportMetadata(nameof(FigureMetadata.NumberOfDoubleParameters), 0)]
    [ExportMetadata(nameof(FigureMetadata.PointParametersNames), new string[] { "Center", "PointOnCircle" })]
    public class Circle : ReactiveObject, IFigure, IDrawingFigure
    {
        [Reactive] public PointF Center { get; set; }
        [Reactive] public PointF PointOnCircle { get; set; }
        [Reactive] public float PointOnCircleX { get; set; }
        [Reactive] public float PointOnCircleY { get; set; }
        [Reactive] public bool IsSelected { get; set; }
        [Reactive] public string Name { get; set; }
        private void InitBinding()
        {
            this.WhenAnyValue(o => o.PointOnCircleX, o => o.PointOnCircleY, (x, y) => new PointF(x, y))
                .Subscribe((x) =>
                {
                    PointOnCircle = x;
                    this.RaisePropertyChanged(nameof(DrawingGeometry));
                }
            );
            this.WhenAnyValue(o => o.Center).Subscribe(o => this.RaisePropertyChanged(nameof(DrawingGeometry)));
        }

        public string DrawingGeometry => $"M{Center.X + Radius},{Center.Y} A{Radius},{Radius},0,1,1,{Center.X - Radius},{Center.Y} A{Radius},{Radius},0,1,1,{Center.X + Radius},{Center.Y} z";
        public float Radius => Distance(Center, PointOnCircle);
        public Circle()
        {
            // Инициализация по умолчанию (если требуется)
            RandomizeParameters();
            Name = "Circle";
            InitBinding();
            // this.WhenAnyValue(x => x.Center).Subscribe(x => Console.WriteLine($"Circle's PointF Center changed: {x}"));

            // this.WhenAnyValue(x => x.Center.X, x => x.Center.Y, (x, y) => { Console.WriteLine($"Circle's x or y changed: x:{x}, y:{y}"); return new PointF(x, y); });
        }
        public Circle(PointF center, PointF pointOnCircle)
        {
            Center = center;
            PointOnCircle = pointOnCircle;
            PointOnCircleX = PointOnCircle.X;
            PointOnCircleY = PointOnCircle.Y;
            RandomizeParameters();
            Name = "Circle";
            InitBinding();
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
            return new Circle(new PointF(100, 100), new PointF(100, 200));
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
        public void RandomizeParameters()
        {
            var centerX = Random.Shared.Next(256);
            var centerY = Random.Shared.Next(256);

            var pointX = Random.Shared.Next(256);
            var pointY = Random.Shared.Next(256);

            Center = new PointF(centerX, centerY);
            PointOnCircle = new PointF(pointX, pointY);
        }
    }
}
