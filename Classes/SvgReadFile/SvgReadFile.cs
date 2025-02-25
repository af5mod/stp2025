using System;
using System.Xml;

namespace SvgImageApp
{
    class SvgReader : ISvgReader
    {
        public void ReadSvgFile(string filePath)
        {
            XmlDocument svgDocument = new XmlDocument();
            string inputDirectory = "./input/";
            if (!Directory.Exists(inputDirectory))
            {
                Directory.CreateDirectory(inputDirectory);
            }
            svgDocument.Load("./input/" + filePath);

            // text
            XmlNodeList texts = svgDocument.GetElementsByTagName("text");
            foreach (XmlNode textNode in texts)
            {
                if (textNode.Attributes != null)
                {
                    string textContent = textNode.InnerText ?? "No text content";
                    Console.WriteLine("Text found with attributes:");
                    foreach (XmlAttribute attr in textNode.Attributes)
                    {
                        Console.WriteLine($" - {attr.Name}: {attr.Value}");
                    }
                    Console.WriteLine($" - Content: {textContent}");
                }
            }

            // square
            XmlNodeList rects = svgDocument.GetElementsByTagName("rect");
            foreach (XmlNode rect in rects)
            {
                if (rect.Attributes != null && rect.Attributes["width"] != null && rect.Attributes["height"] != null)
                {
                    Console.WriteLine("Rectangle found with attributes:");
                    foreach (XmlAttribute attr in rect.Attributes)
                    {
                        Console.WriteLine($" - {attr.Name}: {attr.Value}");
                    }
                }
            }

            // circle
            XmlNodeList circles = svgDocument.GetElementsByTagName("circle");
            foreach (XmlNode circle in circles)
            {
                if (circle.Attributes != null && circle.Attributes["r"] != null)
                {
                    Console.WriteLine("Circle found with attributes:");
                    foreach (XmlAttribute attr in circle.Attributes)
                    {
                        Console.WriteLine($" - {attr.Name}: {attr.Value}");
                    }
                }
            }
        }
    }
}