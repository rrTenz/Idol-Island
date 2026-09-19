using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using System;
using TMPro;

public class UpdateGameProgress : MonoBehaviour
{
    public Image Image_DecisionCheckmark;
    public GameObject Canvas_Decisions;

    float timer_1_Second;
    float timer_10_Second = 10.0f;
    float timer_15_Second;

    int prevDay_index = 0;
    int numberOfPhasesInCurrentDay = 0;
    float minutesPerPhase = 0;

    public TMP_Text Text_TimeUntilNextPhase;

    // Start is called before the first frame update
    void Start()
    {
        //TODO remove
        if (Global.currentGame == null)
        {
            Global.currentGame = new Global.Game();
            Global.loginStatusArrayList = new List<bool>();
            Global.loginStatusArrayList.Add(true);
            Global.currentGame.eventByDayArray = new byte[1];
        }

        GameListeners.Subscribe_ListenForLoginChanges();
        GameListeners.Subscribe_ListenFor_currentPhaseOfDay_Changes();
        GameListeners.Subscribe_ListenFor_currentPhaseOfDay_StartTime_Changes();
        Image_DecisionCheckmark.color = Color.red;
    }

    // Update is called once per frame
    void Update()
    {
        StartNewGame(); //check to see if a new game should be started

        timer_1_Second += Time.deltaTime;
        if (timer_1_Second >= 1.0f)
        {
            timer_1_Second = 0;
            if (Global.waitForCallbackBeforeAdvancing > 0)
                Global.waitForCallbackBeforeAdvancing--;
        }

        timer_10_Second += Time.deltaTime;
        if(timer_10_Second >= 10.0f)
        {
            timer_10_Second = 0;
            SendToDatabase_PingServer(Global.currentGame.GameId, Global.getMainPlayerIndex_currentGame());
        }

        timer_15_Second += Time.deltaTime;
        if (timer_15_Second >= 15.0f)
        {
            timer_15_Second = 0;
            GameListeners.GetPingTimes_and_UpdateLogins();
        }

        if (Global.playerLockedInList == null)
        {
            if(Global.currentGame.hasStarted == false)
            {
                Global.playerLockedInList = new List<bool>(new bool[Global.currentGame.playerCount]);
                SendToDatabase_LockedIn(Global.currentGame.GameId, Global.getMainPlayerIndex_currentGame(), false);
            }

            GameListeners.Subscribe_ListenFor_LockedIn_Changes();
        }

        if(Global.advanceToNextPhaseOfTurn)
        {
            Global.advanceToNextPhaseOfTurn = false;
            Advance_to_NextPhaseOfTurn();
        }

        if(prevDay_index != Global.currentGame.currentDay)
        {
            numberOfPhasesInCurrentDay = Number_of_Phases_In_Current_Day();
            minutesPerPhase = (float)Global.currentGame.TimePerTurn_minutes / (float)numberOfPhasesInCurrentDay;
        }
        prevDay_index = Global.currentGame.currentDay;

        UpdateTimeLeftInPhase();
    }

    static private int Get_FirstLoggedIn_index()
    {
        for (int i = 0; i < Global.loginStatusArrayList.Count; i++)
        {
            if(Global.loginStatusArrayList[i] == true)
            {
                return i;
            }
        }
        return -1;
    }

    private void UpdateTimeLeftInPhase()
    {
        if (Global.waitForCallbackBeforeAdvancing > 0)
            return;

        long now_utcOffset = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long start_utcOffset = Global.currentGame.phaseStartedAt_UTCTime_SecondsSinceEpoch;
        double secondPerPhase = minutesPerPhase * 60;
        long howMuchTimeHasPassed = now_utcOffset - start_utcOffset;
        long secondsLeft = (long)(secondPerPhase - howMuchTimeHasPassed);

        if(secondsLeft <= 0)
        {
            Advance_to_NextPhaseOfTurn();
            secondsLeft = 0;
        }

        TimeSpan t = TimeSpan.FromSeconds(secondsLeft);
        string formattedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                        t.Hours,
                        t.Minutes,
                        t.Seconds);

        Text_TimeUntilNextPhase.text = "Day " + Global.currentGame.currentDay + " | " + CurrentPhaseToString() + " | " + formattedTime;
    }

    private string CurrentPhaseToString()
    {
        if (Global.currentGame.currentDay >= Global.currentGame.eventByDayArray.Length)
        {
            return "The game has ended";
        }

        string phaseString = "";

        Global.EventMask eventMask = (Global.EventMask)Global.currentGame.eventByDayArray[Global.currentGame.currentDay];

        switch (Global.currentGame.currentPhaseOfDay)
        {
            case Global.EventSequence.CampTime_First:
            case Global.EventSequence.CampTime_PostChallenge:
            case Global.EventSequence.CampTime_PostSwapOrMerge:
                phaseString = "Camp Time";
                break;
            case Global.EventSequence.RewardOrImmunity:
                if ((eventMask & Global.EventMask.Reward) == Global.EventMask.Reward)
                    phaseString = "Reward Challenge";
                if ((eventMask & Global.EventMask.Immunity) == Global.EventMask.Immunity)
                    phaseString = "Immunity Challenge";
                break;
            case Global.EventSequence.SwapOrMerge:
                if ((eventMask & Global.EventMask.Swap) == Global.EventMask.Swap)
                    phaseString = "Swap";
                if ((eventMask & Global.EventMask.Merge) == Global.EventMask.Merge)
                    phaseString = "Merge";
                break;
            case Global.EventSequence.Tribal:
                if(Global.currentGame.currentDay == Global.currentGame.daysOnIsland)
                    phaseString = "Final Tribal Council";
                else
                    phaseString = "Tribal Council";
                break;
        }

        return phaseString;
    }

    static private int Number_of_Phases_In_Current_Day()
    {
        if(Global.currentGame.currentDay >= Global.currentGame.eventByDayArray.Length)
        {
            return 0; //the game has ended
        }

        int phaseCount = 1; //There will always be at least 1 phase (first camp time)

        Global.EventMask eventMask = (Global.EventMask)Global.currentGame.eventByDayArray[Global.currentGame.currentDay];

        if ((eventMask & Global.EventMask.Reward) == Global.EventMask.Reward)
            phaseCount += 2;    //Reward and Post Reward Camp Time
        if ((eventMask & Global.EventMask.Immunity) == Global.EventMask.Immunity)
            phaseCount += 3;    //Immunity, Post Immunity Camp Time, Tribal
        if ((eventMask & Global.EventMask.Swap) == Global.EventMask.Swap)
            phaseCount += 2;    //Swap and Post Swap Camp Time
        if ((eventMask & Global.EventMask.Merge) == Global.EventMask.Merge)
            phaseCount += 2;    //Merge and Post Merge Camp Time
        if ((eventMask & Global.EventMask.Final_Tribal) == Global.EventMask.Final_Tribal)
            phaseCount += 1;    //Just First Camp time and Final Tribal

        return phaseCount;
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // App is paused
            SendToDatabase_LogginStatus(Global.currentGame.GameId, Global.getMainPlayerIndex_currentGame(), false);
        }
        else
        {
            // App resumed
            SendToDatabase_LogginStatus(Global.currentGame.GameId, Global.getMainPlayerIndex_currentGame(), true);
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            // App resumed
            SendToDatabase_LogginStatus(Global.currentGame.GameId, Global.getMainPlayerIndex_currentGame(), true);
        }
        else
        {
            // App is paused
            SendToDatabase_LogginStatus(Global.currentGame.GameId, Global.getMainPlayerIndex_currentGame(), false);
        }

    }

    private void OnApplicationQuit()
    {
        if(Global.currentGame.GameId > 0)
        {
            SendToDatabase_LogginStatus(Global.currentGame.GameId, Global.getMainPlayerIndex_currentGame(), false);
        }
    }

    static public void SendToDatabase_LogginStatus(long gameId, int playerIndex, bool isLoggedIn)
    {
        if(Global.DatabaseRef == null)
        {
            Global.DatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        }

        Global.DatabaseRef.Child("Game").Child("" + gameId).Child("userLoggedIn").Child("" + playerIndex).SetValueAsync(isLoggedIn);

        if(isLoggedIn == false)
        {
            UpdateDestinationToCurrentPosition();
        }
    }

    static public void SendToDatabase_PingServer(long gameId, int playerIndex)
    {
        if (Global.DatabaseRef == null)
        {
            Global.DatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        }

        Global.DatabaseRef.Child("Game").Child("" + gameId).Child("userPingTime").Child("" + playerIndex).SetValueAsync(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    static public void SendToDatabase_LockedIn(long gameId, int playerIndex, bool isLockedIn)
    {
        if (Global.DatabaseRef == null)
        {
            Global.DatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        }

        Global.DatabaseRef.Child("Game").Child("" + gameId).Child("LockedIn").Child("" + playerIndex).SetValueAsync(isLockedIn);
    }

    static private void UpdateDestinationToCurrentPosition()
    {
        int index = Global.mainPlayerIndex;
        Global.currentGame.destinationArray[index] = Global.playerPositionArray_currentGame[index];
        Global.stopMoving_mainPlayer = true;

        Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("destinationArray").Child("" + Global.mainPlayerIndex).Child("x").SetValueAsync(Global.currentGame.destinationArray[index].x);
        Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("destinationArray").Child("" + Global.mainPlayerIndex).Child("y").SetValueAsync(Global.currentGame.destinationArray[index].y);
        Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("destinationArray").Child("" + Global.mainPlayerIndex).Child("z").SetValueAsync(Global.currentGame.destinationArray[index].z);
    }

    private void StartNewGame()
    {
        if (Global.currentGame.hasStarted == false)
        {
            int firstLoggedIn_index = Get_FirstLoggedIn_index();
            if (firstLoggedIn_index == Global.mainPlayerIndex)   //if you are the first player in the list that is logged in, you will update the server
            {
                if (Global.DatabaseRef == null)
                {
                    Global.DatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
                }

                Global.waitForCallbackBeforeAdvancing = 10;

                //Start new Game
                Global.currentGame.hasStarted = true;
                Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("hasStarted").SetValueAsync(true);

                Global.currentGame.currentDay = 1;
                Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("currentDay").SetValueAsync(Global.currentGame.currentDay);

                Global.currentGame.dayStartedAt_UTCTime_SecondsSinceEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("dayStartedAt_UTCTime_SecondsSinceEpoch").SetValueAsync(Global.currentGame.dayStartedAt_UTCTime_SecondsSinceEpoch);

                Global.currentGame.currentPhaseOfDay = Global.EventSequence.CampTime_First;
                Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("currentPhaseOfDay").SetValueAsync((int)Global.currentGame.currentPhaseOfDay);

                Global.currentGame.phaseStartedAt_UTCTime_SecondsSinceEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("phaseStartedAt_UTCTime_SecondsSinceEpoch").SetValueAsync(Global.currentGame.phaseStartedAt_UTCTime_SecondsSinceEpoch);

                InitializeTribes();
            }
        }
    }

    void InitializeTribes()
    {
        int playerCount = Global.currentGame.playerCount;
        int initialTribalCount = Global.currentGame.beginningNumerOfTribes;
        int playersPerTribe = playerCount / initialTribalCount;


        int[] whichTribeArray = new int[playerCount];

        int index = 0;
        for (int i = 0; i < initialTribalCount; i++)
        {
            for(int j = 0; j < playersPerTribe; j++)
            {
                whichTribeArray[index++] = i;
            }
        }

        for (int i = 0; i < whichTribeArray.Length; i++)
        {
            int temp = whichTribeArray[i];
            int randomIndex = UnityEngine.Random.Range(i, whichTribeArray.Length);
            whichTribeArray[i] = whichTribeArray[randomIndex];
            whichTribeArray[randomIndex] = temp;
        }

        Debug.Log(whichTribeArray);


        if (Global.DatabaseRef == null)
        {
            Global.DatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        }

        for (int i = 0; i < whichTribeArray.Length; i++)
        {
            Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("whichTribeArray").Child("" + i).SetValueAsync(whichTribeArray[i]);
        }
    }

    void Advance_to_NextPhaseOfTurn()
    {
        if (Global.currentGame.currentDay >= Global.currentGame.eventByDayArray.Length)
        {
            return; //the game has ended
        }

        int firstLoggedIn_index = Get_FirstLoggedIn_index();
        if (firstLoggedIn_index == Global.mainPlayerIndex)   //if you are the first player in the list that is logged in, you will update the server
        {
            if (Global.DatabaseRef == null)
            {
                Global.DatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
            }

            Global.EventMask eventMask = (Global.EventMask)Global.currentGame.eventByDayArray[Global.currentGame.currentDay];

            Global.EventSequence nextPhaseOfDay = Global.currentGame.currentPhaseOfDay;
            if(eventMask == Global.EventMask.Nothing)
            {
                Advance_to_NextTurn();
                return;
            }
            else if(Global.currentGame.currentPhaseOfDay == Global.EventSequence.CampTime_First)                
            {
                if(eventMask == Global.EventMask.Final_Tribal)
                {
                    nextPhaseOfDay = Global.EventSequence.Tribal;
                }
                else if((eventMask & Global.EventMask.Merge) == Global.EventMask.Merge || (eventMask & Global.EventMask.Swap) == Global.EventMask.Swap)
                {
                    nextPhaseOfDay = Global.EventSequence.SwapOrMerge;
                }
                else// if((eventMask & Global.EventMask.Reward) == Global.EventMask.Reward || (eventMask & Global.EventMask.Immunity) == Global.EventMask.Immunity)
                {
                    nextPhaseOfDay = Global.EventSequence.RewardOrImmunity;
                }
            }
            else if (Global.currentGame.currentPhaseOfDay == Global.EventSequence.SwapOrMerge)
            {
                nextPhaseOfDay = Global.EventSequence.CampTime_PostSwapOrMerge;
            }
            else if (Global.currentGame.currentPhaseOfDay == Global.EventSequence.CampTime_PostSwapOrMerge)
            {
                if ((eventMask & Global.EventMask.Final_Tribal) == Global.EventMask.Final_Tribal)
                {
                    nextPhaseOfDay = Global.EventSequence.Tribal;
                }
                else
                {
                    nextPhaseOfDay = Global.EventSequence.RewardOrImmunity;
                }
            }
            else if(Global.currentGame.currentPhaseOfDay == Global.EventSequence.RewardOrImmunity)
            {
                nextPhaseOfDay = Global.EventSequence.CampTime_PostChallenge;
            }
            else if (Global.currentGame.currentPhaseOfDay == Global.EventSequence.CampTime_PostChallenge)
            {
                nextPhaseOfDay = Global.EventSequence.Tribal;
            }
            else if (Global.currentGame.currentPhaseOfDay == Global.EventSequence.Tribal)
            {
                Advance_to_NextTurn();
                return;
            }

            if (nextPhaseOfDay != Global.currentGame.currentPhaseOfDay)
            {
                Global.waitForCallbackBeforeAdvancing = 10;

                Global.currentGame.currentPhaseOfDay = nextPhaseOfDay;
                Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("currentPhaseOfDay").SetValueAsync((int)Global.currentGame.currentPhaseOfDay);

                Global.currentGame.phaseStartedAt_UTCTime_SecondsSinceEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("phaseStartedAt_UTCTime_SecondsSinceEpoch").SetValueAsync(Global.currentGame.phaseStartedAt_UTCTime_SecondsSinceEpoch);
            }
        }

    }

    void Advance_to_NextTurn()
    {
        if (Global.currentGame.hasStarted == true)
        {
            int firstLoggedIn_index = Get_FirstLoggedIn_index();
            if (firstLoggedIn_index == Global.mainPlayerIndex)   //if you are the first player in the list that is logged in, you will update the server
            {
                if (Global.DatabaseRef == null)
                {
                    Global.DatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
                }

                Global.waitForCallbackBeforeAdvancing = 10;

                Global.currentGame.currentDay++;
                Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("currentDay").SetValueAsync(Global.currentGame.currentDay);
                Global.currentGame.dayStartedAt_UTCTime_SecondsSinceEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("dayStartedAt_UTCTime_SecondsSinceEpoch").SetValueAsync(Global.currentGame.dayStartedAt_UTCTime_SecondsSinceEpoch);

                Global.currentGame.currentPhaseOfDay = Global.EventSequence.CampTime_First;
                Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("currentPhaseOfDay").SetValueAsync((int)Global.currentGame.currentPhaseOfDay);

                Global.currentGame.phaseStartedAt_UTCTime_SecondsSinceEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Global.DatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("phaseStartedAt_UTCTime_SecondsSinceEpoch").SetValueAsync(Global.currentGame.phaseStartedAt_UTCTime_SecondsSinceEpoch);
            }
        }
    }

    public void ButtonClick_ShowDecisionCanvas()
    {
        Canvas_Decisions.SetActive(true);
        Image_DecisionCheckmark.color = Color.white;
    }

    public void ButtonClick_CloseDecisionCanvas()
    {
        SendToDatabase_LockedIn(Global.currentGame.GameId, Global.getMainPlayerIndex_currentGame(), false);
        Image_DecisionCheckmark.color = Color.red;
        Canvas_Decisions.SetActive(false);
    }

    public void ButtonClick_LockInDecision()
    {
        SendToDatabase_LockedIn(Global.currentGame.GameId, Global.getMainPlayerIndex_currentGame(), true);
        Image_DecisionCheckmark.color = Color.green;
        Canvas_Decisions.SetActive(false);
    }
}
