using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Agent_Controller : Agent
{    
    //Pellet Variables
    [SerializeField] private Transform target;
    public int pelletCount;
    public GameObject food;
    [SerializeField] private List<GameObject> spawnedPelletsList = new List<GameObject>();

    //Agent Variables
    [SerializeField] private float moveSpeed = 4f;
    private Rigidbody rb;

    //Enviorment Variables
    [SerializeField] private Transform enviormentLocation;
    Material envMaterial;
    public GameObject env;

    //Time Keeping Variables
    [SerializeField] private int timeForEpidose;
    private float timeLeft;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envMaterial = env.GetComponent<Renderer>().material;
    }

    public override void OnEpisodeBegin()
    {
        //Agent
        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));

        //Pellet
        CreatePellet();
        //target.localPosition = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));

        //Timer
        episodeTImerNew();
    }

    private void Update()
    {
        CheckRemainingTime();
    }

    private void CreatePellet()
    {
        if(spawnedPelletsList.Count != 0)
        {
            RemovePellet(spawnedPelletsList);
        }

        for(int i = 0; i < pelletCount; i++)
        {
            int counter = 0;
            bool distanceGood;
            bool alreadyDecremented = false;

            //Spawning Pellet
            GameObject newPellet = Instantiate(food);
            //Make pellet child of the enviormnt
            newPellet.transform.parent = enviormentLocation;
            //Givev Random SPawn Slocation
            Vector3 pelletLoction = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));

            if(spawnedPelletsList.Count != 0)
            {
                for(int k = 0; k < spawnedPelletsList.Count; k++)
                {
                    if(counter < 10)
                    {
                        distanceGood = CheckOverlap(pelletLoction, spawnedPelletsList[k].transform.localPosition, 1f);
                        if(distanceGood == false)
                        {
                            pelletLoction = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));
                            k--;
                            alreadyDecremented = true;
                            Debug.Log("Too close to other pellet");
                        }
                        distanceGood = CheckOverlap(pelletLoction, transform.localPosition, 1f);
                        if(distanceGood == false)
                        {
                            Debug.Log("Too close to agent");
                            pelletLoction = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));
                            if(alreadyDecremented == false)
                            {
                                k--;
                            }
                        }
                        counter++;
                    }
                    else
                    {
                        k = spawnedPelletsList.Count;
                    }
                }
            }

            //Spawn in new location
            newPellet.transform.localPosition = pelletLoction;
            //Add list
            spawnedPelletsList.Add(newPellet);            
        }
    }


    public bool CheckOverlap(Vector3 objectWeWantToAvoidOverlapping, Vector3 alreadtEcistingObj, float minDistanceWanted)
    {
        float DistanceBetweenObj = Vector3.Distance(objectWeWantToAvoidOverlapping, alreadtEcistingObj);

        if(minDistanceWanted <=  DistanceBetweenObj)
        {
            return true;
        }
        return false;
    }

    private void RemovePellet(List<GameObject> toBeDeletedGameObjectList)
    {
        foreach(GameObject i in toBeDeletedGameObjectList)
        {
            Destroy(i.gameObject);
        }
        toBeDeletedGameObjectList.Clear();
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
        if(other.gameObject.tag == "Pellet")
        {
            //Remove from list
            spawnedPelletsList.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(5f);
            if(spawnedPelletsList.Count == 0)
            {
                envMaterial.color = Color.green;
                RemovePellet(spawnedPelletsList);
                AddReward(5f);
                EndEpisode();
            }
        }
        if(other.gameObject.tag == "Walls")
        {
            envMaterial.color = Color.red;
            RemovePellet(spawnedPelletsList);
            AddReward(-10f);
            EndEpisode();
        }
    }

    private void episodeTImerNew()
    {
        timeLeft = Time.time + timeForEpidose;
    }

    private void CheckRemainingTime()
    {
        if(Time.time >= timeLeft)
        {
            envMaterial.color = Color.blue;
            AddReward(-15f);
            RemovePellet(spawnedPelletsList);
            EndEpisode();
        }
    }
}
