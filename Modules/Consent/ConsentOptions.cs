// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace HomeServing.SSO.Modules.Consent
{
    public class ConsentOptions
    {
        public static bool EnableOfflineAccess = true;
        public static string OfflineAccessDisplayName = "离线访问";

        public static string OfflineAccessDescription =
            "访问您的应用程序和资源，即使您处于脱机状态";

        public static readonly string MustChooseOneErrorMessage = "您必须至少选择一个权限";
        public static readonly string InvalidSelectionErrorMessage = "错误的选择";
    }
}