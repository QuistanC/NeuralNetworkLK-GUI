using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.NetworkLogic.Utilities.DataProcessHelper;

public static class IdexFileReader
{
    public static int ReadBigEndianInt(byte[] bytes, int startIndex)
    {
        return (bytes[startIndex] << 24)
             | (bytes[startIndex + 1] << 16)
             | (bytes[startIndex + 2] << 8)
             | bytes[startIndex + 3];
    }

    public static double[][] LoadImages(string filePath)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        using (BinaryReader br = new BinaryReader(fs))
        {
            // Read header
            int magicNumber = ReadBigEndianInt(br.ReadBytes(4), 0);
            if (magicNumber != 2051) throw new Exception("Invalid magic number for images file.");

            int numberOfImages = ReadBigEndianInt(br.ReadBytes(4), 0);
            int numberOfRows = ReadBigEndianInt(br.ReadBytes(4), 0);
            int numberOfColumns = ReadBigEndianInt(br.ReadBytes(4), 0);

            // Initialize data storage
            double[][] images = new double[numberOfImages][];
            int imageSize = numberOfRows * numberOfColumns;

            for (int i = 0; i < numberOfImages; i++)
            {
                images[i] = new double[imageSize];
                for (int j = 0; j < imageSize; j++)
                {
                    images[i][j] = br.ReadByte() / 255.0; // Normalize to [0, 1]
                }
            }

            return images;
        }
    }

    public static int[] LoadLabels(string filePath)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        using (BinaryReader br = new BinaryReader(fs))
        {
            // Read header
            int magicNumber = ReadBigEndianInt(br.ReadBytes(4), 0);
            if (magicNumber != 2049) throw new Exception("Invalid magic number for labels file.");

            int numberOfLabels = ReadBigEndianInt(br.ReadBytes(4), 0);

            // Initialize data storage
            int[] labels = new int[numberOfLabels];
            for (int i = 0; i < numberOfLabels; i++)
            {
                labels[i] = br.ReadByte();
            }

            return labels;
        }
    }

    public static double[][] OneHotEncodeLabels(int[] labels, int numClasses)
    {
        double[][] encodedLabels = new double[labels.Length][];
        for (int i = 0; i < labels.Length; i++)
        {
            encodedLabels[i] = new double[numClasses];
            encodedLabels[i][labels[i]] = 1.0; // Set the correct class to 1
        }
        return encodedLabels;
    }





}
