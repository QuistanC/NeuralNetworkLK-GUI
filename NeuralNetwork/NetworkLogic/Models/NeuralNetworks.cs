using NeuralNetwork.NetworkLogic.Models;
using NeuralNetwork.NetworkLogic.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using CommunityToolkit.Mvvm.Input;

namespace NeuralNetwork.NetworkLogic.Models;

    public class NeuralNetworks
    {
        private List<Layer> Layers = new();
        public int InputSize { get; private set; }
        public int OutputSize { get; private set; }
        public int NumLayers { get; private set; }
        public int NeuronsPerLayer { get; private set; }
        public string ModelName { get; set; }
        public int Generation { get; set; }

        public NeuralNetworks( int numLayers, int neuronsPerLayer)
        {
            InputSize = 784;
            OutputSize = 10;
            InitializeNetwork( numLayers, neuronsPerLayer);
        }

        public void InitializeNetwork(int numLayers, int neuronsPerLayer)
        {
            NumLayers = numLayers;
            NeuronsPerLayer = neuronsPerLayer;

            Layers.Clear();

            InitializeLayers();
            ConnectLayers();
        }

        private void ConnectLayers()
        {
            for (int i = 0; i < Layers.Count - 1; i++)
            {
                Layers[i].NextLayer = Layers[i + 1];
                Layers[i + 1].PrevLayer = Layers[i];
            }
        }

        private void InitializeLayers()
        {
            Layers.Add(new Layer(InputSize, NeuronsPerLayer));

            for (int i = 1; i < NumLayers - 1; i++)
            {
                Layers.Add(new Layer(NeuronsPerLayer, NeuronsPerLayer));
            }

            Layers.Add(new Layer(NeuronsPerLayer, OutputSize));
        }

        public void Train(double[][] inputs, double[][] targets, int epochs, double learningRate = 0.1)
        {
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double totalError = 0.0;

                for (int i = 0; i < inputs.Length; i++)
                {
                    // Forward pass
                    double[] output = Predict(inputs[i]);

                    // Compute error
                    double error = 0.0;
                    for (int j = 0; j < output.Length; j++)
                    {
                        error += Math.Pow(output[j] - targets[i][j], 2);
                    }
                    totalError += error;

                    // Backward pass
                    Layers[^1].Backward(targets[i], learningRate);
                }

                Console.WriteLine($"Epoch {epoch + 1}, Error: {totalError}");
            }
        }

        public double[] Predict(double[] input)
        {
            double[] outputs = input;
            foreach (var layer in Layers)
            {
                layer.Inputs = outputs;
                layer.Forward();
                outputs = layer.Outputs;
            }
            return outputs;
        }

        public double EvaluateAccuracy(double[][] testInputs, int[] testLabels)
        {
            int correctPredictions = 0;

            for (int i = 0; i < testInputs.Length; i++)
            {
                double[] prediction = Predict(testInputs[i]);
                int predictedLabel = Array.IndexOf(prediction, prediction.Max());

                if (predictedLabel == testLabels[i])
                {
                    correctPredictions++;
                }
            }

            return (double)correctPredictions / testInputs.Length;
        }
    /* public void SaveModel(string directory, int generation)
     {
         string fileName = $"{ModelName}_{generation}.txt";
         string filePath = Path.Combine(directory, fileName);

         using StreamWriter writer = new(filePath);

         writer.WriteLine(ModelName);
         writer.WriteLine(generation);

         foreach (var layer in Layers)
         {
             for (int i = 0; i < layer.Weights.GetLength(0); i++)
             {
                 writer.WriteLine(string.Join(",", layer.Weights.GetRow(i)));
             }
             writer.WriteLine(string.Join(",", layer.Biases));
         }
     }*/

    public void SaveModel(string directory, int generation)
    {
        string fileName = $"{ModelName}_{generation}.txt";
        string filePath = Path.Combine(directory, fileName);

        using StreamWriter writer = new(filePath);

        writer.WriteLine(ModelName);
        writer.WriteLine(generation);
        writer.WriteLine(NumLayers); // Save the number of layers
        writer.WriteLine(NeuronsPerLayer); // Save the number of neurons per layer

        foreach (var layer in Layers)
        {

            for (int i = 0; i < layer.Weights.GetLength(0); i++)
            {
                var row = layer.Weights.GetRow(i);
                //writer.WriteLine(string.Join(",", row));
                writer.WriteLine(string.Join(",", row.Select(x => x.ToString("G", CultureInfo.InvariantCulture))));
            }

            var biases = layer.Biases;
            //writer.WriteLine(string.Join(",", biases));
            writer.WriteLine(string.Join(",", biases.Select(x => x.ToString("G", CultureInfo.InvariantCulture))));
        }
    }

    public static NeuralNetworks LoadModel(string filePath)
    {
        using StreamReader reader = new StreamReader(filePath);

        // Read the model name and generation (metadata)
        string modelName = reader.ReadLine();
        int generation = int.Parse(reader.ReadLine());

        // Read the number of layers and neurons per layer
        int numLayers = int.Parse(reader.ReadLine());
        int neuronsPerLayer = int.Parse(reader.ReadLine());

        // Initialize the network with the sizes
        NeuralNetworks network = new NeuralNetworks(numLayers, neuronsPerLayer)
        {
            ModelName = modelName,
            Generation = generation
        };

        // Load weights and biases for each layer
        for (int layerIndex = 0; layerIndex < numLayers; layerIndex++)
        {
            Console.WriteLine($"Loading Layer {layerIndex + 1}");

            // Calculate the number of rows and columns based on the layer index
            int numRows = (layerIndex == 0) ? 784 : neuronsPerLayer; // First layer has 784 inputs
            int numColumns = (layerIndex == numLayers - 1) ? 10 : neuronsPerLayer; // Last layer has 10 outputs

            double[,] weights = new double[numRows, numColumns];

            // Read weights row by row
            for (int i = 0; i < numRows; i++)
            {
                string line = reader.ReadLine();
                // Use InvariantCulture to parse numbers correctly
                double[] weightRow = line.Split(',').Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToArray();

                // Validate if the number of columns in weightRow matches the expected numColumns
                if (weightRow.Length != numColumns)
                {
                    Console.WriteLine($"Warning: Expected {numColumns} columns, but found {weightRow.Length} in row {i + 1}");
                    throw new FormatException($"Mismatch in weight row size: expected {numColumns}, but got {weightRow.Length}.");
                }

                // Assign the values to the weights matrix
                for (int j = 0; j < numColumns; j++)
                {
                    weights[i, j] = weightRow[j];
                }

            }

            // Load biases
            double[] biases = reader.ReadLine().Split(',').Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToArray();

            // Set weights and biases into the corresponding layer
            network.Layers[layerIndex].Weights = weights;
            network.Layers[layerIndex].Biases = biases;
        }

        return network;
    }


    public void PrintImage(double[] image, int width = 28, int height = 28)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double pixelValue = image[y * width + x];
                    char pixelChar = pixelValue > 0.8 ? '#' : pixelValue > 0.5 ? '+' : pixelValue > 0.2 ? '.' : ' ';
                    Console.Write(pixelChar);
                }
                Console.WriteLine();
            }
        }
    }

