using System;
using SvgImageApp;

class Program
{
    static void Main(string[] args)
    {
        ISvgImageCreator svgCreator = new SvgImageCreator();
        svgCreator.CreateSvg(200, 100, "Hello, SVG!");
        svgCreator.SaveSvg("image.svg");
        
        Console.WriteLine("SVG file created and saved as 'image.svg'.");

        SvgReader reader = new SvgReader();
        reader.ReadSvgFile("image.svg");
    }
}