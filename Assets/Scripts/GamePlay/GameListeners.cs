using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using System;

public static class GameListeners
{
    //// Start is called before the first frame update
    //void Start()
    //{
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}


    public static void Subscribe_ListenForLoginChanges()
    {
        if (Global.DatabaseRef == null)
        {
            Global.DatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        }
        string refStr = "Game/" + Global.currentGame.GameId + "/userLoggedIn";
        FirebaseDatabase.DefaultInstance
          .GetReference(refStr)
          .ValueChanged += HandleNewMessages;
    }


    private static void HandleNewMessages(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = args.Snapshot;

        Global.loginStatusArrayList = new List<bool>();
        Global.loginStatusArrayList.Clear();
        foreach (var userLoginStatus in snapshot.Children)
        {
            while (Global.loginStatusArrayList.Count <= Convert.ToInt32(userLoginStatus.Key))
                Global.loginStatusArrayList.Add(false);
            Global.loginStatusArrayList[Convert.ToInt32(userLoginStatus.Key)] = (bool)userLoginStatus.Value;
        }

        Debug.Log(Global.loginStatusArrayList);
    }

    public static void GetPingTimes_and_UpdateLogins()
    {
        FirebaseDatabase.DefaultInstance
        .GetReference("Game/" + Global.currentGame.GameId + "/userPingTime")
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

                List<long> pingList = new List<long>();
                foreach (var ping in snapshot.Children)
                {
                    while (pingList.Count <= Convert.ToInt32(ping.Key))
                        pingList.Add(0);
                    pingList[Convert.ToInt32(ping.Key)] = Convert.ToInt64(ping.Value);
                }


                while (Global.loginStatusArrayList.Count < pingList.Count)
                    Global.loginStatusArrayList.Add(false);
                for (int i = 0; i < pingList.Count; i++)
                {
                    long diff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - pingList[i];
                    if (diff > 12)   //Assume the user is logged out if they haven't sent a ping in over 12 seconds, they should send a ping every 10 seconds
                    {
                        Global.loginStatusArrayList[i] = false; //set the local login status to false, but leave the server's value alone
                    }
                }
            }
        });
    }


    public static void Subscribe_ListenFor_LockedIn_Changes()
    {
        if (Global.DatabaseRef == null)
        {
            Global.DatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        }
        string refStr = "Game/" + Global.currentGame.GameId + "/LockedIn";
        FirebaseDatabase.DefaultInstance
          .GetReference(refStr)
          .ValueChanged += HandleNewMessages_LockedIn;
    }


    private static void HandleNewMessages_LockedIn(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = args.Snapshot;
        int lockedIn_count = 0;

        if (Global.playerLockedInList == null)
        {
            Global.playerLockedInList = new List<bool>();
            Global.playerLockedInList.Clear();
        }
        foreach (var userLockedInStatus in snapshot.Children)
        {
            while (Global.loginStatusArrayList.Count <= Convert.ToInt32(userLockedInStatus.Key))
                Global.loginStatusArrayList.Add(false);
            Global.loginStatusArrayList[Convert.ToInt32(userLockedInStatus.Key)] = (bool)userLockedInStatus.Value;
            if ((bool)userLockedInStatus.Value == true)
            {
                lockedIn_count++;
            }
        }

        if (lockedIn_count == Global.currentGame.playerCount)
        {
            Global.advanceToNextPhaseOfTurn = true;
        }
    }


    public static void Subscribe_ListenFor_currentPhaseOfDay_Changes()
    {
        if (Global.DatabaseRef == null)
        {
            Global.DatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        }
        string refStr = "Game/" + Global.currentGame.GameId + "/currentPhaseOfDay";
        FirebaseDatabase.DefaultInstance
          .GetReference(refStr)
          .ValueChanged += HandleNewMessages_currentPhaseOfDay;
    }


    private static void HandleNewMessages_currentPhaseOfDay(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = args.Snapshot;

        //TODO what to do when phase changes

        foreach (var childSnapshot in snapshot.Children)
        {
            if (childSnapshot.Key.Equals("currentPhaseOfDay"))
            {
                Global.currentGame.currentPhaseOfDay = (Global.EventSequence)Convert.ToInt32(childSnapshot.Value);
            }
        }
    }


    public static void Subscribe_ListenFor_currentPhaseOfDay_StartTime_Changes()
    {
        if (Global.DatabaseRef == null)
        {
            Global.DatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        }
        string refStr = "Game/" + Global.currentGame.GameId + "/phaseStartedAt_UTCTime_SecondsSinceEpoch";
        FirebaseDatabase.DefaultInstance
          .GetReference(refStr)
          .ValueChanged += HandleNewMessages_currentPhaseOfDay_StartTime;
    }


    private static void HandleNewMessages_currentPhaseOfDay_StartTime(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = args.Snapshot;

        if (snapshot.Key.Equals("phaseStartedAt_UTCTime_SecondsSinceEpoch"))
        {
            Global.currentGame.phaseStartedAt_UTCTime_SecondsSinceEpoch = Convert.ToInt64(snapshot.Value);
            Global.waitForCallbackBeforeAdvancing = 0;
        }
    }
}
