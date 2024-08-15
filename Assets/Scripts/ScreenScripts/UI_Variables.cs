using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Variables : MonoBehaviour
{

    private GeneticManager geneticManager;

    [Header("Information Panel")]
    public TextMeshProUGUI GenerationCounter;
    public TextMeshProUGUI GenomeCounter;
    public TextMeshProUGUI FitnessCounter;

    // Start is called before the first frame update
    void Start()
    {
        geneticManager = GameObject.Find("AI_Manager").GetComponent<GeneticManager>();

        GenerationCounter.text = geneticManager.currentGeneration + "";
        GenomeCounter.text = geneticManager.currentGenome + "";
    }

    public void setBestFitness(string bestFitness)
    {
        FitnessCounter.text = bestFitness;
    }

    public void setCurrentGenome(string currentGenome)
    {
        GenomeCounter.text = currentGenome; 
    }

    public void setCurrentGeneration(string currentGeneration)
    {
        GenerationCounter.text = currentGeneration;
    }
}
