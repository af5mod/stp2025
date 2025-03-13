using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Drawing;
using GraphicEditor;

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
    
}
