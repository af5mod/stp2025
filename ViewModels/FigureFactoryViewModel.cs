using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reactive;
using ReactiveUI;

namespace GraphicEditor.ViewModels
{
    public class FigureFactoryViewModel : ViewModelBase
    {
        public required ILogic FigureService { get; set; }
        public required MainWindowViewModel MainWindowViewModel { get; set; }
        public required string FigureType { get; set; }
        public ReactiveCommand<Unit, Unit> CreateFigureCommand { get; set; }


        public FigureFactoryViewModel()
        {
            // Не работает: invalid thread exception
            CreateFigureCommand = ReactiveCommand.Create(CreateAndAdd);
        }

        IFigure CreateFigure()
        {
            return FigureFabric.CreateFigure(FigureType);
        }

        public void CreateAndAdd(){
            FigureService?.AddFigure(CreateFigure());
        }
    }
}