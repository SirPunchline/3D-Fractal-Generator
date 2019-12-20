/* Class that creates a fractal tree using turtle graphics
 *      and L-Systems to be put on a Viewport3D
 * Created By: Hyechan Jun
 * For CS 212 at Calvin College
 * 
 * This tree uses a recursive L-system and turtle graphics to draw the tree
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
 *   * = draw a leaf
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
    class Tree
    {
        // The color of the tree
        private DiffuseMaterial color = new DiffuseMaterial(new SolidColorBrush(Colors.Brown));

        /* Explicit constructor for the tree
         * Parameters: height, the relative height of the tree
         *              thickness, the thickness of the trunk and each branch
         *              branchAngle, the angle that each branch branches off at
         *              complexity, the recursive complexity of the tree
         *              origin, the point which the tree originates from
         *              modelGroup, the group of 3D models to which the tree will be added
         *              leaves, a boolean to have leaves on the tree or not
         * Postcondition: a tree is created and added to the modelGroup
         */
        public Tree(double height, double thickness, int branchAngle, int complexity,
                        Point3D origin, Model3DGroup modelGroup, bool leaves)
        {
            // create the string that represents the tree using the L-System
            // Complexity is multiplied by 2 because new branches are drawn every 2 recursions
            string inputStr = fract("fA", height, branchAngle, complexity * 2);

            // Decode the L-System to draw the tree
            run(origin, height, thickness, branchAngle, inputStr, modelGroup, leaves);
        }

        /* Method that takes an input string and decodes it according to the L-System grammar
         * Parameters: input, the input string
         *              height, the height of a tree (used to calculate leaf size)
         *              branchAngle, the angle at which branches branch
         *              complexity, the number of recursion levels
         * Returns: a string that contains the recursively defined tree
         */
        private string fract(string input, double height, int branchAngle, int complexity)
        {
            string output = "";             // the final string starts out empty
            Random rand = new Random();     // a random variable to introduce randomness into the tree
            int rotAngle = rand.Next(branchAngle, branchAngle + 5);     // randomize the branchAngle a little
            int leafLengthInt = (int)(height / 2);          // Set the leaf size
            string leafLength = "00" + leafLengthInt;       // Since leaf size will never exceed 10, add two 0s
                                                            // to satisfy the three-digit rule of the L-System
            
            // Pad the rotation angle with 0s to satisfy the 3-digit rule
            string rotString = "";      // A string containing the angle of rotation

            // Pad with 2 zeroes if the angle is less than 10
            if (rotAngle < 10)
            {
                rotString += "00" + rotAngle;
            }
            // Pad with 1 zero if 10 <= angle < 100
            else if (rotAngle < 100)
            {
                rotString += "0" + rotAngle;
            }
            // Pad with no zeroes if angle >= 100
            else
            {
                rotString += rotAngle;
            }

            // Base case: recursive complexity is 0
            if (complexity == 0)
            {
                // Just return the string given
                return input;
            }
            // Otherwise use the L-System grammar to replace parts of the string
            else
            {
                for (int i = 0; i < input.Length; i++)
                {
                    switch (input[i])
                    {
                        // If there is a nonterminal A in the string, replace with three new branches
                        case 'A':
                            output += "f>(030)B";
                            int randTurn = rand.Next(1, 5);
                            for (int j = 0; j < randTurn; j++)
                            {
                                output += "\\(" + rotString + ")";
                            }
                            output += "B";
                            randTurn = rand.Next(2, 4);
                            for (int j = 0; j < randTurn; j++)
                            {
                                output += "\\(" + rotString + ")";
                            }
                            output += "B";
                            break;
                        // If there is a nonterminal B in the string, draw a branch at a randomly rotated angle
                        case 'B':
                            output += "[^^fL\\(" + rotString + ")\\(" + rotString +
                                        ")\\(" + rotString + ")AL]";
                            break;
                        // If there is a nonterminal L in the string, draw a series of leaves
                        case 'L':
                            // Leaves can be generated in one of 3 random ways
                            int select = (int)(rand.NextDouble() * 3);
                            if (select == 0)
                                output += "[^(060)[*(" + leafLength + ")g(" + leafLength +
                                            ")*(" + leafLength + ")g(" + leafLength + ")*(" +
                                            leafLength + ")]" + "+(050)g(" + leafLength + ")g(" +
                                            leafLength + ")*(" + leafLength + ")g(" + leafLength +
                                            ")*(" + leafLength + ")]";
                            else if (select == 1)
                                output += "[^(060)*(" + leafLength + ")]";
                            else output += "[&(070)*(" + leafLength + ")]";
                            break;
                        // Default case: if the character is a terminal, just copy it down
                        default:
                            output += input[i];
                            break;
                    }
                }

                // Return the result of recursing at a lower complexity
                return fract(output, height, branchAngle, complexity - 1);
            }
        }

        /* Method to run the string through a decoder that translates the L-System
         *      grammar to turtle movements
         * Parameters: origin, the point of origin
         *              height, the height of each segment
         *              thickness, the thickness of each segment
         *              defaultVal, a default value for angles
         *              input, the input string that has been run through the recursive
         *                      L-System grammar
         *              modelGroup, the model group to add the 3D models to
         *              leaves, a boolean that asks whether to draw leaves
         * Postcondition: A tree is added to modelGroup
         */
        private void run(Point3D origin, double height, double thickness,
                        double defaultVal, string input, Model3DGroup modelGroup,
                        bool leaves)
        {
            // Make a new turtle to draw the tree
            Turtle turtle = new Turtle(origin, height, thickness, color);

            // Create a variable to hold any specific numbers
            double num;

            // Counter variable for while loop
            int i = 0;
            while (i < input.Length)
            {
                // Check for parentheses, which denote specific numbers
                if (i != input.Length - 1 && input[i + 1] == '(')
                {
                    // If there is a parenthesis, get the next three characters
                    //      (which should all be numbers) and put them in num
                    string substr = input.Substring(i + 2, 3);
                    num = Double.Parse(substr);
                }
                // Otherwise reset num to defaultVal
                else num = defaultVal;
                // Switch statement to control the turtle by its rules
                // Fairly self-explanatory
                switch(input[i])
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
                    case '*':
                        // If the user wants leaves, give them leaves
                        if (leaves) turtle.drawLeaf(num);
                        break;
                    default:
                        // If an unrecognized character is in the string, just skip it
                        break;
                }
                // Increment i
                i++;
            }

            // Get the models from the turtle and add them to modelGroup
            for (int j = 0; j < turtle.getModels().Children.Count(); j++)
            {
                modelGroup.Children.Add(turtle.getModels().Children[j]);
            }
        }
    }
}
