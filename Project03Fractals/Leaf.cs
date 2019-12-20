/* A leaf class that defines what a 3D leaf is
 * Created by: Hyechan Jun
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
    class Leaf
    {
        // The leaf mesh
        MeshGeometry3D myLeaf = new MeshGeometry3D();
        // The leaf color
        private DiffuseMaterial color = new DiffuseMaterial(new SolidColorBrush(Colors.Green));

        /* Explicit constructor for leaves
         * Parameters: size, the size of the leaf
         *              origin, the origin point of the leaf
         *              up, the direction the leaf is pointing
         *              right, the right-hand side of the leaf
         *              models, a group of 3D models to add the leaf to
         */
        public Leaf(double size, Point3D origin, Vector3D up, Vector3D right, Model3DGroup models)
        {
            // Normalize both vectors
            up.Normalize();
            right.Normalize();

            // Find a perpendicular vector to both and normalize that too
            Vector3D perpVec = Vector3D.CrossProduct(up, right);
            perpVec.Normalize();

            // Set magnitude of all vectors to size
            up *= size;
            right *= size;
            perpVec *= size;

            // Create the six other points of the leaf
            //      (leaves are double square pyramids)
            Point3D point2 = origin + up;       // the top point
            Point3D midP = calcMidpoint(origin, point2); // a midpoint to calculate side points with
            // The side points
            Point3D point3 = midP + right;
            Point3D point4 = midP + perpVec;
            Point3D point5 = midP - right;
            Point3D point6 = midP - perpVec;

            // Add all points to the positions array
            myLeaf.Positions.Add(origin);
            myLeaf.Positions.Add(point2);
            myLeaf.Positions.Add(point3);
            myLeaf.Positions.Add(point4);
            myLeaf.Positions.Add(point5);
            myLeaf.Positions.Add(point6);

            // Create all faces of the leaf
            // Every face requires 1 triangle, so 8 total
            myLeaf.TriangleIndices.Add(0);
            myLeaf.TriangleIndices.Add(2);
            myLeaf.TriangleIndices.Add(5);

            myLeaf.TriangleIndices.Add(0);
            myLeaf.TriangleIndices.Add(5);
            myLeaf.TriangleIndices.Add(4);

            myLeaf.TriangleIndices.Add(0);
            myLeaf.TriangleIndices.Add(4);
            myLeaf.TriangleIndices.Add(3);

            myLeaf.TriangleIndices.Add(0);
            myLeaf.TriangleIndices.Add(3);
            myLeaf.TriangleIndices.Add(2);

            myLeaf.TriangleIndices.Add(1);
            myLeaf.TriangleIndices.Add(5);
            myLeaf.TriangleIndices.Add(2);

            myLeaf.TriangleIndices.Add(1);
            myLeaf.TriangleIndices.Add(4);
            myLeaf.TriangleIndices.Add(5);

            myLeaf.TriangleIndices.Add(1);
            myLeaf.TriangleIndices.Add(3);
            myLeaf.TriangleIndices.Add(4);

            myLeaf.TriangleIndices.Add(1);
            myLeaf.TriangleIndices.Add(2);
            myLeaf.TriangleIndices.Add(3);

            // Create a 3D model using the mesh and the color
            GeometryModel3D model = new GeometryModel3D(myLeaf, color);

            // Add it to the model group
            models.Children.Add(model);
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
    }
}
