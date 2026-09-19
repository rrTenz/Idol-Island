using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Auth;
using System;
using TMPro;
using UnityEngine.SceneManagement;

//scrollable - https://www.youtube.com/watch?v=1-_-716Ouy8


public class Canvas_Menu : MonoBehaviour
{
    private DatabaseReference mDatabaseRef;

    public GameObject Panel_CurrentGames;
    public GameObject Panel_OpenGames;
    public Button Button_Prefab;
    public Button Button_First_Button;

    public GameObject Canvas_Main;

    public GameObject Canvas_Popup;
    public TMP_Text TMP_Text_PopupMessage;

    public GameObject Canvas_PopupConfirm;
    public TMP_Text TMP_Text_PopupMessageConfirm;
    public TMP_Text TMP_Text_PopupMessageConfirm_Confirm;
    public TMP_Text TMP_Text_PopupMessageConfirm_Cancel;

    public GameObject Canvas_Loading;
    public GameObject Canvas_Loading_Game;

    public GameObject Canvas_GameNotStarted;
    public Button Button_LeftButton;
    public TMP_Text TMP_Text_GameName;
    public TMP_Text TMP_Text_ParchmentSummaryText;
    public TMP_Text TMP_Text_ParchmentSummaryText2;
    public Slider Slider_WhichCharacter;
    public TMP_Text TMP_Text_CharacterName;
    public TMP_Text TMP_Text_CharacterXofY;
    public TMP_Text TMP_Text_LeftButtonText;
    public TMP_Text TMP_Text_RightButtonText;

    bool lookingAtOpenGame = false;
    bool joinSelectedGame = false;
    bool addedSuccessfully = false;
    bool removedSuccessfully = false;
    bool refreshAfterPopup = false;
    bool goToMainScreen_delayed = false;
    bool dontShowProgress = false;

    public enum MenuCanvas : int
    {
        Main = 0,
        Popup,
        PopupConfirm,
        Loading,
        GameNotStarted,
        OpenGame
    }

    public TMP_Text TMP_Text_Progress;

    public TMP_InputField InputField_MainUserId;

    private Global.Game[] Buttons_MyGames;
    private Global.Game[] Buttons_OpenGames;

    private float screenHeight;
    private float screenWidth;

    List<Button> buttonList = new List<Button>();

    List<Global.Game> localGameList = new List<Global.Game>();
    List<Global.Game> localGameList_openGames = new List<Global.Game>();


    DataBaseState dataBaseState;
    DataBaseState dataBase_GameIndex;
    List<long> userGameList = new List<long>();
    List<Global.CharacterOptions> userCharacterList = new List<Global.CharacterOptions>();
    long GameIndex = 0;

    enum DataBaseState : int
    {
        Init = 0,
        GetMainUser,
        GetMainUser_Wait,
        GetNextGameIndex,
        GetMainGames,
        GetMainGames_Wait,
        GetOpenGames,
        GetOpenGames_Wait,
        PopulateButtons,
        Idle,

        NewUserLogin
    }

    enum AddPlayerToGameState : int
    {
        Init = 0,
        GetDesiredGame,
        AddUserToGame,
        AddGameToUser,
        Done,
        Idle
    }

    AddPlayerToGameState addPlayerToGameState = AddPlayerToGameState.Idle;
    long gameId_addPlayer;

    private int progressStep = -1;
    private const int stepCount = (int)DataBaseState.Idle - 1;


    // Start is called before the first frame update
    void Start()
    {
        Global.mainPlayerId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        InputField_MainUserId.text = "" + Global.mainPlayerId;

        Debug.Log("FireBaseDataBase Start");

        // Get the root reference location of the database.
        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("FireBaseDataBase Got ref");

        Global.currentGame = null;
        Global.localUser = null;


        //Button_First_Button.onClick.AddListener(CreateGame);

        Global.creatingCharacter = false;

        Global.isLoggedIntoCurrentGame_mainUser = false;

        OpenCanvas(MenuCanvas.Loading);

        if(Global.NewUserCreatedOnLoginScreen && Global.newUser != null)
        {
            writeNewUser(Global.newUserId, Global.newUser.username, Global.newUser.email);
            dataBaseState = DataBaseState.NewUserLogin;
        }
        else
        {
            dataBaseState = DataBaseState.Init;
        }
    }

    private void writeNewUser(string userId, string name, string email)
    {
        Global.User user = new Global.User(name, email, new List<long>(), new List<Global.CharacterOptions>());
        string json = JsonUtility.ToJson(user);


        DatabaseReference mDatabaseRef;
        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        mDatabaseRef.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {

            }
            else if (task.IsCompleted)
            {

            }
            dataBaseState = DataBaseState.Init;
        });
    }

    // Update is called once per frame
    void Update()
    {
        //if (screenHeight != Display.main.systemHeight || screenWidth != Display.main.systemWidth)
        //{
        //    screenHeight = Display.main.systemHeight;
        //    screenWidth = Display.main.systemWidth;

        //    RectTransform rt = (RectTransform)PanelLeft.transform;
        //    float halfWidth = rt.rect.width / 2;
        //    float halfHeight = rt.rect.height / 2;

        //    Set_Size(PanelLeft, screenWidth * 0.5f, screenHeight * 0.85f);
        //    Set_Size(ScrollMyGames, screenWidth * 0.5f, screenHeight * 0.85f);
        //    Set_Size(PanelLeft_Games, screenWidth * 0.5f, screenHeight * 0.85f);
        //    ScrollMyGames.transform.position = new Vector3(PanelLeft.transform.position.x + halfWidth, PanelLeft.transform.position.y + halfHeight);
        //    PanelLeft_Games.transform.position = ScrollMyGames.transform.position;

        //    Set_Size(PanelRight, screenWidth * 0.5f, screenHeight * 0.85f);
        //    Set_Size(ScrollOpenGames, screenWidth * 0.5f, screenHeight * 0.85f);
        //    Set_Size(PanelRight_Games, screenWidth * 0.5f, screenHeight * 0.85f);
        //    ScrollOpenGames.transform.position = new Vector3(PanelRight.transform.position.x - halfWidth, PanelRight.transform.position.y + halfHeight);
        //    PanelRight_Games.transform.position = ScrollOpenGames.transform.position;
        //}

        Update_DataBase_State();

        if (dontShowProgress)
        {
            TMP_Text_Progress.text = "";
        }
        else
        {
            TMP_Text_Progress.text = "" + progressStep + " of " + (stepCount - 1);
        }


        if(goToMainScreen_delayed)
        {
            OpenCanvas(MenuCanvas.Main);
        }

        if (Canvas_Loading.activeSelf)
        {
            if (addedSuccessfully)
            {
                addedSuccessfully = false;
                dontShowProgress = false;
                OpenCanvas(MenuCanvas.Popup);
                TMP_Text_PopupMessage.text = "You were added to the game successfully!";
                refreshAfterPopup = true;
            }
            if (removedSuccessfully)
            {
                removedSuccessfully = false;
                dontShowProgress = false;
                OpenCanvas(MenuCanvas.Popup);
                TMP_Text_PopupMessage.text = "You have successfully been removed from the game";
                refreshAfterPopup = true;
            }
        }

        if(switchToGameScreen)
            SceneManager.LoadScene((int)Global.Screen.Screen_Game);
    }

    private void Awake()
    {
        OpenCanvas(MenuCanvas.Loading);
    }

    private void OpenCanvas(MenuCanvas menuCanvas)
    {
        Canvas_Main.SetActive(false);
        Canvas_Popup.SetActive(false);
        Canvas_PopupConfirm.SetActive(false);
        Canvas_Loading.SetActive(false);
        Canvas_GameNotStarted.SetActive(false);

        switch(menuCanvas)
        {
            case MenuCanvas.Main:
                Canvas_Main.SetActive(true);
                break;
            case MenuCanvas.Popup:
                Canvas_Popup.SetActive(true);
                break;
            case MenuCanvas.PopupConfirm:
                Canvas_PopupConfirm.SetActive(true);
                break;
            case MenuCanvas.Loading:
                Canvas_Loading.SetActive(true);
                break;
            case MenuCanvas.GameNotStarted:
                Canvas_GameNotStarted.SetActive(true);
                break;
            case MenuCanvas.OpenGame:
                Canvas_GameNotStarted.SetActive(true);
                break;
        }
    }

    public static void Set_Size(GameObject gameObject, float width, float height)
    {
        if (gameObject != null)
        {
            var rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(width, height);
            }
        }
    }

    Global.Game gameFromDb;
    List<string> userIdList = new List<string>();
    List<int> userCharacterIndexList = new List<int>();
    List<int> dayEliminatedList = new List<int>();
    List<Vector3> destinationArrayList = new List<Vector3>();
    List<int> tribeColorIndexArray = new List<int>();
    List<string> tribeNamesArray = new List<string>();
    List<int> tribeSwapsAtArray = new List<int>();
    List<int> tribesCountArray = new List<int>();
    List<byte> eventByDayArray = new List<byte>();

    private void Update_DataBase_State()
    {
        String username = "";
        String email = "";

        if((int)dataBaseState > progressStep)
            progressStep = (int)dataBaseState;  //don't bounce up and down for each new game
        if (progressStep >= stepCount)
            progressStep = stepCount - 1;

        switch (dataBaseState)
        {
            case DataBaseState.Init:
                dataBaseState = DataBaseState.GetMainUser;
                userGameList.Clear();
                userCharacterList.Clear();
                userIdList.Clear();
                userCharacterIndexList.Clear();
                localGameList.Clear();
                localGameList_openGames.Clear();
                tribeColorIndexArray.Clear();
                tribeNamesArray.Clear();
                tribeSwapsAtArray.Clear();
                tribesCountArray.Clear();
                eventByDayArray.Clear();
                OpenCanvas(MenuCanvas.Loading);
                if(Global.currentGame != null)
                    Global.currentGame.GameId = 0;  //not logged into a game
                break;
            case DataBaseState.GetMainUser:
                dataBaseState = DataBaseState.GetMainUser_Wait;

                FirebaseDatabase.DefaultInstance
                  .GetReference("users/" + Global.mainPlayerId)
                  .GetValueAsync().ContinueWith(task => {
                      if (task.IsFaulted)
                      {
                          // Handle the error...
                          Debug.Log("FireBaseDataBase Faulted");
                      }
                      else if (task.IsCompleted)
                      {
                          DataSnapshot snapshot = task.Result;
                          // Do something with snapshot...
                          Debug.Log("FireBaseDataBase Task Complete");

                          foreach (var childSnapshot in snapshot.Children)
                          {
                              Debug.Log(childSnapshot);
                              if (childSnapshot.Key.Equals("Games"))
                              {
                                  foreach (var game in childSnapshot.Children)
                                  {
                                      userGameList.Add(Convert.ToInt64(game.Value));
                                  }
                              }
                              else if (childSnapshot.Key.Equals("email"))
                              {
                                  email = (String)childSnapshot.Value;
                              }
                              else if (childSnapshot.Key.Equals("username"))
                              {
                                  username = (String)childSnapshot.Value;
                              }
                              else if (childSnapshot.Key.Equals("jsonCharacterString"))
                              {
                                  Global.CharacterOptions[] characters = Newtonsoft.Json.JsonConvert.DeserializeObject<Global.CharacterOptions[]> ((String)childSnapshot.Value);
                                  userCharacterList = new List<Global.CharacterOptions>(characters);
                              }

                          }
                          Global.localUser = new Global.User(username, email, userGameList, userCharacterList);
                          localGameList.Clear();
                          localGameList_openGames.Clear();
                          dataBaseState = DataBaseState.GetNextGameIndex;
                      }
                  });
                break;
            case DataBaseState.GetMainUser_Wait:
                //do nothing here
                break;
            case DataBaseState.GetNextGameIndex:
                if (userGameList.Count > 0)
                {
                    GameIndex = userGameList[0];
                    userGameList.RemoveAt(0);
                    dataBaseState = DataBaseState.GetMainGames;
                }
                else
                {
                    dataBaseState = DataBaseState.GetOpenGames;
                }
                break;
            case DataBaseState.GetMainGames:
                dataBaseState = DataBaseState.GetMainGames_Wait;

                String refStr = "Game/" + GameIndex;
                FirebaseDatabase.DefaultInstance
                  .GetReference(refStr)
                  .GetValueAsync().ContinueWith(task => {
                      if (task.IsFaulted)
                      {
                          // Handle the error...
                          Debug.Log("FireBaseDataBase Faulted");
                      }
                      else if (task.IsCompleted)
                      {
                          DataSnapshot snapshot = task.Result;
                          // Do something with snapshot...
                          Debug.Log("FireBaseDataBase Task Complete");

                          get_gameDataFromDb(snapshot);

                          localGameList.Add(gameFromDb);

                          dataBaseState = DataBaseState.GetNextGameIndex;
                      }
                  });
                break;
            case DataBaseState.GetMainGames_Wait:
                //do nothing here
                break;
            case DataBaseState.GetOpenGames:
                dataBaseState = DataBaseState.GetOpenGames_Wait;

                FirebaseDatabase.DefaultInstance
                  .GetReference("Game").LimitToLast(10)
                  .GetValueAsync().ContinueWith(task => {
                      if (task.IsFaulted)
                      {
                          // Handle the error...
                          Debug.Log("FireBaseDataBase Faulted");
                      }
                      else if (task.IsCompleted)
                      {
                          DataSnapshot snapshot = task.Result;
                          // Do something with snapshot...
                          Debug.Log("FireBaseDataBase Task Complete");

                          foreach (var childSnapshot in snapshot.Children)
                          {
                              get_gameDataFromDb(childSnapshot);

                              bool mainUserInThisGame = false;
                              foreach (string id in gameFromDb.userIdArray)
                              {
                                  if (id == Global.mainPlayerId)
                                  {
                                      mainUserInThisGame = true;
                                      break;
                                  }
                              }
                              if (!mainUserInThisGame && !gameFromDb.hasStarted)
                              {

                                  localGameList_openGames.Add(gameFromDb);
                              }
                          }

                          dataBaseState = DataBaseState.PopulateButtons;
                      }
                  });
                break;
            case DataBaseState.GetOpenGames_Wait:
                //do nothing here
                break;
            case DataBaseState.PopulateButtons:
                Buttons_MyGames = new Global.Game[localGameList.Count];
                int i = 0;
                bool firstButton = true;
                foreach (Global.Game game in localGameList)
                {
                    Debug.Log("Add Button - My Games");
                    Button gameObject;
                    if (firstButton)
                    {
                        gameObject = Button_First_Button;
                        firstButton = false;
                    }
                    else
                    {
                        gameObject = Instantiate(Button_Prefab);
                        gameObject.transform.SetParent(Panel_CurrentGames.transform);
                        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Button_First_Button.GetComponent<RectTransform>().rect.width, Button_First_Button.GetComponent<RectTransform>().rect.height);
                        gameObject.transform.localScale = new Vector3(1, 1, 1);
                    }

                    TMP_Text[] newText = gameObject.GetComponentsInChildren<TMP_Text>();

                    newText[0].text = game.gameName;

                    newText[1].text = "Players: " + game.playerCount + " of " + game.MaxPlayers;

                    if (game.TimePerTurn_minutes > 1440) //minutes in a day
                        newText[2].text = "Time Per Turn: " + ((double)((double)game.TimePerTurn_minutes / 1440.0)).ToString("0.0") + " Days";
                    else if (game.TimePerTurn_minutes > 60) //minutes in an hour
                        newText[2].text = "Time Per Turn: " + ((double)((double)game.TimePerTurn_minutes / 60.0)).ToString("0.0") + " Hours";
                    else
                        newText[2].text = "Time Per Turn: " + game.TimePerTurn_minutes.ToString("0.0") + " Minutes";

                    if (game.currentDay == 0)
                    {
                        newText[3].text = "Has not started";
                    }
                    else
                    {
                        newText[3].text = "Day " + game.currentDay + " of " + game.daysOnIsland;
                    }

                    gameObject.name = "" + i;
                    gameObject.onClick.AddListener(delegate { ClickMyGame(gameObject.name); });
                    buttonList.Add(gameObject);

                    Buttons_MyGames[i++] = game;
                }


                Buttons_OpenGames = new Global.Game[localGameList_openGames.Count];
                i = 0;
                foreach (Global.Game game in localGameList_openGames)
                {
                    Debug.Log("Add Button - Open Games");
                    Button gameObject = Instantiate(Button_Prefab);
                    gameObject.transform.SetParent(Panel_OpenGames.transform);
                    gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Button_First_Button.GetComponent<RectTransform>().rect.width, Button_First_Button.GetComponent<RectTransform>().rect.height);
                    gameObject.transform.localScale = new Vector3(1, 1, 1);

                    TMP_Text[] newText = gameObject.GetComponentsInChildren<TMP_Text>();

                    newText[0].text = game.gameName;

                    newText[1].text = "Players: " + game.playerCount + " of " + game.MaxPlayers;

                    if (game.TimePerTurn_minutes > 1440) //minutes in a day
                        newText[2].text = "Time Per Turn: " + ((double)((double)game.TimePerTurn_minutes / 1440.0)).ToString("0.0") + " Days";
                    else if (game.TimePerTurn_minutes > 60) //minutes in an hour
                        newText[2].text = "Time Per Turn: " + ((double)((double)game.TimePerTurn_minutes / 60.0)).ToString("0.0") + " Hours";
                    else
                        newText[2].text = "Time Per Turn: " + game.TimePerTurn_minutes.ToString("0.0") + " Minutes";

                    newText[3].text = "" + game.daysOnIsland + " Days Long";

                    gameObject.name = "" + i;
                    gameObject.onClick.AddListener(delegate { ClickOpenGame(gameObject.name); });
                    buttonList.Add(gameObject);

                    Buttons_OpenGames[i++] = game;
                }

                dataBaseState = DataBaseState.Idle;
                OpenCanvas(MenuCanvas.Main);
                break;
            case DataBaseState.Idle:
            case DataBaseState.NewUserLogin:
                break;
        }

        viewGameDetails();

        addPlayerToGame();
    }

    public void get_gameDataFromDb(DataSnapshot snapshot)
    {
        gameFromDb = new Global.Game();
        foreach (var childSnapshot in snapshot.Children)
        {
            Debug.Log(childSnapshot);
            if (childSnapshot.Key.Equals("userIdArray"))
            {
                userIdList.Clear();
                foreach (var userID in childSnapshot.Children)
                {
                    while (userIdList.Count <= Convert.ToInt32(userID.Key))
                        userIdList.Add("");
                    try
                    {
                        userIdList[Convert.ToInt32(userID.Key)] = "" + Convert.ToInt32(userID.Value);
                    }
                    catch
                    {
                        userIdList[Convert.ToInt32(userID.Key)] = "" + (String)(userID.Value);
                    }
                }
            }
            else if (childSnapshot.Key.Equals("userCharacterIndexArray"))
            {
                userCharacterIndexList.Clear();
                foreach (var day in childSnapshot.Children)
                {
                    while (userCharacterIndexList.Count <= Convert.ToInt32(day.Key))
                        userCharacterIndexList.Add(0);
                    userCharacterIndexList[Convert.ToInt32(day.Key)] = Convert.ToInt32(day.Value);
                }
            }
            else if (childSnapshot.Key.Equals("dayEliminated"))
            {
                dayEliminatedList.Clear();
                foreach (var day in childSnapshot.Children)
                {
                    while (dayEliminatedList.Count <= Convert.ToInt32(day.Key))
                        dayEliminatedList.Add(0);
                    dayEliminatedList[Convert.ToInt32(day.Key)] = Convert.ToInt32(day.Value);
                }
            }
            else if (childSnapshot.Key.Equals("destinationArray"))
            {
                destinationArrayList.Clear();
                foreach (var destination in childSnapshot.Children)
                {
                    Debug.Log("" + destination);

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

                    while (destinationArrayList.Count <= Convert.ToInt32(destination.Key))
                        destinationArrayList.Add(new Vector3(0, 0, 0));
                    destinationArrayList[Convert.ToInt32(destination.Key)] = new Vector3(x, y, z);
                }

            }
            else if (childSnapshot.Key.Equals("MaxPlayers"))
            {
                gameFromDb.MaxPlayers = Convert.ToInt32(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("TimePerTurn_minutes"))
            {
                gameFromDb.TimePerTurn_minutes = Convert.ToInt32(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("gameName"))
            {
                gameFromDb.gameName = (String)childSnapshot.Value;
            }
            else if (childSnapshot.Key.Equals("hasStarted"))
            {
                gameFromDb.hasStarted = Convert.ToBoolean(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("GameId"))
            {
                gameFromDb.GameId = Convert.ToInt64(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("tribeColorIndexArray"))
            {
                tribeColorIndexArray.Clear();
                foreach (var color in childSnapshot.Children)
                {
                    while (tribeColorIndexArray.Count <= Convert.ToInt32(color.Key))
                        tribeColorIndexArray.Add(0);
                    tribeColorIndexArray[Convert.ToInt32(color.Key)] = Convert.ToInt32(color.Value);
                }
            }
            else if (childSnapshot.Key.Equals("tribeNamesArray"))
            {
                tribeNamesArray.Clear();
                foreach (var name in childSnapshot.Children)
                {
                    while (tribeNamesArray.Count <= Convert.ToInt32(name.Key))
                        tribeNamesArray.Add("");
                    tribeNamesArray[Convert.ToInt32(name.Key)] = (String)name.Value;
                }
            }
            else if (childSnapshot.Key.Equals("tribeSwapsAtArray"))
            {
                tribeSwapsAtArray.Clear();
                foreach (var swap in childSnapshot.Children)
                {
                    while (tribeSwapsAtArray.Count <= Convert.ToInt32(swap.Key))
                        tribeSwapsAtArray.Add(0);
                    tribeSwapsAtArray[Convert.ToInt32(swap.Key)] = Convert.ToInt32(swap.Value);
                }
            }
            else if (childSnapshot.Key.Equals("tribesCountArray"))
            {
                tribesCountArray.Clear();
                foreach (var count in childSnapshot.Children)
                {
                    while (tribesCountArray.Count <= Convert.ToInt32(count.Key))
                        tribesCountArray.Add(0);
                    tribesCountArray[Convert.ToInt32(count.Key)] = Convert.ToInt32(count.Value);
                }
            }
            else if (childSnapshot.Key.Equals("beginningNumerOfTribes"))
            {
                gameFromDb.beginningNumerOfTribes = Convert.ToInt32(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("playersAtMerge"))
            {
                gameFromDb.playersAtMerge = Convert.ToInt32(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("numberOfSwaps"))
            {
                gameFromDb.numberOfSwaps = Convert.ToInt32(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("isRealTime"))
            {
                gameFromDb.isRealTime = Convert.ToBoolean(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("daysOnIsland"))
            {
                gameFromDb.daysOnIsland = Convert.ToInt32(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("currentDay"))
            {
                gameFromDb.currentDay = Convert.ToInt32(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("dayStartedAt_UTCTime_SecondsSinceEpoch"))
            {
                gameFromDb.dayStartedAt_UTCTime_SecondsSinceEpoch = Convert.ToInt64(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("currentPhaseOfDay"))
            {
                gameFromDb.currentPhaseOfDay = (Global.EventSequence)Convert.ToInt32(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("phaseStartedAt_UTCTime_SecondsSinceEpoch"))
            {
                gameFromDb.phaseStartedAt_UTCTime_SecondsSinceEpoch = Convert.ToInt64(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("gameCreatedBy_playerId"))
            {
                try
                {
                    gameFromDb.gameCreatedBy_playerId = "" + Convert.ToInt32(childSnapshot.Value);
                }
                catch
                {
                    gameFromDb.gameCreatedBy_playerId = "" + (String)(childSnapshot.Value);
                }
            }
            else if (childSnapshot.Key.Equals("gameCreatedBy_characterName"))
            {
                gameFromDb.gameCreatedBy_characterName = (String)childSnapshot.Value;
            }
            else if (childSnapshot.Key.Equals("minPlayerLevel"))
            {
                gameFromDb.minPlayerLevel = Convert.ToInt32(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("maxPlayerLevel"))
            {
                gameFromDb.maxPlayerLevel = Convert.ToInt32(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("tribeCountIncreases"))
            {
                gameFromDb.tribeCountIncreases = Convert.ToBoolean(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("maxTribeCount"))
            {
                gameFromDb.maxTribeCount = Convert.ToInt32(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("showSwapDetails"))
            {
                gameFromDb.showSwapDetails = Convert.ToBoolean(childSnapshot.Value);
            }
            else if (childSnapshot.Key.Equals("eventByDayArray"))
            {
                eventByDayArray.Clear();
                foreach (var count in childSnapshot.Children)
                {
                    while (eventByDayArray.Count <= Convert.ToInt32(count.Key))
                        eventByDayArray.Add(0);
                    eventByDayArray[Convert.ToInt32(count.Key)] = Convert.ToByte(count.Value);
                }
            }
            else if (childSnapshot.Key.Equals("playerCount"))
            {
                gameFromDb.playerCount = Convert.ToInt32(childSnapshot.Value);
            }
        }

        gameFromDb.userIdArray = new string[userIdList.Count];
        userIdList.CopyTo(gameFromDb.userIdArray);

        gameFromDb.userCharacterIndexArray = new int[userCharacterIndexList.Count];
        userCharacterIndexList.CopyTo(gameFromDb.userCharacterIndexArray);

        gameFromDb.dayEliminated = new int[dayEliminatedList.Count];
        dayEliminatedList.CopyTo(gameFromDb.dayEliminated);

        gameFromDb.destinationArray = new Vector3[destinationArrayList.Count];
        destinationArrayList.CopyTo(gameFromDb.destinationArray);

        gameFromDb.tribeColorIndexArray = new int[tribeColorIndexArray.Count];
        tribeColorIndexArray.CopyTo(gameFromDb.tribeColorIndexArray);

        gameFromDb.tribeNamesArray = new string[tribeNamesArray.Count];
        tribeNamesArray.CopyTo(gameFromDb.tribeNamesArray);

        gameFromDb.tribeSwapsAtArray = new int[tribeSwapsAtArray.Count];
        tribeSwapsAtArray.CopyTo(gameFromDb.tribeSwapsAtArray);

        gameFromDb.tribesCountArray = new int[tribesCountArray.Count];
        tribesCountArray.CopyTo(gameFromDb.tribesCountArray);

        gameFromDb.eventByDayArray = new byte[eventByDayArray.Count];
        eventByDayArray.CopyTo(gameFromDb.eventByDayArray);

        gameFromDb.playerCount = 0;
        foreach(string id in userIdList)
        {
            if (id.Length > 2)  //2 should account for "", "0", and "-1"
                gameFromDb.playerCount++;
        }
    }

    void viewGameDetails()
    {
        if (Canvas_GameNotStarted.activeSelf)
        {
            Global.Game selectedGame;
            int gameIndex = gameSelected_Index;
            int myIndex = -1;

            if (lookingAtOpenGame)
            {
                TMP_Text_LeftButtonText.text = "Done";
                TMP_Text_RightButtonText.text = "Join";
                Slider_WhichCharacter.gameObject.SetActive(true);
                selectedGame = Buttons_OpenGames[gameIndex];
            }
            else
            {
                TMP_Text_LeftButtonText.text = "Leave Game";
                TMP_Text_RightButtonText.text = "Done";
                Slider_WhichCharacter.gameObject.SetActive(false);
                selectedGame = Buttons_MyGames[gameIndex];

                for (int i = 0; i < selectedGame.userIdArray.Length; i++)
                {
                    if (Global.mainPlayerId == selectedGame.userIdArray[i])
                    {
                        myIndex = i;
                        break;
                    }
                }
            }


            // Left Side Summary information (Basics)
            TMP_Text_GameName.text = selectedGame.gameName;
            if(myIndex == -1 && lookingAtOpenGame == false)
            {
                Debug.Log("User not found in userIdArray !!!!!!!!!!!!!!!!");
                TMP_Text_ParchmentSummaryText.text = "You were not found in this game.";
                TMP_Text_ParchmentSummaryText2.text = "";
                Button_LeftButton.gameObject.SetActive(false);
                return;
            }

            Button_LeftButton.gameObject.SetActive(true);
            int myCharacterIndex;
            string myCharacterName;
            if (lookingAtOpenGame)
            {
                Slider_WhichCharacter.maxValue = Global.localUser.Characters.Count;
                TMP_Text_CharacterXofY.text = "Character: " + Slider_WhichCharacter.value + " of " + Slider_WhichCharacter.maxValue;
                TMP_Text_CharacterName.text = Global.localUser.Characters[(int)Slider_WhichCharacter.value - 1].Name;
                myCharacterName = TMP_Text_CharacterName.text;
            }
            else
            {
                myCharacterIndex = selectedGame.userCharacterIndexArray[myIndex];
                myCharacterName = Global.localUser.Characters[myCharacterIndex].Name;
            }

            float timePerTurn = selectedGame.TimePerTurn_minutes;
            string turnString = "";
            if (timePerTurn >= 1440)
            {
                timePerTurn /= 1440;
                turnString = timePerTurn.ToString("#.#") + " Days";
            }
            else
            {
                turnString = timePerTurn.ToString("#.#") + " Minutes";
            }


            string swapNumber;
            string mergeNumber;
            bool showSwapAts = selectedGame.showSwapDetails;
            if (showSwapAts)
            {
                swapNumber = "" + selectedGame.numberOfSwaps;
                mergeNumber = "" + selectedGame.playersAtMerge;
            }
            else
            {
                swapNumber = "Secret";
                mergeNumber = "Secret";
            }

            TMP_Text_ParchmentSummaryText.text =
                "My Character: " + myCharacterName + "\n" +
                "Beginning Tribe Count: " + selectedGame.beginningNumerOfTribes + "\n" +
                "Players at Merge: " + mergeNumber + "\n" +
                "Tribe Swap Count: " + swapNumber + "\n" +
                "Max Time Per Turn: " + turnString + "\n" +
                "Days on Island: " + selectedGame.daysOnIsland + "\n" +
                "Min Player Level: " + selectedGame.minPlayerLevel + "\n" +
                "Max Player Level: " + selectedGame.maxPlayerLevel + "\n" +
                "";

            // Right Side Summary information (Details)
            string TribeNames = "";
            for (int i = 0; i < selectedGame.tribeNamesArray.Length; i++)
            {
                if (i == selectedGame.tribeNamesArray.Length - 1)
                    TribeNames += selectedGame.tribeNamesArray[i];
                else
                    TribeNames += selectedGame.tribeNamesArray[i] + ", ";
            }
            string TribesColors = "";
            for (int i = 0; i < selectedGame.tribeColorIndexArray.Length; i++)
            {
                List<Global.TribeColor> TribeColor_List = Global.Init_TribeColor_List();
                TribesColors += "\n  " + TribeColor_List[selectedGame.tribeColorIndexArray[i]].name;
            }
            string SwapsHidden = "";
            string SwapAt_str = "";
            string TribesAtSwap_stt = "";
            if (showSwapAts == false)
            {
                SwapsHidden = "Tribe Swaps are Secret";
            }
            if (showSwapAts == true)
            {
                SwapsHidden = "Tribe Swaps are Public Knowledge";
                if (selectedGame.tribeSwapsAtArray.Length > 0)
                {
                    SwapAt_str = "Swap Tribes At: ";
                    TribesAtSwap_stt = "Tribe Count at Swap: ";
                }
                for (int i = 0; i < selectedGame.tribeSwapsAtArray.Length; i++)
                {
                    if (i == selectedGame.tribeSwapsAtArray.Length - 1)
                        SwapAt_str += selectedGame.tribeSwapsAtArray[i];
                    else
                        SwapAt_str += selectedGame.tribeSwapsAtArray[i] + ", ";

                    if (i == selectedGame.tribeSwapsAtArray.Length - 1)
                        TribesAtSwap_stt += selectedGame.tribeSwapsAtArray[i];
                    else
                        TribesAtSwap_stt += selectedGame.tribeSwapsAtArray[i] + ", ";
                }
            }

            TMP_Text_ParchmentSummaryText2.text =
                "Tribe Count: " + selectedGame.maxTribeCount + "\n" +
                "Tribe List: " + TribeNames + "\n" +
                "Tribe Colors: " + TribesColors + "\n" +
                SwapsHidden + "\n" +
                SwapAt_str + "\n" +
                TribesAtSwap_stt + "\n\n" +
                "Players: " + selectedGame.playerCount + " of " + selectedGame.MaxPlayers + "\n(waiting for " + (selectedGame.MaxPlayers - selectedGame.playerCount) + " more)\n" +
                "";


            if (joinSelectedGame)
            {
                joinSelectedGame = false;

                myIndex = -1;
                for (int i = 0; i < selectedGame.userIdArray.Length; i++)
                {
                    if (selectedGame.userIdArray[i] == "0" || selectedGame.userIdArray[i] == "")
                    {
                        //empty spot found in array
                        myIndex = i;
                        break;
                    }
                }

                if (myIndex == -1)
                {
                    OpenCanvas(MenuCanvas.Popup);
                    TMP_Text_PopupMessage.text = "Could not find an open spot in this game. This game might be full.";
                }
                else
                {
                    addPlayerToGameState = AddPlayerToGameState.Init;
                    gameId_addPlayer = selectedGame.GameId;
                }
            }
        }
    }

    Global.Game game_addPlayerGame;
    void addPlayerToGame()
    {
        switch(addPlayerToGameState)
        {
            case AddPlayerToGameState.Init:
                addPlayerToGameState = AddPlayerToGameState.GetDesiredGame;
                OpenCanvas(MenuCanvas.Loading);
                dontShowProgress = true;
                break;
            case AddPlayerToGameState.GetDesiredGame:
                addPlayerToGameState = AddPlayerToGameState.Idle;
                if (gameId_addPlayer > 0)
                {
                    String refStr = "Game/" + gameId_addPlayer;
                    FirebaseDatabase.DefaultInstance
                      .GetReference(refStr)
                      .GetValueAsync().ContinueWith(task => {
                          if (task.IsFaulted)
                          {
                              // Handle the error...
                              OpenCanvas(MenuCanvas.Popup);
                              TMP_Text_PopupMessage.text = "Could not add you to the game. Please try again later.";
                              addPlayerToGameState = AddPlayerToGameState.Idle;
                          }
                          else if (task.IsCompleted)
                          {
                              DataSnapshot snapshot = task.Result;
                              // Do something with snapshot...
                              Debug.Log("FireBaseDataBase Task Complete");

                              get_gameDataFromDb(snapshot);

                              game_addPlayerGame = (gameFromDb);

                              addPlayerToGameState = AddPlayerToGameState.AddUserToGame;
                          }
                      });
                }
                else
                {
                    OpenCanvas(MenuCanvas.Popup);
                    TMP_Text_PopupMessage.text = "Could not add you to the game. Please try again later.";
                    addPlayerToGameState = AddPlayerToGameState.Idle;
                }
                break;
            case AddPlayerToGameState.AddUserToGame:

                bool foundSpot = false;
                for(int i = 0; i < game_addPlayerGame.userCharacterIndexArray.Length; i++)
                {
                    if(game_addPlayerGame.userIdArray[i] == "0" || game_addPlayerGame.userIdArray[i] == "")
                    {
                        foundSpot = true;
                        game_addPlayerGame.userIdArray[i] = Global.mainPlayerId;
                        game_addPlayerGame.userCharacterIndexArray[i] = (int)Slider_WhichCharacter.value - 1;
                        game_addPlayerGame.playerCount += 1;
                        break;
                    }
                }

                if (foundSpot)
                {
                    string json = JsonUtility.ToJson(game_addPlayerGame);

                    addPlayerToGameState = AddPlayerToGameState.Idle;
                    mDatabaseRef.Child("Game").Child("" + game_addPlayerGame.GameId).SetRawJsonValueAsync(json).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            OpenCanvas(MenuCanvas.Popup);
                            TMP_Text_PopupMessage.text = "Could not add you to the game. Please try again later.";
                            addPlayerToGameState = AddPlayerToGameState.Idle;
                        }
                        else if (task.IsCompleted)
                        {
                            addPlayerToGameState = AddPlayerToGameState.AddGameToUser;
                        }
                    });
                }
                else
                {
                    OpenCanvas(MenuCanvas.Popup);
                    TMP_Text_PopupMessage.text = "This game is now full.";
                    addPlayerToGameState = AddPlayerToGameState.Idle;
                }
                break;
            case AddPlayerToGameState.AddGameToUser:
                Global.localUser.Games.Add(game_addPlayerGame.GameId);

                Global.User_Simple userTemp = new Global.User_Simple(Global.localUser);
                string json2 = JsonUtility.ToJson(userTemp);

                addPlayerToGameState = AddPlayerToGameState.Idle;
                mDatabaseRef.Child("users").Child("" + Global.mainPlayerId).SetRawJsonValueAsync(json2).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        OpenCanvas(MenuCanvas.Popup);
                        TMP_Text_PopupMessage.text = "Could not add you to the game. Please try again later.";
                        addPlayerToGameState = AddPlayerToGameState.Idle;
                    }
                    else if (task.IsCompleted)
                    {
                        addPlayerToGameState = AddPlayerToGameState.Done;
                    }
                });

                break;
            case AddPlayerToGameState.Done:
                addedSuccessfully = true;
                addPlayerToGameState = AddPlayerToGameState.Idle;
                break;
            case AddPlayerToGameState.Idle:
                break;
        }
    }


    public static TMP_Text FindGameObjectInChildWithTag_TMP_Text(GameObject parent, string tag)
    {
        Transform t = parent.transform;

        for (int i = 0; i < t.childCount; i++)
        {
            if (t.GetChild(i).gameObject.tag == tag)
            {
                GameObject gameObject = t.GetChild(i).gameObject;
                return gameObject.GetComponent<TMP_Text>();
            }

        }

        return null;
    }

    int gameSelected_Index;
    bool switchToGameScreen = false;
    void ClickMyGame(String index)
    {
        Debug.Log("ClickMyGame " + index);

        int gameIndex = Int32.Parse(index);

        //if (localGameList[gameIndex].hasStarted)
        if (true)
        {
            Global.currentGame = localGameList[gameIndex];
            Canvas_Loading_Game.SetActive(true);
            switchToGameScreen = true;

            if (InputField_MainUserId.text.Length > 0)
                Global.mainPlayerId = InputField_MainUserId.text;
        }
        else
        {
            gameSelected_Index = gameIndex;
            OpenCanvas(MenuCanvas.GameNotStarted);
            lookingAtOpenGame = false;
        }
    }

    void ClickOpenGame(String index)
    {
        Debug.Log("ClickOpenGame " + index);

        if (Global.localUser.Characters.Count == 0)
        {

            OpenCanvas(MenuCanvas.Popup);
            TMP_Text_PopupMessage.text = "You must create a character before joining an open game.";
        }
        else
        {
            int gameIndex = Int32.Parse(index);
            gameSelected_Index = gameIndex;
            OpenCanvas(MenuCanvas.GameNotStarted);
            lookingAtOpenGame = true;
        }
    }

    public void Click_CreatePlayer()
    {
        Global.creatingCharacter = true;
        SceneManager.LoadScene((int)Global.Screen.Screen_CreatePlayer);
    }

    public void Click_NewGame()
    {
        Global.creatingCharacter = true;    //we will use this variable for a "New game" too
        if (Global.localUser.Characters == null || Global.localUser.Characters.Count == 0)
        {
            OpenCanvas(MenuCanvas.Popup);
            TMP_Text_PopupMessage.text = "You must create a player before you can start a new game.";
        }
        else
        {
            SceneManager.LoadScene((int)Global.Screen.Screen_NewGame);
        }
    }

    public void ButtonClick_Withdraw()
    {
        if (lookingAtOpenGame)  //pressed 'done' looking at open game
        {
            OpenCanvas(MenuCanvas.Main);
        }
        else    //pressed 'leave game' button
        {
            TMP_Text_PopupMessageConfirm.text = "Are you sure you want to withdraw your character from this game?";
            TMP_Text_PopupMessageConfirm_Confirm.text = "Withdraw From Game";
            TMP_Text_PopupMessageConfirm_Cancel.text = "Cancel";
            OpenCanvas(MenuCanvas.PopupConfirm);
        }
    }

    public void ButtonClick_Withdraw_Confirm()
    {
        int gameIndex = gameSelected_Index;
        int myIndex = -1;
        for (int i = 0; i < Buttons_MyGames[gameIndex].userIdArray.Length; i++)
        {
            if (Global.mainPlayerId == Buttons_MyGames[gameIndex].userIdArray[i])
            {
                myIndex = i;
                break;
            }
        }

        if (myIndex == -1)
        {
            OpenCanvas(MenuCanvas.Popup);
            TMP_Text_PopupMessage.text = "Failed to remove you from the game.";
        }
        else
        {
            OpenCanvas(MenuCanvas.Loading);
            dontShowProgress = true;
            Buttons_MyGames[gameIndex].userIdArray[myIndex] = "0";    //set this user to 0
            Buttons_MyGames[gameIndex].playerCount -= 1;            //decrement player count

            Global.Game game = Buttons_MyGames[gameIndex];
            string json = JsonUtility.ToJson(game);

            mDatabaseRef.Child("Game").Child("" + Buttons_MyGames[gameIndex].GameId).SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    OpenCanvas(MenuCanvas.Popup);
                    TMP_Text_PopupMessage.text = "Could not remove user. Please try again later.";
                }
                else if (task.IsCompleted)
                {
                    removedSuccessfully = true;
                    //goToMainScreen_delayed = true;
                }
            });




            Global.localUser.Games.RemoveAt(gameIndex);     //remove the game from the user's list of games
            Global.User_Simple userTemp = new Global.User_Simple(Global.localUser);
            string json2 = JsonUtility.ToJson(userTemp);

            addPlayerToGameState = AddPlayerToGameState.Idle;
            mDatabaseRef.Child("users").Child("" + Global.mainPlayerId).SetRawJsonValueAsync(json2).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    OpenCanvas(MenuCanvas.Popup);
                    TMP_Text_PopupMessage.text = "Could not remove user. Please try again later.";
                    addPlayerToGameState = AddPlayerToGameState.Idle;
                }
                else if (task.IsCompleted)
                {
                    OpenCanvas(MenuCanvas.Main);
                }
            });

        }
    }

    public void ButtonClick_Withdraw_Cancel()
    {
        OpenCanvas(MenuCanvas.OpenGame);
    }

    public void Click_Refresh()
    {
        bool isFirst = true;
        foreach (Button button in buttonList)
        {
            if (!isFirst)
            {
                Destroy(button.gameObject);
            }
            isFirst = false;
        }
        buttonList.Clear();

        Button gameObject = Button_First_Button;
        TMP_Text[] newText = gameObject.GetComponentsInChildren<TMP_Text>();
        newText[0].text = "";
        newText[1].text = "";
        newText[2].text = "";
        newText[3].text = "";

        if (InputField_MainUserId.text.Length > 0)
            Global.mainPlayerId = InputField_MainUserId.text;

        progressStep = -1;
        dataBaseState = DataBaseState.Init;
    }

    public void Button_DismissPopup_click()
    {
        OpenCanvas(MenuCanvas.Main);
        if(refreshAfterPopup)
        {
            refreshAfterPopup = false;
            Click_Refresh();
        }
    }

    public void ButtonClick_CloseGameNotStarted()
    {
        if (lookingAtOpenGame)  //join open game
        {
            joinSelectedGame = true;
        }
        else     //Done looking at game that hasn't started yet
        {
            OpenCanvas(MenuCanvas.Main);
        }
    }

    public void Button_DismissPopupConfirm_Cancel_click()
    {
        OpenCanvas(MenuCanvas.Main);
    }

    public void Button_DismissPopupConfirm_Confirm_click()
    {
        OpenCanvas(MenuCanvas.Main);
    }

    public void Button_DecrementCharacter_click()
    {
        if (Slider_WhichCharacter.value > Slider_WhichCharacter.minValue)
        {
            Slider_WhichCharacter.value -= 1;
            if (Slider_WhichCharacter.value < Slider_WhichCharacter.minValue)
            {
                Slider_WhichCharacter.value = Slider_WhichCharacter.minValue;
            }
        }
    }

    public void Button_IncrementCharacter_click()
    {
        if (Slider_WhichCharacter.value < Slider_WhichCharacter.maxValue)
        {
            Slider_WhichCharacter.value += 1;
            if (Slider_WhichCharacter.value > Slider_WhichCharacter.maxValue)
            {
                Slider_WhichCharacter.value = Slider_WhichCharacter.maxValue;
            }
        }
    }

    public void ButtonClick_Logout()
    {
        FirebaseAuth.DefaultInstance.SignOut();
        SceneManager.LoadScene((int)Global.Screen.Screen_Login);
    }
}
