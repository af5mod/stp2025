using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using System.IO;
using System.Text.RegularExpressions;

using DynamicData;

namespace GraphicEditor
{
    public class FigureService 
    {
        public readonly SourceCache<IFigure,string> _figures = new(fig=>fig.Name); // Все фигуры
        private readonly HashSet<IFigure> _selectedFigures = new(); // Выбранные фигуры

        public IEnumerable<IFigure> Figures => _figures.Items;

        public IEnumerable<string> FigureNamesToCreate => FigureFabric.AvailableFigures; //список всех доступных имен фигур

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

        public IFigure Create(string name, IDictionary<string, Point> parameters, IDictionary<string, double> doubleparameters)
        {
            if (!FigureFabric.AvailableFigures.Contains(name))
            {
                throw new ArgumentException($"Фигура с именем {name} не найдена.", nameof(name));
            }

            var figure = FigureFabric.CreateFigure(name);
            figure.SetParameters(doubleparameters, parameters);

            if (figure is Circle circle)
            {
                var center = (Point)parameters["Center"];
                var pointOnCircle = (Point)parameters["PointOnCircle"];
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

        public IFigure? Find(Point p, double eps)
        {
            return Figures.FirstOrDefault(f => f.IsIn(p, eps));
        }


        public IEnumerable<(string, Type)> GetParameters(string figure)
        {
            throw new NotImplementedException();
        }

        private void LoadLine(Match lineMatch)
        {
            double x1 = double.Parse(lineMatch.Groups["x1"].Value);
            double y1 = double.Parse(lineMatch.Groups["y1"].Value);
            double x2 = double.Parse(lineMatch.Groups["x2"].Value);
            double y2 = double.Parse(lineMatch.Groups["y2"].Value);

            var line = FigureFabric.CreateFigure("Line") as Line;
            line.SetParameters(new Dictionary<string, double>(), new Dictionary<string, Point>
                {
                    {"First", new Point { X = x1, Y = y1 }},
                    {"Second", new Point { X = x2, Y = y2 }}
                });

            AddFigure(line);
        }

        private void LoadCircle(Match circleMatch)
        {
            double cx = double.Parse(circleMatch.Groups["cx"].Value);
            double cy = double.Parse(circleMatch.Groups["cy"].Value);
            double r = double.Parse(circleMatch.Groups["r"].Value);

            var circle = FigureFabric.CreateFigure("Circle") as Circle;
            circle.SetParameters(new Dictionary<string, double>(), new Dictionary<string, Point>
                {
                    {"Center", new Point { X = cx, Y = cy }},
                    {"RadiusPoint", new Point { X = cx + r, Y = cy }}
                });

            AddFigure(circle);
        }

        public void Load(string FilePath, string FileFormat)
        {
            if (FilePath.ToLower() != "svg")
                return;

            string svgContent = File.ReadAllText(FilePath);

            var lineMatch = Regex.Match(svgContent, @"<line\s+x1='(?<x1>[\d.]+)'\s+y1='(?<y1>[\d.]+)'\s+x2='(?<x2>[\d.]+)'\s+y2='(?<y2>[\d.]+)'");
            if (lineMatch.Success)
            {
                LoadLine(lineMatch);
            }

            var circleMatch = Regex.Match(svgContent, @"<circle\s+cx='(?<cx>[\d.]+)'\s+cy='(?<cy>[\d.]+)'\s+r='(?<r>[\d.]+)'");
            if (circleMatch.Success)
            {
                LoadCircle(circleMatch);
            }
        }


        public void Save(string FilePath, string FileFormat)
        {
            if (FileFormat.ToLower() != "svg")
                return;

            string svgHeader = "<svg width='500' height='500' xmlns='http://www.w3.org/2000/svg'>\n";
            string svgFooter = "</svg>";
            string svgContent = "";

            foreach (var figure in Figures)
            {
                if (figure is Line line)
                {
                    svgContent += $"<line x1='{line.Start.X}' y1='{line.Start.Y}' x2='{line.End.X}' y2='{line.End.Y}' " +
                                  "style='stroke:rgb(99,99,99);stroke-width:2' />\n";
                }
                else if (figure is Circle circle)
                {
                    double radius = Math.Sqrt(Math.Pow(circle.PointOnCircle.X - circle.Center.X, 2) +
                                              Math.Pow(circle.PointOnCircle.Y - circle.Center.Y, 2));

                    svgContent += $"<circle cx='{circle.Center.X}' cy='{circle.Center.Y}' r='{radius}' " +
                                  "style='stroke:rgb(99,99,99);stroke-width:2; fill:none' />\n";
                }
            }


            string fullSvgContent = svgHeader + svgContent + svgFooter;
            File.WriteAllText(FilePath, fullSvgContent);
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
    }
}
