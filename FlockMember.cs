using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System;

// new trick; every time the calculations are made - the final instruction will be carried out twice; both executions will be moderated (probably values halved)
// next optimization; move the moving out of the calculation loops; then calculations can be made,
//say seven-eight-times a second and movement can be updated at an appropriate pace.
// this will save soo much 


public class FlockMember : MonoBehaviour
{
    public FlockManager myManager;
    public FlockMember thisFlockMember;
    public bool turning;
    public float speed;
    public float fishUpdateRate = 0.0f;

    private bool myManagerIsReady = false;

    /// <summary>
    /// Spread of 0.0166 equals 60 frame/second
    /// </summary>
    public float minWaitTimeSpreadInSeconds;
    public float maxWaitTimeSpreadInSeconds;

    // navigation-relevant variables
    public Vector3 positionNow;
    // The list of flockMembers (this needs to be extensible, dynamic to cope with fish death.)
    private List<FlockMember> flockMemberList;
    private int flockMemberListCount;

    //float sqrNeighbourDistance;
    //float sqrAvoidanceDistance;


    private void OnEnable()
    {

        IndividualizeFish();

        //speed = Random.Range(myManager.minSpeed, myManager.maxSpeed);

        StartSwimming();
    }

    public void IndividualizeFish()
    {

        fishUpdateRate = UnityEngine.Random.Range(minWaitTimeSpreadInSeconds, maxWaitTimeSpreadInSeconds);
        myManager = GetComponentInParent<FlockManager>();
        thisFlockMember = GetComponent<FlockMember>();

    }

    /// <summary>
    /// FishNavigation is started via a seperate method
    /// for added functionality later.
    /// </summary>
    public void StartSwimming()
    {
        StartCoroutine(FishNavigation());
    }

    /// <summary>
    /// FishNavigation doesn't stink.
    /// Every fishUpdateRate seconds
    /// </summary>
    bool checkLockA = false;
    bool checkLockB = false;

    IEnumerator FishNavigation()
    {
        while (myManager == null)
        {
            myManager = GetComponentInParent<FlockManager>();

            yield return new WaitForSecondsRealtime(fishUpdateRate);
        }
        while (myManager != null)
        {

            yield return new WaitForSecondsRealtime(fishUpdateRate);

            //ApplyRules();

            //speed = Random.Range(myManager.minSpeed, myManager.maxSpeed);
            // determin the bounding box of the manager cube - radius swinLimits
            Bounds b = new Bounds(/*myManager.transform.position*/myManager.flockCentre, myManager.swinLimits * 2);

            RaycastHit hit = new RaycastHit();
            Vector3 directionCurrent = transform.forward; // incoming direction
            Vector3 directionNew = Vector3.zero; // outgoing direction            
            Vector3 positionNow = transform.position; // this variable serves to limits 
                                                      // seperate calls to the transform component.
                                                      // not sure if it is an optimization

            turning = false; // turning defaults to false

            // if fish is not within bounding box;
            // fish turns towards goalPos
            if (!b.Contains(positionNow))
            {
                turning = true;
                directionNew = myManager.flockCentre - positionNow;
            }
            // if fish hits something with its ray
            // direction is set utilizing the Vector3.Reflect;
            // fish turns
            checkLockA = !checkLockA; // dumb system to delay raycast to only happen ever fourth round of this loop.
            if (checkLockA)
            {
                checkLockB = !checkLockB;
                if (checkLockB)
                {
                    if (Physics.Raycast(positionNow, directionCurrent * 10, out hit, myManager.avoidanceDistance * 5))
                    {
                        turning = true;
                        Debug.DrawRay(positionNow, directionCurrent * 10, Color.red);
                        directionNew = Vector3.Reflect(directionCurrent, hit.normal);
                        //Debug.Log("Raycast!");

                    }
                }
            }
            // if it is not outside the bounds, not detecting
            //forward that it is about to hit something;
            // fish doesn't turn
            if (turning) // if turning, member rotation is slerped over Time
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      Quaternion.LookRotation(directionNew),
                                                      myManager.rotationSpeed * Time.deltaTime);
            }
            else // not turning, change speed 10% of the time.
            {
                if (Random.Range(0, 100) < 10)
                {
                    speed = Random.Range(/*myManager.minSpeed*/ speed, myManager.maxSpeed);
                }
            }
            ApplyRules();

            myManagerIsReady = true;
        }
    }
    // Update method is not used here, because it clogs the main thread,
    // since all the flock-member try to update at the same time if utilizing Update
    //void FixedUpdate() { }

    /// <summary>
    /// Each gameObject in gos is checked
    ///  1) if they are within neighbourhood-distance;
    ///  1a) those fitting the parameters alter the
    ///      calculation of vavoid, vcentre and gSpeed.
    /// If there are neighbours,
    ///   direction and rotation are updated.     
    /// </summary>
    /// 

    Vector3 vcentre = Vector3.zero;
    Vector3 vavoid = Vector3.zero;

    void ApplyRules()
    {
        // Array of FlockMembers to iterate over
        if (flockMemberList == null)
        {
            flockMemberList = myManager.allFlockmembers;
        }
        // update internal flock list if manager list changed
        if (myManager.allFlockmembers.Count != flockMemberListCount)
        {
            flockMemberList = myManager.allFlockmembers;
            flockMemberListCount = flockMemberList.Count;
        }

        positionNow = transform.position;

        vcentre = Vector3.zero;
        vavoid = Vector3.zero;
        float gSpeed = 0.1f;
        //float nDistance;
        float groupSize = 0;

        float sqrNeighbourDistance = myManager.neighbourDistance * myManager.neighbourDistance;
        float sqrAvoidanceDistance = myManager.avoidanceDistance * myManager.avoidanceDistance;

        //Bounds fishPrivateSpace = new Bounds(transform.position, new Vector3(myManager.avoidanceDistance, myManager.avoidanceDistance, myManager.avoidanceDistance));

        foreach (FlockMember flockMember in flockMemberList) // 
        {
            //if (go != this.gameObject)
            if (flockMember != thisFlockMember)
            {
                // distance to go is measured, if less than neighbourDistance
                // it gets the neighbour-treatment                           
                Vector3 lenghtToOther = flockMember.positionNow - positionNow;
                float sqrLengthToOther = lenghtToOther.sqrMagnitude;

                if (sqrLengthToOther < sqrNeighbourDistance)
                {
                    vcentre += flockMember.positionNow;
                    groupSize++;
                    if (sqrLengthToOther < sqrAvoidanceDistance)
                    {
                        vavoid = vavoid + (positionNow - flockMember.positionNow)/**myManager.avoidanceCoefficient*/;
                    }
                    gSpeed = gSpeed + flockMember.speed;
                }
            }
        }

        if (groupSize > 0)
        {
            vcentre = vcentre / groupSize;
            speed = gSpeed / groupSize;

            // Enforcing speed limit
            if (speed > myManager.maxSpeed)
            {
                speed = myManager.maxSpeed;
            }

            //Vector3 direction = (vcentre + vavoid) - positionNow;
            //if (direction != Vector3.zero)
            //{
            //    transform.rotation = Quaternion.Slerp(transform.rotation,
            //                                            Quaternion.LookRotation(direction),
            //                                            myManager.rotationSpeed * Time.deltaTime);
            //}
        }
    }

    public GameObject leader;

    enum MemberState
    {
        notReady, isolated, flocking, flockingToLeader,
    }

    private void FixedUpdate()
    {
        Vector3 direction;
        if (myManagerIsReady)
        {
            if (myManager.designatingLeader)
            {   // if member is following a leader as centre                
                direction = (vcentre + vavoid + (leader.transform.position - positionNow)) - positionNow;
            }
            else
            {
                direction = (vcentre + vavoid) - positionNow;
            }
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                                                            Quaternion.LookRotation(direction),
                                                            myManager.rotationSpeed * Time.deltaTime);
                }

            transform.Translate(0, 0, Time.deltaTime * speed);
        }
        else
        {
            //Debug.Log("Manager not ready.");
        }
    }

}
