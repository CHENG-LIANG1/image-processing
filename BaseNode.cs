using System;
using System.Diagnostics;
using img_processor;


// Author: Cheng Liang (Louis)
// Student ID: N10346911

namespace NodeAbstraction
{
    public abstract class Node
    {
        // constant variables that can be used in subclasses
        protected const byte ALPHA_VALUE = 255;
        protected const byte MAX_RGB_VALUE = 255;
        protected const byte MIN_RGB_VALUE = 0;

        private Node _previousNode = null;
        private string parameter { get; }
        private double num { get; }

        /// <summary>
        /// Create a new node with a string parameter
        /// </summary>
        /// <param name="parameter"> the string parameter </param>
        public Node(string parameter)
        {
            this.parameter = parameter;
        }

        /// <summary>
        /// Create a node with no parameter
        /// </summary>
        public Node()
        {
        }

        /// <summary>
        /// create a node with a double parameter
        /// </summary>
        /// <param name="num"> the double parameter</param>
        public Node(double num)
        {
            this.num = num;
        }


        public abstract string GetName();
        public abstract Image Process(Image input);

        /// <summary>
        /// Process the input image and takes optional parameters to diaplay logs and save intermediate result 
        /// </summary>
        /// <param name="input"> Image input </param>
        /// <param name="logging"> a boolean variable </param>
        /// <param name="saveDir"> the directory to save </param>
        /// <returns> Image output </returns>
        public Image StartProcessor(Image input, bool logging = false, string saveDir = "")
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Image output = Process(input);
            stopwatch.Stop();

            if (logging == true)
            {
                Console.WriteLine("\n    Node: " + GetName());
                Console.WriteLine("   Input: Image (" + input.ToString() + ")");
                Console.WriteLine("          Processing...");
                string outputSize = output.ToString();
                Console.WriteLine("          Took:" + stopwatch.Elapsed + "s");
                Console.WriteLine("  Output: Image (" + outputSize + ")");
                if(saveDir != "")
                {
                    Console.WriteLine("Wrote to: " + saveDir + "/" + GetName() + ".png");
                }
            }
            if (saveDir != "")
            {
                output.Write(saveDir + "/" + GetName());
            }
            return output;
        }

        /// <summary>
        /// link the nodes
        /// </summary>
        /// <param name="node"></param>
        public void SetInput(Node node)
        {
            _previousNode = node;
        }

        /// <summary>
        /// Get the output of the linked nodes
        /// </summary>
        /// <param name="input"> Image input </param>
        /// <param name="logging"> a boolean variable </param>
        /// <param name="saveDir"> the directory to save </param>
        /// <returns> Image output </returns>
        public Image GetOutput(Image input, bool logging = false, string saveDir = "")
        {
            if (_previousNode != null)
            {
                input = _previousNode.GetOutput(input, logging, saveDir);
            }
            Image output = StartProcessor(input, logging, saveDir) ;
            return output;
        }

    }
}
