using System;
using System.Collections.Generic;
using System.IO;

namespace GraphicEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            var figures = new List<IFigure>
             {
                 new Line(new Point { X = 1, Y = 10 }, new Point { X = 100, Y = 100 }),
                 new Circle(new Point { X = 250, Y = 250 }, new Point { X = 300, Y = 250 })
             };


            var figureService = new FigureService();
            foreach (var figure in figures)
            {
                figureService.AddFigure(figure);
            }


            figureService.Save("output.svg", "svg");
            Console.WriteLine("Фигуры успешно сохранены в файл output.svg");

            //figureService.Load("output.svg", "svg");
            //figureService.PrintFigures();

        }
    }
}
