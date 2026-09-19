using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Database;

public class CreateCharacter : MonoBehaviour
{
    public GameObject Canvas_Appearance;
    public GameObject Canvas_Attributes;
    public GameObject Canvas_Popup;

    public GameObject Custom_simple_human_prefab;

    public Button Button_Male;
    public Button Button_Female;

    public InputField InputFieldEnterPlayerName;

    public Button Button_Suit;
    public Button Button_Custom;

    public Slider Slider_RotateCharacter;
    public GameObject PlayerHolderGameObject;


    public Slider Slider_Skin;
    public Slider Slider_Hair;
    public Slider Slider_Hair_Color;
    //public Slider Slider_Hair_Cut;
    public Slider Slider_Beard;
    //public Slider Slider_Beard_Color;
    public Slider Slider_Size;
    public Slider Slider_Height;

    public GameObject GO_SuitHolder;
    public Slider Slider_Which_Suit;
    public Slider Slider_Banker_Texture;

    public GameObject GO_CustomHolder;
    public Slider Slider_Shirt_Style;
    public Slider Slider_Design;
    public Slider Slider_Pants;
    public Slider Slider_Shoes;
    public Slider Slider_Shoe_Color;
    public Slider Slider_Glasses;
    public Slider Slider_Chain;
    public Slider Slider_Scarf;


    //Attributes Canvas
    public TMP_Text TMP_Text_PlayersName;
    public TMP_Text TMP_Text_PointsRemaining;

    public Slider Slider_Strength;
    public Slider Slider_Stamina;
    public Slider Slider_Athletic;
    public Slider Slider_Speed;
    public Slider Slider_Swimming;
    public Slider Slider_Puzzles;
    public Slider Slider_CampSkills;

    private const int max_Points = 20;
    private int totalUsedPoints = 0;


    //Popup Canvas
    public TMP_Text TMP_Text_PopupMessage;

    DatabaseReference mDatabaseRef;


    private enum CreateCharacterState : int
    {
        Idle = 0,
        CreatingCharacter = 1,
        Success = 2,
        Failed = 3,
        Finish = 4
    }

    private CreateCharacterState createCharacterState = CreateCharacterState.Idle;

    // Start is called before the first frame update
    void Start()
    {
        Global.creatingCharacter = true;
        Global.newCharacterOptions = new Global.CharacterOptions();

        Canvas_Appearance.SetActive(true);
        Canvas_Attributes.SetActive(false);
        Canvas_Popup.SetActive(false);

        createCharacterState = CreateCharacterState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        if (Global.newCharacterOptions == null)
            return;

        if(Global.newCharacterOptions.MaleFemale == 0)  //if the character is male
        {
            Button_Male.interactable = false;
            Button_Female.interactable = true;
            //Slider_Hair_Cut.gameObject.SetActive(true);
            Slider_Beard.gameObject.SetActive(true);
            //Slider_Beard_Color.gameObject.SetActive(true);
            Slider_Hair.maxValue = 3;

            Global.newCharacterOptions.HairMale = (int)Slider_Hair.value;
        }
        else if (Global.newCharacterOptions.MaleFemale == 1)  //if the character is female
        {
            Button_Male.interactable = true;
            Button_Female.interactable = false;
            //Slider_Hair_Cut.gameObject.SetActive(false);
            Slider_Beard.gameObject.SetActive(false);
            //Slider_Beard_Color.gameObject.SetActive(false);
            Slider_Hair.maxValue = 1;

            Global.newCharacterOptions.HairFemale = (int)Slider_Hair.value;
        }

        //Size and Height
        double x, y, z;
        x = z = Slider_Size.value * (1.0/50.0) + 0.5;
        y = Slider_Height.value * (1.0 / 166.0) + 0.6;
        Custom_simple_human_prefab.transform.localScale = new Vector3((float)x, (float)y, (float)z);
        Global.newCharacterOptions.SizeScale = (float)x;
        Global.newCharacterOptions.HeightScale = (float)y;

        if (Global.newCharacterOptions.SuitOrCloth == 0)  //if doing Suit
        {
            Button_Suit.interactable = false;
            Button_Custom.interactable = true;
            GO_SuitHolder.SetActive(true);
            GO_CustomHolder.SetActive(false);
        }
        else if (Global.newCharacterOptions.SuitOrCloth == 1)  //if doing Custom
        {
            Button_Suit.interactable = true;
            Button_Custom.interactable = false;
            GO_SuitHolder.SetActive(false);
            GO_CustomHolder.SetActive(true);
        }

        PlayerHolderGameObject.transform.eulerAngles = new Vector3(
            PlayerHolderGameObject.transform.eulerAngles.x,
            180 + Slider_RotateCharacter.value,
            PlayerHolderGameObject.transform.eulerAngles.z
        );


        Global.newCharacterOptions.SkinColor = (int)Slider_Skin.value;
        Global.newCharacterOptions.HairColor = (int)Slider_Hair_Color.value;
        Global.newCharacterOptions.HairCut = 0; // (int)Slider_Hair_Cut.value;
        Global.newCharacterOptions.Beard = (int)Slider_Beard.value;
        Global.newCharacterOptions.BeardTexture = (int)Slider_Hair_Color.value; //(int)Slider_Beard_Color.value;


        Global.newCharacterOptions.WhichSuit = (int)Slider_Which_Suit.value;
        if(Global.newCharacterOptions.WhichSuit == 0)   //banker suit
        {
            Slider_Banker_Texture.gameObject.SetActive(true);
            Global.newCharacterOptions.BankerTexture = (int)Slider_Banker_Texture.value;
        }
        else
        {
            Slider_Banker_Texture.gameObject.SetActive(false);
        }

        if ((int)Slider_Shirt_Style.value == 4)  //jacket
        {
            Global.newCharacterOptions.Top = 3; //tank top under jacket
            Global.newCharacterOptions.TanktopTexture = 0;
            Global.newCharacterOptions.Jacket = 1;
            Slider_Design.maxValue = 11;
            Global.newCharacterOptions.JacketTexture = (int)Slider_Design.value;
        }
        else
        {
            Global.newCharacterOptions.Jacket = 0;
            Global.newCharacterOptions.Top = (int)Slider_Shirt_Style.value;
            switch ((int)Slider_Shirt_Style.value)
            {
                case 0: //Pullover
                    Slider_Design.maxValue = 16;
                    Global.newCharacterOptions.PulloverTexture = (int)Slider_Design.value;
                    break;
                case 1: //Shirt
                    Slider_Design.maxValue = 13;
                    Global.newCharacterOptions.ShirtTexture = (int)Slider_Design.value;
                    break;
                case 2: //t-shirt
                    Slider_Design.maxValue = 20;
                    Global.newCharacterOptions.TshirtTexture = (int)Slider_Design.value;
                    break;
                case 3: //tank top
                    Slider_Design.maxValue = 10;
                    Global.newCharacterOptions.TanktopTexture = (int)Slider_Design.value;
                    break;
            }
        }

        if((int)Slider_Pants.value <= 14)   //pants
        {
            Global.newCharacterOptions.WhichTrouser = 0;
            Global.newCharacterOptions.PantsTexture = (int)Slider_Pants.value;
        }
        else   //shorts
        {
            Global.newCharacterOptions.WhichTrouser = 1;
            Global.newCharacterOptions.ShortsTexture = (int)Slider_Pants.value - 14;
        }

        Global.newCharacterOptions.Shoes = (int)Slider_Shoes.value;
        switch(Global.newCharacterOptions.Shoes)
        {
            case 0:
                Slider_Shoe_Color.maxValue = 7;
                Global.newCharacterOptions.Shoe1Texture = (int)Slider_Shoe_Color.value;
                break;
            case 1:
                Slider_Shoe_Color.maxValue = 6;
                Global.newCharacterOptions.Shoe2Texture = (int)Slider_Shoe_Color.value;
                break;
            case 2:
                Slider_Shoe_Color.maxValue = 5;
                Global.newCharacterOptions.Shoe3Texture = (int)Slider_Shoe_Color.value;
                break;
        }

        if((int)Slider_Glasses.value == 0)
        {
            Global.newCharacterOptions.GlassesOn = 0;

        }
        else
        {
            Global.newCharacterOptions.GlassesOn = 1;
            Global.newCharacterOptions.GlassesTexture = (int)Slider_Glasses.value - 1;
        }

        if ((int)Slider_Chain.value == 0)
        {
            Global.newCharacterOptions.Chain = 0;
        }
        else if ((int)Slider_Chain.value >= 1 && (int)Slider_Chain.value <= 4)
        {
            Global.newCharacterOptions.Chain = 1;
            Global.newCharacterOptions.Chain1Texture = (int)Slider_Chain.value - 1;
        }
        else if ((int)Slider_Chain.value >= 5 && (int)Slider_Chain.value <= 7)
        {
            Global.newCharacterOptions.Chain = 2;
            Global.newCharacterOptions.Chain2Texture = (int)Slider_Chain.value - 5;
        }
        else if ((int)Slider_Chain.value >= 8 && (int)Slider_Chain.value <= 10)
        {
            Global.newCharacterOptions.Chain = 3;
            Global.newCharacterOptions.Chain3Texture = (int)Slider_Chain.value - 8;
        }


        if ((int)Slider_Scarf.value == 0)
        {
            Global.newCharacterOptions.ScarfValue = 0;

        }
        else
        {
            Global.newCharacterOptions.ScarfValue = 1;
            Global.newCharacterOptions.ScarfTexture = (int)Slider_Scarf.value - 1;
        }

        //Attributes
        totalUsedPoints = (int)(Slider_Strength.value + Slider_Stamina.value + Slider_Athletic.value + Slider_Speed.value + Slider_Swimming.value + Slider_Puzzles.value + Slider_CampSkills.value);
        TMP_Text_PointsRemaining.text = "" + (max_Points - totalUsedPoints) + " of " + max_Points;


        //Character Creation State
        if (createCharacterState == CreateCharacterState.Failed)
        {
            // Handle the error...
            Debug.Log("FireBaseDataBase Faulted");
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "Character Creation Failed\n\nPlease check your connection and try again";
        }
        else if (createCharacterState == CreateCharacterState.Success)
        {
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "Character Created Successfully";
        }
    }

    public void Button_Male_click()
    {
        Global.newCharacterOptions.MaleFemale = 0;
    }

    public void Button_Female_click()
    {
        Global.newCharacterOptions.MaleFemale = 1;
    }

    public void Button_Suit_click()
    {
        Global.newCharacterOptions.SuitOrCloth = 0;
    }

    public void Button_Custom_click()
    {
        Global.newCharacterOptions.SuitOrCloth = 1;
    }

    public void Button_CancelCreatePlayer_click()
    {
        SceneManager.LoadScene((int)Global.Screen.Screen_Menu);
    }

    public void Button_GoToAttributes_click()
    {

        if (InputFieldEnterPlayerName.text.Length < 2 || InputFieldEnterPlayerName.text.Length > 20)
        {
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "Please enter a valid name that is 2-20 letters long.";
        }
        else
        {
            bool sameName = false;
            foreach(Global.CharacterOptions character in Global.localUser.Characters)
            {
                if(character.Name == InputFieldEnterPlayerName.text)
                {
                    sameName = true;
                    break;
                }
            }

            if (sameName)
            {
                Canvas_Popup.SetActive(true);
                TMP_Text_PopupMessage.text = "You already have a character named \n'" + InputFieldEnterPlayerName.text + "'\n\nPlease choose another name.";
            }
            else
            {
                Canvas_Appearance.SetActive(false);
                Canvas_Attributes.SetActive(true);
                Canvas_Popup.SetActive(false);
                TMP_Text_PlayersName.text = InputFieldEnterPlayerName.text;
                Global.newCharacterOptions.Name = TMP_Text_PlayersName.text;
            }
        }
    }

    public void Button_DismissPopup_click()
    {
        Canvas_Popup.SetActive(false);

        //Character Creation State
        if (createCharacterState == CreateCharacterState.Failed)
        {
            createCharacterState = CreateCharacterState.Idle;
        }
        else if (createCharacterState == CreateCharacterState.Success)
        {
            createCharacterState = CreateCharacterState.Finish;
            SceneManager.LoadScene((int)Global.Screen.Screen_Menu);
        }
    }

    public void Button_BackToAppearance_click()
    {
        Canvas_Appearance.SetActive(true);
        Canvas_Attributes.SetActive(false);
        Canvas_Popup.SetActive(false);
    }

    public void Button_FinishCharacterCreation_click()
    {
        if(max_Points - totalUsedPoints < 0)
        {
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "You have used too many attribute points.\n\nPlease adjust the sliders until the Points Remaining is 0.";
        }
        else if(max_Points - totalUsedPoints != 0)
        {
            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "You have not used all of your attribute points.\n\nPlease adjust the sliders until the Points Remaining is 0.";
        }
        else
        {
            mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

            Global.newCharacterOptions.stats.Strength = (int)Slider_Strength.value;
            Global.newCharacterOptions.stats.Stamina = (int)Slider_Stamina.value;
            Global.newCharacterOptions.stats.Athletic = (int)Slider_Athletic.value;
            Global.newCharacterOptions.stats.Speed = (int)Slider_Speed.value;
            Global.newCharacterOptions.stats.Swimming = (int)Slider_Swimming.value;
            Global.newCharacterOptions.stats.Puzzles = (int)Slider_Puzzles.value;
            Global.newCharacterOptions.stats.CampSkills = (int)Slider_CampSkills.value;

            //Add Character to User
            Global.localUser.Characters.Add(Global.newCharacterOptions);

            //Write to database
            Global.User_Simple userTemp = new Global.User_Simple(Global.localUser);
            string json = JsonUtility.ToJson(userTemp);

            mDatabaseRef.Child("users").Child("" + Global.mainPlayerId).SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    createCharacterState = CreateCharacterState.Failed;
                }
                else if (task.IsCompleted)
                {
                    createCharacterState = CreateCharacterState.Success;
                }
            });

            Canvas_Popup.SetActive(true);
            TMP_Text_PopupMessage.text = "Creating Character...";
            createCharacterState = CreateCharacterState.CreatingCharacter;
        }
    }
}
