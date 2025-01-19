using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.NetworkLogic.Utilities.ImageHelperMethods;

public class ConvoluteImage
{
    public double[,] Convolve(double[,] image, double[,] kernel, int stride = 1, int padding = 0)
    {
        int inputHeight = image.GetLength(0);
        int inputWidth = image.GetLength(1);
        int kernelHeight = kernel.GetLength(0);
        int kernelWidth = kernel.GetLength(1);

        int outputHeight = (inputHeight - kernelHeight + 2 * padding) / stride + 1;
        int outputWidth = (inputWidth - kernelWidth + 2 * padding) / stride + 1;

        double[,] output = new double[outputHeight, outputWidth];

        for (int y = 0; y < outputHeight; y++)
        {
            for (int x = 0; x < outputWidth; x++)
            {
                double sum = 0.0;

                for (int i = 0; i < kernelHeight; i++)
                {
                    for (int j = 0; j < kernelWidth; j++)
                    {
                        int inputY = y * stride + i;
                        int inputX = x * stride + j;

                        sum += image[inputY, inputX] * kernel[i, j];
                    }
                }
                output[y, x] = sum;
            }
        }


        return output;
    }
}
