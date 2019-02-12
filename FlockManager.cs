using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fish-like flocking.
/// Current version is not dynamic; I want the members to be able to change manager
/// depending on criteria like last proximity to current manager;
/// The things taken care of with the parenting of all the fish to the manager
/// should be switched around to lists; and that should be a direct path to implementing dynamic
/// manager switching.
/// </summary>

public class FlockManager : MonoBehaviour
{

    #region FlockManager Variables

    public GameObject fishPrefab;
    public GameObject[] allFish;
    public List<FlockMember> allFlockmembers;
    public int numFish = 20;
    public Vector3 swinLimits /*= new Vector3(5,5,5)*/;
    public Vector3 spawnLimits /*= new Vector3(5,5,5)*/;

    private Vector3 originalFlockManagerPosition;

    [HideInInspector]
    public Vector3 flockCentre; // note, it is the centre of the box determining if a fish is within a radius defined by swimLimits 
    public GameObject goalObjectPrefab; // prefab to be instantiated

    public GameObject goalGameObject; // instantiated flockCentre and temporary leader object.

    public bool designatingLeader = false;

    public float timeBetweenGoalUpdate = 30.1f;

    private bool isMovingFlockCentreObject = false;


    [Header("Fishy settings.")]
    [Range(0.1f, 5.0f)]
    public float minSpeed;
    [Range(0.2f, 25.0f)]
    public float maxSpeed;
    [Range(0.0f, 25.0f)]
    public float neighbourDistance;
    [Range(0.1f, 6.0f)]
    public float avoidanceDistance;
    [Range(0.1f, 5.0f)]
    public float rotationSpeed;
    [Range(0.1f, 5.0f)]
    public float avoidanceCoefficient;

    #endregion



    private void OnEnable()
    {
        // store original position, so that goalPos can be reset when getting too far away
        originalFlockManagerPosition = transform.position;

        goalGameObject = EmeraldObjectPool.Spawn(goalObjectPrefab, transform.position, Quaternion.identity);
        //goalGameObject = goalObjectPrefab;


        flockCentre = goalGameObject.transform.position /*+ new Vector3(Random.Range(-spawnLimits.x, spawnLimits.x),
                                                                        Random.Range(-spawnLimits.y, spawnLimits.y),
                                                                        Random.Range(-spawnLimits.z, spawnLimits.z))*/;
        StartCoroutine(Initialize(numFish));

    }

    /// <summary>
    /// Spawns flockmembers at controlled Vector3 locations,
    /// sets the manager object & childs each member 
    /// to the GO that FlockManager is on.
    /// </summary>
    /// <param name="numberOfFish"></param>
    IEnumerator Initialize(int numberOfFish)
    {
        allFish = new GameObject[numFish];
        for (int i = 0; i < numFish; i++)
        {
            Vector3 pos = this.transform.position + new Vector3(Random.Range(-spawnLimits.x, spawnLimits.x),
                                                                Random.Range(-spawnLimits.y, spawnLimits.y),
                                                                Random.Range(-spawnLimits.z, spawnLimits.z));
            allFish[i] = (GameObject)Instantiate(fishPrefab, pos, Quaternion.identity);
            allFish[i].GetComponent<FlockMember>().myManager = this;
            allFish[i].transform.SetParent(transform);
            allFlockmembers.Add(allFish[i].GetComponent<FlockMember>());

            allFish[i].GetComponent<FlockMember>().leader = goalGameObject;

            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(SetNewGoalLoop());
        StartCoroutine(LeaderOrNot());

    }
    //}    public void Initialize(int numberOfFish)
    //{
    //    allFish = new GameObject[numFish];
    //    for (int i = 0; i < numFish; i++)
    //    {
    //        Vector3 pos = this.transform.position + new Vector3(Random.Range(-spawnLimits.x, spawnLimits.x),
    //                                                            Random.Range(-spawnLimits.y, spawnLimits.y),
    //                                                            Random.Range(-spawnLimits.z, spawnLimits.z));
    //        allFish[i] = (GameObject)Instantiate(fishPrefab, pos, Quaternion.identity);
    //        allFish[i].GetComponent<FlockMember>().myManager = this;
    //        allFish[i].transform.SetParent(transform);
    //        allFlockmembers.Add(allFish[i].GetComponent<FlockMember>());
    //    }
    //}

    // FixedUpdate is called 60 times a second.
    void FixedUpdate()
    {
        if (isMovingFlockCentreObject)
        {
            goalGameObject.transform.position = Vector3.MoveTowards(
                                                            goalGameObject.transform.position, flockCentre,
                                                            maxSpeed * Time.deltaTime);
            if (Vector3.Distance(flockCentre, goalGameObject.transform.position) < 0.5f)
            {
                //Debug.Log("goalGameObject was found in close proximity");
                isMovingFlockCentreObject = false;
            }
        }
    }

    /// <summary>
    /// Until this behavoir is not enabled, this loop will update goalPos 
    /// at an interval of timeBetweenGoalUpdate.
    /// If goalPos gets too far from the original spawning-position
    /// the position of goalPos will be set much closer to home.
    /// </summary>
    /// <returns></returns>
    IEnumerator SetNewGoalLoop()
    {
        while (enabled)
        {
            if (Vector3.Distance(flockCentre, originalFlockManagerPosition) > 30f)
            {
                flockCentre = originalFlockManagerPosition + new Vector3(Random.Range(-swinLimits.x, swinLimits.x),
                                                            Random.Range(-swinLimits.y, swinLimits.y),
                                                            Random.Range(-swinLimits.z, swinLimits.z));
                isMovingFlockCentreObject = true;
                //Debug.Log("goalPos redefined to be: '" + flockCentre + "': Closer to the original position " + originalFlockManagerPosition);
            }
            else
            {
                flockCentre = goalGameObject.transform.position + new Vector3(Random.Range(-swinLimits.x, swinLimits.x),
                                                                            Random.Range(-swinLimits.y, swinLimits.y),
                                                                            Random.Range(-swinLimits.z, swinLimits.z));
                isMovingFlockCentreObject = true;
                Debug.Log("goalPos redefined to " + flockCentre);
            }
            yield return new WaitForSecondsRealtime(timeBetweenGoalUpdate);
        }
    }

    IEnumerator LeaderOrNot()
    {
        //while (enabled)
        //{
        //    if (Input.GetMouseButtonDown(1)) designatingLeader = !designatingLeader;
        //    yield return new WaitForFixedUpdate();
        //}
        yield return new WaitForEndOfFrame();
    }
}
