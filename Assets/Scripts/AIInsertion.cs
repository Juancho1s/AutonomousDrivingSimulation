using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NNet))]
public class AIInsertion : MonoBehaviour
{
    [Header("References")]
    private APIClient API;

    private Vector3 startPosition, startRotation;
    private NNet network;

    [Range(-1f, 1f)]
    public float a, t;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultipler = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;

    [Header("Network Options")]
    public int LAYERS = 5;
    public int NEURONS = 30;

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    private float aSensor, bSensor, cSensor;

    private void Awake()
    {
        API = new();
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        network = GetComponent<NNet>();

        network.initializeAIInsertion(LAYERS, NEURONS);

        StartCoroutine(API.SendRequest_weightsAndBiases(24, (weights, biases) =>
        {
            Debug.Log("Biases received: " + biases.Count); // Log the count of biases received
            Debug.Log("Assigning biases to network..."); // Log before assignment
            network.biases = new List<List<float>>(biases);
            Debug.Log("Biases in network: " + network.biases.Count); // Log the count after assignment
            Debug.Log("Assigning weights to network..."); // Log before assignment

            int layerIndex = 0;
            foreach (var layer in weights.Values)
            {
                int neuronIndex = 0;
                foreach (var neuron in layer.Values)
                {
                    for (int i = 0; i < neuron.Count; i++)
                    {
                        network.weights[layerIndex][neuronIndex, i] = neuron[i];
                    }
                    neuronIndex++;
                }
                layerIndex++;
            }

            Debug.Log("Weights Inserted");

            

        }));
    }


    public void Reset()
    {

        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Reset();
    }

    private void FixedUpdate()
    {

        InputSensors();
        lastPosition = transform.position;


        (a, t) = network.RunNetwork(aSensor, bSensor, cSensor);


        MoveCar(a, t);

        timeSinceStart += Time.deltaTime;

        CalculateFitness();

        //a = 0;
        //t = 0;


    }


    private void CalculateFitness()
    {

        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness = (totalDistanceTravelled * distanceMultipler) + (avgSpeed * avgSpeedMultiplier) + (((aSensor + bSensor + cSensor) / 3) * sensorMultiplier);

        if (timeSinceStart > 20 && overallFitness < 40)
        {
            Reset();
        }

        if (overallFitness >= 1000)
        {
            //Saves network to a JSON
            Reset();
        }

    }

    private void InputSensors()
    {

        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit))
        {
            aSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = b;

        if (Physics.Raycast(r, out hit))
        {
            bSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = c;

        if (Physics.Raycast(r, out hit))
        {
            cSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

    }

    private Vector3 inp;
    public void MoveCar(float v, float h)
    {
        inp = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, v * 11.4f), 0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp;

        transform.eulerAngles += new Vector3(0, (h * 90) * 0.02f, 0);
    }

}