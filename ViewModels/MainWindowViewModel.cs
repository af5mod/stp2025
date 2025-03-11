
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reactive;
using DynamicData;
using DynamicData.Alias;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

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

        [Reactive]
        public string SelectedFigureName { get; set; }

        public FigureViewModel? SelectedFigureViewModel
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedFigureName))
                {
                    return null;
                }
                else
                {
                    return new FigureViewModel(_figureService.Figures.FirstOrDefault((x) => x.Name == SelectedFigureName)!);
                }
            }
        }
        public ObservableCollection<FigureFactoryViewModel> FigureFactories { get; } = [];
        private readonly ReadOnlyObservableCollection<string> _readonlyFiguresNames;
        public ReadOnlyObservableCollection<string> ReadonlyFigureNames => _readonlyFiguresNames;
        private readonly ReadOnlyObservableCollection<IFigure> _readonlyFigures;
        public ReadOnlyObservableCollection<IFigure> ReadonlyFigures => _readonlyFigures;
        private readonly ReadOnlyObservableCollection<IFigure> _readonlySelectedFigures;
        public ReadOnlyObservableCollection<IFigure> ReadonlySelectedFigures => _readonlySelectedFigures;

        public MainWindowViewModel()
        {
            _figureService = new FigureService();
            foreach (var figureName in FigureFabric.AvailableFigures)
            {
                FigureFactories.Add(new() { FigureService = _figureService, FigureType = figureName, MainWindowViewModel = this });
            }

            // var line = FigureFabric.CreateFigure("Line");

            // line.SetParameters(new Dictionary<string, double>(), new Dictionary<string, PointF>
            //      {
            //          {"Start", new PointF { X = 1F, Y = 1F }},
            //          {"End", new PointF {X =2F, Y =2F}}
            //      });

            // _figureService.AddFigure(line);
            // var circle = FigureFabric.CreateFigure("Circle");

            // circle.SetParameters(new Dictionary<string, double>(), new Dictionary<string, PointF>
            //      {
            //          {"Center", new PointF { X = 1F, Y = 1F }},
            //          {"PointOnCircle", new PointF {X =2F, Y =2F}}
            //      });

            // _figureService.AddFigure(circle);

            //var line = new Line(new Point { X = 1, Y = 10 }, new Point { X = 100, Y = 100 });
            //.AddFigure(line);            

            // var circle = new Circle(new Point { X = 1, Y = 10 }, new Point { X = 100, Y = 100 });
            // _figureService.AddFigure(circle);

            // Подключение реактивных источников
            _figureService.Connect()
                .Bind(out _readonlyFigures)
                .Subscribe();

            _figureService.Connect().Select(f => f.Name).SortAndBind(out _readonlyFiguresNames)
                .Subscribe();

            _figureService.ConnectSelections()
                .Bind(out _readonlySelectedFigures)
                .Subscribe();

            // Инициализация команд
            ClearAllCommand = ReactiveCommand.Create(_figureService.ClearAll);
            DeleteSelectedCommand = ReactiveCommand.Create(() =>
                _figureService.RemoveFigures(ReadonlySelectedFigures));

            this.WhenAnyValue(o => o.SelectedFigureName).Subscribe(o => this.RaisePropertyChanged(nameof(SelectedFigureViewModel)));

            // CanvasClickCommand = ReactiveCommand.Create<PointerPressedEventArgs>(args =>
            // {
            //     var isMultiSelect = (args.KeyModifiers & KeyModifiers.Control) != 0;
            //     _figureService.Select(args.GetPosition(MainCanvas), isMultiSelect);
            // });
        }
    }
}
