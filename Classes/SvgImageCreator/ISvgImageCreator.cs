namespace SvgImageApp
{
    public interface ISvgImageCreator
    {
        void CreateSvg(int width, int height, string content);
        void SaveSvg(string filePath);
    }
}