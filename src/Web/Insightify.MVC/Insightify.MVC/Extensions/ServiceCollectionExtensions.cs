﻿using Insightify.MVC.Clients;
using Insightify.MVC.Infrastructure;
using Insightify.MVC.Services.FinancialData;
using Insightify.MVC.Services.News;
using Insightify.MVC.Services.Posts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Refit;

namespace Insightify.MVC.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var identityUrl = configuration.GetValue<string>("IdentityUrl");

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(setup => setup.ExpireTimeSpan = TimeSpan.FromMinutes(60))
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = identityUrl.ToString();
                //options.SignedOutRedirectUri = callBackUrl.ToString();
                options.ClientId = "mvc";
                options.ClientSecret = "S0M3 MAG1C UN!C0RNS CR3AT3D TH1S S3CR3T";
                options.ResponseType = "code";
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.RequireHttpsMetadata = false;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("posts");
                options.Scope.Add("news");
                options.Scope.Add("gateway");
            });

            return services;
        }
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var gatewayUrl = configuration.GetValue<string>("GatewayUrl") ?? "http://localhost";

            services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<IPostsService, PostsService>();
            services.AddScoped<INewsService, NewsService>();
            services.AddScoped<IFinancialDataService, FinancialDataService>();

            services.AddRefitClient<IPostsClient>()
                .ConfigureHttpClient(cfg => cfg.BaseAddress = new Uri(gatewayUrl))
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddRefitClient<INewsClient>()
                .ConfigureHttpClient(cfg => cfg.BaseAddress = new Uri(gatewayUrl))
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddRefitClient<IFinancialDataClient>()
                .ConfigureHttpClient(cfg => cfg.BaseAddress = new Uri(gatewayUrl))
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            return services;
        }
    }
}
