// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FrogJunction;

public class CharacterSelectUIEventHandler : MonoBehaviour
{
    public AuthDialog dlgAuthRegistration;
    
    public Image characterImage;
    public Image houseImage;

    public InputField inputCharacterName;

    public Text labelNameError;

    public List<GameObject> controlsToToggle;
    public IntersceneData intersceneData;
    
    private int currentCharacterModel = 0;
    private int currentHouseModel = 0;

    private bool authMode = false;  // this flag controls if the auth dialog is creating a new user or logging in
    private bool verifyMode = false;   // this flag controls if we need to confirm the user account


    private int SelectedCharacter
    {
        get => currentCharacterModel;
        set {
            currentCharacterModel = value;
            UpdateCharacterModel();
        }
    }

    private int SelectedHouse
    {
        get => currentHouseModel;
        set {
            currentHouseModel = value;
            UpdateHouseModel();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SelectedCharacter = 0;
        SelectedHouse = 0;
        dlgAuthRegistration.Visible = false;
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Maybe a mistake handling the incrementing through the character models and house models here
    // as it creates an implementation dependency on the IntersceneData class, and it could be 
    // encapsulated there, but I thought I might do some specialized iteration in the interface. 
    // Also, it's a demo of AWS services, not a guide on proper code structure :D
    public void IncrementCharacterModel()
    {
        if(SelectedCharacter < intersceneData.CharacterList.Count - 1)
        {
            SelectedCharacter++;
        }
        else
        {
            SelectedCharacter = 0;
        }
    }

    public void DecrementCharacterModel()
    {
        if(SelectedCharacter == 0)
        {
            SelectedCharacter = intersceneData.CharacterList.Count - 1;
        }
        else
        {
            SelectedCharacter--;
        }
    }

    private void UpdateCharacterModel()
    {
        characterImage.sprite = intersceneData.CharacterList[SelectedCharacter].SELECTION_SPRITE;   
    }

    public void IncrementHouseModel()
    {
        if(SelectedHouse < intersceneData.HouseList.Count - 1)
        {
            SelectedHouse++;
        }
        else
        {
            SelectedHouse = 0;
        }
    }

    public void DecrementHouseModel()
    {
        if(SelectedHouse == 0)
        {
            SelectedHouse = intersceneData.HouseList.Count - 1;
        }
        else
        {
            SelectedHouse--;
        }
    }

    private void UpdateHouseModel()
    {
        houseImage.sprite = intersceneData.HouseList[SelectedHouse].SELECTION_SPRITE;   
    }

    public void OnCancelPressed()
    {
        SceneManager.LoadScene("main");
    }

    public void OnCreatePressed()
    {
        if(String.IsNullOrEmpty(inputCharacterName.text))
        {
            labelNameError.gameObject.SetActive(true);
            labelNameError.text = "You must select a character name before you can create a new character!";
        }
        else
        {
            labelNameError.gameObject.SetActive(false);
            EnableControls(false);        
            dlgAuthRegistration.Visible = true;
            dlgAuthRegistration.DialogMode = AuthDialog.Mode.Create;
            authMode = false;
            verifyMode = false;
        }
    }

    public void OnCancelAuthDialog()
    {
        EnableControls(true);
        dlgAuthRegistration.Visible = false;
        authMode = false;
        verifyMode = false;
    }

    public void OnCreateAuthDialog(string email, string password)
    {
        if(authMode)
        {
            PlayerIdentificationSystem.Instance.Login(email, password,
                () => 
                {
                    // We are logged in! Now we need to store the player data
                    // and then launch the game
                    OnCancelAuthDialog();
                    StoreCharacterAndLaunchGame();
                },
                () =>
                {
                    // OnLoginNewPasswordRequired
                    // This should be impossible as we don't require an immediate password change
                    // when a new user is created from the client, only when the user is created from the 
                    // AWS console
                    dlgAuthRegistration.ShowErrorText("Somehow the user is being asked to change their password. This shouldn't happen!");
                },
                (errorMessage) => 
                { 
                    dlgAuthRegistration.ShowErrorText(errorMessage);
                }
            );
        }
        else if(verifyMode)
        {
            PlayerIdentificationSystem.Instance.ConfirmSignupRequest(email, password, // password is now the verification code
                () =>
                {
                    authMode = true;
                    verifyMode = false;
                    dlgAuthRegistration.DialogMode = AuthDialog.Mode.Login;
                },
                (errorMessage) =>
                {
                    dlgAuthRegistration.ShowErrorText(errorMessage);
                });
        }
        else
        {
            // Store off the selected character info before going further in to authentication flow
            // This way, the game doesn't need to load the data we already have from the game service.
            intersceneData.CharacterName = inputCharacterName.text;
            intersceneData.SelectedCharacter = intersceneData.CharacterList[SelectedCharacter];
            intersceneData.SelectedHouse = intersceneData.HouseList[SelectedHouse];

            dlgAuthRegistration.ClearErrorText();
            PlayerIdentificationSystem.Instance.CreateAccount(email, password,
                () =>
                {
                    dlgAuthRegistration.DialogMode = AuthDialog.Mode.Verify;
                    verifyMode = true;
                    authMode = false;
                },
                (errorMessage) =>
                {
                    dlgAuthRegistration.ShowErrorText(errorMessage);
                }
            );
        }
    }

    private void EnableControls(bool on)
    {
        foreach(var control in controlsToToggle)
        {
            control.SetActive(on);
        }
    }

    private void StoreCharacterAndLaunchGame()
    {
        var createPlayerData = new CreatePlayerData()
        {
            name = intersceneData.CharacterName,
            model = intersceneData.SelectedCharacter.NAME,
            house = intersceneData.SelectedHouse.NAME
        };
        PlayerDataSystem.Instance.CreatePlayerData(createPlayerData,
            ()=> {
                // Data has been successfully stored off, launch the game
                SceneManager.LoadScene("game");
            },
            (string error) => {
                // This is a conundrum, the player has created an account but
                // won't have any associated player data. I don't handle it very
                // gracefully here, basically the player can try again
                // and when they try to create the account, it's already created
                // and will go ahead and try to save. In a production game
                // this should be a better experience!
                labelNameError.gameObject.SetActive(true);
                labelNameError.text = "Unable to create character data. Please try again.";
            });
    }
}
