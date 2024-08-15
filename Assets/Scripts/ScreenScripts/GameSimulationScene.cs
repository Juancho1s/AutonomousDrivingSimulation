using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSimulationScene : MonoBehaviour
{
    public Text text;
    private NNet nNet;
    private CarControl carControl;

    // Start is called before the first frame update
    void Start()
    {
        // No need to get the StatsManager component, use the Singleton instance
        nNet = GameObject.Find("jeep").GetComponent<NNet>();
        carControl = GameObject.Find("jeep").GetComponent<CarControl>();

        carControl.LAYERS = StatsManager.Instance.LAYERS;
        carControl.NEURONS = StatsManager.Instance.NEURONS;

        Debug.Log($"Layers: {StatsManager.Instance.LAYERS}, Neurons: {StatsManager.Instance.NEURONS}");
    }

    public void EndSimulationButton()
    {
        SceneManager.LoadScene("mainMenu");
    }
}
