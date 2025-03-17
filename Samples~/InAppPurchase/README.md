## README - AccelByte Unity SDK Apple Samples - In App Purchase

This sample showcases how to integrate Unity In-App purchase with AGS. It will act as thirdparty server that hold each player entitlement. In this sample, there will be only three product types (consumable, non-consumable, and subscription).

### Important Components
1. InAppPurchaseHandler.cs
This script is responsible for handling In App Purchase and synchronizing it with AGS. Referring to [Unity Documentation about Product types](https://docs.unity3d.com/Manual/UnityIAPDefiningProducts.html), there are three items with three different types. `GoldProductId` & `GoldProductType` will represent `Consumables`, `WeaponProductId` & `WeaponProductType` will represent `Non-Consumables` and `SeasonPassProductId` & `SeasonProductType` will represent `Subscription`. Each product has their own `BuyButton`, `SyncButton` and `CurrentValueText`. `BuyButton` will trigger the In App Purchase mechanism from Unity. `SyncButton` will get the latest value that the prayer holds. At last, the values are written in the `CurrentValueText`.

**Please Note** This sample only covers three Entitlements with three different products. It might not work if there are more or less products registered.

2. LoginController.cs
This script is responsible to control the SampleScene during Pre-Login and After-Login.

### Additional Notes
This sample is built using the IPhone 11 (Portrait) scene. If this sample is built using the previous IPhone model, some UI Elements (Buttons and Text) may disappear.