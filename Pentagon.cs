using System;
using System.Collections.Generic;
using System.Composition;
using System.Drawing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GraphicEditor
{
    [Export(typeof(IFigure))]
    [ExportMetadata(nameof(FigureMetadata.Name), "Pentagon")]
    [ExportMetadata(nameof(FigureMetadata.IconPath), "/Assets/Pentagon.svg")]
    [ExportMetadata(nameof(FigureMetadata.NumberOfPointParameters), 5)]
    [ExportMetadata(nameof(FigureMetadata.NumberOfDoubleParameters), 0)]
    [ExportMetadata(nameof(FigureMetadata.PointParametersNames), new[] { "V1", "V2", "V3", "V4", "V5" })]
    public class Pentagon : ReactiveObject, IFigure, IDrawingFigure
    {
        [Reactive] public bool IsSelected { get; set; }
        [Reactive] public string Name { get; set; }
        
        [Reactive]
        public List<PointF> Vertices { get; set; } = new List<PointF>(5);

        public PointF Center
        {
            get
            {
                float sumX = 0, sumY = 0;
                foreach (var pt in Vertices)
                {
                    sumX += pt.X;
                    sumY += pt.Y;
                }
                return new PointF(sumX / 5, sumY / 5);
            }
            set
            {
                var currentCenter = Center;
                var delta = new PointF(value.X - currentCenter.X, value.Y - currentCenter.Y);
                Move(delta);
            }
        }

        public string DrawingGeometry => 
            $"M{Vertices[0].X},{Vertices[0].Y} " +
            $"L{Vertices[1].X},{Vertices[1].Y} " +
            $"L{Vertices[2].X},{Vertices[2].Y} " +
            $"L{Vertices[3].X},{Vertices[3].Y} " +
            $"L{Vertices[4].X},{Vertices[4].Y} Z";

        // Пустой конструктор
        public Pentagon()
        {
            Name = "Pentagon";
            RandomizeParameters();
            this.WhenAnyValue(x => x.Vertices).Subscribe(_ => this.RaisePropertyChanged(nameof(Center)));
        }

        public void RandomizeParameters()
        {
            Vertices = new List<PointF>();
            for (int i = 0; i < 5; i++)
            {
                Vertices.Add(new PointF(
                    Random.Shared.Next(100, 500),
                    Random.Shared.Next(100, 500)
                ));
            }
        }

        public void Move(PointF vector)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = new PointF(
                    Vertices[i].X + vector.X,
                    Vertices[i].Y + vector.Y
                );
            }
        }

        public void Rotate(PointF rotationCenter, float angle)
        {
            double rad = angle * Math.PI / 180;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);

            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = RotatePoint(Vertices[i], rotationCenter, cosA, sinA);
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
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = new PointF(
                    Vertices[i].X * dx,
                    Vertices[i].Y * dy
                );
            }
        }

        public void Scale(PointF scaleCenter, float dr)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = new PointF(
                    scaleCenter.X + (Vertices[i].X - scaleCenter.X) * dr,
                    scaleCenter.Y + (Vertices[i].Y - scaleCenter.Y) * dr
                );
            }
        }

        public void Reflection(PointF a, PointF b)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = ReflectPoint(Vertices[i], a, b);
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

        public IFigure Clone() => new Pentagon { Vertices = new List<PointF>(Vertices) };

        public bool IsIn(PointF point, float eps)
        {
            int j = Vertices.Count - 1;
            bool inside = false;
            for (int i = 0; i < Vertices.Count; i++)
            {
                if (((Vertices[i].Y > point.Y) != (Vertices[j].Y > point.Y)) &&
                    (point.X < (Vertices[j].X - Vertices[i].X) * (point.Y - Vertices[i].Y) / 
                    (Vertices[j].Y - Vertices[i].Y) + Vertices[i].X))
                    inside = !inside;
                j = i;
            }
            return inside;
        }

        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, PointF> pointParams)
        {
            if (pointParams?.Count == 5)
            {
                Vertices = new List<PointF>
                {
                    pointParams["V1"],
                    pointParams["V2"],
                    pointParams["V3"],
                    pointParams["V4"],
                    pointParams["V5"]
                };
            }
            else throw new ArgumentException("Pentagon requires exactly 5 point parameters");
        }

        public void Draw(IDrawing drawing) => throw new NotImplementedException();

        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();
    }
}