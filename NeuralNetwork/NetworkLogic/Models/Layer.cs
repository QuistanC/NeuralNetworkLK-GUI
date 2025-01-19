using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.NetworkLogic.Models;

public class Layer
{
    public int NumNeurons { get; }
    public int NumInputs { get; }
    public double[,] Weights { get; set; }
    public double[] Inputs { get; set; }
    public double[] Outputs { get; set; }
    public double[] Biases { get; set; }
    public double[] Gradients { get; set; }
    public Layer? PrevLayer { get; set; }
    public Layer? NextLayer { get; set; }

    public Layer(int numInputs, int numNeurons)
    {
        NumNeurons = numNeurons;
        NumInputs = numInputs;
        Outputs = new double[numNeurons];
        Inputs = new double[numInputs];
        Weights = new double[numInputs, numNeurons]; // Corrected dimensions
        Biases = new double[numNeurons];
        Gradients = new double[numNeurons];

        Random rand = new Random();
        for (int i = 0; i < numNeurons; i++)
        {
            Biases[i] = rand.NextDouble();
            for (int j = 0; j < numInputs; j++)
            {
                Weights[j, i] = rand.NextDouble() - 0.5; // Adjusted for flipped dimensions
            }
        }
    }

    public void Forward()
    {
        for (int i = 0; i < NumNeurons; i++)
        {
            Outputs[i] = 0.0;
            for (int j = 0; j < Inputs.Length; j++)
            {
                Outputs[i] += Inputs[j] * Weights[j, i]; // Flipped access
            }
            Outputs[i] += Biases[i];
            Outputs[i] = SigmoidActivation(Outputs[i]);
        }

        if (NextLayer != null)
        {
            NextLayer.Inputs = Outputs;
            NextLayer.Forward();
        }
    }

    public void Backward(double[] target = null, double learningRate = 0.1)
    {
        if (NextLayer == null && target != null)
        {
            for (int i = 0; i < NumNeurons; i++)
            {
                Gradients[i] = (Outputs[i] - target[i]) * ActivationDerivative(Outputs[i]);
            }
        }
        else
        {
            for (int i = 0; i < NumNeurons; i++)
            {
                Gradients[i] = 0.0;
                for (int j = 0; j < NextLayer.NumNeurons; j++)
                {
                    Gradients[i] += NextLayer.Gradients[j] * NextLayer.Weights[i, j]; // Flipped access
                }
                Gradients[i] *= ActivationDerivative(Outputs[i]);
            }
        }

        for (int i = 0; i < NumNeurons; i++)
        {
            for (int j = 0; j < Inputs.Length; j++)
            {
                Weights[j, i] -= Gradients[i] * Inputs[j] * learningRate; // Flipped access
            }
            Biases[i] -= Gradients[i] * learningRate;
        }

        if (PrevLayer != null)
        {
            PrevLayer.Backward();
        }
    }

    private double SigmoidActivation(double x) => 1.0 / (1.0 + Math.Exp(-x));
    private double ActivationDerivative(double x) => x * (1.0 - x);
}
