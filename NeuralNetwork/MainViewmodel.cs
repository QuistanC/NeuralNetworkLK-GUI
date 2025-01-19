using Microsoft.Win32;
using NeuralNetwork.NetworkLogic.Utilities.DataProcessHelper;
using NeuralNetwork.NetworkLogic.Utilities.ImageHelperMethods;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using NeuralNetwork.NetworkLogic.Models;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;

namespace NeuralNetwork;

public partial class MainViewmodel : ObservableObject
{
    [ObservableProperty]
    private BitmapImage _originalImage;

    [ObservableProperty]
    private string _predictionResult;

    [ObservableProperty]
    private double _accuracy;

    [ObservableProperty]
    private double _error;

    [ObservableProperty]
    private string _selectedModel;

    private NeuralNetworks _currentNetwork;
    private string _selectedModelDirectory;

    [ObservableProperty]
    private int _currentGeneration;

    [ObservableProperty]
    private int _newModelLayers;

    [ObservableProperty]
    private int _newModelNeurons;

    public MainViewmodel()
    {
        Models = new ObservableCollection<string>();
        _selectedModelDirectory = "NetworkModels";
        if (!Directory.Exists(_selectedModelDirectory))
            Directory.CreateDirectory(_selectedModelDirectory);

        //TrainNetworkCommand = new RelayCommand(async () => await TrainNetwork());

        LoadAllModels();
    }

    public ObservableCollection<string> Models { get; set; }

    //public ICommand TrainNetworkCommand { get; }

    [RelayCommand]
    private void LoadAllModels()
    {
        Models.Clear();
        var modelFiles = Directory.GetFiles(_selectedModelDirectory, "*.txt");
        foreach (var file in modelFiles)
        {
            Models.Add(Path.GetFileNameWithoutExtension(file));
        }
    }

    [RelayCommand]
    private void InitializeNewModel()
    {
        // Default values for layers and neurons
        int numLayers = 2; // Default number of layers
        int neuronsPerLayer = 1; // Default number of neurons

        if(NewModelLayers < numLayers) NewModelLayers = numLayers;
        if(NewModelNeurons < neuronsPerLayer) NewModelNeurons = neuronsPerLayer;

        // You can allow the user to input these values as needed.
        _currentNetwork = new NeuralNetworks(NewModelLayers, NewModelNeurons)
        {
            ModelName = $"Model_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        CurrentGeneration = 0;
        _currentNetwork.SaveModel(_selectedModelDirectory, CurrentGeneration);

        PredictionResult = "New model initialized.";
    }

    [RelayCommand]
    private async Task TrainNetwork()
    {
        if (_currentNetwork == null)
        {
            PredictionResult = "Initialize or load a model first.";
            return;
        }

        // Load training data (dummy data in this case, replace with actual data)
        double[][] trainImages = GenerateTrainingData();
        double[][] trainLabels = GenerateTrainingLabels();

        // Train the network
        await Task.Run(() =>
        {
            _currentNetwork.Train(trainImages, trainLabels, epochs: 10);
        });

        // Increment generation
        CurrentGeneration++;

        // Save the updated model
        _currentNetwork.SaveModel(_selectedModelDirectory, CurrentGeneration);
        //LoadAllModels();

        PredictionResult = $"Training completed. Generation: {CurrentGeneration}";
    }
    //Refactor

    [RelayCommand]
    private void UploadImage()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Image Files (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp",
            Title = "Select an Image"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            string imagePath = openFileDialog.FileName;
            // Process the image for prediction
            PredictImage(imagePath);
        }
    }

    private void PredictImage(string imagePath)
    {
        // Logic to preprocess the image and call the Predict method in your NeuralNetwork class
        var processedImage = ImageConverter.ImageToArray(imagePath);
        var prediction = _currentNetwork.Predict(processedImage);

        int predictedLabel = Array.IndexOf(prediction, prediction.Max());

        // Update the ViewModel properties for the predicted label, accuracy, etc.
        PredictionResult = $"Predicted Label: {predictedLabel}";
        // Update other properties like Accuracy or Error if applicable
    }

    [RelayCommand]
    private void LoadModel()
    {
        if (string.IsNullOrEmpty(SelectedModel))
        {
            PredictionResult = "Select a model to load.";
            return;
        }

        string modelPath = Path.Combine(_selectedModelDirectory, $"{SelectedModel}.txt");
        _currentNetwork = NeuralNetworks.LoadModel(modelPath);
        PredictionResult = $"Model {SelectedModel} loaded.";
    }

    private double[][] GenerateTrainingData()
    {
        string trainingDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TrainingData");

        string trainImagesPath = Path.Combine(trainingDataPath, "train-images.idx3-ubyte");

        double[][] trainImages = IdexFileReader.LoadImages(trainImagesPath);
        int originalWidth = 28, originalHeight = 28;
        int poolSize = 2, stride = 2;

        double[][] pooledTrainImages = trainImages
            .Select(image => Pooling.ApplyPooling(image, originalWidth, originalHeight, poolSize, stride))
            .ToArray();
        return pooledTrainImages;
    }
    private double[][] GenerateTrainingLabels()
    {
        string trainingDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TrainingData");
        string trainLabelsPath = Path.Combine(trainingDataPath, "train-labels.idx1-ubyte");
        int[] trainLabels = IdexFileReader.LoadLabels(trainLabelsPath);
        double[][] trainLabelsOneHot = IdexFileReader.OneHotEncodeLabels(trainLabels, 10);

        return trainLabelsOneHot;
    }
}


