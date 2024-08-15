using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timeMultiplier : MonoBehaviour
{

    private StatsManager statsManager;
   void Start()
    {
        statsManager = GameObject.FindObjectOfType<StatsManager>();

        
        Time.timeScale = statsManager.timeMultiplier;
    }
}
