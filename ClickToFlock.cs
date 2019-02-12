using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToFlock : MonoBehaviour
{
    FlockMember thisMember;
    FlockManager memberFlockManager;

    // Start is called before the first frame update
    void Start()
    {
        thisMember = GetComponent<FlockMember>();
        memberFlockManager = thisMember.myManager;

    }

    private void OnMouseDown() // inelegant as fuck, but point proven.
    {

        //memberFlockManager.goalGameObject = this.gameObject;
        if (Input.GetKeyDown(KeyCode.Z))
        {
            for (int i = 0; i < memberFlockManager.allFish.Length; i++)
            {
                if (memberFlockManager.allFish[i] != thisMember)
                {
                    memberFlockManager.allFish[i].GetComponent<FlockMember>().leader = gameObject;
                    Debug.Log("Leader of " + memberFlockManager.allFish[i].name + " now has a new leader");
                }

            }
        }
        else
        {
            for (int i = 0; i < memberFlockManager.allFish.Length; i++)
            {
                memberFlockManager.allFish[i].GetComponent<FlockMember>().leader = memberFlockManager.goalGameObject;

            }
            memberFlockManager.designatingLeader = !memberFlockManager.designatingLeader;
        }
    }
}
