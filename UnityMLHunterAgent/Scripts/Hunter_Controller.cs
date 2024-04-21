using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Hunter_Controller : Agent
{
    //Hunter Variables
    [SerializeField] private float moveSpeed = 4f;
    private Rigidbody rb;

    Material envMaterial;
    public GameObject env;

    public GameObject prey;
    public Agent_Controller classObject;

     public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envMaterial = env.GetComponent<Renderer>().material;
    }

    public override void OnEpisodeBegin()
    {
        //Hunter
        Vector3 spawnLocation = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));

        bool distanceGood = classObject.CheckOverlap(prey.transform.localPosition, spawnLocation, 5f);

        while(!distanceGood)
        {
            spawnLocation = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));
            distanceGood = classObject.CheckOverlap(prey.transform.localPosition, spawnLocation, 5f);
        }

        transform.localPosition = spawnLocation;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        //sensor.AddObservation(target.localPosition);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);

        /*
        Vector3 velocity = new Vector3(moveX, 0f, moveZ);
        velocity = velocity.normalized * Time.deltaTime * moveSpeed;

        transform.localPosition += velocity;
        */
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> ContinuousActions = actionsOut.ContinuousActions;
        ContinuousActions[0] = Input.GetAxisRaw("Horizontal");
        ContinuousActions[1] = Input.GetAxisRaw("Vertical");

    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Agent")
        {
            AddReward(10f);
            classObject.AddReward(-15f);
            envMaterial.color = Color.yellow;
            classObject.EndEpisode();
            EndEpisode();

        }
        if(other.gameObject.tag == "Walls")
        {
            envMaterial.color = Color.red;
            AddReward(-10f);
            classObject.EndEpisode();
            EndEpisode();
        }
    }
}