using System;
using System.Collections.Generic;
using System.Composition;
using System.Drawing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GraphicEditor
{
    [Export(typeof(IFigure))]
    [ExportMetadata(nameof(FigureMetadata.Name), "Rectangle")]
    [ExportMetadata(nameof(FigureMetadata.IconPath), "/Assets/Rectangle.svg")]
    [ExportMetadata(nameof(FigureMetadata.NumberOfPointParameters), 2)]
    [ExportMetadata(nameof(FigureMetadata.NumberOfDoubleParameters), 2)]
    [ExportMetadata(nameof(FigureMetadata.PointParametersNames), new[] { "TopLeft", "BottomRight" })]
    [ExportMetadata(nameof(FigureMetadata.DoubleParametersNames), new[] { "Width", "Height" })]
    public class Rectangle : ReactiveObject, IFigure, IDrawingFigure
    {
        [Reactive] public bool IsSelected { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public PointF Center { get; set; }
        [Reactive] public PointF TopLeft { get; set; }
        [Reactive] public PointF BottomRight { get; set; }
        [Reactive] public float Width { get; set; }
        [Reactive] public float Height { get; set; }

        public string DrawingGeometry => 
            $"M{TopLeft.X},{TopLeft.Y} L{BottomRight.X},{TopLeft.Y} L{BottomRight.X},{BottomRight.Y} L{TopLeft.X},{BottomRight.Y} Z";

        public Rectangle()
        {
            RandomizeParameters();
            Name = "Rectangle";
            
            this.WhenAnyValue(x => x.TopLeft, x => x.BottomRight)
                .Subscribe(_ => UpdateCenter());
        }

        private void UpdateCenter()
        {
            Center = new PointF(
                (TopLeft.X + BottomRight.X) / 2,
                (TopLeft.Y + BottomRight.Y) / 2
            );
        }

        public void RandomizeParameters()
        {
            var x = Random.Shared.Next(100, 500);
            var y = Random.Shared.Next(100, 500);
            
            TopLeft = new PointF(x, y);
            BottomRight = new PointF(
                x + Random.Shared.Next(50, 200),
                y + Random.Shared.Next(50, 200)
            );
        }

        public void Move(PointF vector)
        {
            TopLeft = new PointF(TopLeft.X + vector.X, TopLeft.Y + vector.Y);
            BottomRight = new PointF(BottomRight.X + vector.X, BottomRight.Y + vector.Y);
        }

        public void Rotate(PointF rotationCenter, float angle)
        {
            var points = new[] { TopLeft, BottomRight };
            double rad = angle * Math.PI / 180;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = RotatePoint(points[i], rotationCenter, cosA, sinA);
            }
            
            TopLeft = points[0];
            BottomRight = points[1];
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
            Width *= dx;
            Height *= dy;
            BottomRight = new PointF(TopLeft.X + Width, TopLeft.Y + Height);
        }

        public void Scale(PointF scaleCenter, float dr)
        {
            var newWidth = Width * dr;
            var newHeight = Height * dr;
            TopLeft = new PointF(
                scaleCenter.X - (scaleCenter.X - TopLeft.X) * dr,
                scaleCenter.Y - (scaleCenter.Y - TopLeft.Y) * dr
            );
            BottomRight = new PointF(TopLeft.X + newWidth, TopLeft.Y + newHeight);
        }

        public IFigure Clone() => new Rectangle
        {
            TopLeft = this.TopLeft,
            BottomRight = this.BottomRight,
            Name = this.Name
        };

        public bool IsIn(PointF point, float eps) => 
            point.X >= TopLeft.X - eps && point.X <= BottomRight.X + eps &&
            point.Y >= TopLeft.Y - eps && point.Y <= BottomRight.Y + eps;

        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, PointF> pointParams)
        {
            if (pointParams != null)
            {
                TopLeft = pointParams["TopLeft"];
                BottomRight = pointParams["BottomRight"];
            }
            if (doubleParams != null)
            {
                Width = (float)doubleParams["Width"];
                Height = (float)doubleParams["Height"];
            }
        }
        public void Reflection(PointF a, PointF b) => throw new NotImplementedException();
        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();
        public void Draw(IDrawing drawing) => throw new NotImplementedException();
    }
}