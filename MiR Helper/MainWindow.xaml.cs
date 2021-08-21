using MiR_Helper.rest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MiR_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Make a new data source object
            robot currentRobot = new robot();

            // New binding object using the path of 'Name' for whatever source object is used
            var ipBindingObject = new Binding("IP");

            // Configure the binding
            ipBindingObject.Mode = BindingMode.TwoWay;
            ipBindingObject.Source = currentRobot;
            ipBindingObject.Converter = IPConverter.Instance;
            ipBindingObject.ConverterCulture = new CultureInfo("en-US");

            // Set the binding to a target object. The TextBlock.Name property on the NameBlock UI element
            BindingOperations.SetBinding(RobotIP, TextBlock.TextProperty, ipBindingObject);
        }

        private void ButtonAddName_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
