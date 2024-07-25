// Copyright (c) 2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Core;
using System;

namespace AccelByte.ThirdParties.Apple
{
    public class AccelByteAppleImp : IAppleImp
    {
        public AccelByteAppleImp()
        {
            AppleExtension.Initialize();
        }

        public Models.AccelByteResult<GetAppleTokenResult, Core.Error> GetAppleIdToken()
        {
            var retval = new Models.AccelByteResult<GetAppleTokenResult, Error>();
            Models.AccelByteResult<GetAppleTokenResult, AccelByteAppleError> tempResult = AppleExtension.GetSignInToken();
            tempResult.OnSuccess(result =>
            {
                retval.Resolve(result);
            }).OnFailed(result =>
            {
                retval.Reject(result);
            });

            return retval;
        }
    }
}
