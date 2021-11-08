using MiR_Helper.rest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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


        private Robot robot;
     

      

        public string StatusId
        {
            get { return (string)GetValue(dependencyStatusId); }
            set { SetValue(dependencyStatusId, value); }
        }

        public string PosX
        {
            get { return (string)GetValue(dependencyPosX); }
            set { SetValue(dependencyPosX, value); }
        }

        public string PosY
        {
            get { return (string)GetValue(dependencyPosY); }
            set { SetValue(dependencyPosY, value); }
        }

        public string Orient
        {
            get { return (string)GetValue(dependencyOrient); }
            set { SetValue(dependencyOrient, value); }
        }

        public string VelocityL
        {
            get { return (string)GetValue(dependencyVelocityL); }
            set { SetValue(dependencyVelocityL, value); }
        }

        public string VelocityA
        {
            get { return (string)GetValue(dependencyVelocityA); }
            set { SetValue(dependencyVelocityA, value); }
        }

        public static readonly DependencyProperty dependencyStatusId =
            DependencyProperty.Register("StatusId", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));


        public static readonly DependencyProperty dependencyPosX =
            DependencyProperty.Register("PosX", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));


        public static readonly DependencyProperty dependencyPosY =
            DependencyProperty.Register("PosY", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));


        public static readonly DependencyProperty dependencyOrient =
            DependencyProperty.Register("Orient", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));


        public static readonly DependencyProperty dependencyVelocityL =
            DependencyProperty.Register("VelocityL", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        
        public static readonly DependencyProperty dependencyVelocityA =
            DependencyProperty.Register("VelocityA", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));



        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            robot = new Robot();
            robot.IpAddress = "192.168.17.174";
            UpdateWindowValues();
            stopTest.IsEnabled = false;

        }

        private async void UpdateWindowValues()
        {
            await robot.UpdateStatus();

            this.StatusId = robot.Status.state_id.ToString();
            this.PosX = robot.Status.position.x.ToString("0.##");
            this.PosY = robot.Status.position.y.ToString("0.##");
            this.Orient = robot.Status.position.orientation.ToString("0.##");
            this.VelocityL = robot.Status.velocity.linear.ToString("0.##");
            this.VelocityA = robot.Status.velocity.angular.ToString("0.##");
        }

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    // Make a new data source object
        //    Robot currentRobot = new Robot();

           
        //    // New binding object using the path of 'Name' for whatever source object is used
        //    //    var ipBindingObject = new Binding("IP");

        //    //    // Configure the binding
        //    //    ipBindingObject.Mode = BindingMode.TwoWay;
        //    //    ipBindingObject.Source = currentRobot;
        //    //    ipBindingObject.Converter = IPConverter.Instance;
        //    //    ipBindingObject.ConverterCulture = new CultureInfo("en-US");



        //    //    // Set the binding to a target object. The TextBlock.Name property on the NameBlock UI element
        //    //    BindingOperations.SetBinding(RobotIP, TextBlock.TextProperty, ipBindingObject);

        //}

        private void ButtonAddName_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void updateStatus_Click(object sender, RoutedEventArgs e)
        {
            await robot.UpdateStatus();
            UpdateWindowValues();
        }

        private async void startTest_Click(object sender, RoutedEventArgs e)
        {
            stopTest.IsEnabled = true;
            startTest.IsEnabled = false;
            robot.UpdateTestStatus += OnUpdateTestStatus;
            var testData = await robot.StoppingDistanceTest();
            robot.SaveDataToFile(testData);
            startTest.IsEnabled = true;
            stopTest.IsEnabled = false;
        }

        private void stopTest_Click(object sender, RoutedEventArgs e)
        {
            startTest.IsEnabled = true;
            stopTest.IsEnabled = false;
            robot.StopTest = true;
        }

        public void OnUpdateTestStatus(object source, MirTestEventArgs args)
        {
            this.StatusId = args.TestData.MiRStatus.state_id.ToString();
            this.PosX = args.TestData.MiRStatus.position.x.ToString("0.##");
            this.PosY = args.TestData.MiRStatus.position.y.ToString("0.##");
            this.Orient = args.TestData.MiRStatus.position.orientation.ToString("0.##");
            this.VelocityL = args.TestData.MiRStatus.velocity.linear.ToString("0.##");
            this.VelocityA = args.TestData.MiRStatus.velocity.angular.ToString("0.##");
        }

    }
}
