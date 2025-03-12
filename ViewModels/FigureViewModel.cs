using ReactiveUI.Fody.Helpers;

namespace GraphicEditor.ViewModels
{
    public class FigureViewModel : ViewModelBase
    {
        public IFigure Figure { get; }

        public FigureViewModel(IFigure figure)
        {
            Figure = figure;
        }
    }
}
