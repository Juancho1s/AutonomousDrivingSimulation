using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class playSimulation : MonoBehaviour
{


    public void Start()
    {
        StatsManager.Instance.LAYERS = 3;
        StatsManager.Instance.NEURONS = 10;
        StatsManager.Instance.mutationRate = 0.055f;
        StatsManager.Instance.population = 50;
        StatsManager.Instance.timeMultiplier = 1f;
    }

    public void StartSimulationButton(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    public void assignLayers(string layers)
    {
        Debug.Log($"Input received for Layers: {layers}");

        if (int.TryParse(layers, out int layerCount))
        {
            if (layerCount > 0)
            {
                StatsManager.Instance.LAYERS = layerCount;
                Debug.Log($"Layers set: {layerCount}");
            }
            else
            {
                Debug.LogWarning($"Invalid input for Layers: {layers}");
            }
        }
        else
        {
            Debug.LogWarning($"Failed to parse Layers input: {layers}");
        }
    }

    public void assignNeurons(string neurons)
    {
        Debug.Log($"Input received for Neurons: {neurons}");

        if (int.TryParse(neurons, out int neuronCount))
        {
            if (neuronCount > 0)
            {
                StatsManager.Instance.NEURONS = neuronCount;
                Debug.Log($"Neurons set: {neuronCount}");
            }
            else
            {
                Debug.LogWarning($"Invalid input for Neurons: {neurons}");
            }
        }
        else
        {
            Debug.LogWarning($"Failed to parse Neurons input: {neurons}");
        }
    }

    public void assignMutationRate(string mutationRate)
    {
        Debug.Log($"Input received for Mutation Rate: {mutationRate}");

        if (float.TryParse(mutationRate, out float mutationRateCount))
        {
            if (mutationRateCount > 0 & mutationRateCount <= 1f)
            {
                StatsManager.Instance.mutationRate = mutationRateCount;
                Debug.Log($"mutation rate set: {mutationRateCount}");
            }
            else
            {
                StatsManager.Instance.mutationRate = 0.1f;
                Debug.LogWarning($"Invalid input for Mutation Rate: {mutationRate}");
            }
        }
        else
        {
            Debug.LogWarning($"Failed to parse Mutation Rate input: {mutationRate}");
        }
    }

    public void assignPopulation(string populatoin)
    {
        Debug.Log($"Input received for Population: {populatoin}");
        if (int.TryParse(populatoin, out int populationCount))
        {
            if (populationCount > 0 & populationCount <= 100)
            {
                StatsManager.Instance.population = populationCount;
                Debug.Log($"Population set: {populationCount}");
            }
            else
            {
                StatsManager.Instance.population = 100;
                Debug.LogWarning($"Invalid input for Population: {populatoin}");
            }
        }
        else
        {
            Debug.LogWarning($"Failed to parse Population input: {populatoin}");
        }
    }

    public void assignTimeMultiplier(string timeMultiplier)
    {
        Debug.Log($"Input received for Time Multiplier: {timeMultiplier}");
        if (float.TryParse(timeMultiplier, out float timeMultiplierCount))
        {
            if (timeMultiplierCount > 0 & timeMultiplierCount <= 20f)
            {
                StatsManager.Instance.timeMultiplier = timeMultiplierCount;
                Debug.Log($"Time Multiplier set: {timeMultiplierCount}");
            }
            else
            {
                StatsManager.Instance.timeMultiplier = 1f;
                Debug.LogWarning($"Invalid input for Time Multiplier: {timeMultiplier}");
            }
        }
        else
        {
            Debug.LogWarning($"Failed to parse Time Multiplier input: {timeMultiplier}");
        }

    }
}
