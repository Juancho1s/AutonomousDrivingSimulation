using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


using MathNet.Numerics.LinearAlgebra;
using System;

using Random = UnityEngine.Random;

public class NNet : MonoBehaviour
{
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 3);

    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();

    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2);

    public List<Matrix<float>> weights = new List<Matrix<float>>();

    public List<List<float>> biases = new List<List<float>>();

    public float fitness;

    public void Initialise(int hiddenLayerCount, int hiddenNeuronCount)
    {
        Reset();

        // For input to first hidden layer
        Matrix<float> inputToH1 = Matrix<float>.Build.Dense(3, hiddenNeuronCount);
        weights.Add(inputToH1);

        biases.Add(new List<float>());
        for (int j = 0; j < hiddenNeuronCount; j++)
            biases[0].Add(Random.Range(-1f, 1f));

        // For hidden to hidden layers
        for (int i = 1; i < hiddenLayerCount; i++)
        {
            Matrix<float> HiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount);
            weights.Add(HiddenToHidden);

            biases.Add(new List<float>());
            for (int j = 0; j < hiddenNeuronCount; j++)
                biases[i].Add(Random.Range(-1f, 1f));
        }

        // For last hidden to output layer
        Matrix<float> OutputWeight = Matrix<float>.Build.Dense(hiddenNeuronCount, 2);
        weights.Add(OutputWeight);


        biases.Add(new List<float>());
        for (int j = 0; j < 2; j++)
            biases[^1].Add(Random.Range(-1f, 1f));

        RandomiseWeights();

        // Initialize the hidden layers after weights and biases
        InitialiseHidden(hiddenLayerCount, hiddenNeuronCount);
    }


    public void Reset()
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weights.Clear();
        biases.Clear();
    }


    public void initializeAIInsertion(int hiddenLayerCount, int hiddenNeuronCount)
    {
        Reset();

        // For input to first hidden layer
        Matrix<float> inputToH1 = Matrix<float>.Build.Dense(3, hiddenNeuronCount);
        weights.Add(inputToH1);

        // For hidden to hidden layers
        for (int i = 1; i < hiddenLayerCount; i++)
        {
            Matrix<float> HiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount);
            weights.Add(HiddenToHidden);
        }

        // For last hidden to output layer
        Matrix<float> OutputWeight = Matrix<float>.Build.Dense(hiddenNeuronCount, 2);
        weights.Add(OutputWeight);

        // Initialize the hidden layers after weights and biases
        InitialiseHidden(hiddenLayerCount, hiddenNeuronCount);
    }




    public NNet InitialiseCopy(int hiddenLayerCount, int hiddenNeuronCount)
    {
        NNet n = new NNet();

        List<Matrix<float>> newWeights = new List<Matrix<float>>();

        for (int i = 0; i < this.weights.Count; i++)
        {
            Matrix<float> currentWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);

            for (int x = 0; x < currentWeight.RowCount; x++)
                for (int y = 0; y < currentWeight.ColumnCount; y++)
                    currentWeight[x, y] = weights[i][x, y];

            newWeights.Add(currentWeight);
        }

        List<List<float>> newBiases = new List<List<float>>();

        newBiases.AddRange(biases);

        n.weights = newWeights;
        n.biases = newBiases;

        n.InitialiseHidden(hiddenLayerCount, hiddenNeuronCount);

        return n;
    }

    public void InitialiseHidden(int hiddenLayerCount, int hiddenNeuronCount)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        for (int i = 0; i < hiddenLayerCount; i++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
            hiddenLayers.Add(newHiddenLayer);
        }

    }

    public void RandomiseWeights()
    {

        for (int i = 0; i < weights.Count; i++)
            for (int x = 0; x < weights[i].RowCount; x++)
                for (int y = 0; y < weights[i].ColumnCount; y++)
                    weights[i][x, y] = Random.Range(-1f, 1f);

        for (int i = 0; i < biases.Count; i++)
            for (int x = 0; x < biases[i].Count; x++)
                biases[i][x] = Random.Range(-1.0f, 1.0f);


    }

    public (float, float) RunNetwork(float a, float b, float c)
    {
        // Assign inputs
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;

        if (weights.Count == 0)
        {
            Debug.Log("weights 0");
        }
        if (biases.Count == 0)
        {
            Debug.Log("biases 0");
        }
        if (hiddenLayers.Count == 0)
        {
            Debug.Log("hiddenLayers 0");
        }

        // Compute first hidden layer
        hiddenLayers[0] = sumLayersAndWeights(inputLayer, weights[0], biases[0]);

        // Compute subsequent hidden layers
        for (int i = 1; i < hiddenLayers.Count; i++)
            hiddenLayers[i] = sumLayersAndWeights(hiddenLayers[i - 1], weights[i], biases[i]);


        // Compute output layer
        outputLayer = sumLayersAndWeights(hiddenLayers[^1], weights[^1], biases[^1]);

        // First output is acceleration and second is steering
        return (Sigmoid(outputLayer[0, 0]), (float)Math.Tanh(outputLayer[0, 1]));
    }


    private Matrix<float> sumLayersAndWeights(Matrix<float> layer, Matrix<float> weights, List<float> layerBiases)
    {
        // Create a new matrix for the next layer's output
        Matrix<float> nextLayer = Matrix<float>.Build.Dense(1, weights.ColumnCount);

        // Iterate through each neuron in the next layer
        for (int i = 0; i < nextLayer.ColumnCount; i++)
        {
            // Calculate the weighted sum for each neuron
            for (int j = 0; j < layer.ColumnCount; j++)
                nextLayer[0, i] += layer[0, j] * weights[j, i];

            // Add the bias and apply the tanh activation function
            nextLayer[0, i] = MathF.Tanh(nextLayer[0, i] + layerBiases[i]);
        }
        return nextLayer;
    }


    private float Sigmoid(float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }

}