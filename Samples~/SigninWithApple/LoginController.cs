// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using UnityEngine.UI;

namespace AccelByte.SdkApple.Samples.SignInWithApple
{
    public class LoginController : MonoBehaviour
    {
        public LoginHandler LoginHandler;
        public Button LoginButton;
        public Text LoginStatus;
        public Button LogoutButton;

        private LoginHandler.LoginResult lastLoginResult;

        private void Start()
        {
            LoginButton?.onClick.AddListener(IntegratedLogin);
            LogoutButton?.onClick.AddListener(Logout);
            UpdateUI();
            LoggedOutState();
        }

        private void IntegratedLogin()
        {
            StartCoroutine(LoginHandler.RunAutoLogin(() =>
            {
                lastLoginResult = LoginHandler.GetCurrentLoginResult();
                UpdateUI();
                LoggedInState();
            }));
        }

        private void Logout()
        {
            LoginHandler.AGSLogout(() =>
            {
                lastLoginResult = LoginHandler.GetCurrentLoginResult();
                UpdateUI();
                LoggedOutState();
            });
        }

        private void UpdateUI()
        {
            string message = "Login Status: ";
            switch (lastLoginResult)
            {
                case LoginHandler.LoginResult.Failed:
                    message += "Login Failed";
                    break;
                case LoginHandler.LoginResult.Success:
                    message += "Login Success";
                    break;
                case LoginHandler.LoginResult.None:
                default:
                    message += "User not signed in";
                    break;
            }
            LoginStatus.text = message;
        }

        private void LoggedInState()
        {
            LoginButton?.gameObject.SetActive(false);
            LogoutButton?.gameObject.SetActive(true);
        }

        private void LoggedOutState()
        {
            LoginButton?.gameObject.SetActive(true);
            LogoutButton?.gameObject.SetActive(false);
        }
    }
}
