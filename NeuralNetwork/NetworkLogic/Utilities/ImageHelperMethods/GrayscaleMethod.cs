using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.NetworkLogic.Utilities.ImageHelperMethods;

public class GrayscaleMethod
{
    public static double[] ImageToGrayscaleArray(Bitmap image)
    {
        int width = image.Width;
        int height = image.Height;
        double[] pixels = new double[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = image.GetPixel(x, y);
                double grayValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3.0 / 255.0;
                pixels[y * width + x] = grayValue;
            }
        }

        return pixels;
    }

}
