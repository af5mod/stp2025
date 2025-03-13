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
    
}
