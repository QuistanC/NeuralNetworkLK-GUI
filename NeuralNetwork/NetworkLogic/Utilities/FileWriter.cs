using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.NetworkLogic.Utilities;

public class FileWriter
{
    public static List<double[]> ReadFile(string path, int numNeurons)
    {
       string[] fileContent = File.ReadAllLines(path);
        List<double[]> weightsList = new List<double[]>();

        foreach (var line in fileContent)
        {
            string[] entries = line.Split(";");



            for(int i = 0; i < entries.Length; i++)
            {
                string entry = entries[i].Trim().Trim('(', ')');

                string[] numbers = entry.Split(",");

                double[] weight = Array.ConvertAll(numbers, double.Parse);

                weightsList.Add(weight);
            }        
        }

        return weightsList;


    }


}
