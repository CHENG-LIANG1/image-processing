using System;
using SixLabors.ImageSharp.PixelFormats;
using NodeAbstraction;

// Author: Cheng Liang (Louis)
// Student ID: N10346911

namespace Nodes
{
    class NodeException : Exception { public NodeException(string message) : base(message) { } }

    class N_Noise : Node
    {
        private double Num { get; set; }
        public override string GetName() { return "N_Noise (percentage of noise " + Num + ")"; }

        private int _maxRange;
        /// <summary>
        /// Construct the noise node with a double
        /// </summary>
        /// <param name="num"> the percentage of noise </param>
        public N_Noise(double num) : base(num)
        {
            Num = num;
            _maxRange = 255;

            if (num >= 0 && num <= 1)
            {
                _maxRange = (int)(_maxRange * num);
            }
            else
            {
                throw new NodeException("  Error: invalid noise percentage! Please enter a double between 0 and 1.\n");
            }
        }
        /// <summary>
        /// Add random noise to the image
        /// </summary>
        /// <param name="input"> Image input </param>
        /// <returns> Image output </returns>
        public override img_processor.Image Process(img_processor.Image input)
        {
            Random rng = new Random();

            img_processor.Image output = new img_processor.Image(input.Width, input.Height);
            for (int i = 0; i < input.Width; i++)
            {
                for (int j = 0; j < input.Height; j++)
                {
                    int R = input[i, j].R;
                    int G = input[i, j].G;
                    int B = input[i, j].B;

                    // add a random value to R, G and B of each pixel to generate noise
                    R += rng.Next(-_maxRange, _maxRange + 1);
                    G += rng.Next(-_maxRange, _maxRange + 1);
                    B += rng.Next(-_maxRange, _maxRange + 1);

                    // prevent image overflow by limiting the range of RGB value
                    R = R > MAX_RGB_VALUE ? MAX_RGB_VALUE : R;
                    R = R < MIN_RGB_VALUE ? MIN_RGB_VALUE : R;

                    G = G > MAX_RGB_VALUE ? MAX_RGB_VALUE : G;
                    G = G < MIN_RGB_VALUE ? MIN_RGB_VALUE : G;

                    B = B > MAX_RGB_VALUE ? MAX_RGB_VALUE : B;
                    B = B < MIN_RGB_VALUE ? MIN_RGB_VALUE : B;

                    // create a new color with the calulated RGB values and store it into a new pixel
                    output[i, j] = new Rgba32((byte)R, (byte)G, (byte)B, ALPHA_VALUE);
                }
            }
            return output;
        }
    }

    class N_ToGreyScale : Node
    {
        public override string GetName() { return "N_ToGreyScale"; }

        public N_ToGreyScale():base() { }

        public override img_processor.Image Process(img_processor.Image input)
        {
            return img_processor.Image.ToGrayscale(input);
        }


    }

    class N_Vignette : Node
    {
        public override string GetName() { return "N_Vignette"; }

        public N_Vignette():base() { }

        /// <summary>
        /// Add vignette effect to the input image
        /// </summary>
        /// <param name="input"> image input </param>
        /// <returns> image output </returns>
        public override img_processor.Image Process(img_processor.Image input)
        {
            double xCenter = input.Width / 2;
            double yCenter = input.Height / 2;

            // calculate the maximum distance
            double maxDistence = (double)(Math.Sqrt(xCenter * xCenter + yCenter * yCenter));

            img_processor.Image output = new img_processor.Image(input.Width, input.Height);

            for (int i = 0; i < input.Width; i++)
            {
                for (int j = 0; j < input.Height; j++)
                {
                    // calculate the distance
                    double distance = (double)(Math.Sqrt((xCenter - i) * (xCenter - i) + (yCenter - j) * (yCenter - j)));

                    // calculate the brightness
                    double brightness = Math.Pow((maxDistence - distance) / maxDistence, 2);

                    int R = (int)(input[i, j].R * brightness);
                    int G = (int)(input[i, j].G * brightness);
                    int B = (int)(input[i, j].B * brightness);

                    // create a new color with the calulated RGB values and store it into a new pixel
                    output[i, j] = new Rgba32((byte)R, (byte)G, (byte)B, ALPHA_VALUE);
                }

            }
            return output;
        }



    }


    public static class KernelFactory
    {
        public static readonly double[,] edgeKernel = new double[,]
        {{-1, -1, -1},
         {-1,  8, -1},
         {-1, -1, -1},
        };

        public static readonly double[,] sharpenKernel = new double[,]
        {{ 0, -1 , 0 },
         {-1,  5, -1 },
         { 0, -1,  0 }
        };

        public static readonly double[,] blurKernel = new double[,]
        {{(double)1/256, (double)4/256,  (double)6/256,  (double)4/256,  (double)1/256},
         {(double)4/256, (double)16/256, (double)24/256, (double)16/256, (double)4/256},
         {(double)6/256, (double)24/256, (double)36/256, (double)24/256, (double)6/256 },
         {(double)4/256, (double)16/256, (double)24/256, (double)16/256, (double)4/256},
         {(double)1/256, (double)4/256,  (double)6/256,  (double)4/256,  (double)1/256},
        };
    }
    public class N_Convolve : Node
    {
        public override string GetName() { return "N_Convolve (" + KernelName + ")"; }

        // data members
        private int _offset;
        private int _kernelWidth;
        private double[,] _kernel;
        private string KernelName { get; set; }

        /// <summary>
        /// Construct the convolve node with a string parameter
        /// </summary>
        /// <param name="kernel"> the name of the kernel </param>
        public N_Convolve(string kernel) : base(kernel)
        {
            KernelName = kernel;

            if (kernel == "edge")
            {
                _kernel = KernelFactory.edgeKernel;
            }
            else if (kernel == "sharpen")
            {
                _kernel = KernelFactory.sharpenKernel;
            }
            else if (kernel == "blur")
            {
                _kernel = KernelFactory.blurKernel;
            }
            else
            {
                throw new NodeException("invalid kernel name! Please enter a correct kernel!\n" +
                                        "  Usage: Node = Convolve kernel = edge / sharpen / blur");
            }
            _kernelWidth = _kernel.GetLength(0);
            _offset = (_kernelWidth - 1) / 2;
        }


        /// <summary>
        /// Convolve the image with a kernel(edge/sharpen/blur)
        /// </summary>
        /// <param name="input"> Image input </param>
        /// <returns></returns>
        public override img_processor.Image Process(img_processor.Image input)
        {
            const byte MAX_RGB_VALUE = 255;
            const byte MIN_RGB_VALUE = 0;

            img_processor.Image output = new img_processor.Image(input.Width - _offset, input.Height - _offset);
            for (int i = _offset; i < input.Width - _offset; i++)
            {
                ; for (int j = _offset; j < input.Height - _offset; j++)
                {
                    double R = 0;
                    double G = 0;
                    double B = 0;

                    for (int x = 0; x < _kernelWidth; x++)
                    {
                        for (int y = 0; y < _kernelWidth; y++)
                        {
                            // calculate the corresponding coordinates to multiply
                            int row = i + x - _offset;
                            int col = j + y - _offset;

                            // sum all the results from multipliacton
                            R += _kernel[x, y] * input[row, col].R;
                            G += _kernel[x, y] * input[row, col].G;
                            B += _kernel[x, y] * input[row, col].B;
                        }
                    }

                    // prevent image overflow by limiting the range of RGB value
                    R = R > MAX_RGB_VALUE ? MAX_RGB_VALUE : R;
                    R = R < MIN_RGB_VALUE ? MIN_RGB_VALUE : R;

                    G = G > MAX_RGB_VALUE ? MAX_RGB_VALUE : G;
                    G = G < MIN_RGB_VALUE ? MIN_RGB_VALUE : G;

                    B = B > MAX_RGB_VALUE ? MAX_RGB_VALUE : B;
                    B = B < MIN_RGB_VALUE ? MIN_RGB_VALUE : B;

                    // create a new color with the calulated RGB values and store it into a new pixel
                    output[i, j] = new Rgba32((byte)R, (byte)G, (byte)B, ALPHA_VALUE);
                }
            }
            return output;
        }
    }
}
