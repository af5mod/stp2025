using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

using DynamicData;

using GraphicEditor.ViewModels;

namespace GraphicEditor.Views
{
    public partial class MainWindow : Window
    {
        Canvas MainCanvasControl {get => MainCanvas;}
        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        public async void Canvas_PointerPressed(object sender,PointerPressedEventArgs args)
        {
            var vm = DataContext as MainWindowViewModel;
            await vm.DeleteSelectedCommand.Execute();
        }

    }
}


