using System;
using System.Collections.Generic;
using System.Composition;
using System.Drawing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GraphicEditor.Models
{
    [Export(typeof(IFigure))]
    [ExportMetadata(nameof(FigureMetadata.Name), "Hexagon")]
    [ExportMetadata(nameof(FigureMetadata.IconPath), "/Assets/Hexagon.svg")]
    [ExportMetadata(nameof(FigureMetadata.NumberOfPointParameters), 6)]
    [ExportMetadata(nameof(FigureMetadata.NumberOfDoubleParameters), 0)]
    [ExportMetadata(nameof(FigureMetadata.PointParametersNames), new[] { "V1", "V2", "V3", "V4", "V5", "V6" })]
    public class Hexagon : ReactiveObject, IFigure, IDrawingFigure
    {
        [Reactive] public bool IsSelected { get; set; }
        [Reactive] public string Name { get; set; }
        
        [Reactive]
        public List<PointF> Vertices { get; set; } = new List<PointF>(6);

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
                return new(sumX / 6, sumY / 6);
            }
            set
            {
                var currentCenter = Center;
                var delta = new PointF(
                    value.X - currentCenter.X,
                    value.Y - currentCenter.Y
                );
                Move(delta);
            }
        }

        public string DrawingGeometry => 
            $"M{Vertices[0].X},{Vertices[0].Y} " +
            $"L{Vertices[1].X},{Vertices[1].Y} " +
            $"L{Vertices[2].X},{Vertices[2].Y} " +
            $"L{Vertices[3].X},{Vertices[3].Y} " +
            $"L{Vertices[4].X},{Vertices[4].Y} " +
            $"L{Vertices[5].X},{Vertices[5].Y} Z";

        public Hexagon()
        {
            Name = "Hexagon";
            RandomizeParameters();
            
            // Уведомление об изменении Center при изменении вершин
            this.WhenAnyValue(x => x.Vertices)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(Center)));
        }

        // Остальные методы остаются без изменений
        public void RandomizeParameters()
        {
            Vertices = new List<PointF>();

            // Случайные параметры: центр и радиус
            float centerX = Random.Shared.Next(100, 500);
            float centerY = Random.Shared.Next(100, 500);
            float radius = Random.Shared.Next(50, 200); // Радиус от 50 до 200

            // Угловой шаг между вершинами (360° / 6 = 60° или π/3 радиан)
            double angleStep = Math.PI / 3;

            // Начальный угол для ориентации шестиугольника
            double startAngle = -Math.PI / 2; // Одна из вершин направлена вверх

            for (int i = 0; i < 6; i++)
            {
                // Вычисляем угол для текущей вершины
                double angle = startAngle + i * angleStep;

                // Вычисляем координаты вершины
                float x = centerX + (float)(radius * Math.Cos(angle));
                float y = centerY + (float)(radius * Math.Sin(angle));

                // Добавляем вершину в список
                Vertices.Add(new PointF(x, y));
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

        public IFigure Clone() => new Hexagon { Vertices = new List<PointF>(Vertices) };

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
            if (pointParams?.Count == 6)
            {
                Vertices = new List<PointF>
                {
                    pointParams["V1"],
                    pointParams["V2"],
                    pointParams["V3"],
                    pointParams["V4"],
                    pointParams["V5"],
                    pointParams["V6"]
                };
            }
            else throw new ArgumentException("Hexagon requires exactly 6 point parameters");
        }

        public void Draw(IDrawing drawing) => throw new NotImplementedException();
        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();
    }
}
