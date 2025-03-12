using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Drawing;
using GraphicEditor;
using System.Collections;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GraphicEditor
{
    public class FigureMetadata
    {
        public string Name { get; set; }
        public int NumberOfDoubleParameters { get; set; }
        public int NumberOfPointParameters { get; set; }
        public IEnumerable<string> PointParametersNames { get; set; }
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
            var assemblies = new[] { typeof(IFigure).Assembly };
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

            foreach (var item in info.AvailableFigures)
            {
                Console.WriteLine(item.Value);
            }
        }

        public static IEnumerable<string> AvailableFigures => info.AvailableFigures.Select(f => f.Metadata.Name);

        public static IEnumerable<FigureMetadata> AvailableMetadata => info.AvailableFigures.Select(f => f.Metadata);

        public static IFigure CreateFigure(string FigureName)
        {
            var lazyFigure = info.AvailableFigures.FirstOrDefault(f =>
                string.Equals(f.Metadata.Name, FigureName, StringComparison.OrdinalIgnoreCase)
                ) ?? throw new ArgumentException($"Фигура '{FigureName}' не зарегистрирована.");

            var figure = lazyFigure.Value.Clone();

            figure.Name = $"{FigureName} {Random.Shared.Next(100)}";
            figure.RandomizeParameters();

            return figure;
        }
    }

    [Export(typeof(IFigure))]
    [ExportMetadata(nameof(FigureMetadata.Name), "Line")]
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
            Name = "Line";
            Center = new PointF { X = (Start.X + End.X) / 2, Y = (Start.Y + End.Y) / 2 };
        }
        public bool IsSelected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get; set; }

        public string DrawingGeometry => throw new NotImplementedException();

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
        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, PointF> pointParams)
        {
            Start = pointParams["Start"];
            End = pointParams["End"];
        }

        public void SetParameters(IDictionary<string, float> doubleParams, IDictionary<string, PointF> pointParams)
        {
            Start = pointParams["Start"];
            End = pointParams["End"];
        }

        public void Draw(IDrawing drawing) => throw new NotImplementedException();
        public bool HitTest(Point point, double epsilon) => throw new NotImplementedException();
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

    [Export(typeof(IFigure))]
    [ExportMetadata(nameof(FigureMetadata.Name), "Circle")]
    [ExportMetadata(nameof(FigureMetadata.NumberOfPointParameters), 2)]
    [ExportMetadata(nameof(FigureMetadata.NumberOfDoubleParameters), 0)]
    [ExportMetadata(nameof(FigureMetadata.PointParametersNames), new string[] { "Center", "PointOnCircle" })]
    public class Circle : ReactiveObject, IFigure, IDrawingFigure
    {
        [Reactive] public PointF Center { get; set; }
        [Reactive] public PointF PointOnCircle { get; set; }

        private void InitBinding()
        {

            this.WhenAnyValue(o => o.Center).Subscribe(o => this.RaisePropertyChanged(nameof(DrawingGeometry)));
        }

        public Circle()
        {
            // Инициализация по умолчанию (если требуется)
            RandomizeParameters();
            Name = "Circle";
            InitBinding();
            // this.WhenAnyValue(x => x.Center).Subscribe(x => Console.WriteLine($"Circle's PointF Center changed: {x}"));

            // this.WhenAnyValue(x => x.Center.X, x => x.Center.Y, (x, y) => { Console.WriteLine($"Circle's x or y changed: x:{x}, y:{y}"); return new PointF(x, y); });
        }
        [Reactive] public bool IsSelected { get; set; }
        [Reactive] public string Name { get; set; }
        public string DrawingGeometry => $"M{Center.X + Radius},{Center.Y} A{Radius},{Radius},0,1,1,{Center.X - Radius},{Center.Y} A{Radius},{Radius},0,1,1,{Center.X + Radius},{Center.Y} z";
        public float Radius => Distance(Center, PointOnCircle);
        public Circle(PointF center, PointF pointOnCircle)
        {
            Center = center;
            PointOnCircle = pointOnCircle;
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

        public PointF RotatePoint(PointF pt, PointF rotationCenter, double cosA, double sinA)
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
            //Center = ReflectPoint(Center, a, b);
            //PointOnCircle = ReflectPoint(PointOnCircle, a, b);
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
        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, PointF> pointParams)
        {
            Center = pointParams["Center"];
            PointOnCircle = pointParams["PointOnCircle"];
        }

        public void Draw(IDrawing drawing) => throw new NotImplementedException();

        public void SetParameters(IDictionary<string, float> doubleParams, IDictionary<string, PointF> pointParams) =>
            throw new NotImplementedException();
        public bool HitTest(Point point, double epsilon) => throw new NotImplementedException();
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


    public class Rectangle : IFigure
    {

        private List<PointF> corners;

        private float _width;
        private float _height;

        public Rectangle(PointF topLeft, float width, float height) // конструктор по левому верхнему углу, ширине и высоте
        {
            _width = width;
            _height = height;
            corners = new List<PointF>
            {
                topLeft,
                new PointF(topLeft.X + width, topLeft.Y),
                new PointF(topLeft.X + width, topLeft.Y + height),
                new PointF(topLeft.X, topLeft.Y + height)
            };
            Name = "Rectangle";

            float sumX = 0, sumY = 0;
            foreach (var pt in corners)
            {
                sumX += pt.X;
                sumY += pt.Y;
            }
            Center = new PointF(sumX / corners.Count, sumY / corners.Count);
        }

        public Rectangle(List<PointF> corners) // конструктор по 4 точкам
        {
            if (corners == null || corners.Count != 4)
                throw new ArgumentException("Для прямоугольника необходимо задать 4 вершины.");
            this.corners = new List<PointF>(corners);
            Name = "Rectangle";
        }

        public PointF Center
        {
            get;

            set;
        }

        public bool IsSelected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get; set; }

        public string DrawingGeometry => throw new NotImplementedException();

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

        // Проверка, находится ли точка внутри прямоугольника.
        // Реализовано с помощью алгоритма «лучевого пересечения».
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

        public void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, PointF> pointParams) // это так делается???? 
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

        public bool HitTest(Point point, double epsilon) => throw new NotImplementedException();
        public void RandomizeParameters() => throw new NotImplementedException();
    }
}
