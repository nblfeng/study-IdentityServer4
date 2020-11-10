// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;

namespace AuthCenter
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),

                new IdentityResource( "account", new[] { "account" } ),
                new IdentityResource( "authority", new[] { "authority" } ),
                new IdentityResource( "role", new[] { JwtClaimTypes.Role } ),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("scope1"),
                new ApiScope("scope2"),
            };

        public static IEnumerable<ApiResource> ApiResources => new ApiResource[]
        {
            new ApiResource
            {
                Name = "api",
                DisplayName = "api资源",
                ApiSecrets = { new Secret( "api.secret".Sha256() ) },
                Scopes = { "scope1" }
            }
        };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "user.client",
                    ClientSecrets = { new Secret( "user.client.secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes = { "scope1" },
                    AccessTokenLifetime = 7200
                },

                new Client
                {
                    ClientId = "console.client",
                    ClientSecrets = { new Secret( "console.client.secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "scope1" },
                },

                new Client
                {
                    ClientId = "web.client",
                    ClientSecrets = { new Secret( "web.client.secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "account",
                        "authority",
                        "role",

                        "scope1"
                    },

                    RedirectUris = { "http://localhost:54578/signin-oidc" },                        // 登录后跳转
                    PostLogoutRedirectUris = { "http://localhost:54578/signout-callback-oidc" },    // 登出后跳转

                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequirePkce = true,
                    AllowAccessTokensViaBrowser = true
                },
            };
    }
}