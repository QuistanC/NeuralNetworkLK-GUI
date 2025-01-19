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

    public static Bitmap ArrayToImage(byte[,,] pixelArray)
    {
#pragma warning disable CA1416 // Validate platform compatibility

        int width = pixelArray.GetLength(1);
        int height = pixelArray.GetLength(0);
        int stride = width % 4 == 0 ? width : width + 4 - width % 4;
        int bytesPerPixel = 3;

        byte[] bytes = new byte[stride * height * bytesPerPixel];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int offset = (y * stride + x) * bytesPerPixel;
                bytes[offset + 0] = pixelArray[y, x, 2]; //blue
                bytes[offset + 1] = pixelArray[y, x, 1]; //green
                bytes[offset + 2] = pixelArray[y, x, 0]; //red
            }
        }

        PixelFormat formatOutput = PixelFormat.Format24bppRgb;
        Rectangle rect = new Rectangle(0, 0, width, height);
        Bitmap image = new(stride, height, formatOutput);
        BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadOnly, formatOutput);
        Marshal.Copy(bytes, 0, imageData.Scan0, bytes.Length);
        image.UnlockBits(imageData);

        Bitmap image2 = new(width, height, PixelFormat.Format32bppArgb);
        Graphics g = Graphics.FromImage(image2);
        g.DrawImage(image, 0, 0);

#pragma warning restore CA1416 // Validate platform compatibility

        return image2;
    }

    //public byte[] imageToByteArray(Image imageIn)
    //{
    //    MemoryStream ms = new MemoryStream();
    //    imageIn.Save(ms, ImageFormat.Gif);
    //    return ms.ToArray();
    //}

    //public Image byteArrayToImage(byte[] byteArrayIn)
    //{
    //    MemoryStream ms = new MemoryStream(byteArrayIn);
    //    Image returnImage = Image.FromStream(ms);
    //    return returnImage;
    //}

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
                    resized.UnlockBits(bitmapData); // Unlock after copying

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


