using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Firebase.Database;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class JumpToTouch : MonoBehaviour
{
    NavMeshAgent agent;
    DatabaseReference mDatabaseRef;
    public TMP_Text TMP_Text_Debug;
    RaycastHit hit_prev;

    public GameObject MainPlayer_cylinder;
    public GameObject mainPlayerObject;
    public GameObject StartLocation0;
    public GameObject StartLocation0_1;
    public GameObject StartLocation1;
    public GameObject StartLocation1_1;
    public GameObject StartLocation2;
    public GameObject StartLocation2_1;
    public GameObject StartLocation3;
    public GameObject StartLocation3_1;
    public GameObject StartLocation4;
    public GameObject StartLocation4_1;
    public GameObject StartLocation5;
    public GameObject StartLocation5_1;
    public GameObject StartLocation6;
    public GameObject StartLocation6_1;
    public GameObject StartLocation7;
    public GameObject StartLocation7_1;
    public GameObject StartLocation8;
    public GameObject StartLocation8_1;
    public GameObject StartLocation9;
    public GameObject StartLocation9_1;
    public GameObject StartLocation10;
    public GameObject StartLocation10_1;
    public GameObject StartLocation11;
    public GameObject StartLocation11_1;
    public GameObject StartLocationPeak;
    public GameObject StartLocation_ChallengeArea;      //challenge starting mat
    public GameObject StartLocation_TribalCouncilArea;  //entrance
    public int JumpToThisLocation = 0;

    public GameObject GO_ChallengeArea;
    public GameObject GO_TribalArea;

    Vector2 touchDown;
    Vector2 touchUp;

    bool jumpToLocation = false;
    List<Vector3> incrementalJumpList = new List<Vector3>();
    Vector3 jumpDestination;
    bool justFinishedJump = false;
    float timeSinceJump;
    float failCount = 0;

    List<GameObject> wayPointList = new List<GameObject>();
    GameObject[] wayPointArray;


    // Start is called before the first frame update
    void Start()
    {
        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        wayPointArray = new GameObject[]
        { StartLocation0, StartLocation1, StartLocation2, StartLocation3, StartLocation4,  StartLocation5,
          StartLocation6, StartLocation7, StartLocation8, StartLocation9, StartLocation10, StartLocation11,
          StartLocation0_1, StartLocation1_1, StartLocation2_1, StartLocation3_1, StartLocation4_1,  StartLocation5_1,
          StartLocation6_1, StartLocation7_1, StartLocation8_1, StartLocation9_1, StartLocation10_1, StartLocation11_1 };

        if (Global.JumpToLocation)
        {
            Global.JumpToLocation = false;
            Global.ShowLoadingScreenWhileWarping = false;
            //jumpDestination = GetRandomStartingLocation(Global.JumpDestination, -2, 2);
            //load_incrementalJumpList(mainPlayerObject.transform.position, jumpDestination, 50f);
            //jumpToLocation = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (Global.LockCameraRotation == false)
        //    return; //don't allow player to move if we are trying to move the camera

        if(Global.JumpToLocation)
        {
            return;
        }

        if(jumpToLocation)
        {
            if(incrementalJumpList.Count > 0)
            {
                mainPlayerObject.transform.position = incrementalJumpList[0];
                transform.position = incrementalJumpList[0];
                incrementalJumpList.RemoveAt(0);
                Global.ShowLoadingScreenWhileWarping = true;
            }
            else
            {
                mainPlayerObject.transform.position = jumpDestination;
                transform.position = GetRayCastAtPosition(jumpDestination);

                if (wayPointList.Count > 0)
                    JumpToNextQueuePosition();
                else
                {
                    jumpToLocation = false;
                    justFinishedJump = true;
                    timeSinceJump = 0;
                }
            }

            return;
        }

        if(justFinishedJump)
        {
            timeSinceJump += Time.deltaTime;
            if(timeSinceJump > 0.5)
            {
                justFinishedJump = true;
                float dist = Vector3.Distance(jumpDestination, mainPlayerObject.transform.position);
                if (dist > 20)
                {
                    jumpDestination = GetRandomStartingLocation(jumpDestination, -2, 2);
                    load_incrementalJumpList(mainPlayerObject.transform.position, jumpDestination, 50f);
                    jumpToLocation = true;

                    failCount++;
                    if(failCount > 5)
                    {
                        Global.JumpDestination = jumpDestination;
                        Global.JumpToLocation = true;
                        Global.ShowLoadingScreenWhileWarping = false;
                        SceneManager.LoadScene((int)Global.Screen.Screen_Game); //restart scene
                    }
                }
                else
                {
                    Global.ShowLoadingScreenWhileWarping = false;
                    justFinishedJump = false;
                }
            }
        }

        if(CheckIfPlayerNeedsToJumpToSomewhere())
        {
            return; //if the player needs to jump to somewhere, don't execute the rest of the code in Update
        }

        bool clickingGuiElement;
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
            {
                if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<CanvasRenderer>() != null)
                {
                    //setting a boolean here, if it was true this means I clicked on a UI element
                    clickingGuiElement = true;
                }
                else
                {
                    clickingGuiElement = false;
                }
            }
            else
            {
                clickingGuiElement = false;
            }
        }
        else
        {
            clickingGuiElement = false;
        }

        //Look for a mouse down followed by a mouse up, measure distance, and if distance is small, move the cylinder


        if (Input.GetMouseButtonDown(0) && !clickingGuiElement)
        {
            if (Global.ignoreRaycast)
            {
                Global.ignoreRaycast = false;
                return;
            }

            touchDown = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && !clickingGuiElement)
        {
            if (Global.ignoreRaycast)
            {
                Global.ignoreRaycast = false;
                return;
            }

            touchUp = Input.mousePosition;

            float touchDown_TouchUp_Diff = Vector2.Distance(touchDown, touchUp);
            //TMP_Text_Debug.text = "" + touchDown_TouchUp_Diff;

            if (touchDown_TouchUp_Diff > Screen.width * 0.03)   //if the difference from the touch down to the touch up is greater than 3% of the screen's width, we'll assume they are trying to move the camera and ignore the "movement" touch
                return;

            //RaycastHit hit;

            //Transform targetTransform = null; // target assigned in inspector 
            RaycastHit[] hits; // returned hits
            //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000))
            //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, float.MaxValue, (1 << 8)))
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hits = Physics.RaycastAll(ray);
            foreach(RaycastHit hit in hits)
            {
                
                if(hit.transform.gameObject.CompareTag("Tree"))
                {
                    TMP_Text_Debug.text = "Tree";
                }
                if (hit.transform.gameObject.CompareTag("ChatCircle"))
                {
                    TMP_Text_Debug.text = "ChatCircle";
                }

                if (hit.point.y >= Global.MIN_ALTITUDE && !hit.transform.gameObject.CompareTag("ChatCircle")) //to prevent player from going under water or click on ChatCircle
                {
                    float position_Diff = Vector3.Distance(transform.position, hit.point);
                    position_Diff = 0;  //TODO remove?
                    Global.doWarp = false;
                    if (position_Diff > 500)
                    {
                        //transform.position = new Vector3(transform.position.x * 1.01f, transform.position.y * 1.01f, transform.position.z * 1.01f);
                        mainPlayerObject.transform.position = transform.position;
                    }
                    else
                    {
                        transform.position = hit.point;
                    }

                    Global.holdPositions = false;
                    Global.holdPosition_timer = 30.0f * Global.currentGame.playerCount;

                    mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("destinationArray").Child("" + Global.mainPlayerIndex).Child("x").SetValueAsync(transform.position.x);
                    mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("destinationArray").Child("" + Global.mainPlayerIndex).Child("y").SetValueAsync(transform.position.y);
                    mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("destinationArray").Child("" + Global.mainPlayerIndex).Child("z").SetValueAsync(transform.position.z);
                }

                //TMP_Text_Debug.text = "" + hit.transform.tag;

                //float distDiff = Vector3.Distance(hit.point, hit_prev.point);
                //TMP_Text_Debug.text = "" + distDiff;
                //
            }
        }

        if(Global.stopMoving_mainPlayer)
        {
            Global.stopMoving_mainPlayer = false;
            transform.position = Global.currentGame.destinationArray[Global.mainPlayerIndex];
        }

    }

    bool atTribal = false;
    bool atChallengeOrSwapOrMerge = false;
    bool atCamp = false;
    public bool CheckIfPlayerNeedsToJumpToSomewhere()
    {
        if(Global.currentGame.currentPhaseOfDay == Global.EventSequence.Tribal)
        {
            float dist = Vector3.Distance(mainPlayerObject.transform.position, GO_TribalArea.transform.position);
            if(dist > 30)
            {
                ButtonClick_GoToCamp(20000);    //go to tribal area
                atTribal = true;
                atChallengeOrSwapOrMerge = false;
                atCamp = false;
                return true;
            }
        }
        else if(Global.currentGame.currentPhaseOfDay == Global.EventSequence.RewardOrImmunity || Global.currentGame.currentPhaseOfDay == Global.EventSequence.SwapOrMerge)
        {
            float dist = Vector3.Distance(mainPlayerObject.transform.position, GO_ChallengeArea.transform.position);
            if (dist > 150)
            {
                ButtonClick_GoToCamp(10000);    //go to challenge area
                atChallengeOrSwapOrMerge = true;
                atTribal = false;
                atCamp = false;
                return true;
            }
        }
        else if ((atTribal || atChallengeOrSwapOrMerge) &&
            (Global.currentGame.currentPhaseOfDay == Global.EventSequence.CampTime_First ||
             Global.currentGame.currentPhaseOfDay == Global.EventSequence.CampTime_PostChallenge ||
             Global.currentGame.currentPhaseOfDay == Global.EventSequence.CampTime_PostSwapOrMerge )  )
        {
            float dist = Vector3.Distance(mainPlayerObject.transform.position, StartLocation0.transform.position);
            if (dist > 30)
            {
                ButtonClick_GoToCamp(0);    //go to start area
                atCamp = true;
                atTribal = false;
                atChallengeOrSwapOrMerge = false;
                return true;
            }

        }

        if (atTribal)
        {
            Debug.Log("Distance from Tribal: " + Vector3.Distance(mainPlayerObject.transform.position, GO_TribalArea.transform.position));
        }
        else if (atChallengeOrSwapOrMerge)
        {
            Debug.Log("Distance from Challenge/Swap/Merge: " + Vector3.Distance(mainPlayerObject.transform.position, GO_ChallengeArea.transform.position));
        }
        else if (atCamp)
        {
            Debug.Log("Distance from Camp: " + Vector3.Distance(mainPlayerObject.transform.position, StartLocation0.transform.position));
        }
        return false;
    }

    public void ButtonClick_GoToCamp(int val)
    {
        JumpToThisLocation = val;
        ButtonClick_GoToCamp();
    }

    float forceY;
    public void ButtonClick_GoToCamp()
    {
        //    mainPlayerObject.transform.position = GetRandomStartingLocation(mainPlayerObject.transform.position, -0.1f, 0.1f);
        //transform.position = mainPlayerObject.transform.position;

        int finalLocation = -1;
        if(JumpToThisLocation == 10000) //challenge area
        {
            JumpToThisLocation = 11;    //go to position 11
            finalLocation = 10000;      //and then the challenge area
        }
        else if (JumpToThisLocation == 20000) //tribal council area
        {
            JumpToThisLocation = 0;    //go to position 0
            finalLocation = 20000;      //and then the challenge area
        }

        int locationIndex = JumpToThisLocation;
        forceY = -99999;
        GameObject StartLocation;
        switch(locationIndex)
        {
            case 0:
            default:
                StartLocation = StartLocation0;
                break;
            case 100:
                StartLocation = StartLocation0_1;
                break;
            case 1:
                StartLocation = StartLocation1;
                break;
            case 101:
                StartLocation = StartLocation1_1;
                break;
            case 2:
                StartLocation = StartLocation2;
                break;
            case 201:
                StartLocation = StartLocation2_1;
                break;
            case 3:
                StartLocation = StartLocation3;
                break;
            case 301:
                StartLocation = StartLocation3_1;
                break;
            case 4:
                StartLocation = StartLocation4;
                break;
            case 401:
                StartLocation = StartLocation4_1;
                break;
            case 5:
                StartLocation = StartLocation5;
                break;
            case 501:
                StartLocation = StartLocation5_1;
                break;
            case 6:
                StartLocation = StartLocation6;
                break;
            case 601:
                StartLocation = StartLocation6_1;
                break;
            case 7:
                StartLocation = StartLocation7;
                break;
            case 701:
                StartLocation = StartLocation7_1;
                break;
            case 8:
                StartLocation = StartLocation8;
                break;
            case 801:
                StartLocation = StartLocation8_1;
                break;
            case 9:
                StartLocation = StartLocation9;
                break;
            case 901:
                StartLocation = StartLocation9_1;
                break;
            case 10:
                StartLocation = StartLocation10;
                break;
            case 1001:
                StartLocation = StartLocation10_1;
                break;
            case 11:
                StartLocation = StartLocation11;
                break;
            case 1101:
                StartLocation = StartLocation11_1;
                break;
            case 99:
                StartLocation = StartLocationPeak;
                break;
            case 10000:
                StartLocation = StartLocation_ChallengeArea;
                forceY = StartLocation.transform.position.y;
                break;
            case 20000:
                StartLocation = StartLocation_TribalCouncilArea;
                break;
        }

        Create_WayPoint_Queue(mainPlayerObject.transform.position, StartLocation);
        if(finalLocation == 10000)
        {
            wayPointList.Add(StartLocation_ChallengeArea);
        }
        else if (finalLocation == 20000)
        {
            wayPointList.Add(StartLocation_TribalCouncilArea);
        }

        JumpToNextQueuePosition();
        //MainPlayer_cylinder.transform.position = jumpDestination;
        //mainPlayerObject.transform.position = jumpDestination;
        //transform.position = jumpDestination;

        //mainPlayerObject.transform.position = GetRandomStartingLocation(mainPlayerObject.transform.position, -0.1f, 0.1f);
        //transform.position = mainPlayerObject.transform.position;

        //Global.holdPositions = true;
        Global.doWarp = true;
        Global.holdPositions = false;


        mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("destinationArray").Child("" + Global.mainPlayerIndex).Child("x").SetValueAsync(transform.position.x);
        mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("destinationArray").Child("" + Global.mainPlayerIndex).Child("y").SetValueAsync(transform.position.y);
        mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("destinationArray").Child("" + Global.mainPlayerIndex).Child("z").SetValueAsync(transform.position.z);

        //mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("arrivedAtPosition").Child("" + Global.mainPlayerIndex).Child("x").SetValueAsync(transform.position.x);
        //mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("arrivedAtPosition").Child("" + Global.mainPlayerIndex).Child("y").SetValueAsync(transform.position.y);
        //mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("arrivedAtPosition").Child("" + Global.mainPlayerIndex).Child("z").SetValueAsync(transform.position.z);

    }

    void JumpToNextQueuePosition()
    {
        jumpDestination = GetRandomStartingLocation(wayPointList[0].transform.position, -2, 2);
        wayPointList.RemoveAt(0);
        load_incrementalJumpList(mainPlayerObject.transform.position, jumpDestination, 50f);
        jumpToLocation = true;
        failCount = 0;
    }

    void Create_WayPoint_Queue(Vector3 startPosition, GameObject destination)
    {
        wayPointList.Clear();
        Vector3 currPosition = startPosition;

        //get closest way point
        float minDist = 999999;
        GameObject nextWayPoint = destination;
        int i = 0;
        int index = 0;
        foreach (GameObject waypoint in wayPointArray)
        {
            float dist = Vector3.Distance(waypoint.transform.position, currPosition);
            if(dist < minDist)
            {
                minDist = dist;
                nextWayPoint = waypoint;
                index = i;
            }
            i++;
        }
        wayPointList.Add(nextWayPoint);
        if (nextWayPoint == destination)
            return;


        //if we are in the inner circle of waypoints, move to the outside ring
        if(index >= wayPointArray.Length / 2)   //if the index is in the upper half, then it's in the inner ring
        {
            index -= (wayPointArray.Length / 2);
            nextWayPoint = wayPointArray[index];
            wayPointList.Add(nextWayPoint);
        }
        if (nextWayPoint == destination)
            return;


        //determine if it's faster to go clockwise or counter clockwise
        int clockwise_count = 0;
        int testIndex = index;
        i = 0;
        while(i < wayPointArray.Length / 2)
        {
            clockwise_count++;
            testIndex++;
            if (testIndex >= wayPointArray.Length / 2)
                testIndex = 0;
            if (wayPointArray[testIndex] == destination)
                break;
            i++;
        }
        int counterClockwise_count = 0;
        testIndex = index;
        i = 0;
        while (i < wayPointArray.Length / 2)
        {
            counterClockwise_count++;
            testIndex--;
            if (testIndex < 0)
                testIndex = (wayPointArray.Length / 2) - 1;
            if (wayPointArray[testIndex] == destination)
                break;
            i++;
        }

        i = 0;
        if (clockwise_count < counterClockwise_count)    //clockwise is shorter
        {
            while (i < wayPointArray.Length / 2)
            {
                index++;
                if (index >= wayPointArray.Length / 2)
                    index = 0;
                wayPointList.Add(wayPointArray[index]);
                if (wayPointArray[index] == destination)
                    return;
                i++;
            }
        }
        else        //counter clockwise is shorter (or equal)
        {
            while (i < wayPointArray.Length / 2)
            {
                index--;
                if (index < 0)
                    index = (wayPointArray.Length / 2) - 1;
                wayPointList.Add(wayPointArray[index]);
                if (wayPointArray[index] == destination)
                    return;
                i++;
            }
        }
    }

    void load_incrementalJumpList(Vector3 startPosition, Vector3 destinationPosition, float maxJumpDistance)
    {
        float dist = Vector3.Distance(startPosition, destinationPosition);
        int numberOfJumps = (int)(dist / maxJumpDistance);

        incrementalJumpList.Clear();

        float x_change = (destinationPosition.x - startPosition.x) / numberOfJumps;
        float z_change = (destinationPosition.z - startPosition.z) / numberOfJumps;
        Vector3 currentPosition = startPosition;
        for (int i = 0; i < numberOfJumps; i++)
        {
            Vector3 nextPosition = new Vector3(currentPosition.x + x_change, currentPosition.y, currentPosition.z + z_change);
            nextPosition = GetRayCastAtPosition(nextPosition);
            //nextPosition = GetRandomStartingLocation(nextPosition, -0.1f, 0.1f);
            incrementalJumpList.Add(nextPosition);
            currentPosition = nextPosition;
        }
    }

    Vector3 GetRandomStartingLocation(Vector3 pos, float min, float max)
    {
        RaycastHit hit;
        int attemptCount = 0;

        while (attemptCount <= 10)
        {
            float x = UnityEngine.Random.Range(min, max) + pos.x;
            float z = UnityEngine.Random.Range(min, max) + pos.z;

            Vector3 position = new Vector3(x, 0, z);
            //Do a raycast along Vector3.down -> if you hit something the result will be given to you in the "hit" variable
            //This raycast will only find results between 'min' and 'max' units of your original "position"
            if (Physics.Raycast(new Vector3(0, max, 0) + position, Vector3.down, out hit, max - min))
            {
                return hit.point;
            }
            else
            {
                Debug.Log("there seems to be no ground at this position");
            }

            attemptCount++;
        }

        return pos;
    }



    Vector3 GetRayCastAtPosition(Vector3 pos)
    {
        RaycastHit hit;
        int attemptCount = 0;

        while (attemptCount <= 10)
        {
            float x = pos.x;
            float z = pos.z;

            Vector3 position = new Vector3(x, 0, z);

            //pos.y = Terrain.activeTerrain.SampleHeight(transform.position);
            //return pos;
            //transform.position = pos;

            //Do a raycast along Vector3.down -> if you hit something the result will be given to you in the "hit" variable
            //This raycast will only find results between 'min' and 'max' units of your original "position"
            if (Physics.Raycast(new Vector3(0, 0, 0) + position, Vector3.down, out hit, 0))
            {
                return hit.point;
            }
            else
            {
                Debug.Log("there seems to be no ground at this position");
            }

            attemptCount++;
        }

        return pos;
    }
}
