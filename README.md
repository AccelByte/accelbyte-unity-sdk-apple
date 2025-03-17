# AccelByte Unity SDK Apple #

Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
This is licensed software from AccelByte Inc, for limitations
and restrictions contact your company contract manager.

# Overview
Unity SDK Apple is an extension package to enable Accelbyte SDK support for Apple. This plugin support the following features:

## Prerequisiste ##
Require ([AccelByte Unity SDK](https://github.com/AccelByte/accelbyte-unity-sdk)) package. Minimum version: 17.5.1

For more information about configuring AccelByte Unity SDK, see [Install and configure the SDK](https://docs.accelbyte.io/gaming-services/getting-started/setup-game-sdk/unity-sdk/#install-and-configure).

## How to Install ##
1. Import "Sign in with Apple Plugin for Unity" from [Asset Store](https://assetstore.unity.com/packages/tools/integration/sign-in-with-apple-plugin-for-unity-152088)
2. In your Unity project, go to `Window > Package Manager`.
3. Click the + icon on the Package Manager window and click `Add package from git URL...`
4. Paste the following link into the URL field and click Add: `https://github.com/AccelByte/accelbyte-unity-sdk-apple.git`
5. Install `AppleGamesPlugin-AccelByte.unitypackage` inside `_Install` directory.
6. Add assembly reference of `Assets/AccelByteExtensions/Apple/com.AccelByte.AppleExtension` to your project.
7. Access AccelByte Apple API from `AccelByte.ThirdParties.Apple.AccelByteApple`

# Features Usage #

## Sign In With Apple ##

We provide easier way to let the player perfrom Sign in With Apple platform.
Therefore player doesn't need to register a new account to AGS to utilize the AGS features.

### Code Implementation ###
1. Header Initialization
```csharp
using AccelByte.Core;
using AppleAuth;
using AppleAuth.Native;
using UnityEngine;
```

2. Get Apple Id Token
```csharp
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
        UnityEngine.Debug.Log("Obtain Apple Id Token Success");
    })
    .OnFailed(result =>
    {
        UnityEngine.Debug.LogWarning("Obtain Apple Id Token Failed");
    });
}

```

3. Login to AGS
```csharp
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
                UnityEngine.Debug.LogError($"Failed to Login with Apple Platfrom [{result.Error.error}]: {result.Error.error_description}");
                return;
            }

            UnityEngine.Debug.Log("Login with AccelByte IAM success");
        });
    }
}
```

The full script on the package sample named "Sign in with Apple".

## In-App Purchasing ##

### Overview ###

There are three kind of `ProductType` based on [Unity Documentation](https://docs.unity3d.com/Manual/UnityIAPDefiningProducts.html). 
Previously we support synchronize consumable, non-consumable entitlements with AccelByte server.
And now also support subscription as well.
However, it requires additional setup through admin portal to change the apple store integration configuration from `V1`(default) into `V2`.

### Configure Your Game ###

> Please contact AccelByte support for guideline document

### Prerequisite ###

Import package [UnityPurchasing](https://docs.unity3d.com/Packages/com.unity.purchasing@4.8/manual/index.html) library to the project. 
This plugin is tested using UnityPurchasing v4.8.0.

Please refers to official [Unity documentation](https://docs.unity3d.com/Manual/UnityIAPSettingUp.html) on how to install it.

> **Important** : Ensure that the player is already **Logged in** using AGS service before using this feature. You may check player's login status by using this snippet `bool isLoggedIn = AccelByteSDK.GetClientRegistry().GetApi().GetUser().Session.IsValid();` and `isLoggedIn` should return `true`.

### Code Implementation ###
1. Sign in With Apple, please refer to [previous part](https://github.com/AccelByte/accelbyte-unity-sdk-apple?tab=readme-ov-file#sign-in-with-apple)

2. Please create `MonoBehavior` class implementing `IDetailedStoreListener`. Unity IAP will handle the purchase and trigger callbacks using this interface. Then prepare the following variables
```csharp
IStoreController storeController;

public Button BuyGoldButton;
public Button BuyWeaponButton;
public Button BuySeasonPassButton;

private string goldProductId = "item_gold"; // assume that the registered consumable product id is named Item_gold
private ProductType goldProductType = ProductType.Consumable;
private string weaponProductId = "item_weapon"; // assume that the registered non-consumable product id is named item_weapon
private ProductType weaponProductType = ProductType.NonConsumable;
private string seasonPassProductId = "item_season_pass"; // assume that the registered subscription product id is named item_season_pass
private ProductType seasonPassProductType = ProductType.Subscription;

private AccelByte.Api.User user;
private AccelByte.Api.Entitlement entitlement;
```

3. Prepare three [Buttons](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/script-Button.html) to trigger the purchasing event. Using Unity Editor's inspector, attach those buttons into `public Button BuyGoldButton;`, `public Button BuyWeaponButton;`, and `public Button BuySeasonPassButton;`.

4. Initialize Purchasing. 
```csharp
void Start()
{
    InitializePurchasing();
}

void InitializePurchasing()
{
    var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

    //Add products that will be purchasable and indicate its type.
    builder.AddProduct(goldProductId, goldProductType);
    builder.AddProduct(weaponProductId, weaponProductType);
    builder.AddProduct(seasonPassProductId, seasonPassProductType);
    UnityPurchasing.Initialize(this, builder);

    // Assign its ApplicationUsername
    if (!string.IsNullOrEmpty(AccelByteSDK.GetClientRegistry().GetApi().GetUser().Session.UserId))
    {
        var uid = System.Guid.Parse(AccelByteSDK.GetClientRegistry().GetApi().GetUser().Session.UserId);
        appleExtensions.SetApplicationUsername(uid.ToString());
    }
    else
    {
        Debug.LogError($"Player is not Logged In. Several features may not work properly");
    }
}

public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
{
    Debug.Log("In-App Purchasing successfully initialized");
    storeController = controller;
    appleExtensions = extensions.GetExtension<IAppleExtensions>();
}
```

7. Prepare several functions that will be trigger the purchasing event
```csharp
private void BuyGold()
{
    storeController.InitiatePurchase(productId);
}

private void BuyWeapon()
{
    storeController.InitiatePurchase(weaponProductId);
}

private void BuySeasonPass()
{
    storeController.InitiatePurchase(seasonPassProductId);
}
```

8. Assign each buttons
```csharp
void Start()
{
    ButtonAssigning();
}

void ButtonAssigning()
{
    BuyGoldButton.onClick.AddListener(BuyGold);
    BuyWeaponButton.onClick.AddListener(BuyWeapon);
    BuySeasonPassButton.onClick.AddListener(BuySeasonPass);
}
```

9. Handle Process Purchase. Please note that it **must** return `PurchaseProcessingResult.Pending` because purchased item will be synchronized with AccelByte's Backend. [reference](https://docs.unity3d.com/2021.3/Documentation/Manual/UnityIAPProcessingPurchases.html). If client successfully purchase item from Apple, `ProcessPurchase` will be triggered, else `OnPurchaseFailed` will be triggered. Also **note** that subscription is treated differently with consumable and non-consumables.
```csharp
public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
{
    var product = purchaseEvent.purchasedProduct;

    Debug.Log($"Purchase Complete - Product: {product.definition.id}");
    if (product.definition.type == ProductType.Subscription)
    {
        AGSSubscriptionEntitlementSync(product);
    }
    else
    {
        AGSEntitlementSync(product);
    }
    
    return PurchaseProcessingResult.Pending;
}

public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
{
    Debug.LogError($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
}
```

10. Sync Purchased Product with AGS
```csharp
private void AGSEntitlementSync(Product purchasedProduct)
{
    // Please note that Sync will work after the player is logged in using AB service
    try
    {
        string productId = purchasedProduct.definition.id;
        string transactionId = purchasedProduct.appleOriginalTransactionID;
        string receiptData = JObject.Parse(purchasedProduct.receipt)["Payload"].ToString();

        AccelByteSDK.GetClientRegistry().GetApi().GetEntitlement().SyncMobilePlatformPurchaseApple(productId
            , transactionId
            , receiptData
            , result =>
            {
                FinalizePurchase(purchasedProduct);
                if (result.IsError)
                {
                    Debug.Log($"{productId} failed to sync with AB [{result.Error.Code}]:{result.Error.Message}");
                    return;
                }
                Debug.Log($"{productId} is synced with AB");
            });
    }
    catch (Exception e)
    {
        FinalizePurchase(purchasedProduct);
        Debug.LogError($"Failed to sync with AB {e.Message}");
    }
}

private void AGSSubscriptionEntitlementSync(Product purchasedSubscription)
{
    // Please note that Sync will work after the player is logged in using AB service
    try
    {
        AccelByteSDK.GetClientRegistry().GetApi().GetEntitlement().SyncMobilePlatformSubscriptionApple(purchasedSubscription.appleOriginalTransactionID
            , result =>
            {
                FinalizePurchase(purchasedSubscription);
                if (result.IsError)
                {
                    Debug.Log($"{purchasedSubscription.definition.id} failed to sync with AB [{result.Error.Code}]:{result.Error.Message}");
                    return;
                }
                Debug.Log($"{purchasedSubscription.definition.id} is synced with AB");
            });
    }
    catch (Exception e)
    {                
        FinalizePurchase(purchasedSubscription);
        Debug.LogError($"Failed to sync with AB {e.Message}");
    }
}
```

11. Finalize Pending Purchase
```csharp
private void FinalizePurchase(Product purchasedProduct)
{
    Debug.Log($"Confirm Pending Purchase for: {purchasedProduct.definition.id}");
    storeController.ConfirmPendingPurchase(purchasedProduct);
}
```

The full script on the package sample named "In App Purchase".