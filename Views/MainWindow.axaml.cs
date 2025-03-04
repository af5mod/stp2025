using System.Collections.ObjectModel;

using Avalonia;
using Avalonia.Controls;
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

    }
}


