
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using DynamicData;
using DynamicData.Alias;
using ReactiveUI;

namespace GraphicEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private double _selectedShapeWidth;
    private double _selectedShapeHeight;

    public double SelectedShapeWidth
    {
        get => _selectedShapeWidth;
        set => this.RaiseAndSetIfChanged(ref _selectedShapeWidth, Math.Round(value, 0));  // округление до целого
    }

    public double SelectedShapeHeight
    {
        get => _selectedShapeHeight;
        set => this.RaiseAndSetIfChanged(ref _selectedShapeHeight, Math.Round(value, 0));  // округление до целого
    }
        private readonly ILogic _figureService;
        public ReactiveCommand<Unit, Unit> ClearAllCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteSelectedCommand { get; }
        // public ReactiveCommand<Point, Unit> CanvasClickCommand { get; }

        private readonly ReadOnlyObservableCollection<string> _readonlyFiguresNames;
        private readonly ReadOnlyObservableCollection<IFigure> _readonlyFigures;
        public ReadOnlyObservableCollection<IFigure> ReadonlyFigures => _readonlyFigures;
        private readonly ReadOnlyObservableCollection<IFigure> _readonlySelectedFigures;
        public ReadOnlyObservableCollection<IFigure> ReadonlySelectedFigures => _readonlySelectedFigures;

        public MainWindowViewModel()
        {
            _figureService = new FigureService();

            // Подключение реактивных источников
            _figureService.Connect()
                .Bind(out _readonlyFigures)
                .Subscribe();
            _figureService.Connect().Select(f=>f.Name).SortAndBind(out _readonlyFiguresNames)
                .Subscribe();

            _figureService.ConnectSelections()
                .Bind(out _readonlySelectedFigures)
                .Subscribe();

            // Инициализация команд
            ClearAllCommand = ReactiveCommand.Create(_figureService.ClearAll);
            DeleteSelectedCommand = ReactiveCommand.Create(() =>
                _figureService.RemoveFigures(ReadonlySelectedFigures));

            // CanvasClickCommand = ReactiveCommand.Create<PointerPressedEventArgs>(args =>
            // {
            //     var isMultiSelect = (args.KeyModifiers & KeyModifiers.Control) != 0;
            //     _figureService.Select(args.GetPosition(MainCanvas), isMultiSelect);
            // });
        }
    }
}
