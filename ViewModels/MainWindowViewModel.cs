using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Avalonia.Input;
using GraphicEditor.Views;

namespace GraphicEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ILogic _figureService;
        private ObservableCollection<IFigure> _figures;
        private ObservableCollection<IFigure> _selectedFigure;

        public IObservableCollection<IFigure> Figures { get => (IObservableCollection<IFigure>)_figures; }
        public IObservableCollection<IFigure> SelectedFigures { get => (IObservableCollection<IFigure>)_selectedFigure; }

        public ReactiveCommand<Unit, Unit> ClearAllCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteSelectedCommand { get; }
        // public ReactiveCommand<Point, Unit> CanvasClickCommand { get; }

        public MainWindowViewModel()
        {
            _figures = new ObservableCollection<IFigure>();
            _selectedFigure = new ObservableCollection<IFigure>();
            _figureService = new FigureService();

            // Подключение реактивных источников
            _figureService.ObservableFigures
                .Bind(Figures)
                .Subscribe();

            _figureService.ConnectSelections
                .Bind(SelectedFigures)
                .Subscribe();

            // Инициализация команд
            ClearAllCommand = ReactiveCommand.Create(_figureService.ClearAll);
            DeleteSelectedCommand = ReactiveCommand.Create(() =>
                _figureService.RemoveFigures(SelectedFigures.ToList()));

            // CanvasClickCommand = ReactiveCommand.Create<PointerPressedEventArgs>(args =>
            // {
            //     var isMultiSelect = (args.KeyModifiers & KeyModifiers.Control) != 0;
            //     _figureService.Select(args.GetPosition(MainCanvas), isMultiSelect);
            // });
        }
    }
}
