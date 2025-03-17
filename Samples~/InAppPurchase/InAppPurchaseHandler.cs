// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using AccelByte.Core;
using AccelByte.Models;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;

namespace AccelByte.SdkApple.Samples.InAppPurchase
{
    public class InAppPurchaseHandler : MonoBehaviour, IDetailedStoreListener
    {
        IStoreController storeController; // The Unity Purchasing system.
        IAppleExtensions appleExtensions;
        
        private AccelByte.Api.User user;
        private AccelByte.Api.Entitlement entitlement;

        [Header("Buy/Purchasing Section")]
        public Button BuyGoldButton;
        public Button BuyWeaponButton;
        public Button BuySeasonPassButton;
        
        public string GoldProductId = "item_gold"; // assume that the registered consumable product id is named Item_gold
        public ProductType GoldProductType = ProductType.Consumable;
        public string WeaponProductId = "item_weapon"; // assume that the registered non-consumable product id is named item_weapon
        public ProductType WeaponProductType = ProductType.NonConsumable;
        public string SeasonPassProductId = "item_season_pass"; // assume that the registered subscription product id is named item_season_pass
        public ProductType SeasonPassProductType = ProductType.Subscription;
        
        [Header("Sync/Query Section")]
        public Button SyncCurrentGoldButton;
        public Button SyncCurrentWeaponButton;
        public Button SyncSeasonPassButton;

        public Text CurrentGoldText;
        public Text CurrentWeaponText;
        public Text CurrentSeasonPassText;
        
        void Start()
        {
            InitializeAGS();
            InitializePurchasing();
            ButtonAssigning();
            ResetValues();
        }

        void ResetValues()
        {
            CurrentGoldText.text = "Current Gold: ???";
            CurrentSeasonPassText.text = "Subscribe Season Pass: ???";
            CurrentWeaponText.text = "Have a weapon: ???";
        }

        /// <summary>
        /// Attach a listener to trigger each delegate functions 
        /// </summary>
        void ButtonAssigning()
        {
            BuyGoldButton?.onClick.AddListener(BuyGold);
            BuyWeaponButton?.onClick.AddListener(BuyWeapon);
            BuySeasonPassButton?.onClick.AddListener(BuySeasonPass);
            
            SyncCurrentGoldButton?.onClick.AddListener(SyncCurrentGold);
            SyncCurrentWeaponButton?.onClick.AddListener(SyncCurrentWeapon);
            SyncSeasonPassButton?.onClick.AddListener(SyncSeasonPass);
        }

        /// <summary>
        /// Trigger AccelByte SDK initialization
        /// </summary>
        void InitializeAGS()
        {
            user = AccelByteSDK.GetClientRegistry().GetApi().GetUser();
            entitlement = AccelByteSDK.GetClientRegistry().GetApi().GetEntitlement();
        }

        /// <summary>
        /// Trigger purchasing initialization
        /// </summary>
        void InitializePurchasing()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            //Add products that will be purchasable and indicate its type.
            builder.AddProduct(GoldProductId, GoldProductType);
            builder.AddProduct(WeaponProductId, WeaponProductType);
            builder.AddProduct(SeasonPassProductId, SeasonPassProductType);
            UnityPurchasing.Initialize(this, builder);

            if (!string.IsNullOrEmpty(user.Session.UserId))
            {
                var uid = System.Guid.Parse(user.Session.UserId);
                appleExtensions.SetApplicationUsername(uid.ToString());
            }
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
            appleExtensions = extensions.GetExtension<IAppleExtensions>();
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
            if (product.definition.type == ProductType.Subscription)
            {
                AGSSubscriptionEntitlementSync(product);
            }
            else
            {
                AGSEntitlementSync(product);
            }

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
            storeController.InitiatePurchase(GoldProductId);
        }

        private void BuyWeapon()
        {
            storeController.InitiatePurchase(WeaponProductId);
        }

        private void BuySeasonPass()
        {
            storeController.InitiatePurchase(SeasonPassProductId);
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
                string productId = purchasedProduct.definition.id;
                string transactionId = purchasedProduct.appleOriginalTransactionID;

                if (string.IsNullOrEmpty(transactionId))
                {
                    transactionId = purchasedProduct.transactionID;
                }
                
                string receiptData = JObject.Parse(purchasedProduct.receipt)["Payload"].ToString();

                entitlement.SyncMobilePlatformPurchaseApple(productId
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
                        
                        //Update latest text
                        if (productId == GoldProductId)
                        {
                            SyncCurrentGold();
                        }
                        else if (productId == WeaponProductId)
                        {
                            SyncCurrentWeapon();
                        }
                    });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to sync with AB {e.Message}");
                FinalizePurchase(purchasedProduct);
            }
        }

        /// <summary>
        /// Synchronize the purchased Subscription with AccelByte's server using AccelByte's SDK
        /// </summary>
        /// <param name="purchasedSubscription">A successful purchased subscription</param>
        private void AGSSubscriptionEntitlementSync(Product purchasedSubscription)
        {
            // Please note that Sync will work after the player is logged in using AB service
            // Please refer to https://github.com/AccelByte/accelbyte-unity-sdk-apple?tab=readme-ov-file#sign-in-with-apple for implementation
            try
            {
                entitlement.SyncMobilePlatformSubscriptionApple(purchasedSubscription.appleOriginalTransactionID
                    , result =>
                    {
                        FinalizePurchase(purchasedSubscription);
                        if (result.IsError)
                        {
                            Debug.Log($"{purchasedSubscription.definition.id} failed to sync with AB [{result.Error.Code}]:{result.Error.Message}");
                            return;
                        }
                        
                        Debug.Log($"{purchasedSubscription.definition.id} is synced with AB");
                        
                        // Update latest text
                        SyncSeasonPass();
                    });
            }
            catch (Exception e)
            {
                FinalizePurchase(purchasedSubscription);
                Debug.LogError($"Failed to sync with AB {e.Message}");
            }
        }

        /// <summary>
        /// This function will trigger the Syncchronizing event
        /// </summary>
        private void SyncCurrentGold()
        {
            AGSSyncCurrentEntitlements(GoldProductId, GoldProductType);
        }

        private void SyncCurrentWeapon()
        {
            AGSSyncCurrentEntitlements(WeaponProductId, WeaponProductType);
        }

        private void SyncSeasonPass()
        {
            AGSSyncCurrentEntitlements(SeasonPassProductId, SeasonPassProductType);
        }

        private void AGSSyncCurrentEntitlements(string productId, ProductType productType)
        {
            if (productType == ProductType.Subscription)
            {
                PlatformStoreId storeId = new PlatformStoreId(PlatformType.Apple);
                entitlement.QueryUserSubscription(storeId, user.Session.UserId, result =>
                {
                    if (result.IsError)
                    {
                        Debug.LogWarning($"Failed to Query Subscription [{result.Error.Code}]:{result.Error.Message}");
                        return;
                    }
                    
                    bool found = false;
                    foreach (var eInfo in result.Value.Data)
                    {
                        if (eInfo.SubscriptionProductId == productId)
                        {
                            found = true;
                            break;
                        }
                    }
                    
                    CurrentSeasonPassText.text = "Subscribe Season Pass: " + found.ToString().ToUpper();
                });
            }
            else if(productType == ProductType.Consumable)
            {
                entitlement.QueryUserEntitlements(callback: result =>
                {
                    if (result.IsError)
                    {
                        Debug.LogWarning($"Failed to Query Entitlement [{result.Error.Code}]:{result.Error.Message}");
                        return;
                    }
                    
                    int ownedGold = 0;
                    bool found = false;
                    foreach (var eInfo in result.Value.data)
                    {
                        if (eInfo.sku == productId)
                        {
                            ownedGold = eInfo.useCount;
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        CurrentGoldText.text = "Current Gold: " + ownedGold.ToString();
                    }
                    else
                    {
                        Debug.LogWarning($"user {user.Session.UserId} doesn't have {productId}");
                    }
                });
            }
            else if (productType == ProductType.NonConsumable)
            {
                entitlement.QueryUserEntitlements(callback: result =>
                {
                    if (result.IsError)
                    {
                        Debug.LogWarning($"Failed to Query Entitlement [{result.Error.Code}]:{result.Error.Message}");
                        return;
                    }

                    bool found = false;
                    foreach (var eInfo in result.Value.data)
                    {
                        if (eInfo.sku == productId)
                        {
                            found = true;
                            break;
                        }
                    }
                    
                    CurrentWeaponText.text = "Have a weapon: " + found.ToString().ToUpper();
                });
            }
        }
    }
}