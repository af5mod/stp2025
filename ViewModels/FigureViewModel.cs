using System.Drawing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using System;

namespace GraphicEditor.ViewModels
{
    public class FigureViewModel : ViewModelBase
    {
        public IFigure Figure { get;set; }

        [Reactive]
        public float CenterX { get; set; }
        [Reactive]
        public float CenterY { get; set; }


        public FigureViewModel(IFigure figure)
        {
            Figure = figure;
            CenterX = Figure.Center.X;
            CenterY = Figure.Center.Y;
            this.WhenAnyValue(o => o.CenterX, o => o.CenterY, (x, y) => new PointF(x, y))
                .Subscribe((x) =>
                {
                    Figure.Center = x;
                }
            );
        }
    }
}
