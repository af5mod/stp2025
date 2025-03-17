using System;
using System.Collections.Generic;
using System.Composition;
using System.Drawing;
using System.Net;

namespace GraphicEditor
{
    [Export(typeof(IFigure))]
    [ExportMetadata(nameof(FigureMetadata.Name), "Line")]
    [ExportMetadata(nameof(FigureMetadata.IconPath), "/Assets/Line.svg")]
    [ExportMetadata(nameof(FigureMetadata.NumberOfPointParameters), 2)]
    [ExportMetadata(nameof(FigureMetadata.NumberOfDoubleParameters), 0)]
    [ExportMetadata(nameof(FigureMetadata.PointParametersNames), new string[] { "Start", "End" })]
    public class Line : IFigure
    {
        public PointF Start { get; private set; }
        public PointF End { get; private set; }
        public PointF Center
        {
            get;
            set;
        }

        public bool IsSelected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get; set; }

        public string DrawingGeometry => throw new NotImplementedException();

        string IDrawingFigure.DrawingGeometry => $"M{Start.X},{Start.Y} L{End.X},{End.Y}";

        public Line()
        {
            // Инициализация по умолчанию (если требуется)
            Start = new PointF(1f, 1f);
            End = new PointF(2f, 2f);
            Name = "Line";
        }

        public Line(PointF start, PointF end)
        {
            Start = start;
            End = end;
        }

        public void Move(PointF vector)
        {
            Start = new PointF(Start.X + vector.X, Start.Y + vector.Y);
            End = new PointF(End.X + vector.X, End.Y + vector.Y);
        }

        public void Rotate(PointF center, float angle)
        {
            double rad = angle * Math.PI / 180;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);
            Start = new PointF(
                (float)(center.X + (Start.X - center.X) * cosA - (Start.Y - center.Y) * sinA),
                (float)(center.Y + (Start.X - center.X) * sinA + (Start.Y - center.Y) * cosA)
            );
            End = new PointF(
                (float)(center.X + (End.X - center.X) * cosA - (End.Y - center.Y) * sinA),
                (float)(center.Y + (End.X - center.X) * sinA + (End.Y - center.Y) * cosA)
            );
        }

        public void Scale(float dx, float dy)
        {
            Start = new PointF(Start.X * dx, Start.Y * dy);
            End = new PointF(End.X * dx, End.Y * dy);
        }

        public void Scale(PointF center, float dr)
        {
            Start = new PointF(center.X + (Start.X - center.X) * dr, center.Y + (Start.Y - center.Y) * dr);
            End = new PointF(center.X + (End.X - center.X) * dr, center.Y + (End.Y - center.Y) * dr);
        }

        public void Reflection(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float d = dx * dx + dy * dy;
            
            float x = ((Start.X * dx + Start.Y * dy - a.X * dx - a.Y * dy) * dx + a.X * d) / d * 2 - Start.X;
            float y = ((Start.X * dx + Start.Y * dy - a.X * dx - a.Y * dy) * dy + a.Y * d) / d * 2 - Start.Y;
            Start = new PointF { X = x, Y = y };
            x = ((End.X * dx + End.Y * dy - a.X * dx - a.Y * dy) * dx + a.X * d) / d * 2 - End.X;
            y = ((End.X * dx + End.Y * dy - a.X * dx - a.Y * dy) * dy + a.Y * d) / d * 2 - End.Y;
            End = new PointF { X = x, Y = y };
        }

        public IFigure Clone()
        {
            return new Line(new PointF(Start.X, Start.Y), new PointF(End.X, End.Y));
        }

        public IEnumerable<IDrawingFigure> GetAsDrawable() => throw new NotImplementedException();
        public bool IsIn(PointF point, float eps) => throw new NotImplementedException();
        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();
        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, PointF> pointParams)
        {
            Start = pointParams["Start"];
            End = pointParams["End"];
        }

        public void SetParameters(IDictionary<string, float> doubleParams, IDictionary<string, PointF> pointParams)
        {
            Start = pointParams["First"];
            End = pointParams["Second"];
        }

        public void Draw(IDrawing drawing) => throw new NotImplementedException();
        public void RandomizeParameters()
        {
            var startX = Random.Shared.Next(256);
            var startY = Random.Shared.Next(256);

            var endX = Random.Shared.Next(256);
            var endY = Random.Shared.Next(256);

            Start = new(startX, startY);
            End = new(endX, endY);
        }
    }
}
