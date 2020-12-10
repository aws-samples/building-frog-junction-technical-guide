// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using System;
using UnityEngine;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;

namespace FrogJunction
{
    public class PlayerIdentificationSystem
    {
        public string PlayerID {get; private set;}
        public AuthenticationResultType AuthenticationTokens {get; private set;}
        
        private readonly AmazonCognitoIdentityProviderClient ipClient;
        private readonly CognitoUserPool userPool;
        private CognitoUser user;
        private string sessionID;


        public PlayerIdentificationSystem()
        {
            ipClient = new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials(),
                
                AWSStaticConfiguration.REGION);
            userPool = new CognitoUserPool(AWSStaticConfiguration.USER_POOL_ID, 
                AWSStaticConfiguration.APP_CLIENT_ID, ipClient);
        }

        public delegate void OnLoginSuccess();
        public delegate void OnLoginNewPasswordRequired();
        public delegate void OnLoginFailure(string errorMessage);
            

        public async void Login(string playerID, string password, OnLoginSuccess onLoginSuccess, OnLoginNewPasswordRequired onLoginNewPasswordRequired, OnLoginFailure onLoginFailure)
        {
            try
            {
                user = userPool.GetUser(playerID);
                var authRequest = new InitiateSrpAuthRequest()
                {
                    Password = password
                };
                Debug.Log("Calling StartWithSrpAuthAsync");
                var authFlowResponse = await user.StartWithSrpAuthAsync(authRequest);
                Debug.Log("StartWithSrpAuthAsync returns " + authFlowResponse.ToString());
                if(authFlowResponse.AuthenticationResult == null)
                {
                    string message = "Error attempting to log in player: ";
                    if(authFlowResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
                    {
                        Debug.Log("New password required");
                        sessionID = authFlowResponse.SessionID;
                        MainThreadDispatcher.Q(()=>{onLoginNewPasswordRequired(); });
                    }
                    else if(authFlowResponse.ChallengeName != null)
                    {
                        message += "Unexpected challenge occurred (auth flow for this sample doesn't handle challenges.) Challenge is " + authFlowResponse.ChallengeName;
                        Debug.LogError(message);
                        MainThreadDispatcher.Q(()=>{onLoginFailure(message);});
                    }
                    else
                    {
                        message += "Unknown error";
                        Debug.LogError(message);
                        MainThreadDispatcher.Q(()=>{onLoginFailure(message);});
                    }
                }
                else
                {
                    Debug.Log($"Player {playerID} logged in successfully!");
                    AuthenticationTokens = authFlowResponse.AuthenticationResult;
                    MainThreadDispatcher.Q(()=>{onLoginSuccess();});
                }
            }
            catch(Exception ex)
            {
                string message = "Error attempting to login player: " + ex.Message;
                Debug.LogError(message);
                MainThreadDispatcher.Q(()=>{onLoginFailure(message); });
            }
        }

        public async void ProvideNewPassword(string playerID, string password, OnLoginSuccess onLoginSuccess, OnLoginFailure onLoginFailure)
        {
            try
            {
                var newPasswordRequest = new RespondToNewPasswordRequiredRequest()
                {
                    SessionID = sessionID,
                    NewPassword = password
                };
                
                Debug.Log("Calling RespondToNewPasswordRequiredAsync");
                var authFlowResponse = await user.RespondToNewPasswordRequiredAsync(newPasswordRequest);
 
                if(authFlowResponse.AuthenticationResult == null)
                {
                    string message = "Error attempting to log in player: ";
                    if(authFlowResponse.ChallengeName != null)
                    {
                        message += "Unexpected challenge occurred (auth flow for this sample doesn't handle challenges.) Challenge is " + authFlowResponse.ChallengeName;
                    }
                    else
                    {
                        message += "Unknown error";
                    }                
                    Debug.LogError(message);
                    MainThreadDispatcher.Q(()=>{onLoginFailure(message);});
                }
                else
                {
                    Debug.Log($"Player {playerID} logged in successfully!");
                    AuthenticationTokens = authFlowResponse.AuthenticationResult;
                    MainThreadDispatcher.Q(()=>{onLoginSuccess();});
                }
            }
            catch(Exception ex)
            {
                string message = "Error attempting to login player: " + ex.Message;
                Debug.LogError(message);
                MainThreadDispatcher.Q(()=>{onLoginFailure(message); });
            }
        }

        public delegate void OnCreateAccountSuccess();
        public delegate void OnCreateAccountFailure(string errorMessage);

        public async void CreateAccount(string playerID, string password, OnCreateAccountSuccess onCreateAccountSuccess, OnCreateAccountFailure onCreateAccountFailure)
        {
            try
            {
                PlayerID = playerID;

                var signUpRequest = new SignUpRequest()
                {
                    ClientId = AWSStaticConfiguration.APP_CLIENT_ID,
                    Username = playerID,
                    Password = password
                };

                Debug.Log("Calling SignUpAsync");
                var signUpResult = await ipClient.SignUpAsync(signUpRequest);
                
                Debug.Log("SignUpRequest returns " + signUpResult.ToString());
                
                if(signUpResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    MainThreadDispatcher.Q(()=>{onCreateAccountSuccess(); });
                }
                else
                {
                    string message = "Error attempting to sign up player: HTTP Error Code " + signUpResult.HttpStatusCode.ToString();
                    Debug.LogError(message);
                    MainThreadDispatcher.Q(()=>{onCreateAccountFailure(message); });
                }
            }
            catch(Exception ex)
            {
                string message = "Error attempting to sign up player: " + ex.Message;
                Debug.LogError(message);
                MainThreadDispatcher.Q(()=>{onCreateAccountFailure(message); });
            }
        }

        public delegate void OnConfirmationSuccess();
        public delegate void OnConfirmationFailure(string errorMessage);

        public async void ConfirmSignupRequest(string username, string code, OnConfirmationSuccess onConfirmationSuccess, OnConfirmationFailure onConfirmationFailure )
        {
            try
            {
                ConfirmSignUpRequest confirmRequest = new ConfirmSignUpRequest()
                {
                    ClientId = AWSStaticConfiguration.APP_CLIENT_ID,
                    Username = username,
                    ConfirmationCode = code
                };

                Debug.Log("Calling ConfirmSignUpAsync");
                var confirmResult = await ipClient.ConfirmSignUpAsync(confirmRequest);

                Debug.Log("ConfirmSignUpRequest returns: " + confirmResult.ToString());

                if(confirmResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    MainThreadDispatcher.Q(()=>{onConfirmationSuccess(); });
                }
                else
                {
                    string message = "Error attempting to confirm player: HTTP Error Code " + confirmResult.HttpStatusCode.ToString();
                    Debug.LogError(message);
                    MainThreadDispatcher.Q(()=>{onConfirmationFailure(message); });
                }
            }
            catch(Exception ex)
            {
                string message = "Error attempting to confirm player: " + ex.Message;
                Debug.LogError(message);
                MainThreadDispatcher.Q(()=>{onConfirmationFailure(message); });
            }
        }

        public static PlayerIdentificationSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlayerIdentificationSystem();

                }
                return _instance;
            }
        }
        private static PlayerIdentificationSystem _instance;

    }
}
