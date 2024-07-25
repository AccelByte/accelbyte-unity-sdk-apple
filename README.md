# AccelByte Unity SDK Apple #
Unity SDK Apple is an extension package to enable Accelbyte SDK support for Apple.

## Prerequisiste ##
Require ([AccelByte Unity SDK](https://github.com/AccelByte/accelbyte-unity-sdk)) package. Minimum version: 16.24.0.

## How to Install ##
1. Import "Sign in with Apple Plugin for Unity" from [Asset Store](https://assetstore.unity.com/packages/tools/integration/sign-in-with-apple-plugin-for-unity-152088)
2. Clone this repository and install the package using UPM with "Add package from disk" option then select the `package.json`.
3. Install `AppleGamesPlugin-AccelByte.unitypackage` inside `_Install` directory.
4. Add assembly reference of `Assets/AccelByteExtensions/Apple/com.AccelByte.AppleExtension` to your project.
5. Access AccelByte Apple API from `AccelByte.ThirdParties.Apple.AccelByteApple`

## Sample Implementation ##
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

private void AppleLogin()
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