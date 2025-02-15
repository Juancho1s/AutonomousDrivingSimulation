using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

public class GeneticManager : MonoBehaviour
{
    [Header("References")]
    public CarControl controller;
    private APIClient API;
    private UI_Variables uI_Variables;

    private StatsManager statsManager;

    [Header("Controls")]
    public int initialPopulation;
    [Range(0.0f, 1.0f)]
    public float mutationRate;

    [Header("Crossover Controls")]
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;
    public int numberToCrossover;

    private List<int> genePool = new List<int>();

    private int naturallySelected;

    private NNet[] population;

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome = 0;

    private void Start()
    {
        API = new APIClient();
        statsManager = GameObject.FindObjectOfType<StatsManager>();
        uI_Variables = GameObject.Find("UI_Manager").GetComponent<UI_Variables>();

        mutationRate = statsManager.mutationRate;
        initialPopulation = statsManager.population;

        CreatePopulation();
    }

    private void CreatePopulation()
    {
        population = new NNet[initialPopulation];
        FillPopulationWithRandomValues(population, 0);
        ResetToCurrentGenome();
        foreach (var i in population)
        {
            i.InitialiseHidden(controller.LAYERS, controller.NEURONS);
        }
    }

    private void ResetToCurrentGenome()
    {
        controller.ResetWithNetwork(population[currentGenome]);
    }

    private void FillPopulationWithRandomValues(NNet[] newPopulation, int startingIndex)
    {
        while (startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = new NNet();
            newPopulation[startingIndex].Initialise(controller.LAYERS, controller.NEURONS);
            startingIndex++;
        }
    }

    public void Death(float fitness, NNet network)
    {

        if (currentGenome < population.Length - 1)
        {

            population[currentGenome].fitness = fitness;
            currentGenome++;
            uI_Variables.setCurrentGenome(currentGenome.ToString());
            ResetToCurrentGenome();

        }
        else
        {
            RePopulate();
        }

    }


    private void RePopulate()
    {
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;
        SortPopulation();

        NNet bestGenome = population[0];

        uI_Variables.setBestFitness(bestGenome.fitness.ToString());
        uI_Variables.setCurrentGeneration(currentGeneration.ToString());

        //send best genome to the server in order it to be saved

        // Define the structure
        var neuralNetwork = new Dictionary<string, Dictionary<string, List<float>>>();


        // iterate corresponding number of layers
        for (int layerIndex = 0; layerIndex < population[0].weights.Count; layerIndex++)
        {
            // Debug.Log("layer" + layerIndex);
            var layer = new Dictionary<string, List<float>>();

            // iterate corresponding number of neurons
            for (int j = 0; j < population[0].weights[layerIndex].RowCount; j++)  // Changed from j = 1 to j = 0, and < instead of <=
            {
                // Debug.Log("neuron" + j);
                var neuronKey = $"neuron{j}";

                var weights = new List<float>();

                for (int k = 0; k < population[0].weights[layerIndex].ColumnCount; k++)
                {
                    // Debug.Log("weight" + k);
                    weights.Add(population[0].weights[layerIndex][j, k]);
                }
                // Add the neuron to the layer
                layer[neuronKey] = weights;
            }

            var layerKey = $"layer{layerIndex}";
            neuralNetwork[layerKey] = layer;
        }


        // StartCoroutine(API.SendRequest_updateTraining(neuralNetwork, new List<float>(population[0].biases), population[0].fitness, 24));
        // StartCoroutine(API.SendRequest_newNNet("complexSampleNetwork", "This is a network for testing", 24, population[0].fitness, population[0].biases, neuralNetwork));



        NNet[] newPopulation = PickBestPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);

        FillPopulationWithRandomValues(newPopulation, naturallySelected);

        population = newPopulation;

        population[^1] = bestGenome;

        currentGenome = 0;

        ResetToCurrentGenome();

    }

    private void Mutate(NNet[] newPopulation)
    {
        for (int i = 0; i < naturallySelected; i++)
        {
            // Mutate weights
            for (int c = 0; c < newPopulation[i].weights.Count; c++)
            {
                if (Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
                }
            }

            // Mutate biases

            for (int r = 0; r < newPopulation[i].biases.Count; r++)
            {
                if (Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    for (int j = 0; j < newPopulation[i].biases[r].Count; j++)
                    {
                        newPopulation[i].biases[r][j] = Mathf.Clamp(newPopulation[i].biases[r][j] + Random.Range(-1f, 1f), -1f, 1f);
                    }
                }
            }

        }

    }

    Matrix<float> MutateMatrix(Matrix<float> A)
    {

        int randomPoints = Random.Range(1, (A.RowCount * A.ColumnCount) / 7);

        Matrix<float> C = A;

        for (int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0, C.RowCount);

            C[randomRow, randomColumn] = Mathf.Clamp(C[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return C;

    }

    private void Crossover(NNet[] newPopulation)
    {
        for (int i = 0; i < numberToCrossover; i += 2)
        {
            int AIndex = i;
            int BIndex = i + 1;

            if (genePool.Count >= 1)
            {
                for (int l = 0; l < 100; l++)
                {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    if (AIndex != BIndex)
                        break;
                }
            }

            NNet Child1 = new NNet();
            NNet Child2 = new NNet();

            Child1.Initialise(controller.LAYERS, controller.NEURONS);
            Child2.Initialise(controller.LAYERS, controller.NEURONS);

            Child1.fitness = 0;
            Child2.fitness = 0;


            for (int w = 0; w < Child1.weights.Count; w++)
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.weights[w] = population[AIndex].weights[w];
                    Child2.weights[w] = population[BIndex].weights[w];
                }
                else
                {
                    Child2.weights[w] = population[AIndex].weights[w];
                    Child1.weights[w] = population[BIndex].weights[w];
                }

            }


            for (int w = 0; w < Child1.biases.Count; w++)
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.biases[w] = population[AIndex].biases[w];
                    Child2.biases[w] = population[BIndex].biases[w];
                }
                else
                {
                    Child2.biases[w] = population[AIndex].biases[w];
                    Child1.biases[w] = population[BIndex].biases[w];
                }

            }

            newPopulation[naturallySelected] = Child1;
            naturallySelected++;

            newPopulation[naturallySelected] = Child2;
            naturallySelected++;

        }
    }

    private NNet[] PickBestPopulation()
    {

        NNet[] newPopulation = new NNet[initialPopulation];

        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation[naturallySelected] = population[i].InitialiseCopy(controller.LAYERS, controller.NEURONS);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;

            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }

        }

        for (int i = 0; i < worstAgentSelection; i++)
        {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }

        }

        return newPopulation;

    }

    private void SortPopulation()
    {
        for (int i = 0; i < population.Length; i++)
        {
            for (int j = i; j < population.Length; j++)
            {
                if (population[i].fitness < population[j].fitness)
                {
                    NNet temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }

    }
}