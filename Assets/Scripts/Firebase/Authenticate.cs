using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.UI;
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class Authenticate : MonoBehaviour
{
    FirebaseAuth auth;

    public Button Button_Login_Toggle;
    public Button Button_NewUser_Toggle;

    public TMP_InputField InputField_UserName;
    public TMP_InputField InputField_Email;
    public TMP_InputField InputField_Password;
    public TMP_InputField InputField_PasswordVerify;

    public Button Button_Login;
    public Button Button_Create;

    public GameObject Canvas_Popup;
    public TMP_Text TMP_Text_PopupMessage;

    bool isLoginSelected;

    bool goToMainMenu = false;


    public enum ErrorMessage : int
    {
        Error_None = 0,
        Error_Login
    }

    ErrorMessage errorMessage;

    // Start is called before the first frame update
    void Start()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            //if a user is already logged in, go to the main menu
            SceneManager.LoadScene((int)Global.Screen.Screen_Menu);
        }

        Global.NewUserCreatedOnLoginScreen = false;
        isLoginSelected = true;

        InputField_Email.text = "rtensmeyer@msn.com";
        InputField_Password.text = "qwertyuiop";

        errorMessage = ErrorMessage.Error_None;
    }

    // Update is called once per frame
    void Update()
    {
        if (InputField_Password.text != InputField_PasswordVerify.text)
        {
            InputField_PasswordVerify.image.color = Color.red;
        }
        else if(InputField_Password.text.Length == 0)
        {
            InputField_PasswordVerify.image.color = Color.white;
        }
        else if (InputField_Password.text == InputField_PasswordVerify.text)
        {
            InputField_PasswordVerify.image.color = Color.green;
        }

        if (isLoginSelected)
        {
            Button_Login_Toggle.interactable = false;
            Button_Login_Toggle.image.color = Color.green;
            Button_NewUser_Toggle.interactable = true;
            Button_NewUser_Toggle.image.color = Color.white;

            InputField_UserName.gameObject.SetActive(false);
            InputField_Email.gameObject.SetActive(true);
            InputField_Password.gameObject.SetActive(true);
            InputField_PasswordVerify.gameObject.SetActive(false);
            Button_Login.gameObject.SetActive(true);
            Button_Create.gameObject.SetActive(false);
        }
        else
        {
            Button_Login_Toggle.interactable = true;
            Button_Login_Toggle.image.color = Color.white;
            Button_NewUser_Toggle.interactable = false;
            Button_NewUser_Toggle.image.color = Color.green;

            InputField_UserName.gameObject.SetActive(true);
            InputField_Email.gameObject.SetActive(true);
            InputField_Password.gameObject.SetActive(true);
            InputField_PasswordVerify.gameObject.SetActive(true);
            Button_Login.gameObject.SetActive(false);
            Button_Create.gameObject.SetActive(true);
        }

        if(goToMainMenu)
            SceneManager.LoadScene((int)Global.Screen.Screen_Menu);

        if (errorMessage != ErrorMessage.Error_None)
        {
            switch(errorMessage)
            {
                case ErrorMessage.Error_Login:
                    ShowPopup("Login Failed");
                    break;
                default:
                    ShowPopup("Unsuccessful");
                    break;
            }
            errorMessage = ErrorMessage.Error_None;
        }
    }

    public void ButtonClick_Create_New_User()
    {

        if (InputField_UserName.text.Length < 2)
        {
            ShowPopup("The User Name must be at least 2 letters long.");
            InputField_UserName.image.color = Color.red;
            return;
        }
        InputField_UserName.image.color = Color.white;

        if (IsValidEmail(InputField_Email.text) == false)
        {
            ShowPopup("Please enter a valid email address");
            InputField_Email.image.color = Color.red;
            return;
        }
        InputField_Email.image.color = Color.white;

        if (InputField_Password.text.Length < 8)
        {
            ShowPopup("The password must be at least 8 characters long.");
            InputField_Password.image.color = Color.red;
            return;
        }
        InputField_Password.image.color = Color.white;

        if (InputField_Password.text != InputField_PasswordVerify.text)
        {
            ShowPopup("The passwords must match.");
            return;
        }



        ShowPopup("Creating New User");
        auth = FirebaseAuth.DefaultInstance;
        auth.CreateUserWithEmailAndPasswordAsync(InputField_Email.text, InputField_Password.text).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                ShowPopup("Creating New User was canceled");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                ShowPopup("We are having trouble with our tree mail system.\n\nPlease check your internet and try again later.");
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);

            Debug.LogError("User Created Successfully.\n\nPlease Login!");

            Global.NewUserCreatedOnLoginScreen = true;
            Global.newUserId = newUser.UserId;
            Global.newUser = new Global.User(InputField_UserName.text, InputField_Email.text, new List<long>(), new List<Global.CharacterOptions>());

            ButtonClick_Login();
        });
    }

    public void ButtonClick_Login()
    {

        if (IsValidEmail(InputField_Email.text) == false)
        {
            ShowPopup("Please enter a valid email address");
            InputField_Email.image.color = Color.red;
            return;
        }
        InputField_Email.image.color = Color.white;

        //I don't want to give this information, it makes it a little easier to guess a password
        //if (InputField_Password.text.Length < 8)
        //{
        //    ShowPopup("The password must be at least 8 characters long.");
        //    InputField_Password.image.color = Color.red;
        //    return;
        //}
        //InputField_Password.image.color = Color.white;

        ShowPopup("Loging In");
        auth = FirebaseAuth.DefaultInstance;
        auth.SignInWithEmailAndPasswordAsync(InputField_Email.text, InputField_Password.text).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                ShowPopup("Login Canceled");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                errorMessage = ErrorMessage.Error_Login;
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);

            goToMainMenu = true;
        });
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                  RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                string domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException e)
        {
            Debug.Log(e);
            return false;
        }
        catch (ArgumentException e)
        {
            Debug.Log(e);
            return false;
        }

        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private void ShowPopup(string message)
    {
        Canvas_Popup.SetActive(true);
        TMP_Text_PopupMessage.text = message;
    }

    public void Button_DismissPopup_click()
    {
        Canvas_Popup.SetActive(false);
    }

    public void ButtonClick_Toggle_Login_NewUser()
    {
        isLoginSelected = !isLoginSelected;

        if (isLoginSelected)
        {
            InputField_Email.text = "rtensmeyer@msn.com";
            InputField_Password.text = "qwertyuiop";
        }
        else
        {
            InputField_Email.text = "";
            InputField_Password.text = "";
        }
    }
}
