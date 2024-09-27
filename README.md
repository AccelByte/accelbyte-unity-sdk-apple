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

## In-App Purchasing ##

### Configure Your Game ###

> Please contact AccelByte support for guideline document

### Prerequisite ###

Import package [UnityPurchasing](https://docs.unity3d.com/Packages/com.unity.purchasing@4.8/manual/index.html) library to the project. 
This plugin is tested using UnityPurchasing v4.8.0.

Please refers to official [Unity documentation](https://docs.unity3d.com/Manual/UnityIAPSettingUp.html) on how to install it.

### Code Implementation ###
1. Sign in With Apple, please refer to [previous part](https://github.com/AccelByte/accelbyte-unity-sdk-apple?tab=readme-ov-file#sign-in-with-apple)

2. Please create `MonoBehavior` class implementing `IDetailedStoreListener`. Unity IAP will handle the purchase and trigger callbacks using this interface. Then prepare the following variables
```csharp
public Button buyButton;
    
IStoreController storeController;
private string productId = "item_gold"; // assume that the registered product id is named Item_gold
private ProductType productType = ProductType.Consumable; // assume that "item_gold" is a Consumables
```

3. Prepare a [Button](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/script-Button.html) to trigger the purchasing event. Using Unity Editor's inspector, attach this button into `public Button buyButton;` 

4. Prepare a function that will be trigger the purchasing event
```csharp
private void BuyGold()
{
    storeController.InitiatePurchase(productId);
}
```

5. Initialize Purchasing. 
```csharp
void Start()
{
    InitializePurchasing();
}

void InitializePurchasing()
{
    var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

    builder.AddProduct(productId, productType);
    UnityPurchasing.Initialize(this, builder);

    buyButton.onClick.AddListener(BuyGold);
}

public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
{
    Debug.Log("In-App Purchasing successfully initialized");
    storeController = controller;
}
``` 

6. Handle Process Purchase. Please note that it **must** return `PurchaseProcessingResult.Pending` because purchased item will be synchronized with AccelByte's Backend. [reference](https://docs.unity3d.com/2021.3/Documentation/Manual/UnityIAPProcessingPurchases.html). If client successfully purchase item from Apple, `ProcessPurchase` will be triggered, else `OnPurchaseFailed` will be triggered
```csharp
public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
{
    var product = purchaseEvent.purchasedProduct;

    Debug.Log($"Purchase Complete - Product: {product.definition.id}");
    AGSEntitlementSync(product);
    
    return PurchaseProcessingResult.Pending;
}

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
    }
```
7. Sync Purchased Product with AGS
```csharp
private void AGSEntitlementSync(Product purchasedProduct)
{
    // Please note that Sync will work after the player is logged in using AB service
    try
    {
        PlatformSyncMobileApple request = new PlatformSyncMobileApple()
        {
            productId = purchasedProduct.definition.id,
            transactionId = purchasedProduct.transactionID,
            receiptData = JObject.Parse(purchasedProduct.receipt)["Payload"].ToString()
        };
        
        AccelByteSDK.GetClientRegistry().GetApi().GetEntitlement().SyncMobilePlatformPurchaseApple(request
            , result =>
            {
                if (result.IsError)
                {
                    Debug.Log($"{request.productId} failed to sync with AB [{result.Error.Code}]:{result.Error.Message}");
                    return;
                }
                Debug.Log($"{request.productId} is synced with AB");
                
                FinalizePurchase(purchasedProduct);
            });
    }
    catch (Exception e)
    {
        Debug.LogError($"Failed to sync with AB {e.Message}");
    }
}
```
8. Finalize Pending Purchase
```csharp
private void FinalizePurchase(Product purchasedProduct)
{
    Debug.Log($"Confirm Pending Purchase for: {purchasedProduct.definition.id}");
    storeController.ConfirmPendingPurchase(purchasedProduct);
}
```

This is the complete script
```csharp
using System;
using AccelByte.Core;
using AccelByte.Models;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;

public class InAppPurchaseHandler : MonoBehaviour, IDetailedStoreListener
{
    public Button buyButton;
    
    IStoreController storeController; // The Unity Purchasing system.
    private string productId = "item_gold";
    private ProductType productType = ProductType.Consumable;
    
    void Start()
    {
        InitializePurchasing();
    }
    
    /// <summary>
    /// Trigger purchasing initialization
    /// </summary>
    void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        //Add products that will be purchasable and indicate its type.
        builder.AddProduct(productId, productType);
        UnityPurchasing.Initialize(this, builder);

        //Attach a listener to trigger purchasing event
        buyButton.onClick.AddListener(BuyGold);
    }
    
    /// <summary>
    /// A callback that will be triggered when the Initialization step is done
    /// Its part of IDetailedStoreListener
    /// No need to attach it anywhere
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("In-App Purchasing successfully initialized");
        storeController = controller;
    }
    
    /// <summary>
    /// A callback that will be triggered when the Initialization step is failed
    /// Its part of IDetailedStoreListener
    /// No need to attach it anywhere
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"Failed to initialize In-App Purchasing [{error}]");
    }
    
    /// <summary>
    /// A callback will be triggered when the Initialization step is failed, with detailed message
    /// Its part of IDetailedStoreListener
    /// No need to attach it anywhere
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"Failed to initialize In-App Purchasing [{error}]:{message}");
    }
    
    /// <summary>
    /// A callback will be triggered when the purchasing is success
    /// Its part of IDetailedStoreListener
    /// No need to attach it anywhere
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        // Retrieve the purchased product
        var product = purchaseEvent.purchasedProduct;

        Debug.Log($"Purchase Complete - Product: {product.definition.id}");
        AGSEntitlementSync(product);
        
        // Because we're going to sync it with AB's server, it must return PurchaseProcessingResult.Pending
        // For detailed explanation, please refer to : https://docs.unity3d.com/2021.3/Documentation/Manual/UnityIAPProcessingPurchases.html
        return PurchaseProcessingResult.Pending;
    }
    
    /// <summary>
    /// A callback will be triggered when the purchasing is failed
    /// Its part of IDetailedStoreListener
    /// No need to attach it anywhere
    /// </summary>
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
    }
    
    /// <summary>
    /// A callback will be triggered when the purchasing is failed, with a failure Description
    /// Its part of IDetailedStoreListener
    /// No need to attach it anywhere
    /// </summary>
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureDescription: {failureDescription}");
    }
    
    /// <summary>
    /// Confirm the pending purchase after sync with AB is done
    /// It is required because there is a synchronization step
    /// </summary>
    private void FinalizePurchase(Product purchasedProduct)
    {
        Debug.Log($"Confirm Pending Purchase for: {purchasedProduct.definition.id}");
        storeController.ConfirmPendingPurchase(purchasedProduct);
    }
    
    /// <summary>
    /// This function will trigger the purchasing event
    /// </summary>
    private void BuyGold()
    {
        storeController.InitiatePurchase(productId);
    }

    /// <summary>
    /// Synchronize the purchased product with AccelByte's server using AccelByte's SDK
    /// </summary>
    /// <param name="purchasedProduct">A successful purchased product</param>
    private void AGSEntitlementSync(Product purchasedProduct)
    {
        // Please note that Sync will work after the player is logged in using AB service
        // Please refer to https://github.com/AccelByte/accelbyte-unity-sdk-apple?tab=readme-ov-file#sign-in-with-apple for implementation
        try
        {
            PlatformSyncMobileApple request = new PlatformSyncMobileApple()
            {
                productId = purchasedProduct.definition.id,
                transactionId = purchasedProduct.transactionID,
                receiptData = JObject.Parse(purchasedProduct.receipt)["Payload"].ToString()
            };
            
            AccelByteSDK.GetClientRegistry().GetApi().GetEntitlement().SyncMobilePlatformPurchaseApple(request
                , result =>
                {
                    if (result.IsError)
                    {
                        Debug.Log($"{request.productId} failed to sync with AB [{result.Error.Code}]:{result.Error.Message}");
                        return;
                    }
                    Debug.Log($"{request.productId} is synced with AB");
                    
                    FinalizePurchase(purchasedProduct);
                });
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to sync with AB {e.Message}");
        }
    }
}

```