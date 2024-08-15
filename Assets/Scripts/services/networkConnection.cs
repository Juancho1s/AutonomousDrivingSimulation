using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

using System;

public class APIClient : MonoBehaviour
{


    [System.Serializable]
    public class NetworkRequest_runNetwork
    {
        public float[] inputs;
        public int userLevel;
    }

    [System.Serializable]
    public class NetworkResponse_runNetwork
    {
        public float a;
        public float t;
    }



    [System.Serializable]
    public class NetworkRequest_weights
    {
        public int userLevel;
    }
    [System.Serializable]
    public class NetworkResponse_weights
    {
        public Dictionary<string, Dictionary<string, List<float>>> weights;
        public List<List<float>> biases;
    }



    [System.Serializable]
    public class NetworkRequest_updates
    {
        public Dictionary<string, Dictionary<string, List<float>>> layers; // Represents the neural network
        public List<float> biases;
        public float fitness;
        public int id_nnet;
    }

    [System.Serializable]
    public class NetworkRequest_NewNNet
    {
        public string name;
        public string description;
        public int new_id;
        public float fitness;
        public List<float> biases;
        public Dictionary<string, Dictionary<string, List<float>>> layers;
    }

    public IEnumerator SendRequest_runNetwork(float aSensor, float bSensor, float cSensor, System.Action<float, float> callback)
    {
        NetworkRequest_runNetwork data = new NetworkRequest_runNetwork
        {
            inputs = new float[] { aSensor, bSensor, cSensor },
            userLevel = 24
        };
        string url = "http://127.0.0.1:5000/run_network"; // Adjust to your Flask API endpoint

        string json = JsonUtility.ToJson(data);
        UnityWebRequest request = generateRequest(json, "POST", url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // Parse the JSON response
            string responseText = request.downloadHandler.text;
            NetworkResponse_runNetwork response = JsonUtility.FromJson<NetworkResponse_runNetwork>(responseText);



            // Use the callback to pass the values of a and t
            callback(response.a, response.t);
        }
    }

    public IEnumerator SendRequest_updateTraining(Dictionary<string, Dictionary<string, List<float>>> layers, List<float> biases, float fitness, int id_nnet)
    {
        var data = new NetworkRequest_updates
        {
            layers = layers,
            biases = biases,
            fitness = fitness,
            id_nnet = id_nnet
        };

        string url = "http://127.0.0.1:5000/train/genome";
        string json = JsonConvert.SerializeObject(data);  // Serialize using Json.NET

        Debug.Log($"{json}, POST, {url}");
        UnityWebRequest request = generateRequest(json, "POST", url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
    }

    public IEnumerator SendRequest_weightsAndBiases(int userLevel, System.Action<Dictionary<string, Dictionary<string, List<float>>>, List<List<float>>> callback)
    {
        var data = new NetworkRequest_weights
        {
            userLevel = userLevel
        };

        string url = "http://127.0.0.1:5000/obtain_weights_biases";
        string json = JsonConvert.SerializeObject(data);

        Debug.Log(json);
        UnityWebRequest request = generateRequest(json, "GET", url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            var response = JsonConvert.DeserializeObject<NetworkResponse_weights>(request.downloadHandler.text);

            // {
            //     "weights": {
            //         "layer1": {
            //             "neuron1": [weight1, weight2, weight3, ...],
            //             "neuron2": [weight4, weight5, weight6, ...],
            //             ...
            //         },
            //         "layer2": {
            //             "neuron1": [weight7, weight8, weight9, ...],
            //             "neuron2": [weight10, weight11, weight12, ...],
            //             ...
            //         },
            //         ...
            //     },
            //     "biases": [bias1, bias2, bias3, ...]
            // }
            callback(response.weights, response.biases);
        }
    }

    public IEnumerator SendRequest_newNNet(string name, string description, int new_id, float fitness, List<float> biases, Dictionary<string, Dictionary<string, List<float>>> layers)
    {
        var data = new NetworkRequest_NewNNet
        {
            name = name,
            description = description,
            new_id = new_id,
            fitness = fitness,
            biases = biases,
            layers = layers
        };

        string url = "http://127.0.0.1:5000/insert_nnet";
        string json = JsonConvert.SerializeObject(data);
        Debug.Log($"{json}, POST, {url}");

        UnityWebRequest request = generateRequest(json, "POST", url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
    }


    private UnityWebRequest generateRequest(string jsonData, string methodType, string URL)
    {
        UnityWebRequest request = new UnityWebRequest(URL, methodType);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        return request;
    }
}
