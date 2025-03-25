using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

using DynamicData;
using System.Reactive.Linq;
using GraphicEditor.Models;


namespace GraphicEditor
{
    public class FigureService : ILogic
    {
        public readonly SourceCache<IFigure, string> _figures = new(f => f.Name); // Все фигуры
        public IObservable<IChangeSet<IFigure, string>> Connect() => _figures.Connect();
        public IEnumerable<IFigure> Figures => _figures.Items;

        // IObservable<IChangeSet<IFigure, string>>
        // IObservable<IChangeSet<IFigure>>
        private readonly SourceList<IFigure> _selectedFigures = new(); // Выбранные фигуры
        public IObservable<IChangeSet<IFigure>> ConnectSelections() => _selectedFigures.Connect();

        public IEnumerable<string> FigureNamesToCreate => FigureFabric.AvailableFigures; //список всех доступных имен фигур

        public void Select(Point point, bool multiSelect = false)
        {
            var figure = _figures.Items
                .Reverse()
                .FirstOrDefault(f => f.IsIn(point, 3.0f));

            if (figure == null)
            {
                if (!multiSelect) _selectedFigures.Clear();
                return;
            }

            if (multiSelect)
            {
                figure.IsSelected = !figure.IsSelected;
                if (figure.IsSelected)
                    _selectedFigures.Add(figure);
                else
                    _selectedFigures.Remove(figure);
            }
            else
            {
                foreach (var f in _selectedFigures.Items)
                    f.IsSelected = false;

                _selectedFigures.Clear();
                figure.IsSelected = true;
                _selectedFigures.Add(figure);
            }
        }

        public void ClearAll()
        {
            _figures.Clear();
            _selectedFigures.Clear();
        }
        public void AddFigure(IFigure figure)
        {
            if (figure == null) throw new ArgumentNullException(nameof(figure));
            _figures.AddOrUpdate(figure);
        }

        public void RemoveFigure(IFigure figure)
        {
            if (figure == null) return;
            _figures.Remove(figure);
            _selectedFigures.Remove(figure);
        }

        public void RemoveFigures(IEnumerable<IFigure> figures)
        {
            _figures.RemoveKeys(figures.Select(f => f.Name));
            _selectedFigures.RemoveMany(figures);
        }

        public IFigure Create(string name, IDictionary<string, PointF> parameters, IDictionary<string, double> doubleparameters)
        {
            if (!FigureFabric.AvailableFigures.Contains(name))
            {
                throw new ArgumentException($"Фигура с именем {name} не найдена.", nameof(name));
            }

            var figure = FigureFabric.CreateFigure(name);
            figure.SetParameters(doubleparameters, parameters);

            if (figure is Circle circle)
            {
                var center = (PointF)parameters["Center"];
                var pointOnCircle = (PointF)parameters["PointOnCircle"];
                figure = new Circle(center, pointOnCircle);
            }
            //пример создания остальных фигур
            //else if (figure is Rectangle rectangle)
            //{
            //    var topLeft = (Point)parameters["TopLeft"];
            //    var bottomRight = (Point)parameters["BottomRight"];
            //    figure = new Rectangle(topLeft, bottomRight);
            //}
            //else if (figure is Triangle triangle)
            //{
            //    var a = (Point)parameters["A"];
            //    var b = (Point)parameters["B"];
            //    var c = (Point)parameters["C"];
            //    figure = new Triangle(a, b, c);
            //}
            //else if (figure is Line line)
            //{
            //    var start = (Point)parameters["Start"];
            //    var end = (Point)parameters["End"];
            //    figure = new Line(start, end);
            //}

            return figure;
        }

        public IFigure? Find(PointF p, float eps)
        {
            return Figures.FirstOrDefault(f => f.IsIn(p, eps));
        }

        public IEnumerable<(string, Type)> GetParameters(string figure)
        {
            throw new NotImplementedException();
        }


        private void LoadLine(Match lineMatch)
        {
            Console.WriteLine("ЗАГРУЗКА ЛИНИИ");

            double x1 = double.Parse(lineMatch.Groups["x1"].Value, CultureInfo.InvariantCulture);
            double y1 = double.Parse(lineMatch.Groups["y1"].Value, CultureInfo.InvariantCulture);
            double x2 = double.Parse(lineMatch.Groups["x2"].Value, CultureInfo.InvariantCulture);
            double y2 = double.Parse(lineMatch.Groups["y2"].Value, CultureInfo.InvariantCulture);

            int r = int.Parse(lineMatch.Groups["r"].Value);
            int g = int.Parse(lineMatch.Groups["g"].Value);
            int b = int.Parse(lineMatch.Groups["b"].Value);

            var line = FigureFabric.CreateFigure("Line");


            line.SetParameters(new Dictionary<string, double>(), new Dictionary<string, PointF>
                {
                    {"Start", new PointF { X = (float)x1, Y = (float)y1 }},
                    {"End", new PointF {X =(float) x2, Y =(float) y2}}
                });
            line.Color = new Avalonia.Media.Color(255, (byte)r, (byte)g, (byte)b);

            AddFigureInCache(line);
        }

        private void LoadCircle(Match circleMatch)
        {
            Console.WriteLine("ЗАГРУЗКА КРУГА");

            double cx = double.Parse(circleMatch.Groups["cx"].Value, CultureInfo.InvariantCulture);
            double cy = double.Parse(circleMatch.Groups["cy"].Value, CultureInfo.InvariantCulture);
            double radius = Double.Parse(circleMatch.Groups["r"].Value, CultureInfo.InvariantCulture);

            int r = int.Parse(circleMatch.Groups["r"].Value);
            int g = int.Parse(circleMatch.Groups["g"].Value);
            int b = int.Parse(circleMatch.Groups["b"].Value);

            var circle = FigureFabric.CreateFigure("Circle");

            circle.SetParameters(new Dictionary<string, double>(), new Dictionary<string, PointF>
                {
                    {"Center", new PointF {X =(float) cx, Y =(float) cy}},
                    {"PointOnCircle", new PointF {X =(float)cx + (float)radius, Y =(float) cy}}
                });
            circle.Color = new Avalonia.Media.Color(255, (byte)r, (byte)g, (byte)b);

            AddFigureInCache(circle);
        }

       private void LoadRectangle(Match rectMatch)
        {
            Console.WriteLine("ЗАГРУЗКА Rectangle");

            double x = double.Parse(rectMatch.Groups["x"].Value, CultureInfo.InvariantCulture);
            double y = double.Parse(rectMatch.Groups["y"].Value, CultureInfo.InvariantCulture);
            double width = double.Parse(rectMatch.Groups["width"].Value, CultureInfo.InvariantCulture);
            double height = double.Parse(rectMatch.Groups["height"].Value, CultureInfo.InvariantCulture);

            int r = int.Parse(rectMatch.Groups["r"].Value);
            int g = int.Parse(rectMatch.Groups["g"].Value);
            int b = int.Parse(rectMatch.Groups["b"].Value);

            var rectangle = FigureFabric.CreateFigure("Rectangle");

            rectangle.SetParameters(
                new Dictionary<string, double>
                {
                    {"Width", width},
                    {"Height", height}
                },
                new Dictionary<string, PointF>
                {
                    {"TopLeft", new PointF { X = (float)x, Y = (float)y }},
                    {"BottomRight", new PointF { X = (float)(x + width), Y = (float)(y + height) }}
                });
            rectangle.Color = new Avalonia.Media.Color(255, (byte)r, (byte)g, (byte)b);

            AddFigureInCache(rectangle);
        }

        private void LoadHexagon(Match hexagonMatch)
        {
            Console.WriteLine("ЗАГРУЗКА Hexagon");

            var points = new Dictionary<string, PointF>();

            points.Add("V1", new PointF(
                float.Parse(hexagonMatch.Groups[1].Value, CultureInfo.InvariantCulture),
                float.Parse(hexagonMatch.Groups[2].Value, CultureInfo.InvariantCulture))
            );

            points.Add("V2", new PointF(
                float.Parse(hexagonMatch.Groups[3].Value, CultureInfo.InvariantCulture),
                float.Parse(hexagonMatch.Groups[4].Value, CultureInfo.InvariantCulture))
            );

            points.Add("V3", new PointF(
                float.Parse(hexagonMatch.Groups[5].Value, CultureInfo.InvariantCulture),
                float.Parse(hexagonMatch.Groups[6].Value, CultureInfo.InvariantCulture))
            );

            points.Add("V4", new PointF(
                float.Parse(hexagonMatch.Groups[7].Value, CultureInfo.InvariantCulture),
                float.Parse(hexagonMatch.Groups[8].Value, CultureInfo.InvariantCulture))
            );

            points.Add("V5", new PointF(
                float.Parse(hexagonMatch.Groups[9].Value, CultureInfo.InvariantCulture),
                float.Parse(hexagonMatch.Groups[10].Value, CultureInfo.InvariantCulture))
            );

            points.Add("V6", new PointF(
                float.Parse(hexagonMatch.Groups[11].Value, CultureInfo.InvariantCulture),
                float.Parse(hexagonMatch.Groups[12].Value, CultureInfo.InvariantCulture))
            );

            int r = int.Parse(hexagonMatch.Groups["r"].Value);
            int g = int.Parse(hexagonMatch.Groups["g"].Value);
            int b = int.Parse(hexagonMatch.Groups["b"].Value);

            var hexagon = FigureFabric.CreateFigure("Hexagon");
            hexagon.SetParameters(new Dictionary<string, double>(), points);
            hexagon.Color = new Avalonia.Media.Color(255, (byte)r, (byte)g, (byte)b);

            AddFigureInCache(hexagon);
        }

        private void LoadPentagon(Match pentagonMatch)
        {
            Console.WriteLine("ЗАГРУЗКА Pentagon");

            var points = new Dictionary<string, PointF>();

            points.Add("V1", new PointF(
                float.Parse(pentagonMatch.Groups[1].Value, CultureInfo.InvariantCulture),
                float.Parse(pentagonMatch.Groups[2].Value, CultureInfo.InvariantCulture))
            );

            points.Add("V2", new PointF(
                float.Parse(pentagonMatch.Groups[3].Value, CultureInfo.InvariantCulture),
                float.Parse(pentagonMatch.Groups[4].Value, CultureInfo.InvariantCulture))
            );

            points.Add("V3", new PointF(
                float.Parse(pentagonMatch.Groups[5].Value, CultureInfo.InvariantCulture),
                float.Parse(pentagonMatch.Groups[6].Value, CultureInfo.InvariantCulture))
            );

            points.Add("V4", new PointF(
                float.Parse(pentagonMatch.Groups[7].Value, CultureInfo.InvariantCulture),
                float.Parse(pentagonMatch.Groups[8].Value, CultureInfo.InvariantCulture))
            );

            points.Add("V5", new PointF(
                float.Parse(pentagonMatch.Groups[9].Value, CultureInfo.InvariantCulture),
                float.Parse(pentagonMatch.Groups[10].Value, CultureInfo.InvariantCulture))
            );

            int r = int.Parse(pentagonMatch.Groups["r"].Value);
            int g = int.Parse(pentagonMatch.Groups["g"].Value);
            int b = int.Parse(pentagonMatch.Groups["b"].Value);

            var pentagon = FigureFabric.CreateFigure("Pentagon");
            pentagon.SetParameters(new Dictionary<string, double>(), points);
            pentagon.Color = new Avalonia.Media.Color(255, (byte)r, (byte)g, (byte)b);

            AddFigureInCache(pentagon);
        }

        private void LoadTriangle(Match triangleMatch)
        {
            Console.WriteLine("ЗАГРУЗКА Triangle");

            var points = new Dictionary<string, PointF>();

            points.Add("Vertex1", new PointF(
                float.Parse(triangleMatch.Groups[1].Value, CultureInfo.InvariantCulture),
                float.Parse(triangleMatch.Groups[2].Value, CultureInfo.InvariantCulture))
            );

            points.Add("Vertex2", new PointF(
                float.Parse(triangleMatch.Groups[3].Value, CultureInfo.InvariantCulture),
                float.Parse(triangleMatch.Groups[4].Value, CultureInfo.InvariantCulture))
            );

            points.Add("Vertex3", new PointF(
                float.Parse(triangleMatch.Groups[5].Value, CultureInfo.InvariantCulture),
                float.Parse(triangleMatch.Groups[6].Value, CultureInfo.InvariantCulture))
            );

            int r = int.Parse(triangleMatch.Groups["r"].Value);
            int g = int.Parse(triangleMatch.Groups["g"].Value);
            int b = int.Parse(triangleMatch.Groups["b"].Value);

            var triangle = FigureFabric.CreateFigure("Triangle");
            triangle.SetParameters(new Dictionary<string, double>(), points);
            triangle.Color = new Avalonia.Media.Color(255, (byte)r, (byte)g, (byte)b);

            Console.WriteLine($"Vertex1: {points["Vertex1"]}");
            Console.WriteLine($"Vertex2: {points["Vertex2"]}");
            Console.WriteLine($"Vertex3: {points["Vertex3"]}");

            AddFigureInCache(triangle);
        }

        public void Load(string FilePath, string FileFormat)
        {
            if (FileFormat.ToLower() != "svg")
                return;

            Console.WriteLine("ЗАГРУЗКА ИЗ ФАЙЛА");

            using (StreamReader sr = new StreamReader(FilePath))
            {
                string? str;
                while ((str = sr.ReadLine()) != null)
                {
                    var lineMatch = Regex.Match(str,
                        @"<line\s+x1='(?<x1>[\d.]+)'\s+y1='(?<y1>[\d.]+)'\s+x2='(?<x2>[\d.]+)'\s+y2='(?<y2>[\d.]+)'\s+style='stroke:rgb\(\d+,\d+,\d+\);stroke-width:\d+;fill:rgb\((?<r>\d+),(?<g>\d+),(?<b>\d+)\);stroke-opacity:none;opacity:(?<opacity>[\d.]+)' />");
                    if (lineMatch.Success)
                    {
                        LoadLine(lineMatch);
                        continue;
                    }

                    var hexagonMatch = Regex.Match(str,
                        @"<polygon\s+points='([\d.]+),([\d.]+)\s+([\d.]+),([\d.]+)\s+([\d.]+),([\d.]+)\s+([\d.]+),([\d.]+)\s+([\d.]+),([\d.]+)\s+([\d.]+),([\d.]+)'\s+style='stroke:rgb\(\d+,\d+,\d+\);stroke-width:\d+;fill:rgb\((?<r>\d+),(?<g>\d+),(?<b>\d+)\);stroke-opacity:none;opacity:([\d.]+)' />");
                    if (hexagonMatch.Success)
                    {
                        LoadHexagon(hexagonMatch);
                        continue;
                    }

                    var pentagonMatch = Regex.Match(str,
                        @"<polygon\s+points='([\d.]+),([\d.]+)\s+([\d.]+),([\d.]+)\s+([\d.]+),([\d.]+)\s+([\d.]+),([\d.]+)\s+([\d.]+),([\d.]+)'\s+style='stroke:rgb\(\d+,\d+,\d+\);stroke-width:\d+;fill:rgb\((?<r>\d+),(?<g>\d+),(?<b>\d+)\);stroke-opacity:none;opacity:([\d.]+)' />");
                    if (pentagonMatch.Success)
                    {
                        LoadPentagon(pentagonMatch);
                        continue;
                    }

                    var rectMatch = Regex.Match(str,
                        @"<rect\s+x='(?<x>[\d.]+)'\s+y='(?<y>[\d.]+)'\s+width='(?<width>[\d.]+)'\s+height='(?<height>[\d.]+)'\s+style='stroke:rgb\(\d+,\d+,\d+\);stroke-width:\d+;fill:rgb\((?<r>\d+),(?<g>\d+),(?<b>\d+)\);opacity:(?<opacity>[\d.]+)' />");
                    if (rectMatch.Success)
                    {
                        LoadRectangle(rectMatch);
                        continue;
                    }

                    var triangleMatch = Regex.Match(str,
                        @"<polygon\s+points='([\d.]+),([\d.]+)\s+([\d.]+),([\d.]+)\s+([\d.]+),([\d.]+)'\s+style='stroke:rgb\(\d+,\d+,\d+\);stroke-width:\d+;fill:rgb\((?<r>\d+),(?<g>\d+),(?<b>\d+)\);stroke-opacity:none;opacity:([\d.]+)' />");
                    if (triangleMatch.Success)
                    {
                        Console.WriteLine(triangleMatch);
                        LoadTriangle(triangleMatch);
                        continue;
                    }

                    var circleMatch = Regex.Match(str,
                        @"<circle\s+cx='(?<cx>[\d.]+)'\s+cy='(?<cy>[\d.]+)'\s+r='(?<r>[\d.]+)'\s+style='stroke:rgb\(\d+,\d+,\d+\);stroke-width:\d+;fill:rgb\((?<r>\d+),(?<g>\d+),(?<b>\d+)\);stroke-opacity:none;opacity:([\d.]+)' />");
                    if (circleMatch.Success)
                    {
                        LoadCircle(circleMatch);
                    }
                }
            }
        }

        private void AddFigureInCache(IFigure figure)
        {
            _figures.AddOrUpdate(figure);
        }

        public void PrintFigures()
        {
            foreach (var figure in _figures.Items)
            {
                Console.WriteLine($"Figure Name: {figure.Name}");
            }
        }

        public void Save(string FilePath, string FileFormat)
        {
            if (FileFormat.ToLower() != "svg")
                return;

            string svgHeader = "<svg width='1000' height='600' xmlns='http://www.w3.org/2000/svg'>\n";
            string svgFooter = "</svg>";
            string svgContent = "";

            PrintFigures();

            foreach (var figure in _figures.Items)
            {
                var color = figure.Color;
                string fillColor = $"rgb({color.R},{color.G},{color.B})";
                double opacity = color.A / 255.0;

                if (figure is Line line)
                {
                    svgContent += $"<line x1='{line.Start.X.ToString(CultureInfo.InvariantCulture)}' " +
                                $"y1='{line.Start.Y.ToString(CultureInfo.InvariantCulture)}' " +
                                $"x2='{line.End.X.ToString(CultureInfo.InvariantCulture)}' " +
                                $"y2='{line.End.Y.ToString(CultureInfo.InvariantCulture)}' " +
                                $"style='stroke:rgb(0,0,0);stroke-width:1;fill:{fillColor};stroke-opacity:none;opacity:{opacity.ToString(CultureInfo.InvariantCulture)}' />\n";
                }
                else if (figure is Circle circle)
                {
                    double radius = Math.Sqrt(Math.Pow(circle.PointOnCircle.X - circle.Center.X, 2) +
                                            Math.Pow(circle.PointOnCircle.Y - circle.Center.Y, 2));

                    svgContent += $"<circle cx='{circle.Center.X.ToString(CultureInfo.InvariantCulture)}' " +
                                $"cy='{circle.Center.Y.ToString(CultureInfo.InvariantCulture)}' " +
                                $"r='{radius.ToString(CultureInfo.InvariantCulture)}' " +
                                $"style='stroke:rgb(0,0,0);stroke-width:1;fill:{fillColor};stroke-opacity:none;opacity:{opacity.ToString(CultureInfo.InvariantCulture)}' />\n";
                }
                else if (figure is GraphicEditor.Models.Rectangle rectangle)
                {
                    float x = rectangle.Point0X;
                    float y = rectangle.Point0Y;
                    float width = rectangle.Point1X - rectangle.Point0X;
                    float height = rectangle.Point1Y - rectangle.Point0Y;

                    svgContent += $"<rect x='{x.ToString(CultureInfo.InvariantCulture)}' " +
                                $"y='{y.ToString(CultureInfo.InvariantCulture)}' " +
                                $"width='{width.ToString(CultureInfo.InvariantCulture)}' " +
                                $"height='{height.ToString(CultureInfo.InvariantCulture)}' " +
                                $"style='stroke:rgb(0,0,0);stroke-width:1;fill:{fillColor};opacity:{opacity.ToString(CultureInfo.InvariantCulture)}' />\n";
                }
                else if (figure is Triangle triangle)
                {
                    string points = $"{triangle.Point0X.ToString(CultureInfo.InvariantCulture)},{triangle.Point0Y.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{triangle.Point1X.ToString(CultureInfo.InvariantCulture)},{triangle.Point1Y.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{triangle.Point2X.ToString(CultureInfo.InvariantCulture)},{triangle.Point2Y.ToString(CultureInfo.InvariantCulture)}";
                                    

                    svgContent += $"<polygon points='{points}' " +
                                $"style='stroke:rgb(0,0,0);stroke-width:1;fill:{fillColor};stroke-opacity:none;opacity:{opacity.ToString(CultureInfo.InvariantCulture)}' />\n";
                }
                else if (figure is Pentagon pentagon)
                {
                    string points = $"{pentagon.Vertex1X.ToString(CultureInfo.InvariantCulture)},{pentagon.Vertex1Y.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{pentagon.Vertex2X.ToString(CultureInfo.InvariantCulture)},{pentagon.Vertex2Y.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{pentagon.Vertex3X.ToString(CultureInfo.InvariantCulture)},{pentagon.Vertex3Y.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{pentagon.Vertex4X.ToString(CultureInfo.InvariantCulture)},{pentagon.Vertex4Y.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{pentagon.Vertex5X.ToString(CultureInfo.InvariantCulture)},{pentagon.Vertex5Y.ToString(CultureInfo.InvariantCulture)}";

                    svgContent += $"<polygon points='{points}' " +
                                $"style='stroke:rgb(0,0,0);stroke-width:1;fill:{fillColor};stroke-opacity:none;opacity:{opacity.ToString(CultureInfo.InvariantCulture)}' />\n";
                }
                else if (figure is Hexagon hexagon)
                {
                    string points = $"{hexagon.Point0X.ToString(CultureInfo.InvariantCulture)},{hexagon.Point0Y.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{hexagon.Point1X.ToString(CultureInfo.InvariantCulture)},{hexagon.Point1Y.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{hexagon.Point2X.ToString(CultureInfo.InvariantCulture)},{hexagon.Point2Y.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{hexagon.Point3X.ToString(CultureInfo.InvariantCulture)},{hexagon.Point3Y.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{hexagon.Point4X.ToString(CultureInfo.InvariantCulture)},{hexagon.Point4Y.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{hexagon.Point5X.ToString(CultureInfo.InvariantCulture)},{hexagon.Point5Y.ToString(CultureInfo.InvariantCulture)}";

                    svgContent += $"<polygon points='{points}' " +
                                $"style='stroke:rgb(0,0,0);stroke-width:1;fill:{fillColor};stroke-opacity:none;opacity:{opacity.ToString(CultureInfo.InvariantCulture)}' />\n";
                }
            }

            string svg = svgHeader + svgContent + svgFooter;
            File.WriteAllText(FilePath, svg);
        }

        private void SaveAsSvg(IFigure figure, string filePath)
        {

            if (figure is Line line)
            {
                var svgContent = $"<svg height=\"200\" width=\"500\" xmlns='http://www.w3.org/2000/svg'><line x1=\"{line.Start.X}\" y1=\"{line.Start.Y}\" x2=\"{line.End.X}\" y2=\"{line.End.Y}\" style=\"stroke:rgb(99,99,99);stroke-width:1\" /></svg>";
                File.WriteAllText(filePath, svgContent);
            }
        }


        public void Select(IFigure f)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFigure> Selected()
        {
            throw new NotImplementedException();
        }

        public void UnSelect(IFigure f)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _figures.Dispose();
            _selectedFigures.Dispose();
        }

        public void Select(IFigure f, bool multiSelect) => throw new NotImplementedException();
    }
}
