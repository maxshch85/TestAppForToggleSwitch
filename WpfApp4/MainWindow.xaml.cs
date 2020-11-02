using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
           
            
            BottomPosition = System.Windows.SystemParameters.PrimaryScreenHeight;
            TopPosition = BottomPosition - Height;
            Style style = this.FindResource("WpfApp4.Style.ToggleSwitchEx") as Style;
            ActivateButton.Style = style;
        }
        public double BottomPosition { get; set; }
        public double TopPosition { get; set; }
        
    }
}
