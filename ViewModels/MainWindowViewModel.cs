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
using System.Reactive.Linq;

namespace GraphicEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ILogic _figureService;
        public ReactiveCommand<Unit, Unit> ClearAllCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteSelectedCommand { get; }
        // public ReactiveCommand<Point, Unit> CanvasClickCommand { get; }

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
