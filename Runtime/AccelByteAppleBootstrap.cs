// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
namespace AccelByte.ThirdParties.Apple
{
    [Preserve]
    internal static class AccelByteAppleBootstrap
    {
        private static AccelByteAppleImp imp;

        [Preserve, RuntimeInitializeOnLoadMethod]
        private static void StartAccelByteSDK()
        {
            AttachImp();
        }

        private static void AttachImp()
        {
            AccelByteApple.ImpGetter = () =>
            {
                if (imp == null)
                {
                    imp = new AccelByteAppleImp();
                }
                return imp;
            };
        }
    }
}