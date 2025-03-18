
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive;
using DynamicData;
using DynamicData.Alias;
using DynamicData.Kernel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GraphicEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ILogic _figureService;

        public ReactiveCommand<Unit, Unit> NewCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveSelectedCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearAllCommand { get; }
        // public ReactiveCommand<Point, Unit> CanvasClickCommand { get; }
        public ObservableCollection<FigureFactoryViewModel> FigureFactories { get; } = [];
        private readonly ReadOnlyObservableCollection<string> _readonlyFiguresNames;
        public ReadOnlyObservableCollection<string> ReadonlyFigureNames => _readonlyFiguresNames;
        private readonly ReadOnlyObservableCollection<FigureViewModel> _readonlyFigures;
        public ReadOnlyObservableCollection<FigureViewModel> ReadonlyFigures => _readonlyFigures;
        private readonly ReadOnlyObservableCollection<IFigure> _readonlySelectedFigures;
        public ReadOnlyObservableCollection<IFigure> ReadonlySelectedFigures => _readonlySelectedFigures;

        [Reactive]
        public FigureViewModel? SelectedFigure { get; set; }

        public MainWindowViewModel()
        {
            _figureService = new FigureService();
            foreach (var figureName in FigureFabric.AvailableFigures)
            {
                FigureFactories.Add(new() { FigureService = _figureService, FigureType = figureName, MainWindowViewModel = this, IconPath = FigureFabric.AvailableMetadata.First(x => x.Name == figureName).IconPath });
            }

            // Подключение реактивных источников
            _figureService.Connect().Transform(figure => new FigureViewModel(figure))
                .Bind(out _readonlyFigures)
                .Subscribe();

            _figureService.Connect().Select(f => f.Name).SortAndBind(out _readonlyFiguresNames)
                .Subscribe();

            _figureService.ConnectSelections()
                .Bind(out _readonlySelectedFigures)
                .Subscribe();

            // Инициализация команд
            ClearAllCommand = ReactiveCommand.Create(_figureService.ClearAll);

            RemoveSelectedCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedFigure != null)
                    _figureService.RemoveFigure(SelectedFigure.Figure);
            });

            SaveCommand = ReactiveCommand.Create(() => { _figureService.Save("test.svg", "svg"); });
            LoadCommand = ReactiveCommand.Create(() => { _figureService.Load("test.svg", "svg"); });
            NewCommand = ReactiveCommand.Create(() => { _figureService.ClearAll(); });

            // CanvasClickCommand = ReactiveCommand.Create<PointerPressedEventArgs>(args =>
            // {
            //     var isMultiSelect = (args.KeyModifiers & KeyModifiers.Control) != 0;
            //     _figureService.Select(args.GetPosition(MainCanvas), isMultiSelect);
            // });
        }
    }
}
