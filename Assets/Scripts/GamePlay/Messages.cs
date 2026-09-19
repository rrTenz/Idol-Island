using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using System;

//https://www.youtube.com/watch?v=HfVuc_uUa8Y
public class Messages : MonoBehaviour
{
    private DatabaseReference mDatabaseRef;

    public TMP_InputField InputField_MessageToSend;
    public Text Text_MessageBox;
    public Scrollbar Scrollbar_RightSide;

    //Chat volume adjust
    public Slider Slider_ChatVolumeAdjust;
    public GameObject GameObject_ChatRadius;
    float chatDistance = 20.0f;
    public Button Button_LockDistanceSlider;
    public float chatRadiusHeight = 1.26f;

    public TMP_Text TMP_Text_Debug;

    bool haveInitChatListener;

    bool moveChatScrollToBottom;

    // Start is called before the first frame update
    void Start()
    {
        Text_MessageBox.text = "";
        sliderSelected = 0;

        //String refStr = "Game/" + Global.currentGame.GameId + "/destinationArray";
        //FirebaseDatabase.DefaultInstance
        //  .GetReference(refStr)
        //  .ValueChanged += HandleNewMessages;

        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        haveInitChatListener = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Global.characterNameArray == null)
            return; //wait for characters to finish initializing before continuing

        if (!haveInitChatListener)
        {
            haveInitChatListener = true;
            ListenForMessage();
        }

        chatDistance = Slider_ChatVolumeAdjust.value;
        GameObject_ChatRadius.transform.localScale = new Vector3(chatDistance * 2, 0.1f, chatDistance * 2);

        //float dist = Vector3.Distance(Global.PlayerObjectList[0].PlayerBody.transform.position, Global.PlayerObjectList[1].PlayerBody.transform.position);
        //TMP_Text_Debug.text = "" + Global.PlayerObjectList[0].PlayerBody.transform.position + "\n" + Global.PlayerObjectList[1].PlayerBody.transform.position + "\n" + dist + "\n";

        if (lockDistanceSliderOn || InputField_MessageToSend.isFocused == true || sliderSelected > 0)
        {
            GameObject_ChatRadius.SetActive(true);
            Slider_ChatVolumeAdjust.gameObject.SetActive(true);
            //GameObject_ChatRadius.transform.position = Global.PlayerObjectList[Global.mainPlayerIndex].PlayerBody.transform.position;
            //TMP_Text_Debug.text += " (IF selected)";
        }
        else if (InputField_MessageToSend.isFocused == false)
        {
            if (sliderSelected == 0)
            {
                GameObject_ChatRadius.SetActive(false);
                Slider_ChatVolumeAdjust.gameObject.SetActive(false);
            }
        }
        if (InputField_MessageToSend.isFocused == false && sliderSelected > 0)
            sliderSelected--;


        if (moveChatScrollToBottom && Scrollbar_RightSide.value > 0)
        {
            Scrollbar_RightSide.value = 0;
            moveChatScrollToBottom = false;
        }
    }

    private void OnDestroy()
    {
        // Replace EventManager.onEvent with whatever class is sending event messages to subscribers

        // Unsubscribe from event(s)
        String refStr = "Game/" + Global.currentGame.GameId + "/messageArray";
        FirebaseDatabase.DefaultInstance
          .GetReference(refStr)
          .ValueChanged -= HandleNewMessages;

        // And stop all coroutines
        StopAllCoroutines();

        Text_MessageBox = null;
    }

    int sliderSelected;
    public void SliderWasSelected()
    {
        sliderSelected = 999999999;     //leave on for a "long" time
    }
    public void SliderWasReleased()
    {
        InputField_MessageToSend.Select();
        sliderSelected = 10;    //hold for a moment to avoid flash when switching to InputField_MessageToSend.Select();
    }

    public void ButtonClick_SendMessage()
    {
        if (InputField_MessageToSend.text.Length == 0)
            return;

        int charIndex = Global.getMainPlayerCharacterIndex_currentGame();

        sliderSelected = 0;

        //get list of users that heard the message
        List<string> recipientIds = new List<string>();
        for(int i = 0; i < Global.playerPositionArray_currentGame.Length; i++)
        {
            if(Vector3.Distance(Global.playerPositionArray_currentGame[Global.mainPlayerIndex], Global.playerPositionArray_currentGame[i]) <= chatDistance)
            {
                recipientIds.Add(Global.currentGame.userIdArray[i]);
            }
        }

        PostMessage(new Message(Global.localUser.Characters[charIndex].Name, InputField_MessageToSend.text, recipientIds));
    }


    void HandleNewMessages(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = args.Snapshot;
        Message messageJSON = StringSerializationAPI.Deserialize(typeof(Message), args.Snapshot.GetRawJsonValue()) as Message;

        bool isFirstMessage = true;
        foreach (var message in snapshot.Children)
        {
            string senderName = "";
            string text = "";
            List<string> recipientIds = new List<string>();
            foreach (var val in message.Children)
            {
                if (val.Key.Equals("senderName"))
                    senderName = (String)val.Value;
                else if (val.Key.Equals("text"))
                    text = (String)val.Value;
                else if (val.Key.Equals("recipientIds"))
                {
                    foreach (var userID in val.Children)
                    {
                        recipientIds.Add((String)userID.Value);
                    }
                }

            }

            if (recipientIds.Contains(Global.mainPlayerId))
            {
                string heardBy_str = "";
                foreach (string id in recipientIds)
                {
                    int playerIndex = -1;
                    for (int i = 0; i < Global.currentGame.userIdArray.Length; i++)
                    {
                        if(Global.currentGame.userIdArray[i] == id && id != Global.mainPlayerId)
                        {
                            playerIndex = i;
                            break;
                        }
                    }

                    if(playerIndex >= 0)
                    {
                        if(heardBy_str.Length == 0)
                        {
                            heardBy_str = "(heard by you, " + Global.characterNameArray[playerIndex];
                        }
                        else
                        {
                            heardBy_str += ", " + Global.characterNameArray[playerIndex];
                        }
                    }
                }

                if(heardBy_str.Length == 0)
                {
                    heardBy_str = "(Heard by nobody)";
                }
                else
                {
                    heardBy_str += ")";
                }


                string message_temp = "-- " + senderName + " said: " + text + "\n   " + heardBy_str + "\n";

                if (isFirstMessage)
                {
                    Text_MessageBox.text = message_temp;
                    isFirstMessage = false;
                }
                else
                {
                    Text_MessageBox.text += message_temp;
                }

                //change how far the players chats travel
            }
        }

        moveChatScrollToBottom = true;
        Scrollbar_RightSide.value = 0;
        InputField_MessageToSend.text = "";
    }

    private void PostMessage(Message message)
    {
        var messageJSON = StringSerializationAPI.Serialize(typeof(Message), message);
        mDatabaseRef.Child("Game").Child("" + Global.currentGame.GameId).Child("messageArray").Push().SetRawJsonValueAsync(messageJSON).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
                SendMessage_fallback();
            else
                SendMessage_callback();
        });
    }

    private void ListenForMessage()
    {

        String refStr = "Game/" + Global.currentGame.GameId + "/messageArray";
        FirebaseDatabase.DefaultInstance
          .GetReference(refStr)
          .ValueChanged += HandleNewMessages;
    }

    private void SendMessage_callback()
    {

    }

    private void SendMessage_fallback()
    {

    }

    bool lockDistanceSliderOn = false;
    public void ButtonClick_Lock_DistanceSlider()
    {
        lockDistanceSliderOn = !lockDistanceSliderOn;

        if(lockDistanceSliderOn)
        {
            Button_LockDistanceSlider.image.color = Color.green;
        }
        else
        {
            Button_LockDistanceSlider.image.color = Color.white;
        }
    }
}
