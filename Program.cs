using System;
using System.IO;
using NodeAbstraction;
using Nodes;

// Author: Cheng Liang (Louis)
// Student ID: N10346911

namespace img_processor
{
    public class Program
    {
        /// <summary>
        /// Get the paths in the command line arguments
        /// </summary>
        /// <param name="args"> command line arguments </param>
        /// <param name="argument"> the argument before the path </param>
        /// <returns> a path </returns>
        private static string GetArgValue(string[] args, string argument)
        {
            string path = "";
            try { path = args[Array.FindIndex(args, delegate (string arg) { return arg == argument; }) + 1]; }
            catch (IndexOutOfRangeException) { }

            return path;
        }
        /// <summary>
        /// Check the existence of an argument in command line arguments
        /// </summary>
        /// <param name="args"> commandline arguments </param>
        /// <param name="argument"> the argument to check </param>
        /// <returns> existence of an argument (boolean) </returns>
        private static bool CheckSingularArg(string[] args, string argument)
        {
            bool argumentExists;
            int index = Array.FindIndex(args, delegate (string arg) { return arg == argument; });

            argumentExists = index == -1 ? false : true; // if index equals -1, the argument does not exist

            return argumentExists;
        }
        public static void DisplayUsage()
        {
            Console.WriteLine("  Usage: img-processor [options] " +
                  "-pipe<path> -input<path> -output<path>(no file extension required)\n  " +
                  "Use -help option for detailed help.\n");
        }



        public static void Main(string[] args)
        {
            // check the existence of certain arguments
            bool verboseExists = CheckSingularArg(args, "-verbose");
            bool saveallExists = CheckSingularArg(args, "-saveall");
            bool helpExists    = CheckSingularArg(args,    "-help");
            bool pipeExists    = CheckSingularArg(args,    "-pipe");
            bool inputExists   = CheckSingularArg(args,   "-input");
            bool outputExists  = CheckSingularArg(args,  "-output");

            string inputPath;
            string pipelinePath;
            string outputPath;

            if (args.Length == 0)// display useage infomation when no argument is given
            {
                Console.WriteLine("  No argument has been entered!\n");
                return;
            }

            if (helpExists == true)// display instructions when user enters "-help" 
            {
                Console.WriteLine("  Usage: img-processor [options] -pipe <path> -input <path> -output <path>");
                Console.WriteLine("  Required parameters:"                                                    );
                Console.WriteLine("      -pipe <path>   : the path to the pipe txt file"                      );
                Console.WriteLine("      -input <path>  : the path to the input image or image directory"     );
                Console.WriteLine("      -output <path> : the path to the output (file or directory) "        +
                                  "(must be a directory if -saveall is enabled or -input is a directory)"     );
                Console.WriteLine("  options:"                                                                );
                Console.WriteLine("      -verbose       : use this option to enable verbose logging"          );
                Console.WriteLine("      -saveall       : use this option to save all intermediate images"    );
                Console.WriteLine("      -help          : display this help\n");
                return;
            }

            // check the existence of "-pipe", "-input" and "output" and get the paths
            if (pipeExists == true && inputExists == true && outputExists == true)
            {
                pipelinePath = GetArgValue(args,   "-pipe");
                inputPath    = GetArgValue(args,  "-input");
                outputPath   = GetArgValue(args, "-output");
            }
            else
            {
                Console.WriteLine(" Error: '-input' or '-pipe' or '-output' missing!, please check your argument!\n");
                DisplayUsage();
                return;
            }

            if (pipelinePath == "" || inputPath == "" || outputPath == "" || pipelinePath == "-input" || inputPath == "-output") // "array.Findex() + 1" method in "GetArgValue" makes "-input" the next index of "-pipe"
                                                                                                                                 // if pipeline path is missing, it will not be empty string. 
            {
                Console.WriteLine("  Error: 'pipeline path' or 'input path' or 'output path' missing! Please check your argument!\n");
                DisplayUsage();
                return;
            }

            Image input = null;
            Image output = null;
            Node endNode = null;

            // load the image file and pipeline file and catch possible exceptions
            try
            {   if (inputPath.Contains("/") == true) // load the image only when input path is not a folder
                { input = new Image(inputPath); }
                endNode = PipelineLoader.LoadPipeline(pipelinePath);
            }
            catch (DirectoryNotFoundException exception)
            { Console.WriteLine("  Error: " + exception.Message + "\n  please enter an valid directory!\n"         ); return; }
            catch (FileNotFoundException exception)
            { Console.WriteLine("  Error: " + exception.Message + "\n  please check the folder or your argument!\n"); return; }
            catch (IndexOutOfRangeException)
            { Console.WriteLine("  Error: the pipeline file contains empty lines!\n"                               ); return; }
            catch (NodeException exception)
            { Console.WriteLine("  Error: " + exception.Message + "\n"                                             ); return; }

            string outputFolder = "";
            string outputName = "";

            if (outputPath.Contains("/") == true) // check if the output path is a directory, file name cannot contain "/", the path which contains "/" must be a directory                               
            {
                outputFolder = outputPath.Split('/')[0];
                outputName = outputPath.Split('/')[1];
                Directory.CreateDirectory(outputFolder);  // create a new folder if the output folder does not exist
            }


            if (inputPath.Contains("/") == false && outputPath.Contains("/") == true)// if input is a folder of images and output path is a directory, run the following code
            {
                string[] fileNames = null;
                try{ fileNames = Directory.GetFiles(inputPath, "*.png"); }
                catch (DirectoryNotFoundException) { Console.WriteLine("  Error: input folder does not exist!\n"); return; }

                if (fileNames.Length == 0) { Console.WriteLine("  Error: the input folder does not contain any image!\n"); return; }
                
                for (int i = 0; i < fileNames.Length; i++)
                {
                    input = new Image(fileNames[i]); // load a folder of images

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n  Running pipeline on " + fileNames[i] + " (" +input.ToString() + ")");
                    Console.ForegroundColor = ConsoleColor.White;
                    string inputImageName = Path.GetFileNameWithoutExtension(fileNames[i]);

                    if (saveallExists == true)
                    {
                        Directory.CreateDirectory(outputFolder);// if the directory does not exist, create a new one with the given name.
                        Directory.CreateDirectory(Path.Combine(outputFolder, "intermediate-" + inputImageName));// create subfolders to store intermediate results
                        output = endNode.GetOutput(input, logging: verboseExists, saveDir: outputFolder + "/" + "intermediate-" + inputImageName);
                    }
                    else
                    {
                        output = endNode.GetOutput(input, logging: verboseExists);
                    }
                    Directory.CreateDirectory(Path.Combine(outputFolder, "main-" + outputName));  // create a subfolder to store a folder of images
                    output.Write(outputFolder + "/" + "main-" + outputName + "/" + inputImageName);
                }

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n  All of the main output images are saved and stored into a folder: " + "main-" + outputName);
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if(inputPath.Contains("/") == false && outputPath.Contains("/") == false) // input path is a directory, while output path is not 
            {
                Console.WriteLine("  Error: input path is a directory, output path must be a directory as well.\n");
                return;
            }


            if (inputPath.Contains("/") == true) // if the input path is a single image, run the following code.
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n  Running pipeline on Image " + "(" + input.ToString() + ")\n");
                Console.ForegroundColor = ConsoleColor.White;
                if (saveallExists == true)
                {
                    if (outputPath.Contains("/") == true)
                    {
                        Console.WriteLine("      -saveall is enabled.");
                        Directory.CreateDirectory(outputFolder);// if the directory does not exist, create a new one with the given name.
                        output = endNode.GetOutput(input, logging: verboseExists, saveDir: outputFolder); // left part of the path is the directory 
                    }
                    else
                    {
                        Console.WriteLine("  Error: '-saveall' is enabled, the output path must be a directory.\n");
                        return;
                    }
                }
                else
                {
                    output = endNode.GetOutput(input, logging: verboseExists);
                }

                output.Write(outputPath);
                Console.WriteLine("\noutput image is written to: " + outputPath + ".png");
            }

            Console.WriteLine("\n  Done!\n");
        }
    }
}