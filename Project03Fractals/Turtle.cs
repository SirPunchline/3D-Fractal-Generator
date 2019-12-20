/* Class for rendering turtle graphics in 3D
 * Created By: Hyechan Jun
 * For CS 212 at Calvin College
 * 
 * Much of the following code was adapted from abiusx's project on Github
 * Link to his project: https://github.com/abiusx/L3D
 * I had to change several things to make it work with WPF and C#
 * 
 * There is also code from other sources I found online, links are on the separate methods
 * All code found online is from tutorials or open source content
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
    class Turtle
    {
        private Point3D position;       // the current position of the turtle
        private Vector3D direction = new Vector3D(0, 1, 0); // the direction the turtle is facing
        private Quaternion quatDir;     // the direction a Quaternion is facing (used for rotations)
        private Vector3D right = new Vector3D(1, 0, 0);     // the direction perpendicular and to the right of the turtle
        private double thickness;       // the thickness of each segment drawn
        private double height;          // the height of each segment drawn (a.k.a the length of each segment)
        private Model3DGroup myModels = new Model3DGroup();     // the modelGroup to hold all drawn meshes
        private Stack<Turtle> state = new Stack<Turtle>();      // the stack to save turtle states
        private DiffuseMaterial baseColor = new DiffuseMaterial();  // the base color of the meshes in modelGroup

        /* Default initialization statement
         * sets position to (0, 0, 0)
         * sets quatDir axis to direction
         * sets thickness to 1
         * sets height to 1
         * sets baseColor to Red
         */
        public Turtle()
        {
            position = new Point3D(0, 0, 0);
            thickness = 1;
            quatDir = new Quaternion(direction, 0);
            height = 1;
            baseColor = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
        }

        /* Explicit initialization statement
         * Parameters: pos, the position of the turtle
         *              high, the height of each segment
         *              thick, the thickness of each segment
         *              color, the color of the mesh
         * Also sets the Quaternion axis to direction
         */
        public Turtle(Point3D pos, double high, double thick, DiffuseMaterial color)
        {
            position = pos;
            thickness = thick;
            quatDir = new Quaternion(direction, 0);
            height = high;
            baseColor = color;
        }

        /* Getter method for models
         * Return: myModels, the modelGroup with the meshes drawn by the turtle
         */
        public Model3DGroup getModels()
        {
            return myModels;
        }

        /* Method to set the height of each segment
         * Parameter: highPercent, a percent of the current height
         * Postcondition: height is set to highPercent
         */
        public void setHeightPercent(double highPercent)
        {
            height *= (highPercent / 100);
        }

        /* Method to thicken the segment
         * Increases the thickness by a set percentage
         * Parameter: thickPercent, the percentage to thicken by
         */
        public void thicken(double thickPercent)
        {
            thickness = thickness + thickness * (thickPercent / 100);
        }

        /* Method to narrow the segment
         * Calls thicken() with a negative percent
         */
        public void narrow(double thinPercent)
        {
            thicken(-thinPercent);
        }

        /* Method to turn the turtle to the right
         * Parameter: Angle, the angle to turn right by
         * Postcondition: turtle is now facing right by angle amount
         */
        public void turnRight(double angle)
        {
            // create a new axis that is perpendicular to direction and right
            Vector3D axis = Vector3D.CrossProduct(direction, right);
            axis.Normalize();

            // Set the axis of the Quaternion to axis
            // What this does is create a new Quaternion that defines an
            //      axis around which the turtle rotates, causing a turn
            quatDir = new Quaternion(axis, angle);

            // rotate both direction and right along the new axis, turning the turtle
            direction = QuatToVect(quatDir, 1, direction);
            right = QuatToVect(quatDir, 1, right);

            // normalize the vectors for future calculations
            direction.Normalize();
            right.Normalize();
        }

        /* Method to turn the turtle left
         * Calls turnRight() with a negative angle
         */
        public void turnLeft(double angle)
        {
            turnRight(-angle);
        }

        /* Method to pitch the turtle up (rotate it along the right axis)
         * Parameter: angle, the angle to pitch by
         */
        public void pitchUp(double angle)
        {
            // Create a new Quaternion with its axis set to right and the
            //      rotation angle set to parameter angle
            quatDir = new Quaternion(right, angle);

            // Rotate direction along the Quaternion's axis
            direction = QuatToVect(quatDir, 1, direction);

            // Normalize direction for future calculations
            direction.Normalize();
        }

        /* Method to pitch the turtle down (rotate it down along the right axis)
         * Calls pitchUp() with a negative angle
         */
        public void pitchDown(double angle)
        {
            pitchUp(-angle);
        }

        /* Method to roll the turtle right (rotate it along its direction)
         * Parameter: angle, the angle to rotate by
         */
        public void rollRight(double angle)
        {
            // Create a new Quaternion along the direction axis with rotation angle
            quatDir = new Quaternion(direction, angle);

            // Rotate the right vector to roll the turtle
            right = QuatToVect(quatDir, 1, right);

            // Normalize for future calculations
            right.Normalize();
        }

        /* Method to roll the turtle left (rotate it along its direction)
         * Calls rotateRight() with a negative angle
         */
        public void rollLeft(double angle)
        {
            rollRight(-angle);
        }

        /* Method to move the turtle along a set distance
         * Parameter: distance, the distance to move
         */
        public void move(double distance)
        {
            // Normalize the direction to set its magnitude to 1
            direction.Normalize();

            // if a distance of 0 is given, default to moving up to a segment's height (aka length)
            if (distance == 0) position += direction * height;

            // else move in direction up to distance
            else position += direction * distance;
        }

        /* Method to save the turtle's current position
         * Postcondition: turtle is saved to stack
         */
        public void save()
        {
            // Make a copy of this turtle and give it all the same variable parameters
            Turtle t = new Turtle();
            t.position = position;
            t.direction = direction;
            t.quatDir = quatDir;
            t.right = right;
            t.thickness = thickness;
            t.height = height;

            // Push it to the stack
            state.Push(t);
        }

        /* Method to restore a saved turtle state
         * Postcondition: turtle is loaded from stack
         */
        public void restore()
        {
            // Get a copy of the turtle from the top of the stack
            Turtle t = state.Peek();

            // Pop the stack
            state.Pop();

            // Set all current variables to the popped turtle's
            position = t.position;
            direction = t.direction;
            quatDir = t.quatDir;
            right = t.right;
            thickness = t.thickness;
            height = t.height;
        }

        /* Method to draw a cylinder along the turtle's direction
         * Postcondition: a cylinder is drawn from the turtle's current position
         *                  to the point one segment height away in direction
         */
        public void draw()
        {
            // Draw the cylinder and store it in myModels
            AddSmoothCylinder(myModels, baseColor, position, direction * height, thickness, 20);

            // Move up to the height
            move(height);
        }

        /* Specialized method to draw leaves on trees
         * Parameter: Size, the size of the leaf
         * Postcondition: a leaf is drawn perpendicular to the turtle's current position
         */
        public void drawLeaf(double size)
        {
            // Call the Leaf constructor and store the result in myModels
            Leaf l = new Leaf(size, position, direction, right, myModels);
        }

        /* Method to convert a Quaternion to a Vector with a certain magnitude
         * Used to rotate vectors, since Quaternions can't do that on their own
         * I did not figure this out; Credit to arussell from CodeProject
         * Link to their website:
         * (https://www.codeproject.com/articles/855693/d-l-system-with-csharp-and-wpf)
         * Parameters: q, a Quaternion with an axis and an angle of rotation
         *              magnitude, the desired magnitude of the resultant vector
         *              nextDir, a vector that we want to rotate
         */
        private Vector3D QuatToVect(Quaternion q, double magnitude, Vector3D nextDir)
        {
            // Create a Rotation matrix
            Matrix3D m = Matrix3D.Identity;

            // Set the rotation matrix according to the Quaternion
            m.Rotate(q);

            // Transform the vector using the rotation matrix
            nextDir = m.Transform(nextDir * magnitude);

            // Return the vector
            return nextDir;
        }

        /* Method to create a cylinder with smooth sides
         * I did not figure this out; Credit to C#Helper.com for hosting this code
         * Link to his website:
         * (http://csharphelper.com/blog/2015/04/draw-smooth-cylinders-using-wpf-and-c/)
         * I only added some comments and changed it to work with a modelGroup instead of a mesh
         * Parameters: myModels, a modelGroup in which to store the cylinder
         *              color, the color of the cylindre
         *              end_point, a point at one end of the cylinder
         *              axis, the axis around which the cylinder is built
         *              radius, the radius of the cylinder
         *              num_sides the number of sides of the cylinder (more = smoother)
         */
        private void AddSmoothCylinder(Model3DGroup myModels, DiffuseMaterial color,
            Point3D end_point, Vector3D axis, double radius, int num_sides)
        {
            // Define new mesh to be the cylinder
            MeshGeometry3D mesh = new MeshGeometry3D();

            // Get two vectors perpendicular to the axis.
            // These will be used to create the points along the radius
            Vector3D v1;

            // If the axis extrudes into the Z axis
            if ((axis.Z < -0.01) || (axis.Z > 0.01))
                // Create a perpendicular vector (perpendicular b/c of dot product)
                v1 = new Vector3D(axis.Z, axis.Z, -axis.X - axis.Y);

            else // if the axis is perpendicular to the Z axis
                // Create a perpendicular vector
                v1 = new Vector3D(-axis.Y - axis.Z, axis.X, axis.X);

            // Find another vector perpendicular to both axis and v1 using cross product
            Vector3D v2 = Vector3D.CrossProduct(v1, axis);

            // Make the vectors have length radius.
            v1 *= (radius / v1.Length);
            v2 *= (radius / v2.Length);

            // Make the top end cap.
            // Make the end point.
            int pt0 = mesh.Positions.Count; // Index of end_point
            mesh.Positions.Add(end_point);

            // Make the top points.
            double theta = 0;                           // Initial angle
            double dtheta = 2 * Math.PI / num_sides;    // Change in angle (between points along radius)
            for (int i = 0; i < num_sides; i++)
            {
                mesh.Positions.Add(end_point +          // Add a point 
                    Math.Cos(theta) * v1 +              // using the cosine of v1
                    Math.Sin(theta) * v2);              // and the sine of v2 to transform the point
                theta += dtheta;                        // Increase angle
            }

            // Make the top triangles.
            // Triangles are made by connecting the end_point to two points along the radius
            int pt1 = mesh.Positions.Count - 1; // Index of last point.
            int pt2 = pt0 + 1;                  // Index of first point.
            for (int i = 0; i < num_sides; i++)
            {
                mesh.TriangleIndices.Add(pt0);      // end_point
                mesh.TriangleIndices.Add(pt1);      // first point on radius
                mesh.TriangleIndices.Add(pt2);      // second point on radius
                pt1 = pt2++;
            }

            // Make the bottom end cap.
            // Make the end point.
            pt0 = mesh.Positions.Count; // Index of end_point2.

            // end_point2 is located on the other side of axis
            Point3D end_point2 = end_point + axis;
            mesh.Positions.Add(end_point2);

            // Make the bottom points the same way as the top
            theta = 0;
            for (int i = 0; i < num_sides; i++)
            {
                mesh.Positions.Add(end_point2 +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2);
                theta += dtheta;
            }

            // Make the bottom triangles.
            pt1 = mesh.Positions.Count - 1; // Index of last point.
            pt2 = pt0 + 1;                  // Index of first point.
            for (int i = 0; i < num_sides; i++)
            {
                mesh.TriangleIndices.Add(pt0);      // end_point2
                mesh.TriangleIndices.Add(pt2);      // first point on radius
                mesh.TriangleIndices.Add(pt1);      // second point on radius
                pt1 = pt2++;
            }

            // Make the sides.
            // Add the points to the mesh.
            // Since WPF Media3D meshes smooth over connected points,
            // the end and side points must be kept separate, resulting in
            // doubling up of points along the edge
            int first_side_point = mesh.Positions.Count;
            theta = 0;
            for (int i = 0; i < num_sides; i++)
            {
                Point3D p1 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                mesh.Positions.Add(p1);         // double up top points
                Point3D p2 = p1 + axis;
                mesh.Positions.Add(p2);         // double up bottom points
                theta += dtheta;
            }

            // Make the side triangles.
            pt1 = mesh.Positions.Count - 2;     // pick four points for each segment of the cylinder
            pt2 = pt1 + 1;
            int pt3 = first_side_point;
            int pt4 = pt3 + 1;
            for (int i = 0; i < num_sides; i++)
            {
                // draw a rectangle over that segment, with shared sides so smoothing occurs
                mesh.TriangleIndices.Add(pt1);
                mesh.TriangleIndices.Add(pt2);
                mesh.TriangleIndices.Add(pt4);

                mesh.TriangleIndices.Add(pt1);
                mesh.TriangleIndices.Add(pt4);
                mesh.TriangleIndices.Add(pt3);

                // increment points, going by 2 because the pattern for points is
                //      bottom end, top end, bottom end, top end, ...
                pt1 = pt3;
                pt3 += 2;
                pt2 = pt4;
                pt4 += 2;
            }

            // Create a model to contain the mesh and give it a color
            GeometryModel3D model = new GeometryModel3D(mesh, color);

            // Add it to the provided myModels parameter
            myModels.Children.Add(model);
        }
    }
}
