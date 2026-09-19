using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;
using System;
using System.Collections.Generic;

public class FireBaseDataBase : MonoBehaviour
{
    //public TMP_Text HelloWorld;

    DatabaseReference mDatabaseRef;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("FireBaseDataBase Start");
        // Get the root reference location of the database.
        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("FireBaseDataBase Got ref");
        //HelloWorld.text = "Got ref";




        //writeNewUser("123", "Ryan", "rtensmeyer@msn.com");
        //updateUserName("123", "Ryan");
        //updateUserEmail("123", "r123@outlook.com");
        //writeNewGame(1, 20, "Game 1", 60, false, new int[] { 123, 124, 0 });
        //writeNewGame(2, 20, "Game 2 is cool", 300, false, new int[] { 123, 124, 0, 0, 0, 0, 0 });
        //writeNewGame(3, 20, "Game 3 empty", 4320, false, new int[] { 0, 0, 0 });


        //Vector3[] randomDestinationArray = get_randomDestinationArray(20, -10, 10);
        //writeNewGame(4, 20, "Game 4 good", 4320, false,
        //    new int[] { 123, 124, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },    //user IDs
        //    new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },    //user Character Indexes
        //    new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },    //Day Eliminated
        //    randomDestinationArray    //Destinations             
        //    );
    }

    private Vector3[] get_randomDestinationArray(int count, int min, int max)
    {
        Vector3[] destinationArray = new Vector3[count];

        System.Random random = new System.Random();

        for(int i = 0; i < count; i++)
        {
            destinationArray[i] = new Vector3(random.Next(min, max) + (float)(random.Next(10)/10.0), -2.1f, random.Next(min, max) + (float)(random.Next(10) / 10.0));
        }
        return destinationArray;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void writeNewUser(string userId, string name, string email)
    {
        Global.User user = new Global.User(name, email, new List<long>(), new List<Global.CharacterOptions>());
        string json = JsonUtility.ToJson(user);

        mDatabaseRef.Child("users").Child(userId).SetRawJsonValueAsync(json);
    }

    private void updateUserName(string userId, string name)
    {
        mDatabaseRef.Child("users").Child(userId).Child("username").SetValueAsync(name);
    }

    private void updateUserEmail(string userId, string email)
    {
        mDatabaseRef.Child("users").Child(userId).Child("email").SetValueAsync(email);
    }

    private void writeNewGame(int gameId, int maxPlayers, string gameName, int timePerTurn_minutes, bool hasStarted, int [] userIdArray, int[] userCharcterIndexArray, int[] dayEliminated, Vector3[] destination)
    {
        Global.Game game = new Global.Game(gameId, gameName, maxPlayers, timePerTurn_minutes, hasStarted, userIdArray, userCharcterIndexArray, dayEliminated, destination);
        string json = JsonUtility.ToJson(game);

        mDatabaseRef.Child("Game").Child("" + gameId).SetRawJsonValueAsync(json);
    }
}
