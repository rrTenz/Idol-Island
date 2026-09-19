using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using UnityEditor;
using UnityEngine.SceneManagement;

public class PlayerLocations : MonoBehaviour
{
    private DatabaseReference mDatabaseRef;

    public GameObject Player_and_Cylinder_prefab;

    public GameObject MainPlayer;
    public GameObject MainPlayer_cylinder;
    public GameObject MainPlayer_simple_custom_human;
    public GameObject MainPlayer_chat_radius;

    public GameObject PlayerHolder_GameObject;

    public GameObject Loading_Canvas;
    public TMP_Text Loading_Text;

    public Button Button_PlayerWalk;
    public Button Button_CameraRotate;
    public Button Button_ZoomIn;
    public Button Button_ZoomOut;

    public GameObject StartPosition_0;
    public GameObject StartPosition_1;
    public GameObject StartPosition_2;
    public GameObject StartPosition_3;
    public GameObject StartPosition_4;
    public GameObject StartPosition_5;
    GameObject[] StartPositionArray;
    int StartPositionArray_index = 0;

    public float cylinderOffset;

    private bool mainPlayerEliminated;

    public float MainPlayer_chat_radius_offset = 1.26f;

    public TMP_Text TMP_Text_Debug;

    private void Awake()
    {
        Loading_Canvas.SetActive(true);
        //Global.userId_passedTo_clothing_script = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        System.Random random = new System.Random();

        StartPositionArray = new GameObject[] { StartPosition_0, StartPosition_1, StartPosition_2, StartPosition_3, StartPosition_4, StartPosition_5 };

        UpdateGameProgress.SendToDatabase_LogginStatus(Global.currentGame.GameId, Global.getMainPlayerIndex_currentGame(), true); //logging into game

        Global.LockCameraRotation = false;
        mainPlayerEliminated = false;
        Global.holdPositions = true;

        Global.playerPositionArray_currentGame = new Vector3[Global.currentGame.playerCount];
        Global.PlayerObjectList.Clear();
        //Global.userId_passedTo_clothing_script = 0;
        bool updateStartingDestinations = false;
        for (int i = 0; i < Global.currentGame.playerCount; i++)
        {
            if(Global.currentGame.userIdArray[i] == Global.mainPlayerId)    //main player
            {
                if(Global.currentGame.dayEliminated[i] >= 0)    //has the main player been eliminated
                {
                    mainPlayerEliminated = true;
                    break;
                }
                else
                {
                    //Global.userId_passedTo_clothing_script = Global.currentGame.userIdArray[i];

                    MainPlayer.transform.position = Global.currentGame.destinationArray[i];
                    if(MainPlayer.transform.position.y >= Global.DEFAULT_BAD_Y)
                    {
                        updateStartingDestinations = true;
                        MainPlayer.transform.position = GetRandomStartingLocation();
                        //MainPlayer.transform.position = new Vector3(Global.currentGame.destinationArray[i].x + StartPosition.transform.position.x,
                        //                                            StartPosition.transform.position.y,
                        //                                            Global.currentGame.destinationArray[i].z + StartPosition.transform.position.z);
                        Global.currentGame.destinationArray[i] = MainPlayer.transform.position;
                    }
                    Global.startingDestination = MainPlayer.transform.position;
                    //MainPlayer.transform.eulerAngles = new Vector3(0, random.Next(360), 0);
                    MainPlayer_cylinder.transform.position = MainPlayer.transform.position;
                    MainPlayer_chat_radius.transform.position = new Vector3(MainPlayer.transform.position.x, MainPlayer.transform.position.y + MainPlayer_chat_radius_offset, MainPlayer.transform.position.z); //4.0 4.5

                    Global.mainPlayerIndex = Global.PlayerObjectList.Count;
                    Global.PlayerObjectList.Add(new Global.PlayerObject(MainPlayer, MainPlayer_cylinder, MainPlayer_simple_custom_human, MainPlayer_chat_radius));

                    MainPlayer.GetComponent<clothing>().set_UserId(Global.mainPlayerId);
                    MainPlayer.GetComponent<MovePlayer>().set_UserId(Global.mainPlayerId);
                }
            }
            else  //all other players
            {
                if (Global.currentGame.dayEliminated[i] >= 0)    //has the main player been eliminated
                {
                    //do nothing for eliminated players
                }
                else
                {
                    GameObject PlayerAndCylinderObject = Instantiate(Player_and_Cylinder_prefab);
                    PlayerAndCylinderObject.transform.SetParent(PlayerHolder_GameObject.transform);

                    GameObject CustomSimpleHumanPrefabTest = PlayerAndCylinderObject.transform.GetChild(0).gameObject;
                    CustomSimpleHumanPrefabTest.GetComponent<clothing>().set_UserId(Global.currentGame.userIdArray[i]);
                    CustomSimpleHumanPrefabTest.GetComponent<MovePlayer>().set_UserId(Global.currentGame.userIdArray[i]);

                    //Give tag to player
                    //while (Global.userId_passedTo_clothing_script > 0) ;  //wait here for userId_passedTo_clothing_script to be set to -1 (see Start routine in clothing.cs)
                    //Global.userId_passedTo_clothing_script = Global.currentGame.userIdArray[i];

                    GameObject PlayerBody = PlayerAndCylinderObject.transform.GetChild(0).gameObject;
                    GameObject DesitnationCylinder = PlayerAndCylinderObject.transform.GetChild(1).gameObject;

                    GameObject SimpleCustomHuman_prefab = PlayerBody.transform.GetChild(0).gameObject;
                    GameObject SimpleCustomHuman = SimpleCustomHuman_prefab.transform.GetChild(1).gameObject;

                    PlayerBody.transform.position = Global.currentGame.destinationArray[i];
                    if (PlayerBody.transform.position.y >= Global.DEFAULT_BAD_Y)
                    {
                        updateStartingDestinations = true;
                        PlayerBody.transform.position = GetRandomStartingLocation();
                        //PlayerBody.transform.position = new Vector3(Global.currentGame.destinationArray[i].x + StartPosition.transform.position.x,
                        //                                            StartPosition.transform.position.y,
                        //                                            Global.currentGame.destinationArray[i].z + StartPosition.transform.position.z);
                        Global.currentGame.destinationArray[i] = PlayerBody.transform.position;
                    }
                    PlayerBody.transform.eulerAngles = new Vector3(0, random.Next(360), 0);
                    DesitnationCylinder.transform.position = PlayerBody.transform.position;

                    if (PlayerBody != null && DesitnationCylinder != null)
                    {
                        Global.PlayerObjectList.Add(new Global.PlayerObject(PlayerBody, DesitnationCylinder, SimpleCustomHuman, null));
                    }
                }
            }
        }

        if(updateStartingDestinations)
        {
            UpdateStartingLocations();
        }

        if (mainPlayerEliminated)
        {
            Loading_Text.text = "You have been eliminated from the Game";
        }
        else
        {
            HandlePositionChanges_fromDataBase();
        }
        firstUpdateAfterStart = true;
        Global.updatePlayersStartingPosition = true;
        checkLoadingScreen = true;
    }

    Vector3 GetRandomStartingLocation()
    {
        RaycastHit hit;
        int attemptCount = 0;

        while (attemptCount <= 10)
        {
            StartPositionArray_index = 0;   //TODO remove this?
            float x = UnityEngine.Random.Range(-10.0f, 10.0f) + StartPositionArray[StartPositionArray_index].transform.position.x;
            float z = UnityEngine.Random.Range(-10.0f, 10.0f) + StartPositionArray[StartPositionArray_index++].transform.position.z;

            Vector3 position = new Vector3(x, 0, z);
            //Do a raycast along Vector3.down -> if you hit something the result will be given to you in the "hit" variable
            //This raycast will only find results between +-10 units of your original"position" (ofc you can adjust the numbers as you like)
            if (Physics.Raycast(new Vector3(0, 10.0f, 0) + position, Vector3.down, out hit, 20.0f))
            {
                return hit.point;
            }
            else
            {
                Debug.Log("there seems to be no ground at this position");
            }

            attemptCount++;
        }

        return StartPositionArray[StartPositionArray_index++].transform.position;
    }

    void UpdateStartingLocations()
    {
        for (int i = 0; i < Global.currentGame.destinationArray.Length; i++)
        {
            Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("destinationArray").Child("" + i).Child("x").SetValueAsync(Global.currentGame.destinationArray[i].x);
            Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("destinationArray").Child("" + i).Child("y").SetValueAsync(Global.currentGame.destinationArray[i].y);
            Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("destinationArray").Child("" + i).Child("z").SetValueAsync(Global.currentGame.destinationArray[i].z);
        }
    }


    // Update is called once per frame
    bool firstUpdateAfterStart = false;
    bool checkLoadingScreen = false;
    Vector3 lastPosition_mainPlayer;
    float samePositionTime = 0;
    void Update()
    {
        int i = 0;
        foreach(Global.PlayerObject playerObject in Global.PlayerObjectList)
        {
            if(playerObject.DesitnationCylinder.transform.position.y != playerObject.PlayerBody.transform.position.y - cylinderOffset)
            {
                //playerObject.DesitnationCylinder.transform.position = new Vector3(playerObject.DesitnationCylinder.transform.position.x, playerObject.PlayerBody.transform.position.y - cylinderOffset, playerObject.DesitnationCylinder.transform.position.z);
            }

            if (firstUpdateAfterStart)
            {
                if (Global.characterScaleArray != null && i < Global.characterScaleArray.Length)
                {
                    //playerObject.PlayerBody.transform.localScale = Global.characterScaleArray[i++];
                    playerObject.SimpleCustomHuman.transform.localScale = new Vector3(100 * Global.characterScaleArray[i].x, 100 * Global.characterScaleArray[i].z, 100 * Global.characterScaleArray[i].y);
                    i++;
                }
            }
        }
        firstUpdateAfterStart = true;


        Button_PlayerWalk.interactable = !Global.LockCameraRotation;
        Button_CameraRotate.interactable = Global.LockCameraRotation;
        
        Global.playerPositionArray_currentGame[Global.mainPlayerIndex] = MainPlayer.transform.position;

        //if(!Global.LockCameraRotation)
        //{
        //    MainPlayer_cylinder.transform.position = MainPlayer_simple_custom_human.transform.position;
        //}
        //getCharacterOptions();

        if(checkLoadingScreen && !Global.updatePlayersStartingPosition)
        {
            Loading_Canvas.SetActive(false);

            if (Global.ShowLoadingScreenWhileWarping)
            {
                Loading_Canvas.SetActive(true);
            }
        }

        //move the "move cylider" back to the player if the player hasn't moved for 1 seconds
        float dist = Vector3.Distance(lastPosition_mainPlayer, MainPlayer.transform.position);        
        if(MainPlayer.GetComponent<MovePlayer>().destermine_new_aim && dist < 0.025f)  //the player is trying to move but the position is not changing
        {
            samePositionTime += Time.deltaTime;
        }
        else if(samePositionTime > 0)
        {
            samePositionTime -= Time.deltaTime;
        }
        if(samePositionTime > 2.0f)
        {
            MainPlayer_cylinder.transform.position = MainPlayer.transform.position;
        }
        lastPosition_mainPlayer = MainPlayer.transform.position;

        if(Global.doWarp)
        {
            samePositionTime = 0;
        }
        //TMP_Text_Debug.text = "Dist: " + dist + "\nsamePositionTime: " + samePositionTime;


        //MainPlayer_chat_radius.transform.position = new Vector3(MainPlayer.transform.position.x, MainPlayer.transform.position.y + MainPlayer_chat_radius_offset, MainPlayer.transform.position.z); //4.0 4.5
    }

    //public static void AddTag(string tagname)
    //{
    //    UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
    //    if ((asset != null) && (asset.Length > 0))
    //    {
    //        SerializedObject so = new SerializedObject(asset[0]);
    //        SerializedProperty tags = so.FindProperty("tags");

    //        for (int i = 0; i < tags.arraySize; ++i)
    //        {
    //            if (tags.GetArrayElementAtIndex(i).stringValue == tagname)
    //            {
    //                return;     // Tag already present, nothing to do.
    //            }
    //        }

    //        tags.InsertArrayElementAtIndex(0);
    //        tags.GetArrayElementAtIndex(0).stringValue = tagname;
    //        so.ApplyModifiedProperties();
    //        so.Update();
    //    }
    //}

    bool haveCharacterOptions = false;
    int characterOptionsIndex = 0;
    bool waitingForCharacterOptions = false;
    void getCharacterOptions()
    {
        if (haveCharacterOptions || waitingForCharacterOptions)
            return;

        if(characterOptionsIndex >= Global.currentGame.userCharacterIndexArray.Length)
        {
            haveCharacterOptions = true;
            return;
        }

        waitingForCharacterOptions = true;
        FirebaseDatabase.DefaultInstance
        .GetReference("users/" + Global.currentGame.userIdArray[characterOptionsIndex] + "/jsonCharacterString")
        .GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {

            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Key.Equals("jsonCharacterString"))
                {
                    Global.CharacterOptions[] characters = Newtonsoft.Json.JsonConvert.DeserializeObject<Global.CharacterOptions[]>((String)snapshot.Value);

                    if(Global.currentGame.userCharacterIndexArray[characterOptionsIndex] < characters.Length)
                    {
                        //TODO assign options to character
                    }
                }
            }
        });
    }

    void HandlePositionChanges_fromDataBase()
    {
        String refStr = "Game/" + Global.currentGame.GameId + "/destinationArray";
        FirebaseDatabase.DefaultInstance
          .GetReference(refStr)
          .ValueChanged += HandleValueChanged_DestinationArray;

        refStr = "Game/" + Global.currentGame.GameId + "/arrivedAtPosition";
        FirebaseDatabase.DefaultInstance
          .GetReference(refStr)
          .ValueChanged += HandleValueChanged_ArrivedAtPosition;
    }


    void HandleValueChanged_DestinationArray(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = args.Snapshot;

        int i = 0;
        foreach (var destination in snapshot.Children)
        {
            if (i != Global.mainPlayerIndex)   //don't try to update position from DB for mainPlayer
            {
                float x = 0, y = 0, z = 0;
                foreach (var val in destination.Children)
                {
                    if (val.Key.Equals("x"))
                        x = (float)Convert.ToDouble(val.Value);
                    else if (val.Key.Equals("y"))
                        y = (float)Convert.ToDouble(val.Value);
                    else if (val.Key.Equals("z"))
                        z = (float)Convert.ToDouble(val.Value);
                }
                UpdatePlayerDestination(Convert.ToInt32(destination.Key), new Vector3(x, y, z));
            }
            i++;
        }
    }

    void UpdatePlayerDestination(int index, Vector3 destination)
    {
        if(index < Global.PlayerObjectList.Count)
        {
            if(Global.PlayerObjectList[index] != null)
                Global.PlayerObjectList[index].DesitnationCylinder.transform.position = destination;
        }
    }


    void HandleValueChanged_ArrivedAtPosition(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = args.Snapshot;

        int i = 0;
        foreach (var position in snapshot.Children)
        {
            if (i != Global.mainPlayerIndex)   //don't try to update position from DB for mainPlayer
            {
                float x = 0, y = 0, z = 0;
                foreach (var val in position.Children)
                {
                    if (val.Key.Equals("x"))
                        x = (float)Convert.ToDouble(val.Value);
                    else if (val.Key.Equals("y"))
                        y = (float)Convert.ToDouble(val.Value);
                    else if (val.Key.Equals("z"))
                        z = (float)Convert.ToDouble(val.Value);
                }
                UpdatePlayerPosition(Convert.ToInt32(position.Key), new Vector3(x, y, z));
                Global.playerPositionArray_currentGame[Convert.ToInt32(position.Key)] = new Vector3(x, y, z);
            }
            i++;
        }
    }

    void UpdatePlayerPosition(int index, Vector3 pos)
    {
        if (index < Global.PlayerObjectList.Count)
        {
            if (Global.PlayerObjectList[index] != null)
                Global.PlayerObjectList[index].PlayerBody.transform.position = pos;
        }
    }

    public void Button_HomeMenu_click()
    {
        foreach (Global.PlayerObject playerObject in Global.PlayerObjectList)
        {
            Destroy(playerObject.PlayerBody);
            Destroy(playerObject.DesitnationCylinder);
            Destroy(playerObject.SimpleCustomHuman);
        }
        Global.PlayerObjectList.Clear();

        UpdateGameProgress.SendToDatabase_LogginStatus(Global.currentGame.GameId, Global.getMainPlayerIndex_currentGame(), false); //logging out of game
        Global.currentGame.GameId = 0; //no longer in a valid game

        SceneManager.LoadScene((int)Global.Screen.Screen_Menu);
    }

    public void ButtonClick_Toggle_LockCameraRotation()
    {
        Global.LockCameraRotation = !Global.LockCameraRotation;
        Global.ignoreRaycast = true;
    }

    bool showMenu = true;
    public GameObject Canvas_MiniMap;
    public GameObject Button_Home;
    public GameObject Button_TurnChecklist;
    public GameObject GO_ChatWindow;
    public GameObject GO_ZoomControls;
    public GameObject GO_WalkControls;
    public GameObject TPM_TimeUntilNextPhase;
    public GameObject Button_GoToCamp;
    public void ButtonClick_Toggle_ShowMenu()
    {
        showMenu = !showMenu;

        if (showMenu)
        {
            Canvas_MiniMap.SetActive(true);
            Button_Home.SetActive(true);
            Button_TurnChecklist.SetActive(true);
            GO_ChatWindow.SetActive(true);
            GO_ZoomControls.SetActive(true);
            TPM_TimeUntilNextPhase.SetActive(true);
            Button_GoToCamp.SetActive(true);
        }
        else
        {
            Canvas_MiniMap.SetActive(false);
            Button_Home.SetActive(false);
            Button_TurnChecklist.SetActive(false);
            GO_ChatWindow.SetActive(false);
            GO_ZoomControls.SetActive(false);
            TPM_TimeUntilNextPhase.SetActive(false);
            Button_GoToCamp.SetActive(false);
        }
    }
}
