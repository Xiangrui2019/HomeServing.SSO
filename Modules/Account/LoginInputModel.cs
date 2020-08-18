// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HomeServing.SSO.Modules.Account
{
    public class LoginInputModel
    {
        [Description("用户名")]
        [Required] public string Username { get; set; }
        [Description("密码")]
        [Required] public string Password { get; set; }
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
    }
}