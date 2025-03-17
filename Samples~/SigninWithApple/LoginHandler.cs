// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using AccelByte.Core;
using AppleAuth;
using AppleAuth.Native;
using UnityEngine;

namespace AccelByte.SdkApple.Samples.SignInWithApple
{
    public class LoginHandler : MonoBehaviour
    {
        private IAppleAuthManager appleAuthManager;
        private string appleIdToken;
        private bool getAppleSignInTokenIsRunning = false;

        public enum LoginResult
        {
            None,
            Success,
            Failed
        }

        private LoginResult currentLoginResult;

        private void Awake()
        {
            InitializeAppleComponent();
        }

        private void InitializeAppleComponent()
        {
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                var deserializer = new PayloadDeserializer();
                appleAuthManager = new AppleAuthManager(deserializer);
            }
        }

        private void Update()
        {
            if (appleAuthManager != null)
            {
                appleAuthManager.Update();
            }
        }

        public LoginResult GetCurrentLoginResult()
        {
            return currentLoginResult;
        }

        /// <summary>
        /// Run Apple Login first, then run AGS Login
        /// </summary>
        /// <param name="postLoginAction">Trigger something after login is done</param>
        public IEnumerator RunAutoLogin(Action postLoginAction)
        {
            GetAppleIdToken();

            //We need to wait until the process is done
            while (getAppleSignInTokenIsRunning)
            {
                yield return new WaitForSeconds(0.5f);
            }

            AGSLogin(appleIdToken, postLoginAction);
            yield return null;
        }

        /// <summary>
        /// Get Apple Sign in Token.
        /// It will be utilized to AGS login Later
        /// Note: this function needs to be run in MainThread
        /// </summary>
        private void GetAppleIdToken()
        {
            getAppleSignInTokenIsRunning = true;
            AccelByte.ThirdParties.Apple.AccelByteApple.GetAppleSignInToken()
                .OnSuccess(result =>
                {
                    appleIdToken = result.AppleIdToken;
                    UnityEngine.Debug.Log("Obtain Apple Id Token Success");
                })
                .OnFailed(result =>
                {
                    UnityEngine.Debug.LogWarning($"Obtain Apple Id Token Failed: {result.Code} {result.Message}");
                })
                .OnComplete(() =>
                {
                    getAppleSignInTokenIsRunning = false;
                });
        }

        /// <summary>
        /// Trigger AGS Login. It requires the fetched Apple Id Token
        /// </summary>
        /// <param name="idToken">fetched Apple IdToken</param>
        /// <param name="postLoginAction">Trigger something after login is done</param>
        private void AGSLogin(string idToken, Action postLoginAction)
        {
            if (!string.IsNullOrEmpty(idToken))
            {
                var platformType = new AccelByte.Models.LoginPlatformType(Models.PlatformType.Apple);

                AccelByteSDK.GetClientRegistry().GetApi().GetUser().LoginWithOtherPlatformV4(
                    platformType
                    , idToken
                    , result =>
                    {
                        if (result.IsError)
                        {
                            currentLoginResult = LoginResult.Failed;
                            postLoginAction?.Invoke();
                            UnityEngine.Debug.LogError($"Failed to Login with Apple Platfrom [{result.Error.error}]: {result.Error.error_description}");
                            return;
                        }

                        currentLoginResult = LoginResult.Success;
                        postLoginAction?.Invoke();
                        UnityEngine.Debug.Log("Login with AccelByte IAM success");
                    });
            }
            else
            {
                currentLoginResult = LoginResult.Failed;
                postLoginAction?.Invoke();
                UnityEngine.Debug.LogWarning("Failed to login with AGS due to missing AppleIdToken");
            }
        }

        /// <summary>
        /// Trigger AGS Logout
        /// </summary>
        /// <param name="postLogoutAction">Trigger something after logout is done</param>
        public void AGSLogout(Action postLogoutAction)
        {
            AccelByteSDK.GetClientRegistry().GetApi().GetUser().Logout(logoutResult =>
            {
                if (logoutResult.IsError)
                {
                    Debug.LogWarning($"Failed to logout");
                    return;
                }

                currentLoginResult = LoginResult.None;
                Debug.Log($"Logout Success");
                postLogoutAction?.Invoke();
            });
        }
    }
}
