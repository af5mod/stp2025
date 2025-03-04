using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Drawing;

namespace GraphicEditor
{
    public class FigureMetadata
    {
        public string Name { get; }
        public int NumberOfDoubleParameters { get; }
        public int NumberOfPointParameters { get; }
        public IEnumerable<string> PointParametersNames { get; }
        public IEnumerable<string> DoubleParametersNames { get; }
    }

    public static class FigureFabric
    {
        class ImportInfo
        {
            [ImportMany] public IEnumerable<Lazy<IFigure, FigureMetadata>> AvailableFigures { get; set; } = [];
        }

        static ImportInfo info;

        static FigureFabric()
        {
            var assemblies = new[] { typeof(Circle).Assembly };
            var conf = new ContainerConfiguration();
            try
            {
                conf = conf.WithAssemblies(assemblies);
            }
            catch (Exception)
            {
                // ignored
            }

            var cont = conf.CreateContainer();
            info = new ImportInfo();
            cont.SatisfyImports(info);
        }

        public static IEnumerable<string> AvailableFigures => info.AvailableFigures.Select(f => f.Metadata.Name);
        public static IEnumerable<FigureMetadata> AvailableMetadata => info.AvailableFigures.Select(f => f.Metadata);

        public static IFigure CreateFigure(string FigureName)
        {
            return info.AvailableFigures.First(f => f.Metadata.Name == FigureName).Value;
        }
    }

    [Export(typeof(IFigure))]
    [ExportMetadata(nameof(FigureMetadata.Name), nameof(Line))]
    [ExportMetadata(nameof(FigureMetadata.NumberOfPointParameters), 2)]
    [ExportMetadata(nameof(FigureMetadata.NumberOfDoubleParameters), 0)]
    [ExportMetadata(nameof(FigureMetadata.PointParametersNames), new string[] { "First", "Second" })]
    public class Line : IFigure
    {
        public PointF Start { get; private set; }
        public PointF End { get; private set; }
        public PointF Center => new PointF((Start.X + End.X) / 2, (Start.Y + End.Y) / 2);

        public string Name => throw new NotImplementedException();

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
            Start = new PointF(Start.X * (float)dx, Start.Y * (float)dy);
            End = new PointF(End.X * (float)dx, End.Y * (float)dy);
        }

        public void Scale(PointF center, float dr)
        {
            Start = new PointF(center.X + (Start.X - center.X) * (float)dr,
                center.Y + (Start.Y - center.Y) * (float)dr);
            End = new PointF(center.X + (End.X - center.X) * (float)dr, center.Y + (End.Y - center.Y) * (float)dr);
        }

        public void Reflection(PointF a, PointF b) => throw new NotImplementedException();

        public IFigure Clone()
        {
            return new Line(new PointF(Start.X, Start.Y), new PointF(End.X, End.Y));
        }

        public IEnumerable<IDrawingFigure> GetAsDrawable() => throw new NotImplementedException();
        public bool IsIn(PointF point, float eps) => throw new NotImplementedException();
        public IFigure Intersect(IFigure other) => throw new NotImplementedException();
        public IFigure Union(IFigure other) => throw new NotImplementedException();
        public IFigure Subtract(IFigure other) => throw new NotImplementedException();
        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, PointF> pointParams) => throw new NotImplementedException();

        public void SetParameters(IDictionary<string, float> doubleParams, IDictionary<string, PointF> pointParams)
        {
            Start = pointParams["First"];
            End = pointParams["Second"];
        }

        public void Draw(IDrawing drawing) => throw new NotImplementedException();
    }

    [Export(typeof(IFigure))]
    [ExportMetadata("Name", nameof(Circle))]
    public class Circle : IFigure
    {
        private PointF center;
        private PointF pointOnCircle;
        public PointF Center => center;
        public string Name => "Circle";
        public Circle(PointF center, PointF pointOnCircle)
        {
            Center = center;
            PointOnCircle = pointOnCircle;
        }
        public void Move(PointF vector)
        {
            center = new PointF(center.X + vector.X, center.Y + vector.Y);
            pointOnCircle = new PointF(pointOnCircle.X + vector.X, pointOnCircle.Y + vector.Y);
        }
         public void Rotate(PointF rotationCenter, float angle)
        {
            double rad = angle * Math.PI / 180;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);
            center = RotatePoint(center, rotationCenter, cosA, sinA);
            pointOnCircle = RotatePoint(pointOnCircle, rotationCenter, cosA, sinA);
        }

        public void Scale(float dx, float dy)
        {
            center = new PointF(center.X * dx, center.Y * dy);
            pointOnCircle = new PointF(pointOnCircle.X * dx, pointOnCircle.Y * dy);
        }
        public void Scale(PointF scaleCenter, float dr)
        {
            center = new PointF(scaleCenter.X + (center.X - scaleCenter.X) * dr,
                                scaleCenter.Y + (center.Y - scaleCenter.Y) * dr);
            pointOnCircle = new PointF(scaleCenter.X + (pointOnCircle.X - scaleCenter.X) * dr,
                                       scaleCenter.Y + (pointOnCircle.Y - scaleCenter.Y) * dr);
        }
        public void Reflection(PointF a, PointF b)
        {
            center = ReflectPoint(center, a, b);
            pointOnCircle = ReflectPoint(pointOnCircle, a, b);
        }
        public IFigure Clone()
        {
            return new Circle(new PointF(center.X, center.Y), new PointF(pointOnCircle.X, pointOnCircle.Y));
        }
        public IEnumerable<IDrawingFigure> GetAsDrawable() => throw new NotImplementedException();
        public bool IsIn(PointF point, float eps)
        {
            float radius = Distance(center, pointOnCircle);
            float distToCenter = Distance(center, point);
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

        public void SetParameters(IDictionary<string, float> doubleParams, IDictionary<string, PointF> pointParams) =>
            throw new NotImplementedException();
    }
}