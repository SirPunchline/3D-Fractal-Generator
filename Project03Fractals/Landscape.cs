/* Class that creates a fractal landscape to be put on a Viewport3D
 * Created By: Hyechan Jun
 * For CS 212 at Calvin College
 * 
 * This class utilizes the midpoint displacement method of generating landscapes
 * Can draw mountains or valleys (really just inverted mountains...)
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
    class Landscape
    {
        // the mesh that holds the land
        private MeshGeometry3D myLand = new MeshGeometry3D();
        // the color of the land
        private DiffuseMaterial color = new DiffuseMaterial(new SolidColorBrush(Colors.DarkOliveGreen));

        /* Explicit constructor for the landscape
         * Parameters: height, the height of the landscape (negative for valleys)
         *              breadth, the breadth of the landscape
         *              complexity, the number of recursions
         *              modelGroup, the 3D model group to add the landscape to
         */
        public Landscape(double height, double breadth, int complexity, Model3DGroup modelGroup)
        {
            // create the landscape's base shape
            myLand = triangle(height, breadth);

            // use the midpoint displacement formula to recursively make landscape
            myLand = fract(height, complexity, myLand);

            // create new model with myLand mesh and color
            GeometryModel3D geo = new GeometryModel3D(myLand, color);

            // add the model to the model group
            modelGroup.Children.Add(geo);
        }

        /* Getter method for the collection of points on the landscape
         * Return: the collection of points that make up the landscape
         */
        public Point3DCollection getLandPos()
        {
            return myLand.Positions;
        }

        /* Method to create the initial pyramid
         * Parameters: height, the height of the pyramid
         *              breadth, the breadth of the pyramid
         * Return: myTriangle, a mesh containing the pyramid
         */
        private MeshGeometry3D triangle(double height, double breadth)
        {
            // the mesh that will become the pyramid
            MeshGeometry3D myTriangle = new MeshGeometry3D();

            // the four points of the pyramid
            myTriangle.Positions.Add(new Point3D(breadth, 0, breadth));     // corner 1 of pyramid
            myTriangle.Positions.Add(new Point3D(0, height, 0));            // peak of pyramid
            myTriangle.Positions.Add(new Point3D(-breadth, 0, breadth));    // corner 2 of pyramid
            myTriangle.Positions.Add(new Point3D(0, 0, -breadth));          // corner 3 of pyramid

            // make the sides (each side is a triangle, don't need a bottom)
            myTriangle.TriangleIndices.Add(0);
            myTriangle.TriangleIndices.Add(1);
            myTriangle.TriangleIndices.Add(2);

            myTriangle.TriangleIndices.Add(3);
            myTriangle.TriangleIndices.Add(1);
            myTriangle.TriangleIndices.Add(0);

            myTriangle.TriangleIndices.Add(2);
            myTriangle.TriangleIndices.Add(1);
            myTriangle.TriangleIndices.Add(3);

            return myTriangle;
        }

        /* Method to compute the midpoint of a 3D line segment
         * Parameters: first and second, both Point3D objects
         * Return: the midpoint between first and second
         */
        private Point3D calcMidpoint(Point3D first, Point3D second)
        {
            Point3D mid = new Point3D((first.X + second.X) / 2,
                                        (first.Y + second.Y) / 2,
                                        (first.Z + second.Z) / 2);
            return mid;
        }

        /* Method to recursively generate terrain using the midpoint displacement formula
         * Parameters: height, the height of the landscape
         *              complexity, the number of recursions
         *              land, a mesh containing the landscape
         * Return: the altered land
         */
        private MeshGeometry3D fract(double height, int complexity, MeshGeometry3D land)
        {
            // new MeshGeometry to hold the fractal-ized version of the landscape
            MeshGeometry3D newLand = new MeshGeometry3D();

            // base case, if complexity is 0 then just return the given landscape
            if (complexity == 0) return land;
            else
            {
                // Loop through the triangle indices, getting each triangle in the mesh
                for (int triangle = 0;
                    triangle < land.TriangleIndices.Count - 1;
                    triangle += 3)
                {
                    // get the three corners of the triangle
                    Point3D point1 = land.Positions[land.TriangleIndices[triangle]];
                    Point3D point2 = land.Positions[land.TriangleIndices[triangle + 1]];
                    Point3D point3 = land.Positions[land.TriangleIndices[triangle + 2]];

                    // find the midpoints of the three sides of the triangle
                    Point3D midpoint1 = calcMidpoint(point1, point2);
                    Point3D midpoint2 = calcMidpoint(point2, point3);
                    Point3D midpoint3 = calcMidpoint(point3, point1);

                    // add the current triangle corners to newLand's Positions array
                    newLand.Positions.Add(point1);
                    newLand.Positions.Add(point2);
                    newLand.Positions.Add(point3);

                    // Check for overlapping points between triangles
                    // if no overlap, add the current midpoints to newLand's Positions array
                    if (!newLand.Positions.Contains(midpoint1)) newLand.Positions.Add(midpoint1);
                    if (!newLand.Positions.Contains(midpoint2)) newLand.Positions.Add(midpoint2);
                    if (!newLand.Positions.Contains(midpoint3)) newLand.Positions.Add(midpoint3);

                    // Get positions of the six points in newLand's Positions array
                    // These will be used for setting the triangle indices
                    int point1Pos = newLand.Positions.IndexOf(point1);
                    int point2Pos = newLand.Positions.IndexOf(point2);
                    int point3Pos = newLand.Positions.IndexOf(point3);
                    int midpoint1Pos = newLand.Positions.IndexOf(midpoint1);
                    int midpoint2Pos = newLand.Positions.IndexOf(midpoint2);
                    int midpoint3Pos = newLand.Positions.IndexOf(midpoint3);

                    // Split the triangle with corners point1, point2, and point3
                    // into four new triangles
                    newLand.TriangleIndices.Add(point1Pos);
                    newLand.TriangleIndices.Add(midpoint1Pos);
                    newLand.TriangleIndices.Add(midpoint3Pos);

                    newLand.TriangleIndices.Add(midpoint1Pos);
                    newLand.TriangleIndices.Add(point2Pos);
                    newLand.TriangleIndices.Add(midpoint2Pos);

                    newLand.TriangleIndices.Add(midpoint3Pos);
                    newLand.TriangleIndices.Add(midpoint2Pos);
                    newLand.TriangleIndices.Add(point3Pos);

                    newLand.TriangleIndices.Add(midpoint3Pos);
                    newLand.TriangleIndices.Add(midpoint1Pos);
                    newLand.TriangleIndices.Add(midpoint2Pos);
                }

                // Randomize the heights of the points
                MeshGeometry3D tempLand = new MeshGeometry3D(); // temporary variable to hold the new randomization
                Random rand = new Random();                     // random variable to generate random doubles

                for (int i = 0; i < newLand.Positions.Count; i++)
                {
                    tempLand.Positions.Add(new Point3D(newLand.Positions[i].X,
                                                        newLand.Positions[i].Y + (rand.NextDouble() - 0.5) * height,
                                                        newLand.Positions[i].Z));
                }

                // Set newLand's positions to the randomized tempLand's positions
                newLand.Positions = tempLand.Positions;

                // recurse with height / 2 (to make the height randomization less drastic)
                //              complexity - 1 (reduce the recursion amount)
                //              newLand (the changed mesh)
                return fract(height / 2, complexity - 1, newLand);
            }
        }
    }
}
