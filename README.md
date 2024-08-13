# AccelByte Unity SDK Apple #

Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
This is licensed software from AccelByte Inc, for limitations
and restrictions contact your company contract manager.

# Overview
Unity SDK Apple is an extension package to enable Accelbyte SDK support for Apple. This plugin support the following features:

## Prerequisiste ##
Require ([AccelByte Unity SDK](https://github.com/AccelByte/accelbyte-unity-sdk)) package. Minimum version: 16.24.0.

For more information about configuring AccelByte Unity SDK, see [Install and configure the SDK](https://docs.accelbyte.io/gaming-services/getting-started/setup-game-sdk/unity-sdk/#install-and-configure).

## How to Install ##
1. Import "Sign in with Apple Plugin for Unity" from [Asset Store](https://assetstore.unity.com/packages/tools/integration/sign-in-with-apple-plugin-for-unity-152088)
2. Clone this repository and install the package using UPM with "Add package from disk" option then select the `package.json`.
3. Install `AppleGamesPlugin-AccelByte.unitypackage` inside `_Install` directory.
4. Add assembly reference of `Assets/AccelByteExtensions/Apple/com.AccelByte.AppleExtension` to your project.
5. Access AccelByte Apple API from `AccelByte.ThirdParties.Apple.AccelByteApple`

# Features Usage #

## Sign In With Apple ##

We provide easier way to let the player perfrom Sign in With Apple platform.
Therefore player doesn't need to register a new account to AGS to utilize the AGS features.

## Configure Your Game ##

To integrate Sign in With Apple and AGS to your game, please follow the [official AGS documentation](https://docs.accelbyte.io/gaming-services/services/access/authentication/apple-identity/#set-up-apple-configuration).
You may skip the `Set up web login` part because this integration is considered in-game login.

### Code Implementation ###
1. Header Initialization

```
using AccelByte.Core;
using AppleAuth;
using AppleAuth.Native;
using UnityEngine;
```

2. Get Apple Id Token

```
private IAppleAuthManager appleAuthManager = null;
private string appleIdToken = "";

private void Start()
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

private void GetAppleIdToken()
{
    AccelByte.ThirdParties.Apple.AccelByteApple.GetAppleSignInToken().OnSuccess(result =>
    {
		appleIdToken = result.AppleIdToken;
        AccelByteDebug.Log("Obtain Apple Id Token Success");
    })
    .OnFailed(result =>
    {
        AccelByteDebug.LogError("Obtain Apple Id Token Failed");
    });
}

```

3. Login to AGS

```
private void AGSLogin()
{
    if (!string.IsNullOrEmpty(appleIdToken))
    {
        AccelByteSDK.GetClientRegistry().GetApi().GetUser().LoginWithOtherPlatformV4(
            AccelByte.Models.PlatformType.Apple
            , appleIdToken
            , result =>
        {
            if (result.IsError)
            {
                AccelByteDebug.LogError($"Failed to Login with Apple Platfrom [{result.Error.error}]: {result.Error.error_description}");
                return;
            }

            AccelByteDebug.Log("Login with AccelByte IAM success");
        });
    }
}
```