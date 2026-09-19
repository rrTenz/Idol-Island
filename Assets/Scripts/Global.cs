using UnityEngine;
using System;
using System.Collections.Generic;
using Firebase.Database;

public class Global : MonoBehaviour
{
    public static Global Instance { get; private set; }

    public static DatabaseReference DatabaseRef;

    public static Game currentGame;
    public static User localUser;
    public static string mainPlayerId;
    public static int mainPlayerIndex;
    //public static int userId_passedTo_clothing_script;
    public static bool mainPlayerMoving;
    public static bool LockCameraRotation;
    public static bool ignoreRaycast;
    public static bool isLoggedIntoCurrentGame_mainUser;

    public static bool stopMoving_mainPlayer;

    public static bool advanceToNextPhaseOfTurn;

    public static bool updatePlayersStartingPosition;
    public static Vector3 startingDestination;
    public static bool holdPositions;
    public static float holdPosition_timer;
    public static bool doWarp;
    public static Vector3 JumpDestination;
    public static bool JumpToLocation;
    public static bool ShowLoadingScreenWhileWarping;

    public static Vector3[] playerPositionArray_currentGame;
    public static List<bool> loginStatusArrayList;
    public static List<bool> playerLockedInList;
    public static int waitForCallbackBeforeAdvancing;

    public static bool NewUserCreatedOnLoginScreen;
    public static User newUser;
    public static string newUserId;

    public const int MAX_TRIBES = 6;
    public const int MAX_LEVEL = 100;
    public const float MIN_ALTITUDE = 1.027f;    //this is the lowest a player can go to prevent them from going under water
    public const float DEFAULT_BAD_Y = 20000;   //a really high Y value to indicate that the user needs to be placed at the 'starting position'

    public static bool creatingCharacter;
    public static CharacterOptions newCharacterOptions;

    public static Vector3[] characterScaleArray;
    public static string[] characterNameArray;

    public static string DebugText;

    public enum Screen: int
    {
        Screen_Login = 0,
        Screen_Menu,
        Screen_Game,
        Screen_CreatePlayer,
        Screen_NewGame
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public enum EventMask : byte
    {
        Nothing = 0x00,
        Reward = 0x01,
        Immunity = 0x02,
        Swap = 0x04,
        Merge = 0x08,
        Final_Tribal = 0x10
    }

    public enum EventSequence : int    //Order: Nothing, Swap/Merge, Nothing, Reward/Immunity, Nothing, Tribal/Final Tribal
    {
        CampTime_First = 0,
        SwapOrMerge,
        CampTime_PostSwapOrMerge,
        RewardOrImmunity,
        CampTime_PostChallenge,
        Tribal
    }

    public class Game
    {
        public long GameId;
        public int MaxPlayers;
        public string gameName;
        public int TimePerTurn_minutes;
        public bool hasStarted;
        public bool hasEnded;
        public GameWinner gameWinner;
        public string[] userIdArray;
        public int[] userCharacterIndexArray;
        public int[] dayEliminated;
        public Vector3[] destinationArray;

        public int playerCount;

        //not passed into function
        public int beginningNumerOfTribes;
        public int playersAtMerge;
        public int numberOfSwaps;
        public bool isRealTime;
        public int daysOnIsland;
        public string gameCreatedBy_playerId;
        public string gameCreatedBy_characterName;
        public int minPlayerLevel;
        public int maxPlayerLevel;

        public bool tribeCountIncreases;
        public int maxTribeCount;
        public string[] tribeNamesArray;
        public int[] tribeColorIndexArray;
        public bool showSwapDetails;
        public int[] tribeSwapsAtArray;
        public int[] tribesCountArray;

        public byte[] eventByDayArray;
        public int currentDay;
        public EventSequence currentPhaseOfDay;
        public long dayStartedAt_UTCTime_SecondsSinceEpoch;
        public long phaseStartedAt_UTCTime_SecondsSinceEpoch;

        public Game()
        {
            
        }

        public Game(long gameID, string gameName, int MaxPlayers, int TimePerTurn_minutes, bool hasStarted, int[] userIdArray, int[] userCharacterIndexArray, int[] dayEliminated, Vector3[] destinationArray)
        {
            this.GameId = gameID;
            this.gameName = gameName;
            this.MaxPlayers = MaxPlayers;
            this.TimePerTurn_minutes = TimePerTurn_minutes;
            this.hasStarted = hasStarted;
            this.hasEnded = false;
            this.userIdArray = new string[userIdArray.Length];
            userIdArray.CopyTo(this.userIdArray, 0);
            this.userCharacterIndexArray = new int[userCharacterIndexArray.Length];
            userCharacterIndexArray.CopyTo(this.userCharacterIndexArray, 0);
            this.dayEliminated = new int[dayEliminated.Length];
            dayEliminated.CopyTo(this.dayEliminated, 0);
            this.destinationArray = new Vector3[destinationArray.Length];
            destinationArray.CopyTo(this.destinationArray, 0);
            gameWinner = new GameWinner();

            playerCount = 0;
            for (int i = 0; i < userIdArray.Length; i++)
            {
                if (userIdArray[i] > 0)
                {
                    playerCount++;
                }
            }
        }
    }

    public class GameWinner
    {
        public string PlayerName;
        public int playerIndex;

        public GameWinner()
        {
            this.PlayerName = "";
            this.playerIndex = -1;
        }
    }

    public class PlayerObject
    {
        public GameObject PlayerBody;
        public GameObject DesitnationCylinder;
        public GameObject SimpleCustomHuman;
        public GameObject ChatRadius;

        public PlayerObject()
        {

        }

        public PlayerObject(GameObject PlayerBody, GameObject DesitnationCylinder, GameObject SimpleCustomHuman, GameObject ChatRadius)
        {
            this.PlayerBody = PlayerBody;
            this.DesitnationCylinder = DesitnationCylinder;
            this.SimpleCustomHuman = SimpleCustomHuman;
            this.ChatRadius = ChatRadius;
        }
    }

    public static List<PlayerObject> PlayerObjectList = new List<PlayerObject>();

    public class User
    {
        public string username;
        public string email;
        public List<long> Games;
        public List<CharacterOptions> Characters;

        public User()
        {
        }

        public User(string username, string email, List<long> Games, List<CharacterOptions> Characters)
        {
            this.username = username;
            this.email = email;
            this.Games = new List<long>(Games);
            this.Characters = new List<CharacterOptions>(Characters);
        }
    }

    public class User_Simple
    {
        public string username;
        public string email;
        public long[] Games;
        public CharacterOptions[] Characters;
        public string jsonCharacterString;

        public User_Simple()
        {
        }

        public User_Simple(User user)
        {
            this.username = user.username;
            this.email = user.email;

            this.Games = new long[user.Games.Count];
            user.Games.CopyTo(this.Games, 0);

            this.Characters = new CharacterOptions[user.Characters.Count];
            user.Characters.CopyTo(this.Characters, 0);

            this.jsonCharacterString = Newtonsoft.Json.JsonConvert.SerializeObject(Characters);
        }
    }

    public class TribeColor
    {
        public int index;
        public int hexCode;
        public string name;

        public TribeColor()
        {
        }

        public TribeColor(int index, int hexCode, string name)
        {
            this.index = index;
            this.hexCode = hexCode;
            this.name = name;
        }
    }

    public class CharacterOptions
    {
        public string Name { get; set; }
        public int SkinColor { get; set; }     //0-5
        public int MaleFemale { get; set; }    //0 = male, 1 = female
        public int HairColor { get; set; }     // 0 = dark  1 = brown  2 = blonde, 3 = green, 4 = red, 5 = pink
        public int HairMale { get; set; }      // choose hair type   hair_a , hair_b  , hair_e (male) , bald (male)
        public int HairFemale { get; set; }    // choose hair type   hair_c , hair_d (female)
        public int HairCut { get; set; }       // 0 = full hair    1 = under cut
        public int Beard { get; set; }         //0-4
        public int BeardTexture { get; set; }  //0-4

        public int SuitOrCloth { get; set; }   //0 = suits, 1 = cloth
        public int WhichSuit { get; set; }     // bankersuit    0
                                            // cocksuit      1
                                            // farmersuit    2
                                            // firemansuit   3
                                            // mechanicsuit  4
                                            // nursesuit     5
                                            // policesuit    6
                                            // roobersuit    7
                                            // securitysuit  8
                                            // sellersuit    9
                                            // workersuit    10
        public int BankerTexture { get; set; } //0-6

        public int Shoes { get; set; }         //0-2
        public int Shoe1Texture { get; set; }  //0-7
        public int Shoe2Texture { get; set; }  //0-6
        public int Shoe3Texture { get; set; }  //0-5

        public int GlassesOn { get; set; }     //0 = no, 1 = yes
        public int GlassesTexture { get; set; }//0-5

        public int Chain { get; set; }         //0-3
        public int Chain1Texture { get; set; } //0-3
        public int Chain2Texture { get; set; } //0-2
        public int Chain3Texture { get; set; } //0-2

        public int ScarfValue { get; set; }    //0 = no, 1 = yes
        public int ScarfTexture { get; set; }  //0-10

        public int WhichTrouser { get; set; }  //0 = pants, 1 = shorts
        public int PantsTexture { get; set; }  //0-14
        public int ShortsTexture { get; set; } //0-10

        public int Top { get; set; }           //0 = pullover  1 = shirt    2 = t_shirt    3 = tanktop
        public int PulloverTexture { get; set; }//0-16
        public int ShirtTexture { get; set; }  //0-13
        public int TshirtTexture { get; set; } //0-20
        public int TanktopTexture { get; set; }//0-10

        public int Jacket { get; set; }        //0 = no, 1 = yes
        public int JacketTexture { get; set; } //0-11

        public float SizeScale;
        public float HeightScale;

        public CharacterStats stats;

        public CharacterOptions()
        {
            SkinColor = 1;
            HairFemale = 1;
            SuitOrCloth = 1;
            stats = new CharacterStats();
            Name = "";
        }
    }


    public class CharacterStats
    {
        public int Strength { get; set; }
        public int Stamina { get; set; }
        public int Athletic { get; set; }
        public int Speed { get; set; }
        public int Swimming { get; set; }
        public int Puzzles { get; set; }
        public int CampSkills { get; set; }

        public CharacterStats()
        {

        }
    }

    // Palate Calculator https://planetcalc.com/5799/
    // Tribe colors: https://docs.google.com/document/d/1-s5SR2CnPNtKEl1r_5yHCH9DMByOPm2LbPrzwSAKknE/edit
    // Name that Color: https://chir.ag/projects/name-that-color
    //static int[] ColourValues = new int[] {
    //    0xFFFFFF,
    //    0xff0000, 0xff3700, 0xff6a00, 0xffa200, 0xffd500, 0xf2ff00, 0xbfff00, 0x88ff00, 0x55ff00, 0x1eff00, 0x00ff15, 0x00ff4d, 0x00ff80, 0x00ffb7, 0x00ffea, 0x00ddff, 0x00aaff, 0x0073ff, 0x0040ff, 0x0009ff, 0x2b00ff, 0x6200ff, 0x9500ff, 0xcc00ff, 0xff00ff,
    //    0x000000
    //};
    public static List<TribeColor> Init_TribeColor_List()
    {
        List<TribeColor> tribeColor_List;
        tribeColor_List = new List<TribeColor>();
        tribeColor_List.Clear();

        //don't add colors to the middle of this list, only the end
        tribeColor_List.Add(new TribeColor(0, 0xFFFFFF, "White"));
        tribeColor_List.Add(new TribeColor(1, 0xff0000, "Red"));
        tribeColor_List.Add(new TribeColor(2, 0xff3700, "Scarlet"));
        tribeColor_List.Add(new TribeColor(3, 0xff6a00, "Blaze Orange"));
        tribeColor_List.Add(new TribeColor(4, 0xffa200, "Orange Peel"));
        tribeColor_List.Add(new TribeColor(5, 0xffa200, "Orange"));
        tribeColor_List.Add(new TribeColor(6, 0xffd500, "Gold"));
        tribeColor_List.Add(new TribeColor(7, 0xf2ff00, "Yellow"));
        tribeColor_List.Add(new TribeColor(8, 0xbfff00, "Lime"));
        tribeColor_List.Add(new TribeColor(9, 0x88ff00, "Chartreuse"));
        tribeColor_List.Add(new TribeColor(10, 0x55ff00, "Bright Green"));
        tribeColor_List.Add(new TribeColor(11, 0x1eff00, "Green 2"));
        tribeColor_List.Add(new TribeColor(12, 0x00ff4d, "Green"));
        tribeColor_List.Add(new TribeColor(13, 0x00ff4d, "Spring Green"));
        tribeColor_List.Add(new TribeColor(14, 0x00ffb7, "Bright Turquoise"));
        tribeColor_List.Add(new TribeColor(15, 0x00ffea, "Cyan"));
        tribeColor_List.Add(new TribeColor(16, 0x00ddff, "Aqua"));
        tribeColor_List.Add(new TribeColor(17, 0x00aaff, "Azure Radiance"));
        tribeColor_List.Add(new TribeColor(18, 0x0073ff, "Azure Radiance Dark"));
        tribeColor_List.Add(new TribeColor(19, 0x0040ff, "Blue Ribbon"));
        tribeColor_List.Add(new TribeColor(20, 0x0009ff, "Blue"));
        tribeColor_List.Add(new TribeColor(21, 0x0009ff, "Indigo"));
        tribeColor_List.Add(new TribeColor(22, 0x6200ff, "Electric Violet 1"));
        tribeColor_List.Add(new TribeColor(23, 0x9500ff, "Electric Violet 2"));
        tribeColor_List.Add(new TribeColor(24, 0xcc00ff, "Electric Violet 3"));
        tribeColor_List.Add(new TribeColor(25, 0xff00ff, "Magenta"));
        tribeColor_List.Add(new TribeColor(26, 0x000000, "Black"));
        tribeColor_List.Add(new TribeColor(27, 0xffde05, "Pagong"));
        tribeColor_List.Add(new TribeColor(28, 0xff9900, "Tagi"));
        tribeColor_List.Add(new TribeColor(29, 0x7dfa00, "Rattana"));
        tribeColor_List.Add(new TribeColor(30, 0x32cdfc, "Kucha"));
        tribeColor_List.Add(new TribeColor(31, 0xa7fa00, "Ogakor"));
        tribeColor_List.Add(new TribeColor(32, 0xff6600, "Barramundi"));
        tribeColor_List.Add(new TribeColor(33, 0xffd500, "Boran"));
        tribeColor_List.Add(new TribeColor(34, 0xe31b2c, "Samburu"));
        tribeColor_List.Add(new TribeColor(35, 0x00a692, "Moto Maji"));
        tribeColor_List.Add(new TribeColor(36, 0xdfff00, "Maraamu"));
        tribeColor_List.Add(new TribeColor(37, 0x99ffff, "Rotu"));
        tribeColor_List.Add(new TribeColor(38, 0xf400a1, "Soliantu"));
        tribeColor_List.Add(new TribeColor(39, 0xff321d, "Chuay Gahn"));
        tribeColor_List.Add(new TribeColor(40, 0x7515d4, "Sook Jai"));
        tribeColor_List.Add(new TribeColor(41, 0xffaa00, "Chuay Jai"));
        tribeColor_List.Add(new TribeColor(42, 0xfff71b, "Jaburu"));
        tribeColor_List.Add(new TribeColor(43, 0x3366ff, "Tambaqui"));
        tribeColor_List.Add(new TribeColor(44, 0xff1d1d, "Jacare"));
        tribeColor_List.Add(new TribeColor(45, 0x87ceeb, "Drake"));
        tribeColor_List.Add(new TribeColor(46, 0xf88017, "Morgan"));
        tribeColor_List.Add(new TribeColor(47, 0x9400d3, "The Outcasts"));
        tribeColor_List.Add(new TribeColor(48, 0x000000, "Balboa"));
        tribeColor_List.Add(new TribeColor(49, 0xbc0e16, "Chapera"));
        tribeColor_List.Add(new TribeColor(50, 0x2f663b, "Mogo Mogo"));
        tribeColor_List.Add(new TribeColor(51, 0xefcc24, "Saboga"));
        tribeColor_List.Add(new TribeColor(52, 0x290fb8, "Chaboga Mogo"));
        tribeColor_List.Add(new TribeColor(53, 0xe21a1d, "Lopevi"));
        tribeColor_List.Add(new TribeColor(54, 0xf6f33c, "Yasur"));
        tribeColor_List.Add(new TribeColor(55, 0xf77613, "Alinta"));
        tribeColor_List.Add(new TribeColor(56, 0x784937, "Koror"));
        tribeColor_List.Add(new TribeColor(57, 0x2840b4, "Ulong"));
        tribeColor_List.Add(new TribeColor(58, 0xe1dc4c, "Nakum"));
        tribeColor_List.Add(new TribeColor(59, 0x69c0c0, "Yaxha"));
        tribeColor_List.Add(new TribeColor(60, 0xdc143c, "Xhakum"));
        tribeColor_List.Add(new TribeColor(61, 0x00a5fa, "Bayoneta"));
        tribeColor_List.Add(new TribeColor(62, 0x7800aa, "Casaya"));
        tribeColor_List.Add(new TribeColor(63, 0xff8609, "La Mina"));
        tribeColor_List.Add(new TribeColor(64, 0x26f823, "Viveros"));
        tribeColor_List.Add(new TribeColor(65, 0x080808, "Gitanos"));
        tribeColor_List.Add(new TribeColor(66, 0xeb1010, "Aitutaki"));
        tribeColor_List.Add(new TribeColor(67, 0xfff105, "Manihiki"));
        tribeColor_List.Add(new TribeColor(68, 0x46ca00, "Puka Puka"));
        tribeColor_List.Add(new TribeColor(69, 0x0047ab, "Rarotonga"));
        tribeColor_List.Add(new TribeColor(70, 0x000000, "Aitutonga"));
        tribeColor_List.Add(new TribeColor(71, 0x7fff00, "Moto"));
        tribeColor_List.Add(new TribeColor(72, 0xff5e00, "Ravu"));
        tribeColor_List.Add(new TribeColor(73, 0x660099, "Bula Bula"));
        tribeColor_List.Add(new TribeColor(74, 0xc80815, "Fei Long"));
        tribeColor_List.Add(new TribeColor(75, 0xffdf2a, "Zhan Hu"));
        tribeColor_List.Add(new TribeColor(76, 0x070300, "Hae Da Fung"));
        tribeColor_List.Add(new TribeColor(77, 0xff9933, "Airai"));
        tribeColor_List.Add(new TribeColor(78, 0x9370db, "Malakal"));
        tribeColor_List.Add(new TribeColor(79, 0xccff99, "Dabu"));
        tribeColor_List.Add(new TribeColor(80, 0xdd241a, "Fang"));
        tribeColor_List.Add(new TribeColor(81, 0xf5ff0a, "Kota"));
        tribeColor_List.Add(new TribeColor(82, 0x2b60de, "Nobag"));
        tribeColor_List.Add(new TribeColor(83, 0xc11b17, "Jalapao"));
        tribeColor_List.Add(new TribeColor(84, 0x302226, "Timbira"));
        tribeColor_List.Add(new TribeColor(85, 0x5dce5d, "Forza"));
        tribeColor_List.Add(new TribeColor(86, 0xffef00, "Foa Foa"));
        tribeColor_List.Add(new TribeColor(87, 0x823c96, "Galu"));
        tribeColor_List.Add(new TribeColor(88, 0x0073b9, "Aiga"));
        tribeColor_List.Add(new TribeColor(89, 0x0156c0, "Heroes"));
        tribeColor_List.Add(new TribeColor(90, 0xcc0012, "Villains"));
        tribeColor_List.Add(new TribeColor(91, 0x090303, "Yin Yang"));
        tribeColor_List.Add(new TribeColor(92, 0x004c9a, "Espada"));
        tribeColor_List.Add(new TribeColor(93, 0xffe400, "La Flor"));
        tribeColor_List.Add(new TribeColor(94, 0xac192b, "Libertad"));
        tribeColor_List.Add(new TribeColor(95, 0xf19027, "Ometepe"));
        tribeColor_List.Add(new TribeColor(96, 0x703293, "Zapatera"));
        tribeColor_List.Add(new TribeColor(97, 0x19171a, "Murlonio"));
        tribeColor_List.Add(new TribeColor(98, 0xa92027, "Savaii"));
        tribeColor_List.Add(new TribeColor(99, 0x014b96, "Upolu"));
        tribeColor_List.Add(new TribeColor(100, 0xfeed01, "Te Tuna"));
        tribeColor_List.Add(new TribeColor(101, 0xfc8501, "Manono"));
        tribeColor_List.Add(new TribeColor(102, 0x6cbdb6, "Salani"));
        tribeColor_List.Add(new TribeColor(103, 0x1c1c1c, "Tikiano"));
        tribeColor_List.Add(new TribeColor(104, 0xff0126, "Kalabaw"));
        tribeColor_List.Add(new TribeColor(105, 0x2150ff, "Matsing"));
        tribeColor_List.Add(new TribeColor(106, 0xffeb00, "Tandang"));
        tribeColor_List.Add(new TribeColor(107, 0x140a0b, "Dangrayne"));
        tribeColor_List.Add(new TribeColor(108, 0x844184, "Bikal"));
        tribeColor_List.Add(new TribeColor(109, 0xed7527, "Gota"));
        tribeColor_List.Add(new TribeColor(110, 0x81c41c, "Enil Edam"));
        tribeColor_List.Add(new TribeColor(111, 0xe6ac1a, "Galang"));
        tribeColor_List.Add(new TribeColor(112, 0xb1201d, "Tadhana"));
        tribeColor_List.Add(new TribeColor(113, 0x4f2a7a, "Kasama"));
        tribeColor_List.Add(new TribeColor(114, 0xf09733, "Aparri"));
        tribeColor_List.Add(new TribeColor(115, 0x2fa976, "Luzon"));
        tribeColor_List.Add(new TribeColor(116, 0x8d2f87, "Solana"));
        tribeColor_List.Add(new TribeColor(117, 0x040404, "Solarrion"));
        tribeColor_List.Add(new TribeColor(118, 0xf89532, "Coyopa"));
        tribeColor_List.Add(new TribeColor(119, 0x5cd7df, "Hunahpu"));
        tribeColor_List.Add(new TribeColor(120, 0x2cca8a, "Huyopa"));
        tribeColor_List.Add(new TribeColor(121, 0x0b58a0, "Escameca"));
        tribeColor_List.Add(new TribeColor(122, 0xe9cc0e, "Masaya"));
        tribeColor_List.Add(new TribeColor(123, 0xc11e21, "Nagarote"));
        tribeColor_List.Add(new TribeColor(124, 0x242026, "Merica"));
        tribeColor_List.Add(new TribeColor(125, 0xda1789, "Bayon"));
        tribeColor_List.Add(new TribeColor(126, 0x029b8d, "Ta Keo"));
        tribeColor_List.Add(new TribeColor(127, 0xd2b423, "Angkor"));
        tribeColor_List.Add(new TribeColor(128, 0xfb693d, "Orkun"));
        tribeColor_List.Add(new TribeColor(129, 0x0c71ef, "Chan Loh"));
        tribeColor_List.Add(new TribeColor(130, 0xfcbd30, "Gondol"));
        tribeColor_List.Add(new TribeColor(131, 0xd12837, "To Tang"));
        tribeColor_List.Add(new TribeColor(132, 0x191718, "Dara"));
        tribeColor_List.Add(new TribeColor(133, 0x500c8f, "Takali"));
        tribeColor_List.Add(new TribeColor(134, 0xdc3d0d, "Vanua"));
        tribeColor_List.Add(new TribeColor(135, 0x4aac1b, "Ika Bula"));
        tribeColor_List.Add(new TribeColor(136, 0x231d1a, "Vinaka"));
        tribeColor_List.Add(new TribeColor(137, 0xa72d21, "Mana"));
        tribeColor_List.Add(new TribeColor(138, 0x105b91, "Nuku"));
        tribeColor_List.Add(new TribeColor(139, 0x19c44e, "Tavua"));
        tribeColor_List.Add(new TribeColor(140, 0xf4cc3c, "Maku Maku"));
        tribeColor_List.Add(new TribeColor(141, 0x0c5ddc, "Levu"));
        tribeColor_List.Add(new TribeColor(142, 0xf1cd1f, "Soko"));
        tribeColor_List.Add(new TribeColor(143, 0xe0192c, "Yawa"));
        tribeColor_List.Add(new TribeColor(144, 0x912e8d, "Solewa"));
        tribeColor_List.Add(new TribeColor(145, 0xed8721, "Malolo"));
        tribeColor_List.Add(new TribeColor(146, 0xa65bb3, "Naviti"));
        tribeColor_List.Add(new TribeColor(147, 0x50c163, "Yanuya"));
        tribeColor_List.Add(new TribeColor(148, 0x100d12, "Lavita"));
        tribeColor_List.Add(new TribeColor(149, 0xcf7315, "Vuku"));
        tribeColor_List.Add(new TribeColor(150, 0x672d7b, "Jabeni"));
        tribeColor_List.Add(new TribeColor(151, 0x398044, "Tiva"));
        tribeColor_List.Add(new TribeColor(152, 0x93d2ff, "Kalokalo"));
        tribeColor_List.Add(new TribeColor(153, 0xfacf22, "Kama"));
        tribeColor_List.Add(new TribeColor(154, 0x1050ba, "Manu"));
        tribeColor_List.Add(new TribeColor(155, 0x008975, "Lesu"));
        tribeColor_List.Add(new TribeColor(156, 0xd32323, "Vata"));
        tribeColor_List.Add(new TribeColor(157, 0xF88741, "Lairo"));
        tribeColor_List.Add(new TribeColor(158, 0xD592CB, "Vokai"));
        tribeColor_List.Add(new TribeColor(159, 0x00AACA, "Lumuwaku"));
        tribeColor_List.Add(new TribeColor(160, 0xDA0017, "Dakal"));
        tribeColor_List.Add(new TribeColor(161, 0x005FCC, "Sele"));
        tribeColor_List.Add(new TribeColor(162, 0x00894B, "Yara"));
        tribeColor_List.Add(new TribeColor(163, 0x2D241E, "Koru"));
        tribeColor_List.Add(new TribeColor(164, 0x5FF0FE, "Luvu"));
        tribeColor_List.Add(new TribeColor(165, 0x93E965, "Ua"));
        tribeColor_List.Add(new TribeColor(166, 0xFFEF5F, "Yase"));
        tribeColor_List.Add(new TribeColor(167, 0xF50E2C, "Viakana"));

        return tribeColor_List;
    }

    public static int getMainPlayerIndex_currentGame()
    {
        int playerIndex = -1;
        for (int i = 0; i < Global.currentGame.userIdArray.Length; i++)
        {
            if (Global.mainPlayerId == Global.currentGame.userIdArray[i])
            {
                playerIndex = i;
                break;
            }
        }
        return playerIndex;
    }

    public static int getMainPlayerCharacterIndex_currentGame()
    {
        return Global.currentGame.userCharacterIndexArray[getMainPlayerIndex_currentGame()];
    }
}


//TODO
//next: split players into tribes, simulate challenges, simulate tribal councils
//Challenges
//Tribal (who am I planning to vote for)
//Make all Canvas backgrounds the same size so it doesn't look like the background is jump (e.g. when there is a popup message)
//Give "Remember username/password" option
//Google Login

//Button Color - http://www.holshousersoftware.com/glass/
//R212, G160, B96
//d4a060
//H33, S55, B83
//Player Name, show over player's head

//Tribes will always be random
//Put Name over character
//Allow player to lie about stats (show increased stregth, decreased camp skills, etc)

//***Challenge Ideas
/*
 * Hot or cold: show distance from object, first one there wins
 * 
 * */


//******Asset Store Ideas
//***Shelter
//$5 - https://assetstore.unity.com/packages/3d/environments/old-wooden-shelter-made-of-branches-162443
//Free - https://assetstore.unity.com/packages/3d/environments/hovel-77502
//***Terrain
//$21 low poly - https://assetstore.unity.com/packages/3d/environments/low-poly-rg-island-mega-pack-199222
//Free - https://assetstore.unity.com/packages/3d/environments/landscapes/free-low-poly-nature-forest-205742
//$25 Island terrain - https://assetstore.unity.com/packages/3d/environments/landscapes/island-terrain-pack-8461


//Done
//Customize player clothing
//Send messages
//Create toggle button for Login/Create
//Advance days


