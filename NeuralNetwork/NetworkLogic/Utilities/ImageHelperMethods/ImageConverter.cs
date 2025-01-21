using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace NeuralNetwork.NetworkLogic.Utilities.ImageHelperMethods;

public class ImageConverter
{
    public static byte[,,]? ImageToArray(Bitmap image)
    {
#pragma warning disable CA1416 // Validate platform compatibility

        int bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
        Rectangle rect = new(0, 0, image.Width, image.Height);
        BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);
        int byteCount = Math.Abs(imageData.Stride) * image.Height;
        byte[] bytes = new byte[byteCount];
        Marshal.Copy(imageData.Scan0, bytes, 0, byteCount);
        image.UnlockBits(imageData);

        byte[,,] pixelValues = new byte[image.Height, image.Width, 3];
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                int offset = y * imageData.Stride + x * bytesPerPixel;
                pixelValues[y, x, 0] = bytes[offset + 2]; //red
                pixelValues[y, x, 1] = bytes[offset + 1]; //green
                pixelValues[y, x, 2] = bytes[offset + 0]; //blue

            }
        }
#pragma warning restore CA1416 // Validate platform compatibility

        return pixelValues;
    }

    public static Bitmap ArrayToBitmap(double[] pixelArray, int width, int height)
    {
        Bitmap bitmap = new Bitmap(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int intensity = (int)(pixelArray[y * width + x] * 255);
                Color color = Color.FromArgb(intensity, intensity, intensity);
                bitmap.SetPixel(x, y, color);
            }
        }

        return bitmap;
    }

    public static double[] ImageToArray(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            throw new FileNotFoundException("The specified image file does not exist.", imagePath);
        }

        try
        {
            using (Bitmap bitmap = new Bitmap(imagePath))
            {
                // Resize the image to 28x28
                using (Bitmap resized = new Bitmap(bitmap, new Size(28, 28)))
                {
                    double[] normalizedArray = new double[28 * 28];

                    // Lock bits for faster pixel access
                    var rect = new Rectangle(0, 0, resized.Width, resized.Height);
                    var bitmapData = resized.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, resized.PixelFormat);

                    int bytesPerPixel = Image.GetPixelFormatSize(resized.PixelFormat) / 8;
                    int stride = bitmapData.Stride;
                    byte[] pixelBuffer = new byte[stride * resized.Height];

                    System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
                    resized.UnlockBits(bitmapData);

                    for (int y = 0; y < 28; y++)
                    {
                        for (int x = 0; x < 28; x++)
                        {
                            int pixelIndex = (y * stride) + (x * bytesPerPixel);

                            // Ensure pixelIndex is within bounds (important for large images)
                            if (pixelIndex + 2 < pixelBuffer.Length)
                            {
                                // Convert RGB to grayscale
                                byte blue = pixelBuffer[pixelIndex];
                                byte green = pixelBuffer[pixelIndex + 1];
                                byte red = pixelBuffer[pixelIndex + 2];

                                double gray = (red + green + blue) / 3.0;
                                normalizedArray[y * 28 + x] = gray / 255.0;
                            }
                        }
                    }

                    return normalizedArray;
                }
            }
        }
        catch (Exception ex)
        {
            // Log or handle the exception
            Console.WriteLine($"Error occurred while processing the image: {ex.Message}");
            throw; // Re-throw the exception after logging
        }
    }
}


