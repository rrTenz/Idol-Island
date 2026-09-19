using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AI;
using Firebase.Database;

public class MovePlayer : MonoBehaviour
{
    private string UserId = "-1";
    private int UserIndex = -1;

    DatabaseReference mDatabaseRef;

    UnityEngine.AI.NavMeshAgent agent;
    public Animator ani;
    public GameObject aim_point;
    public GameObject aim_point_Cylinder;
    public Vector3 lastPosition;

    public bool execute_walking;
    public bool execute_sitting;
    public bool execute_stealing;
    public bool execute_picking_up;

    public bool execute_running;

    public float walk_speed;
    public float run_speed;

    Vector3 sitting_position;
    Vector3 sitting_Rotation;

    Vector3 stealing_position;
    Vector3 stealing_Rotation;


    public bool walk;
    public bool run;
    public bool sit;
    public bool steal;
    public bool pick_up;

    public bool ForceAnimation;
    public int AnimateInteger_legs;
    public int AnimateInteger_arms;

    public bool destermine_new_aim;



    void Start()
    {
        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        sitting_position = new Vector3(0, 0, 0);
        sitting_Rotation = new Vector3(180, -90, -90);

        stealing_position = new Vector3(0, 0.04185915f, -0.07200003f);
        stealing_Rotation = new Vector3(0, 180, 0);

    }

    bool in_sitting;
    bool in_stealing;
    bool in_pickup;

    Coroutine sitting_start;
    Coroutine stealing_start;
    Coroutine pickup_start;

    public GameObject crowbar;

    IEnumerator sitting_down()
    {
        yield return new WaitForSeconds(0);

        transform.parent = aim_point.transform;




        Destroy(agent);

        ani.SetInteger("legs", 3);
        ani.SetInteger("arms", 3);

        transform.localPosition = sitting_position;
        transform.localEulerAngles = sitting_Rotation;



        yield return new WaitForSeconds(5);

        agent = gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();


        in_sitting = false;
        destermine_new_aim = false;
        transform.parent = null;

        StopCoroutine(sitting_start);
    }


    IEnumerator stealing_execute()
    {
        yield return new WaitForSeconds(0);
        crowbar.SetActive(true);
        transform.parent = aim_point.transform;
        transform.localPosition = stealing_position;
        transform.localEulerAngles = stealing_Rotation;

        ani.SetInteger("legs", 5);
        ani.SetInteger("arms", 22);

        yield return new WaitForSeconds(5);
        crowbar.SetActive(false);
        in_stealing = false;
        destermine_new_aim = false;
        transform.parent = null;

        StopCoroutine(stealing_start);
    }


    IEnumerator pickup_execute()
    {
        yield return new WaitForSeconds(0);



        ani.SetInteger("legs", 32);
        ani.SetInteger("arms", 32);


        yield return new WaitForSeconds(2);

        in_pickup = false;
        destermine_new_aim = false;


        StopCoroutine(pickup_start);
    }

    public bool ready;
    private bool isFirstUpdate = true;
    private float Update_DB_CurrentPosition_time;

    float timer = 0.0f;
    bool haveUpdated_lastPosition = false;
    void Update()
    {
        timer += Time.deltaTime;
        int seconds = (int)(timer % (5 * 60));
        
        if(Global.holdPositions)
        {
            transform.position = aim_point_Cylinder.transform.position;
        }
        //if(Global.holdPosition_timer > 0)
        //{
        //    Global.holdPosition_timer -= Time.deltaTime;
        //    transform.position = aim_point_Cylinder.transform.position;
        //}

        //TODO
        Update_DB_CurrentPosition_time += Time.deltaTime;
        if (Update_DB_CurrentPosition_time > 3.0 && Vector3.Distance(transform.position, lastPosition) > 0.25f)
        {
            Update_DB_CurrentPosition();
        }

        if (!isFirstUpdate)
        {
            if (destermine_new_aim)
            {
                aim_point_Cylinder.transform.localScale = new Vector3(1, 0.01f, 1);
            }
            else
            {
                aim_point_Cylinder.transform.localScale = new Vector3(0, 0, 0);
            }
        }

        if (Global.creatingCharacter)
        {
            ForceAnimation = true;
            if(AnimateInteger_arms < 9)
            {
                AnimateInteger_arms = 9;
            }
            if (seconds >= 5)
            {
                timer = 0.0f;
                AnimateInteger_arms++;
                if(AnimateInteger_arms > 12)
                {
                    AnimateInteger_arms = 9;
                }
            }
        }

        if (ForceAnimation)
        {
            ani.SetInteger("legs", AnimateInteger_legs);
            ani.SetInteger("arms", AnimateInteger_arms);
        }
        if (!ready)
        {
            return;
        }

        if(Vector3.Distance(transform.position, aim_point.transform.position) > 0.25f)
        {
            aim_point = aim_point_Cylinder;

            destermine_new_aim = true;
            if (UserId == Global.mainPlayerId)
                Global.mainPlayerMoving = destermine_new_aim;
        }

        if (destermine_new_aim)
        {
            if (walk)
            {

                if (Vector3.Distance(transform.position, aim_point.transform.position) > 0.25f)
                {

                    agent.speed = walk_speed;
                    agent.SetDestination(aim_point.transform.position);
                    ani.SetInteger("arms", 1);
                    ani.SetInteger("legs", 1);
                }

                if (Vector3.Distance(transform.position, aim_point.transform.position) < 0.25f)
                {
                    agent.speed = 0;

                    ani.SetInteger("arms", 5);
                    ani.SetInteger("legs", 5);

                    destermine_new_aim = false;
                    if (UserId == Global.mainPlayerId)
                        Global.mainPlayerMoving = destermine_new_aim;
                    Update_DB_CurrentPosition();
                }

            }
            if (run)
            {

                if (Vector3.Distance(transform.position, aim_point.transform.position) > 0.25f)
                {
                    Debug.Log("going to run");
                    agent.speed = run_speed;
                    agent.SetDestination(aim_point.transform.position);
                    ani.SetInteger("arms", 2);
                    ani.SetInteger("legs", 2);
                }

                if (Vector3.Distance(transform.position, aim_point.transform.position) < 0.25f)
                {
                    agent.speed = 0;

                    ani.SetInteger("arms", 5);
                    ani.SetInteger("legs", 5);

                    destermine_new_aim = false;
                    if (UserId == Global.mainPlayerId)
                        Global.mainPlayerMoving = destermine_new_aim;
                    Update_DB_CurrentPosition();
                }

            }
            if (sit && !in_sitting)
            {

                if (Vector3.Distance(transform.position, aim_point.transform.position) > 0.25f)
                {

                    agent.speed = walk_speed;
                    agent.SetDestination(aim_point.transform.position);
                    ani.SetInteger("arms", 1);
                    ani.SetInteger("legs", 1);
                }

                if (Vector3.Distance(transform.position, aim_point.transform.position) < 0.25f)
                {
                    agent.speed = 0;


                    if (!in_sitting)
                    {
                        in_sitting = true;

                        sitting_start = StartCoroutine(sitting_down());
                    }

                }

            }
            if (steal && !in_stealing)
            {

                if (Vector3.Distance(transform.position, aim_point.transform.position) > 0.25f)
                {

                    agent.speed = walk_speed;
                    agent.SetDestination(aim_point.transform.position);
                    ani.SetInteger("arms", 1);
                    ani.SetInteger("legs", 1);
                }

                if (Vector3.Distance(transform.position, aim_point.transform.position) < 0.25f)
                {
                    agent.speed = 0;



                    if (!in_stealing)
                    {
                        in_stealing = true;

                        stealing_start = StartCoroutine(stealing_execute());
                    }

                }


            }
            if (pick_up && !in_pickup)
            {
                if (Vector3.Distance(transform.position, aim_point.transform.position) > 0.25f)
                {

                    agent.speed = walk_speed;
                    agent.SetDestination(aim_point.transform.position);
                    ani.SetInteger("arms", 1);
                    ani.SetInteger("legs", 1);
                }

                if (Vector3.Distance(transform.position, aim_point.transform.position) < 0.25f)
                {
                    agent.speed = 0;



                    if (!in_pickup)
                    {
                        in_pickup = true;

                        pickup_start = StartCoroutine(pickup_execute());
                    }

                }
            }
        }
        isFirstUpdate = false;
        lastPosition = transform.position;
    }//Update

    public void set_UserId(string id)
    {
        UserId = id;

        for(int i = 0; i < Global.currentGame.userIdArray.Length; i++)
        {
            if(Global.currentGame.userIdArray[i] == UserId)
            {
                UserIndex = i;
                break;
            }
        }
    }


    private void Update_DB_CurrentPosition()
    {
        if (UserIndex >= 0 && UserId == Global.mainPlayerId)
        {
            float position_Diff = Vector3.Distance(transform.position, Global.currentGame.destinationArray[UserIndex]);
            if (position_Diff < 500)
            {
                mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("arrivedAtPosition").Child("" + UserIndex).Child("x").SetValueAsync(transform.position.x);
                mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("arrivedAtPosition").Child("" + UserIndex).Child("y").SetValueAsync(transform.position.y);
                mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("arrivedAtPosition").Child("" + UserIndex).Child("z").SetValueAsync(transform.position.z);
                Update_DB_CurrentPosition_time = 0;
            }
            else {
                Debug.Log("Big Dist Diff: " + position_Diff);
                //transform.position = Global.currentGame.destinationArray[UserIndex];    //TODO figure this out
            }

        }
    }

}
