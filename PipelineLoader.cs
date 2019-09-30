using System;
using System.Collections.Generic;
using System.IO;
using NodeAbstraction;
using Nodes;

// Author: Cheng Liang (Louis)
// Student ID: N10346911

namespace img_processor
{
    class PipelineLoader
    {
        /// <summary>
        /// create a linked node using a pipeline file path
        /// </summary>
        /// <param name="path"> the path of the pipeline file </param>
        /// <returns> the final node (linked) </returns>
        public static Node LoadPipeline(string path)
        {
            List<Node> nodeList = new List<Node>();

            string[] lines = File.ReadAllLines(path);
            string kernelName = "";
            double noisePercent = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] result1 = lines[i].Split(' ');
                string[] result2 = result1[0].Split('=');

                if (lines[i][0] != '#') // comment a line by adding a "#" at the beginning of each line
                {
                    // if the length of the splited line is greater than one, it means that the nodes require some parameters
                    if (result1.Length > 1)
                    {
                        string[] value = result1[1].Split('=');
                        if (value[1] == "edge" || value[1] == "sharpen" || value[1] == "blur")
                        {
                            kernelName = value[1];
                        }
                        else if (double.TryParse(value[1], out noisePercent))
                        {
                            noisePercent = double.Parse(value[1]);
                        }
                    }

                    switch(result2[result2.Length -1])
                    {
                        case "convolve":
                            Node convolveNode = new N_Convolve(kernelName);
                            nodeList.Add(convolveNode);
                            break;
                        case "noise":
                            Node noiseNode = new N_Noise(noisePercent);
                            nodeList.Add(noiseNode);
                            break;
                        case "vignette":
                            Node vignetteNode = new N_Vignette();
                            nodeList.Add(vignetteNode);
                            break;
                        case "greyscale":
                            Node greyscaleNode = new N_ToGreyScale();
                            nodeList.Add(greyscaleNode);
                            break;
                        default:
                            throw new NodeException("  Node " + result2[result2.Length - 1] + " does not exist!"); // throw an exception if the node does not exist
                    }
                }
            }

            // link the nodes
            Node previousNode = null;
            Node newNode = null;
            Node endNode = nodeList[nodeList.Count - 1]; 

            if (nodeList.Count > 1)// if there are more than one node, link these nodes
            {
                for (int i = 1; i < nodeList.Count - 1; i++)
                {
                    newNode = nodeList[i];
                    previousNode = nodeList[i - 1];
                    newNode.SetInput(previousNode);
                }
                endNode.SetInput(newNode);
            }
            return endNode;
        }
    }
}
