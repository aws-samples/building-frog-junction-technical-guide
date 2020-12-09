// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using UnityEngine;
using UnityEngine.SceneManagement;
using FrogJunction;

public class MainMenuUIEventHandler : MonoBehaviour
{
    public AuthDialog dlgAuthLogin;

    public IntersceneData intersceneData;

    private bool changePasswordMode = false;

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        dlgAuthLogin.Visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLoginPressed()
    {
        dlgAuthLogin.DialogMode = AuthDialog.Mode.Login;
        dlgAuthLogin.Visible = true;
        changePasswordMode = false;
    }

    public void OnNewCharacterPressed()
    {
        SceneManager.LoadScene("characterselect");
    }

    public void OnCancelAuthDialog()
    {
        dlgAuthLogin.Visible = false;
    }

    public void OnLoginAuthDialog(string email, string password)
    {
        if(changePasswordMode)
        {
            PlayerIdentificationSystem.Instance.ProvideNewPassword(email, password,
                () => 
                { 
                    // OnLoginSuccess
                    dlgAuthLogin.DialogMode = AuthDialog.Mode.Login;
                    changePasswordMode = false;
                    LaunchGame();
                },  
                (errorMessage) => 
                { 
                    // OnLoginFailure
                    dlgAuthLogin.ShowErrorText(errorMessage);
                }
            );
        }
        else
        {
            PlayerIdentificationSystem.Instance.Login(email, password,
                () => 
                { 
                    // OnLoginSuccess
                    LaunchGame();
                },
                () =>
                {
                    // OnLoginNewPasswordRequired
                    dlgAuthLogin.DialogMode = AuthDialog.Mode.Change;
                    dlgAuthLogin.inputPassword.text = "";
                    changePasswordMode = true;

                },
                (errorMessage) => 
                { 
                    // OnLoginFailure
                    dlgAuthLogin.ShowErrorText(errorMessage);
                }
            );
        }
    }

    private void LaunchGame()
    {
        PlayerDataSystem.Instance.GetPlayerData(
            ()=>
            {
                InventorySystem.Instance.GetInventory(
                    ()=>
                    {
                        PlayerDataHandler.SetInterscenePlayerData(intersceneData, PlayerDataSystem.Instance.CachedPlayerData);
                        SceneManager.LoadScene("game");
                    },
                    (string error)=>
                    {
                        dlgAuthLogin.ShowErrorText(error);
                    });
            },
            (string error)=>
            {
                dlgAuthLogin.ShowErrorText(error);
            });
    }

}
