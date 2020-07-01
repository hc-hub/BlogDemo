// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace BlogIdp
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),  
            };
        }
        //返回Api资源列表
        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                //  new ApiResource("Api名字", "Api显示的名字")
                new ApiResource("restapi", "My RESTAPI")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                // client credentials flow client
                new Client
                {
                    ClientId = "client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "api1" }
                },

                // MVC client using hybrid flow
                new Client
                {
                    ClientId = "mvcclient",
                    ClientName = "MVC 客户端",

                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,//允许授权类型
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },//客户端机密—仅与需要机密的流相关

                    RedirectUris = { "https://localhost:7001/signin-oidc" },//指定允许uri将令牌或授权代码返回给这个uri
                    FrontChannelLogoutUri = "https://localhost:7001/signout-oidc",//为基于HTTP前端通道的注销在客户端指定注销URI。
                    PostLogoutRedirectUris = { "https://localhost:7001/signout-callback-oidc" },//指定注销后允许重定向到的uri

                    AllowOfflineAccess = true,//获取或设置一个值，该值指示是否[允许脱机访问]。默认值为false。

                    //AllowedScopes = { "openid" },//允许请求的范围
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Profile,
                        "restapi"
                    }
                },

                // SPA client using code flow + pkce
                new Client
                {
                    ClientId = "spa",
                    ClientName = "SPA Client",
                    ClientUri = "http://identityserver.io",

                    AllowedGrantTypes = GrantTypes.Code,//指定允许的授权类型(AuthorizationCode, Implicit,  Hybrid, ResourceOwner, ClientCredentials的合法组合)
                    RequirePkce = true,//指定基于授权代码的令牌是否需要验证密钥请求(默认为false)。
                    RequireClientSecret = false,//如果设置为false，在令牌端点请求令牌时不需要客户机机密(默认为true)

                    RedirectUris =  //指定允许uri将令牌或授权代码返回给这个uri
                    {
                        "http://localhost:5002/index.html",
                        "http://localhost:5002/callback.html",
                        "http://localhost:5002/silent.html",
                        "http://localhost:5002/popup.html",
                    },

                    PostLogoutRedirectUris = { "http://localhost:5002/index.html" },//指定注销后允许重定向到的uri
                    AllowedCorsOrigins = { "http://localhost:5002" },//获取或设置JavaScript客户端允许的CORS源。

                    AllowedScopes = { "openid", "profile", "api1" }//允许请求的范围
                    //AllowedScopes = { IdentityServerConstants.StandardScopes.OpenId,IdentityServerConstants.StandardScopes.Profile}
                }
            };
        }
    }
}