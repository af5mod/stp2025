using System.Xml;

namespace SvgImageApp
{
    public class SvgImageCreator : ISvgImageCreator
    {
        private XmlDocument _svgDocument;
        private XmlElement? _rootElement;

        public SvgImageCreator()
        {
            _svgDocument = new XmlDocument();
        }

        public void CreateSvg(int width, int height, string content)
        {
            _rootElement = _svgDocument.CreateElement("svg");  // new elem of XML this tag SVG
            
            _rootElement.SetAttribute("width", width.ToString());    //size of SVG image
            _rootElement.SetAttribute("height", height.ToString());  //size of SVG image
            _rootElement.SetAttribute("xmlns", "http://www.w3.org/2000/svg"); //namespace for elem SVG 

            // add text
            var contentElement = _svgDocument.CreateElement("text");
            contentElement.SetAttribute("x", "10"); // top left x
            contentElement.SetAttribute("y", "20"); // top left y
            contentElement.InnerText = content; // place text on svg
            _rootElement.AppendChild(contentElement); 

            // add square
            var rectElement = _svgDocument.CreateElement("rect");  // figure square
            rectElement.SetAttribute("x", "50"); // top left x
            rectElement.SetAttribute("y", "50"); // top left y
            rectElement.SetAttribute("width", "50"); // width 
            rectElement.SetAttribute("height", "50"); // height
            rectElement.SetAttribute("fill", "blue"); // color fill
            _rootElement.AppendChild(rectElement);

            // add circle
            var circleElement = _svgDocument.CreateElement("circle"); // figure circle
            circleElement.SetAttribute("cx", "150");    //  middle of circle by x
            circleElement.SetAttribute("cy", "75");     //  -//- by y
            circleElement.SetAttribute("r", "30");      // radius
            circleElement.SetAttribute("fill", "red");  // color fill
            _rootElement.AppendChild(circleElement);

            // add root elem in doc
            _svgDocument.AppendChild(_rootElement);
        }

        public void SaveSvg(string filePath)
        {
            filePath = "./output/" + filePath;
            if (_svgDocument != null)
            {
                _svgDocument.Save(filePath);

                string xmlFilePath = System.IO.Path.ChangeExtension(filePath, ".xml");
                _svgDocument.Save(xmlFilePath);
            }
            else
            {
                throw new InvalidOperationException("SVG document is not created yet.");
            }
        }
    }
}