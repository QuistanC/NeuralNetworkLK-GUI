using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.NetworkLogic.Utilities.ImageHelperMethods;

public class Pooling
{

    public static double[,] ApplyMaxPooling(double[,] imageInput, int poolSize, int stride)
    {
        int inputHeight = imageInput.GetLength(0);
        int inputWidth = imageInput.GetLength(1);

        int outputHeight = (inputHeight - poolSize) / stride + 1;
        int outputWidth = (inputWidth - poolSize) / stride + 1;

        double[,] output = new double[outputHeight, outputWidth];

        for (int y = 0; y < outputHeight; y++)
        {
            for (int x = 0; x < outputWidth; x++)
            {
                double max = double.MinValue;

                for (int i = 0; i < poolSize; i++)
                {
                    for (int j = 0; j < poolSize; j++)
                    {
                        int inputY = y * stride + i;
                        int inputX = x * stride + j;

                        max = Math.Max(max, imageInput[inputY, inputX]);
                    }
                }

                output[y, x] = max;
            }
        }

        return output;
    }

    public static double[] ApplyPooling(double[] input, int originalWidth, int originalHeight, int poolSize, int stride)
    {
        double[,] reshaped = ReshapeTo2D(input, originalWidth, originalHeight);
        double[,] pooled = ApplyMaxPooling(reshaped, poolSize, stride);
        return Flatten2D(pooled);
    }

    private static double[,] ReshapeTo2D(double[] input, int width, int height)
    {
        double[,] output = new double[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                output[y, x] = input[y * width + x];
            }
        }
        return output;
    }

    private static double[] Flatten2D(double[,] input)
    {
        int height = input.GetLength(0);
        int width = input.GetLength(1);
        double[] flattened = new double[height * width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flattened[y * width + x] = input[y, x];
            }
        }
        return flattened;
    }

}

