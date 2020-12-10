// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using UnityEngine;
using UnityEngine.UI;
using FrogJunction;

public class APITest : MonoBehaviour
{
    public IntersceneData intersceneData;

    public InputField emailInput;
    public InputField passwordInput;
    public InputField updatePasswordInput;
    public Button loginButton;
    public Button updatePasswordButton;
    public Button callAPIButton;
    
    public InputField createEmailInput;
    public InputField createPasswordInput;
    public InputField verifyInput;
    public Button createButton;
    public Button verifyButton;

    public Text statusText;

    // Start is called before the first frame update
    void Start()
    {
        updatePasswordInput.interactable = false;
        updatePasswordButton.interactable = false;
        callAPIButton.interactable = false;
        verifyButton.interactable = false;
    }

    public void OnLogin()
    {
        PlayerIdentificationSystem.Instance.Login(
            emailInput.text,
            passwordInput.text,
            ()=>
            {
                // OnLoginSuccess
                callAPIButton.interactable = true;
                AddStatusText("Login approved, you may now call the API");
                DisplayTokens();
            },
            () =>
            {
                // OnLoginNewPasswordRequired
                AddStatusText("Login approved, but you need to change your password");
                updatePasswordInput.interactable = true;
                updatePasswordButton.interactable = true;
            },
            (errorMessage) =>
            {
                // OnLoginFailure
                AddStatusText("Couldn't log in: " + errorMessage);
            }
        );
    }

    public void OnUpdatePassword()
    {
        PlayerIdentificationSystem.Instance.ProvideNewPassword(
            emailInput.text, 
            updatePasswordInput.text,
            () => 
            { 
                // OnLoginSuccess
                callAPIButton.interactable = true;
                AddStatusText("Login approved, you may now call the API");
                DisplayTokens();
            },  
            (errorMessage) => 
            { 
                // OnLoginFailure
                AddStatusText("Couldn't log in: " + errorMessage);
            }
        );
}

    public void OnCallAPI()
    {
        APIRequest.Instance.GetRequest("test",
            (long code, string result) =>
            {
                AddStatusText($"Successful API Call {code}: {result}");
            },
            (long code, string error) =>
            {
                AddStatusText($"Unsuccessful API Call {code}: {error}");
            });
    }

    public void OnCreateUser()
    {
        PlayerIdentificationSystem.Instance.CreateAccount(
            createEmailInput.text,
            createPasswordInput.text,
            ()=>
            {
                verifyButton.interactable = true;
                AddStatusText("User created. Check your e-mail for verification code and verify user");
            },
            (errorMessage)=>
            {
                AddStatusText("Couldn't create user: " + errorMessage);
            }
        );
    }

    public void OnVerifyUser()
    {
        PlayerIdentificationSystem.Instance.ConfirmSignupRequest(
            createEmailInput.text,
            verifyInput.text,
            ()=>
            {
                AddStatusText("User verified. You may now log in using the Auth/Test UI");
            },
            (errorMessage)=>
            {
                AddStatusText("Couldn't verify user: " + errorMessage);
            }
        );
    }

    private void AddStatusText(string message)
    {
        statusText.text += (message + "\n");
    }

    private void DisplayTokens()
    {
        var tokens = PlayerIdentificationSystem.Instance.AuthenticationTokens;
        AddStatusText("ID Token: " + tokens.IdToken);
        AddStatusText("Token Type: " + tokens.TokenType);
        AddStatusText("Token Expires: " + tokens.ExpiresIn);
    }
}
