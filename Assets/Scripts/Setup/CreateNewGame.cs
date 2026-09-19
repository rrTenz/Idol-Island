using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Database;

public class CreateNewGame : MonoBehaviour
{
    public GameObject Canvas_First;

    public GameObject Canvas_Popup;
    public TMP_Text TMP_Text_PopupMessage;

    public Slider Slider_BeginningNumberOfTribes;
    public TMP_Text TMP_Text_BeginningNumberOfTribes;
    public Slider Slider_NumberOfCastaways;
    public TMP_Text TMP_Text_NumberOfCastaways;
    public Slider Slider_NumberOfSwaps;
    public TMP_Text TMP_Text_NumberOfSwaps;
    public Slider Slider_NumberAtMerge;
    public TMP_Text TMP_Text_NumberAtMerge;

    int numberOfCastaways;
    const int maxCastaways = 40;
    bool isMultiDay;
    int minutesPerTurn;
    int maxNumberOfTribes;

    public InputField InputFieldEnterGameName;

    public Button Button_RealTime;
    public Button Button_MultiDay;
    public Slider Slider_TimePerTurn;
    public TMP_Text TMP_Text_TimePerTurn;
    public Slider Slider_DaysOnIsland;
    public TMP_Text TMP_Text_DaysOnIsland;
    public Slider Slider_WhichCharacter;
    public TMP_Text TMP_Text_WhichCharacter;
    public TMP_Text TMP_Text_CharactersName;
    public Slider Slider_MinLevel;
    public TMP_Text TMP_Text_MinLevel;
    public Slider Slider_MaxLevel;
    public TMP_Text TMP_Text_MaxLevel;

    public Slider Slider_RotateCharacter;
    public GameObject PlayerHolderGameObject;
    public GameObject Custom_simple_human_prefab;

    public GameObject Canvas_Details;
    public GameObject Canvas_TextEntry;
    public InputField InputField_TextEntry;
    public GameObject Canvas_PickColor;
    public Slider Slider_ColorPicker;
    public Image Image_ColorPicker;
    public Image Image_ColorPicker_Frame;
    public GameObject Canvas_SliderWithText;
    public Image Image_Slider_BackgroundColor;
    public Image Image_Slider_FillColor;
    public Slider Slider_SliderWithText;
    public TMP_Text TMP_Text_SliderWithText;
    public Button Button_SliderWithText;
    public GameObject Canvas_Toggle;
    public Button Button_No;
    public Button Button_Yes;
    public TMP_Text TMP_Text_GameName;
    public TMP_Text TMP_Text_Instructions;
    public Button Button_Previous;
    public Button Button_Next;
    public TMP_Text TMP_Text_ParchmentSummaryText;
    public TMP_Text TMP_Text_ParchmentSummaryText2;
    public Button Button_CreateGame;

    public Text DebugText;

    public enum TriState : int
    {
        Unknown = -1,
        No = 0,
        Yes = 1
    }


    public enum DetailsState : int
    {
        WillTribesIncrease,
        MaxTribeCount,
        TribeNames,
        TribeColors,
        SwapAtHidden,
        SwapAt,
        TribesAtSwap,
        Confirm
    }

    long gameID;
    DetailsState detailsState;
    int detailsState_Index = 0;
    TriState willTribesIncrease;
    TriState showSwapAts;
    List<string> tribeNames_List;
    List<int> tribeColorsIndex_List;
    List<int> SwapsAt_List; //a list of when the swaps will occur (with this number of players left)
    List<int> TribesAtSwap_List; //a list of when the swaps will occur (with this number of players left)
    List<Global.TribeColor> TribeColor_List;

    // Start is called before the first frame update
    void Start()
    {
        Canvas_First.SetActive(true);
        Canvas_Details.SetActive(false);
        Canvas_Popup.SetActive(false);

        isMultiDay = true;

        Slider_BeginningNumberOfTribes.maxValue = Global.MAX_TRIBES;
        Slider_NumberOfCastaways.maxValue = maxCastaways / Slider_BeginningNumberOfTribes.value;

        Slider_TimePerTurn.value = 10;

        willTribesIncrease = TriState.Unknown;
        showSwapAts = TriState.Unknown;
        maxNumberOfTribes = -1;

        tribeNames_List = new List<string>();
        tribeColorsIndex_List = new List<int>();
        TribeColor_List = new List<Global.TribeColor>();
        SwapsAt_List = new List<int>();
        TribesAtSwap_List = new List<int>();

        TribeColor_List = Global.Init_TribeColor_List();
    }

    int BeginningNumberOfTribes_prev = 1;
    int minAtMerge_prev = 0;
    int maxAtMerge_prev = 0;
    int sliderWithText_value_prev = 0;
    // Update is called once per frame
    void Update()
    {
        DebugText.text = Global.DebugText;
        if (Canvas_First.activeSelf)
        {
            TMP_Text_BeginningNumberOfTribes.text = "Beginning # of Tribes: " + Slider_BeginningNumberOfTribes.value;
            maxNumberOfTribes = (int)Slider_BeginningNumberOfTribes.value + 1; //+1 for merged tribe

            Slider_NumberOfCastaways.maxValue = maxCastaways / Slider_BeginningNumberOfTribes.value;
            numberOfCastaways = (int)(Slider_BeginningNumberOfTribes.value * Slider_NumberOfCastaways.value);
            TMP_Text_NumberOfCastaways.text = "# of Players: " + numberOfCastaways;

            if (Slider_BeginningNumberOfTribes.value != BeginningNumberOfTribes_prev)
            {
                Slider_NumberOfCastaways.value = Slider_NumberOfCastaways.maxValue / 2;
            }
            BeginningNumberOfTribes_prev = (int)Slider_BeginningNumberOfTribes.value;

            int minAtMerge = (int)(numberOfCastaways * 0.4);
            int maxAtMerge = (int)(numberOfCastaways * 0.7);
            if (minAtMerge < 3)
                minAtMerge = 3;
            if (minAtMerge > maxAtMerge)
                maxAtMerge = minAtMerge;
            Slider_NumberAtMerge.minValue = minAtMerge;
            Slider_NumberAtMerge.maxValue = maxAtMerge;
            TMP_Text_NumberAtMerge.text = "# of Players at Merge: " + Slider_NumberAtMerge.value;

            if (minAtMerge != minAtMerge_prev || maxAtMerge != maxAtMerge_prev)
            {
                Slider_NumberAtMerge.value = (maxAtMerge - minAtMerge) / 2 + minAtMerge;
            }
            minAtMerge_prev = minAtMerge;
            maxAtMerge_prev = maxAtMerge;

            Slider_NumberOfSwaps.minValue = 0;
            Slider_NumberOfSwaps.maxValue = (int)(numberOfCastaways - Slider_NumberAtMerge.value - 1);
            TMP_Text_NumberOfSwaps.text = "# of Tribe Swaps: " + Slider_NumberOfSwaps.value;

            if (isMultiDay)
            {
                Button_RealTime.interactable = true;
                Button_MultiDay.interactable = false;

                //0.1 - 10 days
                Slider_TimePerTurn.minValue = 1;    //1 = 0.1 days
                Slider_TimePerTurn.maxValue = 100;  //100 = 10.0 days
                minutesPerTurn = (int)(Slider_TimePerTurn.value * 24 * 60 / 10);
                if(Slider_TimePerTurn.value / 10 == 1)
                    TMP_Text_TimePerTurn.text = "" + (Slider_TimePerTurn.value / 10) + " Day";
                else
                    TMP_Text_TimePerTurn.text = "" + (Slider_TimePerTurn.value / 10) + " Days";
            }
            else
            {
                Button_RealTime.interactable = false;
                Button_MultiDay.interactable = true;

                //1 - 100 minutes
                Slider_TimePerTurn.minValue = 1;
                Slider_TimePerTurn.maxValue = 100;
                minutesPerTurn = (int)Slider_TimePerTurn.value;
                if (Slider_TimePerTurn.value == 1)
                    TMP_Text_TimePerTurn.text = "" + (Slider_TimePerTurn.value) + " Min";
                else
                    TMP_Text_TimePerTurn.text = "" + (Slider_TimePerTurn.value) + " Mins";
            }

            Slider_DaysOnIsland.minValue = numberOfCastaways - 2;   //fastest season = 1 illimination every day, +1 final tribal council, - 3 because of final 3
            Slider_DaysOnIsland.maxValue = numberOfCastaways * 3;
            TMP_Text_DaysOnIsland.text = "Days on Island: " + Slider_DaysOnIsland.value;

            PlayerHolderGameObject.transform.eulerAngles = new Vector3(
                PlayerHolderGameObject.transform.eulerAngles.x,
                180 + Slider_RotateCharacter.value,
                PlayerHolderGameObject.transform.eulerAngles.z
            );

            int characterCount = Global.localUser.Characters.Count;
            Slider_WhichCharacter.maxValue = characterCount;
            Global.newCharacterOptions = Global.localUser.Characters[(int)Slider_WhichCharacter.value - 1];
            TMP_Text_WhichCharacter.text = "Character: " + Slider_WhichCharacter.value + " of " + Slider_WhichCharacter.maxValue;
            TMP_Text_CharactersName.text = Global.localUser.Characters[(int)Slider_WhichCharacter.value - 1].Name;

            //Size and Height
            double x, y, z;
            x = z = Global.localUser.Characters[(int)Slider_WhichCharacter.value - 1].SizeScale;
            y = Global.localUser.Characters[(int)Slider_WhichCharacter.value - 1].HeightScale;
            Custom_simple_human_prefab.transform.localScale = new Vector3((float)x, (float)y, (float)z);
            Global.newCharacterOptions.SizeScale = (float)x;
            Global.newCharacterOptions.HeightScale = (float)y;

            Slider_MinLevel.minValue = 1;
            Slider_MinLevel.maxValue = Global.MAX_LEVEL;
            TMP_Text_MinLevel.text = "" + Slider_MinLevel.value;

            Slider_MaxLevel.minValue = Slider_MinLevel.value;
            Slider_MaxLevel.maxValue = Global.MAX_LEVEL;
            TMP_Text_MaxLevel.text = "" + Slider_MaxLevel.value;
        }
        if(Canvas_Details.activeSelf)
        {
            if(detailsState == DetailsState.WillTribesIncrease)
            {
                Button_Previous.gameObject.SetActive(false);
                Button_Next.gameObject.SetActive(true);
                Button_CreateGame.gameObject.SetActive(false);

                if (Slider_NumberOfSwaps.value == 0 || Slider_BeginningNumberOfTribes.value == Global.MAX_TRIBES)
                {
                    DetailsState_next();
                }
                else
                {
                    Canvas_TextEntry.SetActive(false);
                    Canvas_PickColor.SetActive(false);
                    Canvas_SliderWithText.SetActive(false);
                    Canvas_Toggle.SetActive(true);

                    TMP_Text_Instructions.text = "Will there ever be more than " + Slider_BeginningNumberOfTribes.value + " tribes after a tribe swap?";
                    if(willTribesIncrease == TriState.Unknown)
                    {
                        Button_No.interactable = true;
                        Button_Yes.interactable = true;
                    }
                    else if(willTribesIncrease == TriState.No)
                    {
                        Button_No.interactable = false;
                        Button_Yes.interactable = true;
                    }
                    else //if (willTribesIncrease == TriState.Yes)
                    {
                        Button_No.interactable = true;
                        Button_Yes.interactable = false;
                    }
                }
            }
            else if (detailsState == DetailsState.Confirm)
            {
                Canvas_TextEntry.SetActive(false);
                Canvas_PickColor.SetActive(false);
                Canvas_SliderWithText.SetActive(false);
                Canvas_Toggle.SetActive(false);

                Button_Next.gameObject.SetActive(false);
                Button_CreateGame.gameObject.SetActive(true);

                TMP_Text_Instructions.text = "Please verify the information below is correct before creating your new game.";
            }
            else
            {
                Button_Previous.gameObject.SetActive(true);
                Button_Next.gameObject.SetActive(true);
                Button_CreateGame.gameObject.SetActive(false);

                if (detailsState == DetailsState.MaxTribeCount)
                {
                    if (willTribesIncrease == TriState.No || Slider_NumberOfSwaps.value == 0)
                    {
                        DetailsState_next();
                    }
                    else
                    {
                        Canvas_TextEntry.SetActive(false);
                        Canvas_PickColor.SetActive(false);
                        Canvas_SliderWithText.SetActive(true);
                        Canvas_Toggle.SetActive(false);

                        Button_SliderWithText.gameObject.SetActive(false);
                        Slider_SliderWithText.gameObject.SetActive(true);

                        TMP_Text_Instructions.text = "What is the highest number of tribes there will ever be during the game?";
                        Slider_SliderWithText.minValue = Slider_BeginningNumberOfTribes.value;
                        Slider_SliderWithText.maxValue = Global.MAX_TRIBES;
                        TMP_Text_SliderWithText.text = "" + Slider_SliderWithText.value;
                        if (Slider_SliderWithText.maxValue > numberOfCastaways)
                        {
                            Slider_SliderWithText.maxValue = numberOfCastaways;
                        }
                    }
                }
                else if (detailsState == DetailsState.TribeNames)
                {
                    if (Slider_NumberOfSwaps.value == 0)
                    {
                        if(detailsState_Index == 0)
                            Button_Previous.gameObject.SetActive(false);
                    }

                    Canvas_TextEntry.SetActive(true);
                    Canvas_PickColor.SetActive(false);
                    Canvas_SliderWithText.SetActive(false);
                    Canvas_Toggle.SetActive(false);

                    if(detailsState_Index == maxNumberOfTribes - 1)
                        TMP_Text_Instructions.text = "Please enter a name for the Merged Tribe";
                    else
                        TMP_Text_Instructions.text = "Please enter a name for Tribe " + (detailsState_Index + 1);
                }
                else if (detailsState == DetailsState.TribeColors)
                {
                    Canvas_TextEntry.SetActive(false);
                    Canvas_PickColor.SetActive(true);
                    Canvas_SliderWithText.SetActive(false);
                    Canvas_Toggle.SetActive(false);

                    if (TribeColor_List == null)
                    {
                        TribeColor_List = new List<Global.TribeColor>();
                    }

                    TMP_Text_Instructions.text = "Please choose a Color for " + tribeNames_List[detailsState_Index];
                    Slider_ColorPicker.minValue = 0;
                    Slider_ColorPicker.maxValue = TribeColor_List.Count - 1;
                    int val = TribeColor_List[(int)Slider_ColorPicker.value].hexCode;

                    string hex = val.ToString("X");
                    float Blue = val & 255;
                    float Green = (val >> 8) & 255;
                    float Red = (val >> 16) & 255;
                    Color tempColor = new Color((float)(Red / 255.0), (float)(Green / 255.0), (float)(Blue / 255.0));
                    Color tempColorFrame = new Color((float)((255 - Red) / 255.0), (float)((255 - Green) / 255.0), (float)((255 - Blue) / 255.0));
                    Image_ColorPicker.color = tempColor;
                    Image_ColorPicker_Frame.color = tempColorFrame;
                    Image_Slider_BackgroundColor.color = tempColor;
                    Image_Slider_FillColor.color = tempColor;
                    //TMP_Text_Instructions.text = "#" + hex;

                    if (TribeColor_List.Count <= detailsState_Index)
                    {
                        tribeColorsIndex_List.Add((int)Slider_ColorPicker.value);
                    }
                    else
                    {
                        tribeColorsIndex_List[detailsState_Index] = (int)Slider_ColorPicker.value;
                    }
                }
                else if (detailsState == DetailsState.SwapAtHidden)
                {
                    if (Slider_NumberOfSwaps.value == 0)
                    {
                        detailsState = DetailsState.Confirm;
                    }
                    else
                    {
                        Canvas_TextEntry.SetActive(false);
                        Canvas_PickColor.SetActive(false);
                        Canvas_SliderWithText.SetActive(false);
                        Canvas_Toggle.SetActive(true);

                        TMP_Text_Instructions.text = "Do you want to customize the tribe swaps? (If not, they will be secret and random)";
                        if (showSwapAts == TriState.Unknown)
                        {
                            Button_No.interactable = true;
                            Button_Yes.interactable = true;
                        }
                        else if (showSwapAts == TriState.No)
                        {
                            Button_No.interactable = false;
                            Button_Yes.interactable = true;
                        }
                        else //if (showSwapAts == TriState.Yes)
                        {
                            Button_No.interactable = true;
                            Button_Yes.interactable = false;
                        }
                    }
                }
                else if (detailsState == DetailsState.SwapAt)
                {
                    if (showSwapAts == TriState.No || Slider_NumberOfSwaps.value == 0)
                    {
                        detailsState = DetailsState.Confirm;
                        Button_Randomize_SwapsAt_List_click();
                    }
                    else
                    {
                        Canvas_TextEntry.SetActive(false);
                        Canvas_PickColor.SetActive(false);
                        Canvas_SliderWithText.SetActive(true);
                        Canvas_Toggle.SetActive(false);

                        Button_SliderWithText.gameObject.SetActive(true);

                        if (showSwapAts == TriState.No || Slider_NumberOfSwaps.value == 0)
                        {
                            DetailsState_next();
                        }

                        int wiggleRoom = numberOfCastaways - (int)Slider_NumberAtMerge.value - 1 - (int)Slider_NumberOfSwaps.value;
                        //if (wiggleRoom < 1)
                        if(false)   //todo
                            Slider_SliderWithText.gameObject.SetActive(false);
                        else
                        {
                            Slider_SliderWithText.gameObject.SetActive(true);
                            Slider_SliderWithText.minValue = 0;
                            Slider_SliderWithText.maxValue = wiggleRoom;

                            if (SwapsAt_List.Count == 0)
                            {
                                Button_Randomize_SwapsAt_List_click();
                            }

                            if ((int)Slider_SliderWithText.value != sliderWithText_value_prev)
                            {
                                int sliderDiff = (int)(Slider_SliderWithText.value - sliderWithText_value_prev);
                                int min = (int)Slider_NumberAtMerge.value + 1;  //e.g. 14 + 1 = 15
                                int max = numberOfCastaways - 1;    //e.g. 20 - 1 = 19

                                if (sliderDiff > 0)
                                {
                                    for (int i = SwapsAt_List.Count - 1; i >= 0; i--)
                                    {
                                        if (i == SwapsAt_List.Count - 1)
                                        {
                                            if (SwapsAt_List[i] - sliderDiff >= min)
                                                SwapsAt_List[i] -= sliderDiff;
                                        }
                                        else
                                        {
                                            if (SwapsAt_List[i] - sliderDiff > SwapsAt_List[i+1])
                                                SwapsAt_List[i] -= sliderDiff;
                                        }
                                    }
                                }
                                else if(sliderDiff < 0)
                                {
                                    for (int i = 0; i < SwapsAt_List.Count; i++)
                                    {
                                        if (i == 0)
                                        {
                                            if (SwapsAt_List[i] - sliderDiff <= max)
                                                SwapsAt_List[i] -= sliderDiff;
                                        }
                                        else
                                        {
                                            if (SwapsAt_List[i] - sliderDiff < SwapsAt_List[i - 1])
                                                SwapsAt_List[i] -= sliderDiff;
                                        }
                                    }
                                }
                            }
                            sliderWithText_value_prev = (int)Slider_SliderWithText.value;
                        }

                        if (Slider_NumberOfSwaps.value == 1)
                            TMP_Text_Instructions.text = "The Tribe Swap will occur when there are this many players left:";
                        else
                            TMP_Text_Instructions.text = "Tribe Swaps will occur when there are this many players left:";
                        TMP_Text_SliderWithText.text = "";
                        for (int i = 0; i < SwapsAt_List.Count; i++)
                        {
                            TMP_Text_SliderWithText.text += "" + SwapsAt_List[i];
                            if (i < SwapsAt_List.Count - 1)
                            {
                                TMP_Text_SliderWithText.text += ", ";
                            }
                        }
                    }
                }
                else if (detailsState == DetailsState.TribesAtSwap)
                {
                    if (showSwapAts == TriState.No || Slider_NumberOfSwaps.value == 0)
                    {
                        detailsState = DetailsState.Confirm;
                        Button_Randomize_SwapsAt_List_click();
                    }

                    Canvas_TextEntry.SetActive(false);
                    Canvas_PickColor.SetActive(false);
                    Canvas_SliderWithText.SetActive(true);
                    Canvas_Toggle.SetActive(false);

                    Button_SliderWithText.gameObject.SetActive(false);

                    TMP_Text_Instructions.text = "How many Tribes will there be at Tribe Swap " + (detailsState_Index + 1) + "? (" + SwapsAt_List[detailsState_Index] + " Players)";

                    init_TribesAtSwap_List();

                    Slider_SliderWithText.minValue = 2;
                    Slider_SliderWithText.maxValue = maxNumberOfTribes - 1; //-1 for merged tribe
                    int playerCount = SwapsAt_List[detailsState_Index];
                    if(maxNumberOfTribes * 2 > playerCount) //at least 2 players per tribe
                    {
                        Slider_SliderWithText.maxValue = playerCount / 2;
                    }

                    TMP_Text_SliderWithText.text = "" + Slider_SliderWithText.value;
                }
            }

            // Left Side Summary information (Basics)
            TMP_Text_ParchmentSummaryText.text =
                "Game Name: " + TMP_Text_GameName.text + "\n" +
                "My Character: " + TMP_Text_CharactersName.text + "\n" +
                "Beginning Tribe Count: " + Slider_BeginningNumberOfTribes.value + "\n" +
                "Players: " + numberOfCastaways + "\n" +
                "Players at Merge: " + Slider_NumberAtMerge.value + "\n" +
                "Tribe Swap Count: " + Slider_NumberOfSwaps.value + "\n" +
                "Max Time Per Turn: " + TMP_Text_TimePerTurn.text + "\n" +
                TMP_Text_DaysOnIsland.text + "\n" +
                "Min Player Level: " + Slider_MinLevel.value + "\n" +
                "Max Player Level: " + Slider_MaxLevel.value + "\n" +
                "";

            // Right Side Summary information (Details)
            string TribeNames = "";
            for (int i = 0; i < tribeNames_List.Count; i++)
            {
                if (i == tribeNames_List.Count - 1)
                    TribeNames += tribeNames_List[i];
                else
                    TribeNames += tribeNames_List[i] + ", ";
            }
            string TribesColors = "";
            for (int i = 0; i < tribeColorsIndex_List.Count; i++)
            {
                TribesColors += "\n  " + TribeColor_List[tribeColorsIndex_List[i]].name;
            }
            string SwapsHidden = "";
            string SwapAt_str = "";
            string TribesAtSwap_stt = "";
            if (showSwapAts == TriState.No)
            {
                SwapsHidden = "Tribe Swaps are Secret";
            }
            if (showSwapAts == TriState.Yes)
            {
                init_TribesAtSwap_List();
                SwapsHidden = "Tribe Swaps are Public Knowledge";
                if (SwapsAt_List.Count > 0)
                {
                    SwapAt_str = "Swap Tribes At: ";
                    TribesAtSwap_stt = "Tribe Count at Swap: ";
                }
                for (int i = 0; i < SwapsAt_List.Count; i++)
                {
                    if (i == SwapsAt_List.Count - 1)
                        SwapAt_str += SwapsAt_List[i];
                    else
                        SwapAt_str += SwapsAt_List[i] + ", ";

                    if (i == TribesAtSwap_List.Count - 1)
                        TribesAtSwap_stt += TribesAtSwap_List[i];
                    else
                        TribesAtSwap_stt += TribesAtSwap_List[i] + ", ";
                }
            }
            if (showSwapAts == TriState.Unknown)
                SwapsHidden = "";

            TMP_Text_ParchmentSummaryText2.text = 
                "Max Tribe Count: " + maxNumberOfTribes + "\n" +
                "Tribe List: " + TribeNames + "\n" +
                "Tribe Colors: " + TribesColors + "\n" +
                SwapsHidden + "\n" +
                SwapAt_str + "\n" +
                TribesAtSwap_stt + "\n" +
                "";
        }

        //Character Creation State
        if (createGameState == CreateGameState.Failed)
        {
            // Handle the error...
            Debug.Log("FireBaseDataBase Faulted");
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "Game Creation Failed\n\nPlease check your connection and try again";
        }
        else if (createGameState == CreateGameState.Success_Game)
        {
            //Update Character
            //Write to database
            Global.localUser.Games.Add(gameID);

            Global.User_Simple userTemp = new Global.User_Simple(Global.localUser);
            string json = JsonUtility.ToJson(userTemp);

            mDatabaseRef.Child("users").Child("" + Global.mainPlayerId).SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    createGameState = CreateGameState.Failed;
                }
                else if (task.IsCompleted)
                {
                    createGameState = CreateGameState.Success_Character_Update;
                }
            });

            createGameState = CreateGameState.CreatingGame;
        }
        else if (createGameState == CreateGameState.Success_Character_Update)
        {
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "Game Created Successfully";
        }
    }

    public void Button_Randomize_SwapsAt_List_click()
    {
        if (Slider_NumberOfSwaps.value > 0)
        {
            int min = (int)Slider_NumberAtMerge.value + 1;  //e.g. 14 + 1 = 15
            int max = numberOfCastaways - 1;    //e.g. 20 - 1 = 19

            SwapsAt_List.Clear();
            for (int i = max; i >= min; i--)
            {
                SwapsAt_List.Add(i);
            }

            System.Random rnd = new System.Random();
            while (SwapsAt_List.Count > Slider_NumberOfSwaps.value)
            {
                int random = rnd.Next(0, SwapsAt_List.Count);
                SwapsAt_List.RemoveAt(random);
            }

            Slider_SliderWithText.value = Slider_SliderWithText.maxValue / 2;
        }
    }

    void init_TribesAtSwap_List()
    {
        if (TribesAtSwap_List.Count != SwapsAt_List.Count)
        {
            TribesAtSwap_List.Clear();
            for (int i = 0; i < SwapsAt_List.Count; i++)
            {
                TribesAtSwap_List.Add((int)Slider_BeginningNumberOfTribes.value);
            }
        }
    }

    public void Button_DetailsState_Next_click()
    {
        BeforeDetailStateChange();

        if (detailsState == DetailsState.WillTribesIncrease && willTribesIncrease == TriState.Unknown)
        {
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "Please choose 'Yes' or 'No'";
        }
        else if (detailsState == DetailsState.SwapAtHidden && showSwapAts == TriState.Unknown)
        {
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "Please choose 'Yes' or 'No'";
        }
        else if(detailsState == DetailsState.TribeNames && (InputField_TextEntry.text.Length < 2 || InputField_TextEntry.text.Length > 20))
        {
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "Please choose a Tribe Name with 2-20 letters";
        }
        else if (detailsState == DetailsState.TribeNames && tribeNameHasBeenUsed())
        {
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "That Tribe Name has already been used.\nPlease pick another Name.";
        }
        else if (detailsState == DetailsState.TribeColors && tribeColorHasBeenUsed())
        {
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "That Tribe Color has already been used.\nPlease pick another Color.";
        }
        else if ((detailsState == DetailsState.TribeNames || detailsState == DetailsState.TribeColors) && detailsState_Index < maxNumberOfTribes - 1)
        {
            detailsState_Index++;
            InputField_TextEntry.text = "";
        }
        else if ((detailsState == DetailsState.TribesAtSwap) && detailsState_Index < SwapsAt_List.Count - 1)
        {
            detailsState_Index++;
            init_TribesAtSwap_List();
            if (detailsState_Index < TribesAtSwap_List.Count)
            {
                Slider_SliderWithText.value = TribesAtSwap_List[detailsState_Index];
            }
        }
        else
        {
            if (detailsState == DetailsState.WillTribesIncrease && willTribesIncrease == TriState.No)
            {
                maxNumberOfTribes = (int)Slider_BeginningNumberOfTribes.value + 1;  //+1 for merged tribe
            }
            if (detailsState == DetailsState.MaxTribeCount)
            {
                maxNumberOfTribes = (int)Slider_SliderWithText.value + 1;     //+1 for merged tribe
            }

            DetailsState_next();

            if(detailsState == DetailsState.TribeColors || detailsState == DetailsState.TribesAtSwap)
            {
                detailsState_Index = 0;
            }
        }

        AfterDetailStateChange();
    }

    public void Button_DetailsState_Previous_click()
    {
        BeforeDetailStateChange();

        if ((detailsState == DetailsState.TribeNames || detailsState == DetailsState.TribeColors) && detailsState_Index > 0)
        {
            detailsState_Index--;
            if (detailsState_Index < tribeNames_List.Count)
            {
                InputField_TextEntry.text = "" + tribeNames_List[detailsState_Index];
            }
        }
        else if ((detailsState == DetailsState.TribesAtSwap) && detailsState_Index > 0)
        {
            detailsState_Index--;
            init_TribesAtSwap_List();
            if (detailsState_Index < TribesAtSwap_List.Count)
            {
                Slider_SliderWithText.value = TribesAtSwap_List[detailsState_Index];
            }
        }
        else
        {
            if (detailsState == DetailsState.MaxTribeCount)
            {
                maxNumberOfTribes = (int)Slider_SliderWithText.value;
            }

            DetailsState_previous();

            if (detailsState == DetailsState.MaxTribeCount && willTribesIncrease == TriState.No)
            {
                DetailsState_previous();
            }

            if (detailsState == DetailsState.TribeNames)
            {
                detailsState_Index = maxNumberOfTribes - 1;
            }

            if (detailsState == DetailsState.Confirm)
            {
                detailsState_Index = SwapsAt_List.Count - 1;
            }

            if (detailsState == DetailsState.MaxTribeCount)
            {
                Slider_SliderWithText.value = maxNumberOfTribes;
            }

            if ((detailsState == DetailsState.SwapAt || detailsState == DetailsState.TribesAtSwap) && showSwapAts == TriState.No)
            {
                detailsState = DetailsState.SwapAtHidden;
            }

            while((int)Slider_NumberOfSwaps.value == 0 && (detailsState == DetailsState.SwapAt || detailsState == DetailsState.SwapAtHidden || detailsState == DetailsState.TribesAtSwap))
            {
                DetailsState_previous();
            }

        }

        AfterDetailStateChange();
    }

    public void BeforeDetailStateChange()
    {
        if (detailsState == DetailsState.MaxTribeCount)
        {
            maxNumberOfTribes = (int)Slider_SliderWithText.value;
            detailsState_Index = 0;
        }
        else if (detailsState == DetailsState.TribeNames)
        {
            if (detailsState_Index < 0)
                detailsState_Index = 0;
            if (detailsState_Index >= maxNumberOfTribes)
                detailsState_Index = maxNumberOfTribes - 1;

            if (tribeNames_List.Count <= detailsState_Index)
            {
                tribeNames_List.Add("" + InputField_TextEntry.text);
            }
            else
            {
                tribeNames_List[detailsState_Index] = "" + InputField_TextEntry.text;
            }
        }
        else if (detailsState == DetailsState.TribesAtSwap)
        {
            if (detailsState_Index < 0)
                detailsState_Index = 0;
            if (detailsState_Index >= SwapsAt_List.Count)
                detailsState_Index = SwapsAt_List.Count - 1;

            TribesAtSwap_List[detailsState_Index] = (int)Slider_SliderWithText.value;
        }
    }

    public void AfterDetailStateChange()
    {
        if (detailsState == DetailsState.MaxTribeCount)
        {
            Slider_SliderWithText.value = maxNumberOfTribes;
        }
        else if (detailsState == DetailsState.TribeNames && tribeNames_List.Count > detailsState_Index)
        {
            InputField_TextEntry.text = tribeNames_List[detailsState_Index];
        }
        else if (detailsState == DetailsState.TribeColors)
        {
            while(detailsState_Index >= tribeColorsIndex_List.Count)
            {
                tribeColorsIndex_List.Add(0);
            }
            Slider_ColorPicker.value = tribeColorsIndex_List[detailsState_Index];
        }
    }

    public bool tribeNameHasBeenUsed()
    {
        if (tribeNames_List.Count != tribeNames_List.Distinct().Count())
            return true;

        return false;
    }

    public bool tribeColorHasBeenUsed()
    {
        if (tribeColorsIndex_List.Count != tribeColorsIndex_List.Distinct().Count())
            return true;

        return false;
    }

    public void Button_No_click()
    {
        if (detailsState == DetailsState.WillTribesIncrease)
        {
            willTribesIncrease = TriState.No;
        }
        else if (detailsState == DetailsState.SwapAtHidden)
        {
            showSwapAts = TriState.No;
        }
    }

    public void Button_Yes_click()
    {
        if (detailsState == DetailsState.WillTribesIncrease)
        {
            willTribesIncrease = TriState.Yes;
        }
        else if (detailsState == DetailsState.SwapAtHidden)
        {
            showSwapAts = TriState.Yes;
        }
    }

    void DetailsState_next()
    {
        detailsState += 1;
        if (detailsState > DetailsState.Confirm)
            detailsState = DetailsState.Confirm;
    }

    void DetailsState_previous()
    {
        detailsState -= 1;

        if (detailsState < DetailsState.WillTribesIncrease)
            detailsState = DetailsState.WillTribesIncrease;
    }

    public void Button_GoToDetails_click()
    {

        if (InputFieldEnterGameName.text.Length < 2 || InputFieldEnterGameName.text.Length > 20)
        {
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "Please enter a valid name for the game that is 2-20 letters long.";
        }
        else
        {
            Canvas_First.SetActive(false);
            Canvas_Details.SetActive(true);
            Canvas_Popup.SetActive(false);
            TMP_Text_GameName.text = InputFieldEnterGameName.text;
            detailsState = DetailsState.WillTribesIncrease;
            detailsState_Index = 0;
            if (detailsState_Index < tribeNames_List.Count)
            {
                InputField_TextEntry.text = "" + tribeNames_List[detailsState_Index];
            }
        }
    }

    public void Button_CancelNewGame_click()
    {
        SceneManager.LoadScene((int)Global.Screen.Screen_Menu);
    }

    public void Button_DismissPopup_click()
    {
        Canvas_Popup.SetActive(false);


        //Game Creation State
        if (createGameState == CreateGameState.Failed)
        {
            createGameState = CreateGameState.Idle;
        }
        else if (createGameState == CreateGameState.Success_Character_Update)
        {
            createGameState = CreateGameState.Finish;
            SceneManager.LoadScene((int)Global.Screen.Screen_Menu);
        }
    }

    public void Button_BackToFirst_click()
    {
        Canvas_First.SetActive(true);
        Canvas_Details.SetActive(false);
        Canvas_Popup.SetActive(false);
    }

    private Vector3[] get_randomDestinationArray(int count, int min, int max)
    {
        Vector3[] destinationArray = new Vector3[count];

        System.Random random = new System.Random();

        for (int i = 0; i < count; i++)
        {
            destinationArray[i] = new Vector3(random.Next(min, max) + (float)(random.Next(10) / 10.0), Global.DEFAULT_BAD_Y, random.Next(min, max) + (float)(random.Next(10) / 10.0));
        }
        return destinationArray;
    }

    public void Button_RealTime_click()
    {
        isMultiDay = false;
    }

    public void Button_MultiDay_click()
    {
        isMultiDay = true;
    }

    public void Button_DecrementTime_click()
    {
        if (Slider_TimePerTurn.value > Slider_TimePerTurn.minValue)
        {
            Slider_TimePerTurn.value -= 1;
            if (Slider_TimePerTurn.value < Slider_TimePerTurn.minValue)
            {
                Slider_TimePerTurn.value = Slider_TimePerTurn.minValue;
            }
        }
    }

    public void Button_IncrementTime_click()
    {
        if (Slider_TimePerTurn.value < Slider_TimePerTurn.maxValue)
        {
            Slider_TimePerTurn.value += 1;
            if (Slider_TimePerTurn.value > Slider_TimePerTurn.maxValue)
            {
                Slider_TimePerTurn.value = Slider_TimePerTurn.maxValue;
            }
        }
    }

    public void Button_DecrementDays_click()
    {
        if (Slider_DaysOnIsland.value > Slider_DaysOnIsland.minValue)
        {
            Slider_DaysOnIsland.value -= 1;
            if (Slider_DaysOnIsland.value < Slider_DaysOnIsland.minValue)
            {
                Slider_DaysOnIsland.value = Slider_DaysOnIsland.minValue;
            }
        }
    }

    public void Button_IncrementDays_click()
    {
        if (Slider_DaysOnIsland.value < Slider_DaysOnIsland.maxValue)
        {
            Slider_DaysOnIsland.value += 1;
            if (Slider_DaysOnIsland.value > Slider_DaysOnIsland.maxValue)
            {
                Slider_DaysOnIsland.value = Slider_DaysOnIsland.maxValue;
            }
        }
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

    public void Button_Decrement_MinPlayerLevel_click()
    {
        if (Slider_MinLevel.value > Slider_MinLevel.minValue)
        {
            Slider_MinLevel.value -= 1;
            if (Slider_MinLevel.value < Slider_MinLevel.minValue)
            {
                Slider_MinLevel.value = Slider_MinLevel.minValue;
            }
        }
    }

    public void Button_Increment_MinPlayerLevel_click()
    {
        if (Slider_MinLevel.value < Slider_MinLevel.maxValue)
        {
            Slider_MinLevel.value += 1;
            if (Slider_MinLevel.value > Slider_MinLevel.maxValue)
            {
                Slider_MinLevel.value = Slider_MinLevel.maxValue;
            }
        }
    }

    public void Button_Decrement_MaxPlayerLevel_click()
    {
        if (Slider_MaxLevel.value > Slider_MaxLevel.minValue)
        {
            Slider_MaxLevel.value -= 1;
            if (Slider_MaxLevel.value < Slider_MaxLevel.minValue)
            {
                Slider_MaxLevel.value = Slider_MaxLevel.minValue;
            }
        }
    }

    public void Button_Increment_MaxPlayerLevel_click()
    {
        if (Slider_MaxLevel.value < Slider_MaxLevel.maxValue)
        {
            Slider_MaxLevel.value += 1;
            if (Slider_MaxLevel.value > Slider_MaxLevel.maxValue)
            {
                Slider_MaxLevel.value = Slider_MaxLevel.maxValue;
            }
        }
    }

    public void Button_Decrement_Color_click()
    {
        if (Slider_ColorPicker.value > Slider_ColorPicker.minValue)
        {
            Slider_ColorPicker.value -= 1;
            if (Slider_ColorPicker.value < Slider_ColorPicker.minValue)
            {
                Slider_ColorPicker.value = Slider_ColorPicker.minValue;
            }
        }
    }

    public void Button_Increment_Color_click()
    {
        if (Slider_ColorPicker.value < Slider_ColorPicker.maxValue)
        {
            Slider_ColorPicker.value += 1;
            if (Slider_ColorPicker.value > Slider_ColorPicker.maxValue)
            {
                Slider_ColorPicker.value = Slider_ColorPicker.maxValue;
            }
        }
    }

    DatabaseReference mDatabaseRef;
    private enum CreateGameState : int
    {
        Idle,
        CreatingGame,
        Success_Game,
        Success_Character_Update,
        Failed,
        Finish
    }
    private CreateGameState createGameState = CreateGameState.Idle;
    public void Button_FinishNewGameCreation_click()
    {
        Global.Game newGame = new Global.Game();
        DateTime utcNow = DateTime.UtcNow;
        newGame.GameId = 0;
        newGame.GameId += (long)utcNow.Year *   10000000000000;
        newGame.GameId += (long)utcNow.Month *  100000000000;
        newGame.GameId += (long)utcNow.Day *    1000000000;
        newGame.GameId += (long)utcNow.Hour *   10000000;
        newGame.GameId += (long)utcNow.Minute * 100000;
        newGame.GameId += (long)utcNow.Second * 1000;
        newGame.GameId += (long)utcNow.Millisecond;

        gameID = newGame.GameId;

        newGame.MaxPlayers = numberOfCastaways;
        newGame.gameName = TMP_Text_GameName.text;
        newGame.TimePerTurn_minutes = minutesPerTurn;
        newGame.hasStarted = false;
        newGame.userIdArray = new string[numberOfCastaways];
        newGame.userIdArray[0] = Global.mainPlayerId;
        newGame.userCharacterIndexArray = new int[numberOfCastaways];
        newGame.userCharacterIndexArray[0] = (int)Slider_WhichCharacter.value - 1;
        newGame.dayEliminated = new int[numberOfCastaways];
        for (int i = 0; i < numberOfCastaways; i++)
        {
            newGame.dayEliminated[i] = -1;
        }
        newGame.destinationArray = get_randomDestinationArray(numberOfCastaways, -10, 10); //TODO put characters on correct island

        newGame.playerCount = 1;

        newGame.beginningNumerOfTribes = (int)Slider_BeginningNumberOfTribes.value;
        newGame.playersAtMerge = (int)Slider_NumberAtMerge.value;
        newGame.numberOfSwaps = (int)Slider_NumberOfSwaps.value;

        newGame.isRealTime = !isMultiDay;
        newGame.daysOnIsland = (int)Slider_DaysOnIsland.value;
        newGame.gameCreatedBy_playerId = Global.mainPlayerId;
        newGame.gameCreatedBy_characterName = TMP_Text_CharactersName.text;
        newGame.minPlayerLevel = (int)Slider_MinLevel.value;
        newGame.maxPlayerLevel = (int)Slider_MaxLevel.value;

        if (willTribesIncrease == TriState.Yes)
            newGame.tribeCountIncreases = true;
        else
            newGame.tribeCountIncreases = false;
        newGame.maxTribeCount = tribeNames_List.Count;
        newGame.tribeNamesArray = new string[tribeNames_List.Count];
        for (int i = 0; i < tribeNames_List.Count; i++)
        {
            newGame.tribeNamesArray[i] = tribeNames_List[i];
        }
        newGame.tribeColorIndexArray = new int[tribeColorsIndex_List.Count];
        for (int i = 0; i < tribeColorsIndex_List.Count; i++)
        {
            newGame.tribeColorIndexArray[i] = tribeColorsIndex_List[i];
        }
        if (showSwapAts == TriState.Yes)
            newGame.showSwapDetails = true;
        else
            newGame.showSwapDetails = false;
        newGame.tribeSwapsAtArray = new int[SwapsAt_List.Count];
        for (int i = 0; i < SwapsAt_List.Count; i++)
        {
            newGame.tribeSwapsAtArray[i] = SwapsAt_List[i];
        }
        newGame.tribesCountArray = new int[TribesAtSwap_List.Count];
        for (int i = 0; i < TribesAtSwap_List.Count; i++)
        {
            newGame.tribesCountArray[i] = TribesAtSwap_List[i];
        }

        //**** Create Event By Day Array ****
        newGame.eventByDayArray = new byte[newGame.daysOnIsland + 1];
        int playersLeft = newGame.MaxPlayers;
        float daysPerElimination = ((float)newGame.daysOnIsland - 1) / ((float)newGame.MaxPlayers - 3);   //-1 for final tribal council, minus 3 players for final 3
        float ImmunityDay_offset = (float)0.5 + daysPerElimination;
        float RewardDay_offset = (float)-0.5 + daysPerElimination;
        int nextSwap_Index = 0;
        int nextSwap_PlayerCount = -1;
        bool addSwap = false;
        bool addMerge = false;

        if (newGame.tribeSwapsAtArray.Length > nextSwap_Index)
        {
            nextSwap_PlayerCount = newGame.tribeSwapsAtArray[nextSwap_Index];
        }
        else
        {
            nextSwap_PlayerCount = -1;
        }

        if(RewardDay_offset < 1)
            RewardDay_offset += daysPerElimination;


        for (int i = 0; i < newGame.daysOnIsland + 1; i++)
        {
            if (i == 0)  //staging/idle day (game has not started)
            {
                newGame.eventByDayArray[i] |= (byte)Global.EventMask.Nothing;
            }
            else if (i == newGame.daysOnIsland)
            {
                if(addMerge)
                {
                    newGame.eventByDayArray[i] |= (byte)Global.EventMask.Merge;
                    addMerge = false;
                }
                newGame.eventByDayArray[i] |= (byte)Global.EventMask.Final_Tribal;
            }
            else
            {
                if (addSwap)
                {
                    newGame.eventByDayArray[i] |= (byte)Global.EventMask.Swap;
                    nextSwap_Index++;
                    addSwap = false;

                    if (newGame.tribeSwapsAtArray.Length > nextSwap_Index)
                    {
                        nextSwap_PlayerCount = newGame.tribeSwapsAtArray[nextSwap_Index];
                    }
                    else
                    {
                        nextSwap_PlayerCount = -1;
                    }
                }

                if (addMerge)
                {
                    newGame.eventByDayArray[i] |= (byte)Global.EventMask.Merge;
                    addMerge = false;
                }

                if (i == (int)RewardDay_offset)
                {
                    newGame.eventByDayArray[i] |= (byte)Global.EventMask.Reward;
                    RewardDay_offset += daysPerElimination;
                }
                if (i == (int)ImmunityDay_offset)
                {
                    newGame.eventByDayArray[i] |= (byte)Global.EventMask.Immunity;
                    playersLeft--;
                    ImmunityDay_offset += daysPerElimination;

                    if (playersLeft == nextSwap_PlayerCount)
                    {
                        addSwap = true;
                    }

                    if (playersLeft == newGame.playersAtMerge)
                    {
                        addMerge = true;
                    }
                }
            }
        }


        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        //Write to database
        string json = JsonUtility.ToJson(newGame);

        mDatabaseRef.Child("Game").Child("" + newGame.GameId).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                createGameState = CreateGameState.Failed;
            }
            else if (task.IsCompleted)
            {
                createGameState = CreateGameState.Success_Game;
            }
        });

        Canvas_Popup.SetActive(true);
        TMP_Text_PopupMessage.text = "Creating Game...";
        createGameState = CreateGameState.CreatingGame;

    }
}
