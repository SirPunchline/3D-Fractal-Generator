/* Class that creates a 3D fractal fern using turtle graphics
 *      and L-Systems to be put on a Viewport3D
 * Created By: Hyechan Jun
 * For CS 212 at Calvin College
 * 
 * This fern uses a recursive L-system and turtle graphics to draw the fern
 * Rules for the turtle:
 *   + = turn right
 *   - = turn left
 *   & = pitch down
 *   ^ = pitch up
 *   \\ = roll left
 *   / = roll right
 *   > = decrease the thickness
 *   < = increase the thickness
 *   f = draw branch (and go forward)
 *   g = go forward
 *   [ = save state
 *   ] = restore state
 * Extra rule: any specific values must be represented in exactly 3 digits
 *              and occur between parentheses
 *              e.g. (300)
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
    class Fern
    {
        // the mesh to hold the fern
        private MeshGeometry3D fern = new MeshGeometry3D();
        // the color of the fern
        private DiffuseMaterial color = new DiffuseMaterial(new SolidColorBrush(Colors.ForestGreen));

        /* Explicit constructor for the Fern
         * Parameters: size, the size of the fern
         *              turnBias, the turn bias of each branch
         *              complexity, the number of recursions
         *              origin, the origin point of the fern
         *              modelGroup, the 3D model group to add the fern to
         */
        public Fern(double size, double turnBias, int complexity,
                        Point3D origin, Model3DGroup modelGroup)
        {
            // generate inputStr based on recursive L-System grammar
            //      complexity * 2 because drawing happens every second step
            string inputStr = fract("AAAAB", complexity * 2);

            // run the turtle graphics to generate the fern
            run(origin, size, turnBias, inputStr, modelGroup);

            // create a model for the fern mesh and color
            GeometryModel3D fernModel = new GeometryModel3D(fern, color);

            // Add the model to the model group
            modelGroup.Children.Add(fernModel);
        }

        /* A method that recursively generates a string using L-system grammar
         * Parameters: input, the input string
         *              complexity, the number of recursions
         * Returns: a string that was parsed by the L-system grammar
         */ 
        private string fract(string input, int complexity)
        {
            // output string
            string output = "";

            // base case, just return the input string
            if (complexity == 0)
            {
                return input;
            }

            else
            {
                // Parsing
                for (int i = 0; i < input.Length; i++)
                {
                    switch (input[i])
                    {
                        // if nonterminal A, draw a branch with a split at the end
                        case 'A':
                            output += "+>(040)=(070)/fC";
                            break;
                        // if nonterminal B, draw a branch with no split at the end
                        case 'B':
                            output += "+>(040)=(070)f";
                            break;
                        // if nonterminal C, generate two split branches on either side of the current branch
                        case 'C':
                            output += "[-(090)=(060)AAAAB][+(090)\\(180)=(060)AAAAB]";
                            break;
                        // if terminal, just append it to string
                        default:
                            output += input[i];
                            break;
                    }
                }

                // recurse with complexity - 1
                return fract(output, complexity - 1);
            }
        }

        /* Method to run the string through a decoder that translates the L-System
         *      grammar to turtle movements
         * Parameters: origin, the point of origin
         *              size, the size of each segment
         *              turnBias, the turning angle for each segment
         *              input, the input string that has been run through the recursive
         *                      L-System grammar
         *              modelGroup, the model group to add the 3D models to
         * Postcondition: A fern is added to modelGroup
         */
        private void run(Point3D origin, double size, double turnBias,
                            string input, Model3DGroup modelGroup)
        {
            // Create a new turtle that starts at origin with size (height),
            //          thickness = 0.2, and color of the fern
            Turtle turtle = new Turtle(origin, size, 0.5, color);

            // Random variable to randomize yaw and roll amounts
            Random rand = new Random();
            int randTurn = rand.Next((int)turnBias - 5, (int)turnBias + 5);

            // a variable to hold any numerical values
            double num;

            // counter variable
            int i = 0;
            while (i < input.Length)
            {
                // If there is a numerical value, set num to that value
                if (i != input.Length - 1 && input[i + 1] == '(')
                {
                    string substr = input.Substring(i + 2, 3);
                    num = Double.Parse(substr);
                }

                // Otherwise just set num to the random angle
                else num = randTurn;

                // switch statement for turtle movements (self-explanatory)
                switch (input[i])
                {
                    case '+':
                        turtle.turnLeft(num);
                        break;
                    case '-':
                        turtle.turnRight(num);
                        break;
                    case '&':
                        turtle.pitchDown(num);
                        break;
                    case '^':
                        turtle.pitchUp(num);
                        break;
                    case '<':
                        turtle.thicken(num);
                        break;
                    case '\\':
                        turtle.rollLeft(num);
                        break;
                    case '/':
                        turtle.rollRight(num);
                        break;
                    case '>':
                        turtle.narrow(num);
                        break;
                    case '=':
                        turtle.setHeightPercent(num);
                        break;
                    case 'f':
                        turtle.draw();
                        break;
                    case 'g':
                        turtle.move(num);
                        break;
                    case '[':
                        turtle.save();
                        break;
                    case ']':
                        turtle.restore();
                        break;
                    // If unknown character, just do nothing
                    default:
                        break;
                }

                // increment counter
                i++;
            }

            // Set the modelGroup's models to the turtle's models
            for (int j = 0; j < turtle.getModels().Children.Count(); j++)
            {
                modelGroup.Children.Add(turtle.getModels().Children[j]);
            }
        }
    }
}
