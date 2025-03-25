using System;
using System.Collections.Generic;
using System.Composition;
using System.Drawing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GraphicEditor.Models
{
    [Export(typeof(IFigure))]
    [ExportMetadata(nameof(FigureMetadata.Name), "Triangle")]
    [ExportMetadata(nameof(FigureMetadata.IconPath), "/Assets/Triangle.svg")]
    [ExportMetadata(nameof(FigureMetadata.NumberOfPointParameters), 3)]
    [ExportMetadata(nameof(FigureMetadata.NumberOfDoubleParameters), 0)]
    [ExportMetadata(nameof(FigureMetadata.PointParametersNames), new[] { "Vertex1", "Vertex2", "Vertex3" })]
    public class Triangle : ReactiveObject, IFigure, IDrawingFigure
    {
        [Reactive] public bool IsSelected { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive]
        public List<PointF> Corners { get; set; }
        [Reactive] public float Point0X { get; set; }
        [Reactive] public float Point0Y { get; set; }
        [Reactive] public float Point1X { get; set; }
        [Reactive] public float Point1Y { get; set; }
        [Reactive] public float Point2X { get; set; }
        [Reactive] public float Point2Y { get; set; }
        [Reactive]
        public Avalonia.Media.Color Color { get; set; }
        public Triangle()
        {
            Corners = new List<PointF>();
            RandomizeParameters();
            Name = "Triangle";
            Point0X = Corners[0].X;
            Point1X = Corners[1].X;
            Point2X = Corners[2].X;

            Point0Y = Corners[0].Y;
            Point1Y = Corners[1].Y;
            Point2Y = Corners[2].Y;


            this.WhenAnyValue(o => o.Point0X, (x) => new PointF(x, Corners[0].Y))
                .Subscribe((x) =>
                {
                    Corners[0] = x;
                    this.RaisePropertyChanged(nameof(DrawingGeometry));
                }
            );

            this.WhenAnyValue(o => o.Point0Y, (x) => new PointF(Corners[0].X, x))
                .Subscribe((x) =>
                {
                    Corners[0] = x;
                    this.RaisePropertyChanged(nameof(DrawingGeometry));
                }
            );

            this.WhenAnyValue(o => o.Point1X, (x) => new PointF(x, Corners[1].Y))
                .Subscribe((x) =>
                {
                    Corners[1] = x;
                    this.RaisePropertyChanged(nameof(DrawingGeometry));
                }
            );

            this.WhenAnyValue(o => o.Point1Y, (x) => new PointF(Corners[1].X, x))
                .Subscribe((x) =>
                {
                    Corners[1] = x;
                    this.RaisePropertyChanged(nameof(DrawingGeometry));
                }
            );

            this.WhenAnyValue(o => o.Point2X, (x) => new PointF(x, Corners[2].Y))
                .Subscribe((x) =>
                {
                    Corners[2] = x;
                    this.RaisePropertyChanged(nameof(DrawingGeometry));
                }
            );

            this.WhenAnyValue(o => o.Point2Y, (x) => new PointF(Corners[2].X, x))
                .Subscribe((x) =>
                {
                    Corners[2] = x;
                    this.RaisePropertyChanged(nameof(DrawingGeometry));
                }
            );

            this.WhenAnyValue(x => x.Corners)
                .Subscribe(o =>
                {
                    this.RaisePropertyChanged(nameof(Center));

                    if (Corners.Count == 0) return;
                    Point0X = Corners[0].X;
                    Point1X = Corners[1].X;
                    Point2X = Corners[2].X;

                    Point0Y = Corners[0].Y;
                    Point1Y = Corners[1].Y;
                    Point2Y = Corners[2].Y;
                });
        }

        public Triangle(PointF vertex1, PointF vertex2, PointF vertex3) : this()
        {
            Corners = new List<PointF> { vertex1, vertex2, vertex3 };
        }

        public Triangle(List<PointF> corners) : this()
        {
            if (corners == null || corners.Count != 3)
                throw new ArgumentException("Для треугольника необходимо задать 3 вершины.");
            this.Corners = new List<PointF>(corners);
        }

        public string DrawingGeometry =>
            $"M{Corners[0].X},{Corners[0].Y} L{Corners[1].X},{Corners[1].Y} L{Corners[2].X},{Corners[2].Y} Z";

        public PointF Center
        {
            get
            {
                float sumX = Corners[0].X + Corners[1].X + Corners[2].X;
                float sumY = Corners[0].Y + Corners[1].Y + Corners[2].Y;
                return new PointF(sumX / 3, sumY / 3);
            }
            set
            {
                var currentCenter = new PointF(
                    (Corners[0].X + Corners[1].X + Corners[2].X) / 3,
                    (Corners[0].Y + Corners[1].Y + Corners[2].Y) / 3
                );

                var offsetX = value.X - currentCenter.X;
                var offsetY = value.Y - currentCenter.Y;

                List<PointF> newCorners = [];


                newCorners.Add(new PointF(Corners[0].X + offsetX, Corners[0].Y + offsetY));
                newCorners.Add(new PointF(Corners[1].X + offsetX, Corners[1].Y + offsetY));
                newCorners.Add(new PointF(Corners[2].X + offsetX, Corners[2].Y + offsetY));

                Corners = newCorners;

                this.RaisePropertyChanged(nameof(Center));
            }
        }
        public void RandomizeParameters()
        {
            Color = Avalonia.Media.Color.FromRgb((byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256));
            Corners = new List<PointF>
            {
                new PointF(Random.Shared.Next(400), Random.Shared.Next(400)),
                new PointF(Random.Shared.Next(400), Random.Shared.Next(400)),
                new PointF(Random.Shared.Next(400), Random.Shared.Next(400))
            };
        }
        public void Move(PointF vector)
        {
            for (int i = 0; i < Corners.Count; i++)
            {
                Corners[i] = new PointF(Corners[i].X + vector.X, Corners[i].Y + vector.Y);
            }
        }

        public void Rotate(PointF rotationCenter, float angle)
        {
            double rad = angle * Math.PI / 180;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);

            for (int i = 0; i < Corners.Count; i++)
            {
                Corners[i] = RotatePoint(Corners[i], rotationCenter, cosA, sinA);
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
            for (int i = 0; i < Corners.Count; i++)
            {
                Corners[i] = new PointF(Corners[i].X * dx, Corners[i].Y * dy);
            }
        }

        public void Scale(PointF scaleCenter, float dr)
        {
            for (int i = 0; i < Corners.Count; i++)
            {
                float newX = scaleCenter.X + (Corners[i].X - scaleCenter.X) * dr;
                float newY = scaleCenter.Y + (Corners[i].Y - scaleCenter.Y) * dr;
                Corners[i] = new PointF(newX, newY);
            }
        }

        public void Reflection(PointF a, PointF b)
        {
            for (int i = 0; i < Corners.Count; i++)
            {
                Corners[i] = ReflectPoint(Corners[i], a, b);
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
            return new Triangle(new List<PointF>(Corners));
        }

        public void Draw(IDrawing drawing) => throw new NotImplementedException();

        public bool IsIn(PointF point, float eps)
        {
            bool inside = false;
            int j = Corners.Count - 1;
            for (int i = 0; i < Corners.Count; i++)
            {
                if (((Corners[i].Y > point.Y) != (Corners[j].Y > point.Y)) &&
                    (point.X < (Corners[j].X - Corners[i].X) * (point.Y - Corners[i].Y) / (Corners[j].Y - Corners[i].Y) + Corners[i].X))
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

                List<PointF> newCorners = [];

                newCorners.Add(pointParams["Vertex1"]);
                newCorners.Add(pointParams["Vertex2"]);
                newCorners.Add(pointParams["Vertex3"]);

                Corners = newCorners;
            }
            else
            {
                throw new ArgumentException("Для Triangle необходимы параметры 'Vertex1', 'Vertex2' и 'Vertex3'.");
            }
        }

        public void SetPosition(PointF vector)
        {
            Center = new PointF(vector.X, vector.Y);
        }
    }
}
