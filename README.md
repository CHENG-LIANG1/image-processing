# Image Processing Pipeline

**pipe1.txt**  and a few sample images are given.

It is also possible to process **a folder of images**.



You can create your own pipeline file, here is the format:

node=convolve kernel=sharpen

node=convolve kernel=blur

node=convolve kernel=edge

node=greyscale

node=noise noisePercent=0.3

node=vignette

**comment a line** by adding a “#” at the beginning of the line, like this ” #node=greyscale ”



noisePercent is how much noise you are going to add to the image, 0 for no noise, and 1 for white noise.

Notice that there is no space on each side of “=”, you can select 3 different kernels to process the input image.

## Simple Usage

In command line or Debug - Properties, type in these arguments

```[-options] -pipe in/pipe1.txt -input in/cat.png -output out/output ```

to process the image pipeline.

**Additionally**,  type in

 `-verbose` to see the logs, including the node name, input size and time took.

 `-saveall` to save all the intermediate result.

`-help` to get detailed help.









