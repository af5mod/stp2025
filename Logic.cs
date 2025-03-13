using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using System.IO;
using System.Text.RegularExpressions;

using DynamicData;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Reactive.Linq;
using DynamicData.Kernel;
using System.Collections.ObjectModel;
using Avalonia.Controls.ApplicationLifetimes;
using Tmds.DBus.Protocol;
using GraphicEditor.ViewModels;

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
            double x1 = double.Parse(lineMatch.Groups["x1"].Value);
            double y1 = double.Parse(lineMatch.Groups["y1"].Value);
            double x2 = double.Parse(lineMatch.Groups["x2"].Value);
            double y2 = double.Parse(lineMatch.Groups["y2"].Value);

            var line = FigureFabric.CreateFigure("Line");
            line.SetParameters(new Dictionary<string, double>(), new Dictionary<string, PointF>
                {
                    {"First", new PointF { X = (float)x1, Y = (float)y1 }},
                    {"Second", new PointF {X =(float) x2, Y =(float) y2}}
                });

            AddFigure(line);
        }

        private void LoadCircle(Match circleMatch)
        {
            double cx = double.Parse(circleMatch.Groups["cx"].Value);
            double cy = double.Parse(circleMatch.Groups["cy"].Value);
            double r = double.Parse(circleMatch.Groups["r"].Value);

            var circle = FigureFabric.CreateFigure("Circle");
            circle.SetParameters(new Dictionary<string, double>(), new Dictionary<string, PointF>
                {
                    {"Center", new PointF {X =(float) cx, Y =(float) cy}},
                    {"RadiusPoint", new PointF {X =(float)cx + (float)r, Y =(float) cy}}
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

        private void SaveAsSvg(IFigure figure, string filePath)
        {

            if (figure is Line line)
            {
                var svgContent = $"<svg height=\"200\" width=\"500\" xmlns='http://www.w3.org/2000/svg'><line x1=\"{line.Start.X}\" y1=\"{line.Start.Y}\" x2=\"{line.End.X}\" y2=\"{line.End.Y}\" style=\"stroke:rgb(99,99,99);stroke-width:2\" /></svg>";
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
