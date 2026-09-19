using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using System;

// SIMPLE modular human https://assetstore.unity.com/packages/3d/characters/humanoids/humans/simple-modular-human-100162
// Hand-painted Island Pack https://assetstore.unity.com/packages/3d/environments/fantasy/hand-painted-island-pack-36959
// Rain https://assetstore.unity.com/?price=0-0&q=rain&orderBy=1
// Create Island Terrain https://www.youtube.com/watch?v=CVI7fBgwP1s

public class clothing : MonoBehaviour
{
    private string UserId = "-1";

    public GameObject skin_head;
    public GameObject skin_body;


    public GameObject cigarette;
    public GameObject crowbar;
    public GameObject fireaxe;
    public GameObject glock;
    public GameObject phone;


    public GameObject beard_a;
    public GameObject beard_b;
    public GameObject beard_c;
    public GameObject beard_d;

    public GameObject hair_a;
    public GameObject hair_b;
    public GameObject hair_c;
    public GameObject hair_d;
    public GameObject hair_e;

    public GameObject cap;
    public GameObject cap2;
    public GameObject cap3;
    public GameObject chain1;
    public GameObject chain2;
    public GameObject chain3;

    public GameObject banker_suit;

    public GameObject cock_suit;
    public GameObject cock_suit_hat;

    public GameObject farmer_suit;
    public GameObject farmer_suit_hat;

    public GameObject fireman_suit;
    public GameObject fireman_suit_hat;

    public GameObject mechanic_suit;
    public GameObject mechanic_suit_hat;

    public GameObject nurse_suit;

    public GameObject police_suit;
    public GameObject police_suit_hat;

    public GameObject roober_suit;
    public GameObject roober_suit_hat;

    public GameObject security_guard_suit;
    public GameObject security_guard_suit_hat;

    public GameObject seller_suit;

    public GameObject worker_suit;
    public GameObject worker_suit_hat;

    public GameObject glasses;
    public GameObject jacket;
    public GameObject pullover;
    public GameObject scarf;
    public GameObject shirt;

    public GameObject shoes1;
    public GameObject shoes2;
    public GameObject shoes3;

    public GameObject shortpants;
    public GameObject t_shirt;
    public GameObject tank_top;
    public GameObject trousers;




    public Texture[] skin_textures;

    public Texture[] beard_textures;

    public Texture[] hair_a_textures;
    public Texture[] hair_b_textures;
    public Texture[] hair_c_textures;
    public Texture[] hair_d_textures;
    public Texture[] hair_e_textures;

    public Texture[] cap_textures;
    public Texture[] cap2_textures;
    public Texture[] cap3_textures;
    public Texture[] chain1_textures;
    public Texture[] chain2_textures;
    public Texture[] chain3_textures;

    public Texture[] banker_suit_texture;

    public Texture cock_suit_texture;


    public Texture farmer_suit_texture;


    public Texture fireman_suit_texture;


    public Texture mechanic_suit_texture;


    public Texture nurse_suit_texture;

    public Texture police_suit_texture;


    public Texture roober_suit_texture;


    public Texture security_guard_suit_texture;


    public Texture seller_suit_texture;

    public Texture worker_suit_texture;


    public Texture[] glasses_texture;
    public Texture[] jacket_textures;
    public Texture[] pullover_textures;
    public Texture[] scarf_textures;
    public Texture[] shirt_textures;

    public Texture[] shoes1_textures;
    public Texture[] shoes2_textures;
    public Texture[] shoes3_textures;

    public Texture[] shortpants_textures;
    public Texture[] t_shirt_textures;
    public Texture[] tank_top_textures;
    public Texture[] trousers_textures;

    public Animator ani;


    public bool show_run;

    public bool isHost;

    bool hat;


    public Global.CharacterOptions CharacterOptions;

    private string userId_passedTo_clothing_script_local = "-1";
    private int userIndex = -1;
    private DatabaseReference mDatabaseRef;


    Coroutine coroutine_random_clothing;

    void start_random_clothing()
    {
        //yield return new WaitForSeconds(0);

        // disapear all cloth, for a new run

        hat = true;

        hair_a.SetActive(false);
        hair_b.SetActive(false);
        hair_c.SetActive(false);
        hair_d.SetActive(false);
        hair_e.SetActive(false);

        beard_a.SetActive(false);
        beard_b.SetActive(false);
        beard_c.SetActive(false);
        beard_d.SetActive(false);

        cap.SetActive(false);
        cap2.SetActive(false);
        cap3.SetActive(false);

        chain1.SetActive(false);
        chain2.SetActive(false);
        chain3.SetActive(false);

        banker_suit.SetActive(false);

        cock_suit.SetActive(false);
        cock_suit_hat.SetActive(false);

        farmer_suit.SetActive(false);
        farmer_suit_hat.SetActive(false);

        fireman_suit.SetActive(false);
        fireman_suit_hat.SetActive(false);

        mechanic_suit.SetActive(false);
        mechanic_suit_hat.SetActive(false);

        nurse_suit.SetActive(false);

        police_suit.SetActive(false);
        police_suit_hat.SetActive(false);

        roober_suit.SetActive(false);
        roober_suit_hat.SetActive(false);

        security_guard_suit.SetActive(false);
        security_guard_suit_hat.SetActive(false);

        seller_suit.SetActive(false);

        worker_suit.SetActive(false);
        worker_suit_hat.SetActive(false);

        glasses.SetActive(false);

        jacket.SetActive(false);

        pullover.SetActive(false);

        scarf.SetActive(false);

        shirt.SetActive(false);

        shoes1.SetActive(false);

        shoes2.SetActive(false);

        shoes3.SetActive(false);

        shortpants.SetActive(false);

        t_shirt.SetActive(false);

        tank_top.SetActive(false);

        trousers.SetActive(false);

        if (Global.creatingCharacter == true || isHost)
        {
            if (Global.newCharacterOptions == null)
            {
                Global.newCharacterOptions = new Global.CharacterOptions();
            }
            CharacterOptions = Global.newCharacterOptions;
        }

        // determining skin color
        int skin_color = CharacterOptions.SkinColor;   // UnityEngine.Random.Range(0, 6);
        if (skin_color < 0 || skin_color > skin_textures.Length - 1)
            skin_color = 0;
        skin_head.GetComponent<Renderer>().materials[0].mainTexture = skin_textures[skin_color];
        skin_body.GetComponent<Renderer>().materials[0].mainTexture = skin_textures[skin_color];



        // determining male or female
        int male_female = CharacterOptions.MaleFemale; // UnityEngine.Random.Range(0, 2);
        if (male_female < 0 || male_female > 1)
            male_female = 0;




        // does a hat fit for the hair


        // determining haircolor
        int hairColor = CharacterOptions.HairColor;    // UnityEngine.Random.Range(0, 4);    // 0 = dark  1 = brown  2 = blonde
        if (hairColor < 0 || hairColor > 23)
            hairColor = 0;

        // male
        if (male_female == 0)
        {
            hat = true;

            // choose hair type   hair_a , hair_b  , hair_e
            int hair = CharacterOptions.HairMale;  // UnityEngine.Random.Range(0, 4);
            if (hair < 0 || hair >= 4)
                hair = 0;

            if (hair == 0)
            {
                hair_a.SetActive(true);

                // 0 = full hair    1 = under cut
                int hair_cut = CharacterOptions.HairCut;   // UnityEngine.Random.Range(0, 2);
                if (hair_cut < 0 || hair_cut >= 2)
                    hair_cut = 0;
                hat = true;

                if (hairColor > 2)
                {
                    hair_a.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[hairColor];
                }
                else if (hairColor == 0)
                {
                    if (hair_cut == 0)
                    {
                        hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[0];
                    }
                    if (hair_cut == 1)
                    {
                        hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[1];
                    }
                }
                else if (hairColor == 1)
                {
                    if (hair_cut == 0)
                    {
                        hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[2];
                    }
                    if (hair_cut == 1)
                    {
                        hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[3];
                    }

                }
                else if (hairColor == 2)
                {
                    if (hair_cut == 0)
                    {
                        hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[4];
                    }
                    if (hair_cut == 1)
                    {
                        hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[5];
                    }

                }


            }

            else if (hair == 1)
            {
                hair_b.SetActive(true);
                hat = false;

                // 0 = full hair    1 = under cut
                int hair_cut = CharacterOptions.HairCut;   // UnityEngine.Random.Range(0, 2);
                if (hair_cut < 0 || hair_cut >= 2)
                    hair_cut = 0;


                if (hairColor > 2)
                {
                    hair_b.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[hairColor];
                }
                else if (hairColor == 0)
                {
                    if (hair_cut == 0)
                    {
                        hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[0];
                    }
                    if (hair_cut == 1)
                    {
                        hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[5];
                    }
                }
                else if (hairColor == 1)
                {
                    if (hair_cut == 0)
                    {
                        hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[1];
                    }
                    if (hair_cut == 1)
                    {
                        hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[3];
                    }

                }
                else if (hairColor == 2)
                {
                    if (hair_cut == 0)
                    {
                        hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[2];
                    }
                    if (hair_cut == 1)
                    {
                        hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[4];
                    }

                }



            }

            if (hair == 2)
            {
                hair_e.SetActive(true);
                hat = false;

                hair_e.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[hairColor];
            }
            if(hair == 3)
            {
                //bald
            }

        }
        // female
        if(male_female == 1)
        {
            
            hat = false;

            // choose hair type   hair_c , hair_d
            int hair = CharacterOptions.HairFemale;    // UnityEngine.Random.Range(0, 2);
            if (hair < 0 || hair >= 2)
                hair = 0;


            if (hair == 0)
            {
                hat = false;
                hair_c.SetActive(true);

                if (hairColor > 2)
                {
                    hair_c.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[hairColor];
                }
                else if (hairColor == 0)
                {
                    hair_c.GetComponent<Renderer>().materials[0].mainTexture = hair_c_textures[0];
                }
                if (hairColor == 1)
                {
                    hair_c.GetComponent<Renderer>().materials[0].mainTexture = hair_c_textures[1];
                }
                if (hairColor == 2)
                {
                    hair_c.GetComponent<Renderer>().materials[0].mainTexture = hair_c_textures[2];
                }
            }
            if (hair == 1)
            {
                hat = false;
                hair_d.SetActive(true);

                if (hairColor > 2)
                {
                    hair_d.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[hairColor];
                }
                else if (hairColor == 0)
                {
                    hair_d.GetComponent<Renderer>().materials[0].mainTexture = hair_d_textures[0];
                }
                else if (hairColor == 1)
                {
                    hair_d.GetComponent<Renderer>().materials[0].mainTexture = hair_d_textures[1];
                }
                else if (hairColor == 2)
                {
                    hair_d.GetComponent<Renderer>().materials[0].mainTexture = hair_d_textures[2];
                }
            }
        }

        // determining beard
        if(male_female == 0)
        {
            int whichBeard = CharacterOptions.Beard;   // UnityEngine.Random.Range(0, 5);
            if (whichBeard < 0 || whichBeard >= 5)
                whichBeard = 0;

            if (whichBeard == 0)
            {
                // none beard
            }
            if (whichBeard == 1)
            {
                // beard a
                beard_a.SetActive(true);
                beard_a.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[CharacterOptions.BeardTexture];
            }
            if (whichBeard == 2)
            {
                // beard b

                beard_b.SetActive(true);
                beard_b.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[CharacterOptions.BeardTexture];
            }
            if (whichBeard == 3)
            {
                // beard c

                beard_c.SetActive(true);
                beard_c.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[CharacterOptions.BeardTexture];
            }
            if (whichBeard == 4)
            {
                // beard d

                beard_d.SetActive(true);
                beard_d.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[CharacterOptions.BeardTexture];
            }
        }


        // determining complet suits or normal cloth
        int suit_or_cloth = CharacterOptions.SuitOrCloth;  // UnityEngine.Random.Range(0, 2);
        if (suit_or_cloth < 0 || suit_or_cloth >= 2)
            suit_or_cloth = 0;

        // determing      suit/ normal cloth to wear


        if (suit_or_cloth == 0)
        {
            // suits

            int which_suit = CharacterOptions.WhichSuit;   // UnityEngine.Random.Range(0,11);
            if (which_suit < 0 || which_suit >= 11)
                which_suit = 0;




            // bankersuit    0
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


            // banker suit
            if (which_suit == 0)
            {
                banker_suit.SetActive(true);

                int which_texture = CharacterOptions.BankerTexture;    // UnityEngine.Random.Range(0, 7);
                if (which_texture < 0 || which_texture >= 7)
                    which_texture = 0;


                banker_suit.GetComponent<Renderer>().materials[0].mainTexture = banker_suit_texture[which_texture];
                
            }
            // cock suit
            if(which_suit == 1)
            {
                cock_suit.SetActive(true);

                cock_suit.GetComponent<Renderer>().materials[0].mainTexture = cock_suit_texture;

                if(hat)
                {
                    cock_suit_hat.SetActive(true);
                    cock_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = cock_suit_texture;
                }
            }
            // farmer suit
            if (which_suit == 2)
            {
                farmer_suit.SetActive(true);

                farmer_suit.GetComponent<Renderer>().materials[0].mainTexture = farmer_suit_texture;

                if (hat)
                {
                    farmer_suit_hat.SetActive(true);
                    farmer_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = farmer_suit_texture;
                }
            }
            // fireman suit
            if (which_suit == 3)
            {
                fireman_suit.SetActive(true);

                fireman_suit.GetComponent<Renderer>().materials[0].mainTexture = fireman_suit_texture;

                if (hat)
                {
                    fireman_suit_hat.SetActive(true);

                    fireman_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = fireman_suit_texture;
                }
            }
            // mechanic suit
            if (which_suit == 4)
            {
                mechanic_suit.SetActive(true);

                mechanic_suit.GetComponent<Renderer>().materials[0].mainTexture = mechanic_suit_texture;

                if (hat)
                {
                    mechanic_suit_hat.SetActive(true);

                    mechanic_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = mechanic_suit_texture;
                }
            }
            // nurse suit
            if (which_suit == 5)
            {
                nurse_suit.SetActive(true);

                nurse_suit.GetComponent<Renderer>().materials[0].mainTexture = nurse_suit_texture;

               
            }
            // police suit
            if (which_suit == 6)
            {
                police_suit.SetActive(true);

                police_suit.GetComponent<Renderer>().materials[0].mainTexture = police_suit_texture;

                if (hat)
                {
                    police_suit_hat.SetActive(true);

                    police_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = police_suit_texture;
                }
            }
            // robber suit
            if (which_suit == 7)
            {
                roober_suit.SetActive(true);

                roober_suit.GetComponent<Renderer>().materials[0].mainTexture = roober_suit_texture;

              
                    roober_suit_hat.SetActive(true);

                    roober_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = roober_suit_texture;


                hair_a.SetActive(false);
                hair_b.SetActive(false);
                hair_c.SetActive(false);
                hair_d.SetActive(false);
                hair_e.SetActive(false);

                beard_a.SetActive(false);
                beard_b.SetActive(false);
                beard_c.SetActive(false);
                beard_d.SetActive(false);
            }
            // security guard suit
            if (which_suit == 8)
            {
                security_guard_suit.SetActive(true);

                security_guard_suit.GetComponent<Renderer>().materials[0].mainTexture = security_guard_suit_texture;

                if (hat)
                {
                    security_guard_suit_hat.SetActive(true);

                    security_guard_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = security_guard_suit_texture;
                }
            }
            // seller suit
            if (which_suit == 9)
            {
                seller_suit.SetActive(true);

                seller_suit.GetComponent<Renderer>().materials[0].mainTexture = seller_suit_texture;

            }
            // worker suit
            if (which_suit == 10)
            {
                worker_suit.SetActive(true);

                worker_suit.GetComponent<Renderer>().materials[0].mainTexture = worker_suit_texture;

                if (hat)
                {
                    worker_suit_hat.SetActive(true);

                    worker_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = worker_suit_texture;
                }
            }




        }
        if (suit_or_cloth == 1)
        {
            // normal cloth

            int shoes = CharacterOptions.Shoes;    // UnityEngine.Random.Range(0, 3);
            if (shoes < 0 || shoes >= 3)
                shoes = 0;

            if (shoes == 0)
            {
                shoes1.SetActive(true);

                int shoes1_texture = CharacterOptions.Shoe1Texture;    // UnityEngine.Random.Range(0, 8);
                if (shoes1_texture < 0 || shoes1_texture >= 8)
                    shoes1_texture = 0;

                shoes1.GetComponent<Renderer>().materials[0].mainTexture = shoes1_textures[shoes1_texture];

            }

            if (shoes == 1)
            {
                shoes2.SetActive(true);

                int shoes2_texture = CharacterOptions.Shoe2Texture;    // UnityEngine.Random.Range(0, 7);
                if (shoes2_texture < 0 || shoes2_texture >= 7)
                    shoes2_texture = 0;

                shoes2.GetComponent<Renderer>().materials[0].mainTexture = shoes2_textures[shoes2_texture];

            }

            if (shoes == 2)
            {
                shoes3.SetActive(true);

                int shoes3_texture = CharacterOptions.Shoe3Texture;    // UnityEngine.Random.Range(0, 6);
                if (shoes3_texture < 0 || shoes3_texture >= 6)
                    shoes3_texture = 0;

                shoes3.GetComponent<Renderer>().materials[0].mainTexture = shoes3_textures[shoes3_texture];

            }


            int glasses_value = CharacterOptions.GlassesOn;    // UnityEngine.Random.Range(0, 2);
            if (glasses_value < 0 || glasses_value >= 2)
                glasses_value = 0;

            if (glasses_value == 1)
            {
                glasses.SetActive(true);

                int texture_choose = CharacterOptions.GlassesTexture;  // UnityEngine.Random.Range(0, 6);
                if (texture_choose < 0 || texture_choose >= 6)
                    texture_choose = 0;

                glasses.GetComponent<Renderer>().materials[0].mainTexture = glasses_texture[texture_choose];
            }

            int chain = CharacterOptions.Chain;    // UnityEngine.Random.Range(0, 4);
            if (chain < 0 || chain >= 4)
                chain = 0;

            if (chain == 0)
            { }
            if (chain == 1)
            {
                chain1.SetActive(true);

                int textures = CharacterOptions.Chain1Texture; // UnityEngine.Random.Range(0, 4);
                if (textures < 0 || textures >= 4)
                    textures = 0;

                chain1.GetComponent<Renderer>().materials[0].mainTexture = chain1_textures[textures];

            }
            if(chain == 2)
            {
                chain2.SetActive(true);

                int textures = CharacterOptions.Chain2Texture; // UnityEngine.Random.Range(0, 2);
                if (textures < 0 || textures >= 2)
                    textures = 0;

                chain2.GetComponent<Renderer>().materials[0].mainTexture = chain2_textures[textures];

            }
            if(chain == 3)
            {
                chain3.SetActive(true);

                int textures = CharacterOptions.Chain3Texture; // UnityEngine.Random.Range(0, 3);
                if (textures < 0 || textures >= 3)
                    textures = 0;

                chain3.GetComponent<Renderer>().materials[0].mainTexture = chain3_textures[textures];

            }

            int scarf_value = CharacterOptions.ScarfValue; // UnityEngine.Random.Range(0, 2);
            if (scarf_value < 0 || scarf_value >= 2)
                scarf_value = 0;

            if (scarf_value == 1)
            {
                scarf.SetActive(true);

                int textures = CharacterOptions.ScarfTexture;  // UnityEngine.Random.Range(0, 11);
                if (textures < 0 || textures >= 11)
                    textures = 0;

                scarf.GetComponent<Renderer>().materials[0].mainTexture = scarf_textures[textures];
            }

            int which_trouser = CharacterOptions.WhichTrouser; // UnityEngine.Random.Range(0, 2);
            if (which_trouser < 0 || which_trouser >= 2)
                which_trouser = 0;

            // trousers
            if (which_trouser == 0)
            {
                trousers.SetActive(true);

                int texture = CharacterOptions.PantsTexture;   // UnityEngine.Random.Range(0, 15);
                if (texture < 0 || texture >= 15)
                    texture = 0;

                trousers.GetComponent<Renderer>().materials[0].mainTexture = trousers_textures[texture];
                
            }
            // short pants
            if (which_trouser == 1)
            {
                shortpants.SetActive(true);


                int texture = CharacterOptions.ShortsTexture;  // UnityEngine.Random.Range(0, 11);
                if (texture < 0 || texture >= 11)
                    texture = 0;

                shortpants.GetComponent<Renderer>().materials[0].mainTexture = shortpants_textures[texture];


            }


            // upper bosy cloth :   0 = pullover  1 = shirt    2 = t_shirt    3 = tanktop
            int upper_cloth = CharacterOptions.Top;    // UnityEngine.Random.Range(0, 4);
            if (upper_cloth < 0 || upper_cloth >= 4)
                upper_cloth = 0;


            if (upper_cloth == 0)
            {
                pullover.SetActive(true);

                int texture = CharacterOptions.PulloverTexture;    // UnityEngine.Random.Range(0, 17);
                if (texture < 0 || texture >= 17)
                    texture = 0;

                pullover.GetComponent<Renderer>().materials[0].mainTexture = pullover_textures[texture];
            }

            if (upper_cloth == 1)
            {
                shirt.SetActive(true);

                int texture = CharacterOptions.ShirtTexture;   // UnityEngine.Random.Range(0, 14);
                if (texture < 0 || texture >= 14)
                    texture = 0;

                shirt.GetComponent<Renderer>().materials[0].mainTexture = shirt_textures[texture];
            }
            if (upper_cloth == 2)
            {
                t_shirt.SetActive(true);

                int texture = CharacterOptions.TshirtTexture;   //UnityEngine.Random.Range(0, 21);
                if (texture < 0 || texture >= 21)
                    texture = 0;

                t_shirt.GetComponent<Renderer>().materials[0].mainTexture = t_shirt_textures[texture];
            }
            if (upper_cloth == 3)
            {
                tank_top.SetActive(true);

                int texture = CharacterOptions.TanktopTexture; // UnityEngine.Random.Range(0, 11);
                if (texture < 0 || texture >= 11)
                    texture = 0;

                tank_top.GetComponent<Renderer>().materials[0].mainTexture = tank_top_textures[texture];
            }




            int jacket_value = CharacterOptions.Jacket;    // UnityEngine.Random.Range(0, 2);
            if (jacket_value < 0 || jacket_value >= 2)
                jacket_value = 0;

            if (jacket_value == 1)
            {
                jacket.SetActive(true);

                int texture = CharacterOptions.JacketTexture;   // UnityEngine.Random.Range(0, 12);
                if (texture < 0 || texture >= 12)
                    texture = 0;

                jacket.GetComponent<Renderer>().materials[0].mainTexture = jacket_textures[texture];
            }

        }




        //yield return new WaitForSeconds(5);

        //StopCoroutine(coroutine_random_clothing);
        //coroutine_random_clothing = StartCoroutine(start_random_clothing());

    }

    bool finishedSetup = false;
    bool haveCharacterOptions = false;
    private void Start()
    {
        getCharacterOptions();
    }

    void getCharacterOptions()
    {
        if (UserId == "-1" && Global.creatingCharacter == false && isHost == false)
            return;

        userId_passedTo_clothing_script_local = UserId;
        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;


        if (Global.creatingCharacter == true || isHost == true)
        {
            if (Global.newCharacterOptions == null)
            {
                Global.newCharacterOptions = new Global.CharacterOptions();
            }
            CharacterOptions = Global.newCharacterOptions;

            if (isHost)
            {
                CharacterOptions.HeightScale = 1.00361443f;
                CharacterOptions.Name = "JP";
                CharacterOptions.SkinColor = 8;
                CharacterOptions.MaleFemale = 0;
                CharacterOptions.HairColor = 7;
                CharacterOptions.HairMale = 1;
                CharacterOptions.HairFemale = 1;
                CharacterOptions.HairCut = 0;
                CharacterOptions.Beard = 0;
                CharacterOptions.BeardTexture = 7;
                CharacterOptions.SuitOrCloth = 1;
                CharacterOptions.WhichSuit = 0;
                CharacterOptions.BankerTexture = 0;
                CharacterOptions.Shoes = 0;
                CharacterOptions.Shoe1Texture = 5;
                CharacterOptions.GlassesOn = 0;
                CharacterOptions.Chain = 0;
                CharacterOptions.ScarfValue = 0;
                CharacterOptions.WhichTrouser = 1;
                CharacterOptions.PantsTexture = 14;
                CharacterOptions.ShortsTexture = 2;
                CharacterOptions.Top = 1;
                CharacterOptions.PulloverTexture = 0;
                CharacterOptions.ShirtTexture = 7;
                CharacterOptions.TshirtTexture = 0;
                CharacterOptions.TanktopTexture = 0;
                CharacterOptions.Jacket = 0;
                CharacterOptions.JacketTexture = 0;
            }

            finishedSetup = true;
        }
        else if (CharacterOptions == null)
        {
            if (userId_passedTo_clothing_script_local == "0")
            {
                userId_passedTo_clothing_script_local = Global.mainPlayerId;
            }

            for (int i = 0; i < Global.currentGame.userIdArray.Length; i++)
            {
                if (Global.currentGame.userIdArray[i] == userId_passedTo_clothing_script_local)
                {
                    userIndex = i;
                    break;
                }
            }

            FirebaseDatabase.DefaultInstance
            .GetReference("users/" + Global.currentGame.userIdArray[userIndex] + "/jsonCharacterString")
            .GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted)
                {
                    finishedSetup = true;
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    if (snapshot.Key.Equals("jsonCharacterString"))
                    {
                        if (snapshot.Value == null)
                        {
                            CharacterOptions = new Global.CharacterOptions();   //Load Default player
                        }
                        else
                        {
                            Global.CharacterOptions[] characters = Newtonsoft.Json.JsonConvert.DeserializeObject<Global.CharacterOptions[]>((String)snapshot.Value);

                            int characterIndex = Global.currentGame.userCharacterIndexArray[userIndex];
                            if (characterIndex < characters.Length)
                            {
                                CharacterOptions = characters[characterIndex];

                                if (Global.characterScaleArray == null || Global.characterScaleArray.Length == 0 || Global.characterScaleArray.Length < Global.currentGame.playerCount)
                                {
                                    Global.characterScaleArray = new Vector3[Global.currentGame.playerCount];
                                    Global.characterNameArray = new string[Global.currentGame.playerCount];
                                }
                                Global.characterScaleArray[userIndex] = new Vector3(CharacterOptions.SizeScale, CharacterOptions.HeightScale, CharacterOptions.SizeScale);
                                Global.characterNameArray[userIndex] = CharacterOptions.Name;

                                //if (userIndex <= Global.characterScaleArray.Length)
                                //{

                                //}
                                //else
                                //{
                                //}
                            }
                            else
                            {
                                CharacterOptions = new Global.CharacterOptions();   //Load Default player
                            }
                        }
                    }
                    finishedSetup = true;
                }
                haveCharacterOptions = true;
            });

        }
    }

    void Update()
    {
        if(haveCharacterOptions == false)
        {
            getCharacterOptions();
        }

        if (finishedSetup == false)
            return;


        if (Global.updatePlayersStartingPosition && UserId == Global.mainPlayerId)
        {
            Global.updatePlayersStartingPosition = false;
            transform.position = Global.startingDestination;
        }

        if (!show_run || Global.creatingCharacter == true)
        {
            show_run = true;

            //coroutine_random_clothing = StartCoroutine(start_random_clothing());
            start_random_clothing();
        }
    }

    public void set_UserId(string id)
    {
        UserId = id;
    }

}

