// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Core;
using AccelByte.SdkApple.Samples.SignInWithApple;
using UnityEngine;
using UnityEngine.UI;

namespace AccelByte.SdkApple.Samples.InAppPurchase
{
    public class InAppPurchaseController : MonoBehaviour
    {
        public LoginHandler LoginHandler;
        public Button LogoutButton;
        public Button LoginButton;
        public GameObject IAPPanel;
        public GameObject LoginPanel;

        private void Start()
        {
            ButtonAssigning();
            LoggedOutState();
        }

        private void ButtonAssigning()
        {
            LoginButton?.onClick.AddListener(Login);
            LogoutButton?.onClick.AddListener(Logout);
        }

        private void Login()
        {
            StartCoroutine(LoginHandler.RunAutoLogin(() =>
            {
                if (LoginHandler.GetCurrentLoginResult() == LoginHandler.LoginResult.Success)
                {
                    Debug.Log($"Login Success");
                    LoggedInState();
                }
            }));
        }

        private void Logout()
        {
            AccelByteSDK.GetClientRegistry().GetApi().GetUser().Logout(logoutResult =>
            {
                if (logoutResult.IsError)
                {
                    Debug.LogWarning($"Failed to Logout [{logoutResult.Error.Code}] : {logoutResult.Error.Message}");
                    return;
                }
                
                LoggedOutState();
                Debug.Log($"Logout Success");
            });
        }

        private void LoggedInState()
        {
            LoginPanel.SetActive(false);
            IAPPanel.SetActive(true);            
        }

        private void LoggedOutState()
        {
            LoginPanel.SetActive(true);
            IAPPanel.SetActive(false);
        }
    }
}