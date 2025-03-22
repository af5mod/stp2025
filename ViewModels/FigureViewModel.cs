using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using System;
using System.Drawing;
using DynamicData.Binding;

namespace GraphicEditor.ViewModels
{
    public class FigureViewModel : ViewModelBase
    {
        [Reactive]
        public IFigure Figure { get; set; }
        [Reactive]
        public float CenterX { get; set; }
        [Reactive]
        public float CenterY { get; set; }


        [Reactive]
        public Avalonia.Media.Color Color { get; set; }
        [Reactive]
        public float Thickness { get; set; }

        public FigureViewModel(IFigure figure)
        {
            Thickness = 1;
            Figure = figure;
            CenterX = Figure.Center.X;
            CenterY = Figure.Center.Y;
            Color = Avalonia.Media.Color.FromRgb((byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256));
            this.WhenAnyValue(o => o.CenterX, o => o.CenterY, (x, y) => new PointF(x, y))
                .Subscribe((x) =>
                {
                    Figure.Center = x;
                }
            );
            Figure.WhenAnyValue(o=>o.Center).Subscribe((x)=>{
                CenterX = x.X;
                CenterY = x.Y;
            });
        }
    }
}
