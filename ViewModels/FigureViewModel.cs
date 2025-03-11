using ReactiveUI.Fody.Helpers;

namespace GraphicEditor.ViewModels
{
    public class FigureViewModel : ViewModelBase
    {
        [Reactive]
        public IFigure Figure { get; set; }

        public FigureViewModel(IFigure figure)
        {
            Figure = figure;
        }
    }
}