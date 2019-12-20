/* MainWindow.xaml.cs handles code for displaying the main window
 * Created By: Hyechan Jun
 * For CS 212 at Calvin College
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project03Fractals
{
    public partial class MainWindow : Window
    {
        private ModelVisual3D myModels = new ModelVisual3D();       // Visual3D element that is displayed on Viewport3D
        private Model3DGroup modelGroup = new Model3DGroup();       // Group of 3D models that make up the visual elements

        public MainWindow()
        {
            InitializeComponent();
        }

        /* Method that initializes the window when it is first loaded
         * Sets sliders to default values (will be overwritten by ComboBox)
         */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            label1.Content = "Height";
            slider1.Value = 10;
            slider1.Minimum = 10;
            slider1.Maximum = 20;

            label2.Content = "Breadth";
            slider2.Value = 1;
            slider2.Minimum = 1;
            slider2.Maximum = 2;

            label3.Content = "Complexity";
            slider3.Value = 0;
            slider3.Minimum = 0;
            slider3.Maximum = 10;
        }

        /* Method to handle the click of the Draw button
         * Depending on the currently selected index of the ComboBox,
         *      changes the image to be drawn
         */
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // if no fractal is selected, just clear the view and models
            if (fractalComboBox.SelectedIndex == 0)
            {
                modelGroup.Children.Clear();
                myViewport3D.Children.Clear();
            }

            // if the selected fractal is a Fern, draw a fern
            else if (fractalComboBox.SelectedIndex == 1)
            {
                // clear the view and models
                modelGroup.Children.Clear();
                myViewport3D.Children.Clear();

                // make a new Fern, based on the slider values and originating from (0, 0, 0)
                Fern f = new Fern(slider1.Value, slider2.Value, (int)slider3.Value,
                                    new Point3D(0, 0, 0), modelGroup);
            }

            // else if the selected fractal is a tree, draw a tree
            else if (fractalComboBox.SelectedIndex == 2)
            {
                // clear the view and models
                modelGroup.Children.Clear();
                myViewport3D.Children.Clear();

                // if the user checked the check box denoted "leaves"
                if (checkBox1.IsChecked == true)
                {
                    // draw a tree with leaves
                    Tree t = new Tree(slider1.Value, 1, (int)slider2.Value, (int)slider3.Value,
                                        new Point3D(0, 0, 0), modelGroup, true);
                }
                else
                {
                    // otherwise draw a tree with no leaves
                    Tree t = new Tree(slider1.Value, 1, (int)slider2.Value, (int)slider3.Value,
                                        new Point3D(0, 0, 0), modelGroup, false);
                }
            }

            // else if the selected fractal is a mountain (landscape)
            else if (fractalComboBox.SelectedIndex == 3)
            {
                // clear the view and models
                modelGroup.Children.Clear();
                myViewport3D.Children.Clear();

                // draw a new landscape
                Landscape ls = new Landscape(slider1.Value, slider2.Value, (int)slider3.Value, modelGroup);

                // if the user wants trees (checkBox is for trees now)
                if (checkBox1.IsChecked == true)
                {
                    // Select random points on the surface of the landscape and draw trees on them
                    Random rand = new Random();
                    for (int i = 0; i < 5; i++)
                    {
                        // Get the collection of points that make up the landscape positions
                        Point3DCollection landPos = ls.getLandPos();

                        // Pick a random point out of that collection
                        int originInt = (int)(rand.NextDouble() * landPos.Count);

                        // Draw a tree at that point
                        Tree t = new Tree(2, 0.2, 30, 5, landPos[originInt], modelGroup, true);
                    }
                }
            }

            // add light
            DirectionalLight dirLight1 = new DirectionalLight();
            dirLight1.Color = Colors.White;
            dirLight1.Direction = new Vector3D(-1, -1, -1);

            // Put the light in the modelGroup so it illuminates everything
            modelGroup.Children.Add(dirLight1);

            // add the modelGroup to the visual element
            myModels.Content = modelGroup;

            // add the visual element to the viewport
            myViewport3D.Children.Add(myModels);

            // Begin rotating the elements in the viewport (excluding the light)
            rotateStart();
        }

        /* Method that takes care of user clicking the zoom in button
         * Postcondition: camera zooms in by a factor of 2
         */
        private void zoomIn_Click(object sender, RoutedEventArgs e)
        {
            camMain.Position = new Point3D(camMain.Position.X / 2, camMain.Position.Y / 2, camMain.Position.Z / 2);
        }

        /* Method that takes care of user clicking the zoom out button
         * Postcondition: camera zooms out by a factor of 2
         */
        private void zoomOut_Click(object sender, RoutedEventArgs e)
        {
            camMain.Position = new Point3D(camMain.Position.X * 2, camMain.Position.Y * 2, camMain.Position.Z * 2);
        }

        /* Method to start the rotation animation
         * Postcondition: the models on screen begin to rotate but not the light
         */
        private void rotateStart()
        {
            // add axis for rotation
            AxisAngleRotation3D axis = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);

            // add rotation properties to the ModelVisual3D myModels
            RotateTransform3D Rotate = new RotateTransform3D(axis);

            // Since the light is the last thing we added, subtract one from Count()
            //      to ensure that the light does not rotate with the rest
            for (int i = 0; i < modelGroup.Children.Count() - 1; i++)
            { 
                modelGroup.Children[i].Transform = Rotate; // Transform everything minus the light via rotation
            }

            // add animation for rotating the body
            DoubleAnimation RotAngle = new DoubleAnimation();
            RotAngle.From = 0;                                              // from angle 0
            RotAngle.To = 360;                                              // to angle 360
            RotAngle.Duration = new Duration(TimeSpan.FromSeconds(20.0));   // every 20 seconds
            RotAngle.RepeatBehavior = RepeatBehavior.Forever;               // repeat forever

            // register "friendly name" for axis
            // this is required to make sure the rotation affects the models
            NameScope.SetNameScope(myViewport3D, new NameScope());
            myViewport3D.RegisterName("rotateAxis", axis);

            // set rotation angle as target property for new animation storyboard
            Storyboard.SetTargetName(RotAngle, "rotateAxis");
            Storyboard.SetTargetProperty(RotAngle, new PropertyPath(AxisAngleRotation3D.AngleProperty));

            // add storyboard to scene
            Storyboard RotView = new Storyboard();
            RotView.Children.Add(RotAngle);

            // begin animation
            RotView.Begin(myViewport3D);
        }

        /* Method to handle user changing the selection box
         * Calls changeSliders, which changes the value and names of the sliders (for different fractals)
         */
        private void fractalComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            changeSliders(fractalComboBox.SelectedIndex);
        }

        /* Method that changes the sliders for the program
         * Depending on the fractal, the sliders should be used for different things
         * Param: selection, the selected index of the combo box
         */
        private void changeSliders(int selection)
        {
            // if the selection is a fern, use fern sliders and hide the checkBox
            if (selection == 1)
            {
                label1.Content = "Size";
                slider1.Value = 5;
                slider1.Minimum = 5;
                slider1.Maximum = 10;

                label2.Content = "TurnBias";
                slider2.Value = 10;
                slider2.Minimum = 10;
                slider2.Maximum = 45;

                label3.Content = "Complexity";
                slider3.Value = 1;
                slider3.Minimum = 1;
                slider3.Maximum = 4;

                label4.Visibility = Visibility.Hidden;
                checkBox1.Visibility = Visibility.Hidden;
                checkBox1.IsChecked = false;
            }

            // if the selection is a tree, use tree sliders and show the checkBox
            // change the checkBox parameter to leaf
            else if (selection == 2)
            {
                label1.Content = "Height";
                slider1.Value = 2;
                slider1.Minimum = 2;
                slider1.Maximum = 10;

                label2.Content = "Branch Angle";
                slider2.Value = 0;
                slider2.Minimum = 0;
                slider2.Maximum = 90;

                label3.Content = "Complexity";
                slider3.Value = 0;
                slider3.Minimum = 0;
                slider3.Maximum = 5;

                label4.Visibility = Visibility.Visible;
                label4.Content = "Leaves?";
                checkBox1.Visibility = Visibility.Visible;
                checkBox1.IsChecked = false;
            }

            // if the selection is a mountain, use mountain sliders
            // Show the checkBox, change it to Trees
            else if (selection == 3)
            {
                label1.Content = "Height";
                slider1.Value = 0;
                slider1.Minimum = -100;
                slider1.Maximum = 100;

                label2.Content = "Breadth";
                slider2.Value = 50;
                slider2.Minimum = 50;
                slider2.Maximum = 100;

                label3.Content = "Complexity";
                slider3.Value = 0;
                slider3.Minimum = 0;
                slider3.Maximum = 6;

                label4.Visibility = Visibility.Visible;
                label4.Content = "Trees?";
                checkBox1.Visibility = Visibility.Visible;
                checkBox1.IsChecked = false;
            }
        }
    }    
}
