// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class AuthDialog : MonoBehaviour
{
    public Image createTitle;
    public Image loginTitle;
    public Image changeTitle;
    public Image verifyTitle;
    public Image imgDialogBackground;
    public InputField inputEmail;
    public InputField inputPassword;
    public Text textStatus;
    public Button cancelButton;
    public Sprite passwordFieldTexture;
    public Sprite verificationFieldTexture;

    public UnityEvent onCancel;
    public UnityEvent<string, string> onCreate;

    private bool isVisible;
    public bool Visible
    {
        get => isVisible;
        set
        {
            isVisible = value;
            imgDialogBackground.gameObject.SetActive(isVisible);
        }
    }

    public enum Mode
    {
        Create,
        Login,
        Change,
        Verify
    };
    private Mode mode;
    public Mode DialogMode
    {
        get => mode;
        set
        {
            mode = value;
            HideTitles();
            inputPassword.text = "%05pYHd%jgiE";
            titleMap[mode].gameObject.SetActive(true);
            if(mode == Mode.Change)
            {
                inputEmail.gameObject.SetActive(false);
                cancelButton.gameObject.SetActive(false);
                inputPassword.GetComponent<Image>().sprite = passwordFieldTexture;
            }
            else if(mode == Mode.Verify)
            {
                cancelButton.gameObject.SetActive(true);
                inputEmail.gameObject.SetActive(true);
                inputPassword.GetComponent<Image>().sprite = verificationFieldTexture;
            }
            else
            {
                cancelButton.gameObject.SetActive(true);
                inputEmail.gameObject.SetActive(true);
                inputPassword.GetComponent<Image>().sprite = passwordFieldTexture;
            }
        }
    }
    
    private Dictionary<Mode, Image> titleMap;

    private void Awake()
    {
        if(titleMap == null)
        {
            titleMap = new Dictionary<Mode, Image>()
            {
                { AuthDialog.Mode.Create, createTitle },
                { AuthDialog.Mode.Login, loginTitle },
                { AuthDialog.Mode.Change, changeTitle },
                { AuthDialog.Mode.Verify, verifyTitle }
            };
        }

        if(onCancel == null)
        {
            onCancel = new UnityEvent();
        }

        if(onCreate == null)
        {
            onCreate = new UnityEvent<string, string>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     public void OnOKPressed()
    {
        string errorText = "";
        if(String.IsNullOrEmpty(inputEmail.text) && mode != Mode.Verify)
        {
            errorText += "e-mail address must be filled out\n";
        }

        if(String.IsNullOrEmpty(inputPassword.text))
        {
            errorText += "password must be filled out\n";
        }

        if(String.IsNullOrEmpty(errorText))
        {
            onCreate.Invoke(inputEmail.text, inputPassword.text);
        }
        else
        {
            ShowErrorText(errorText);
        }
    }

    public void OnCancelPressed()
    {
        ClearDialog();
        onCancel.Invoke();
    }

    public void ShowErrorText(string errorText)
    {
        textStatus.text = "Error: " + errorText;
    }

    public void ClearErrorText()
    {
        textStatus.text = "";
    }

    private void ClearDialog()
    {
        textStatus.text = "";
        inputEmail.text = "";
        inputPassword.text = "";
    }

    private void HideTitles()
    {
        foreach(var item in titleMap.Values)
        {
            item.gameObject.SetActive(false);
        }
    }
}
