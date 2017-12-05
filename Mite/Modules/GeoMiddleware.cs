﻿using Microsoft.Owin;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.Extensions;
using Mite.ExternalServices.IpApi.Requests;
using Mite.ExternalServices.YandexGeocoder.Requests;
using Mite.Models;
using NLog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mite.Modules
{
    public class GeoMiddleware : OwinMiddleware
    {
        private readonly AppUserManager userManager;
        private readonly HttpClient httpClient;
        private readonly ICityService cityService;
        private readonly ILogger logger;

        public GeoMiddleware(OwinMiddleware next, AppUserManager userManager, HttpClient httpClient, ICityService cityService, 
            ILogger logger) : base(next)
        {
            this.userManager = userManager;
            this.httpClient = httpClient;
            this.cityService = cityService;
            this.logger = logger;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var userIdentity = context.Request.User.Identity;
            var ip = context.Request.RemoteIpAddress == context.Request.LocalIpAddress ?
                "188.162.195.31" : context.Request.RemoteIpAddress;

            if (context != null && userIdentity != null && userIdentity.IsAuthenticated)
            {
                var cityId = userIdentity.GetClaimValue(ClaimConstants.UserCityId);
                if (cityId == null)
                {
                    var user = await userManager.FindByNameAsync(userIdentity.Name);
                    if(user.CityId == null)
                    {
                        var city = await GetCityAsync(ip);
                        user.CityId = city.Id;
                        await userManager.UpdateAsync(user);
                        userIdentity.AddUpdateClaim(context.Authentication, ClaimConstants.UserCityId, city.Id.ToString());
                    }
                    userIdentity.AddUpdateClaim(context.Authentication, ClaimConstants.UserCityId, user.CityId.ToString());
                }
            }
            await Next.Invoke(context);
        }
        private async Task<CityModel> GetCityAsync(string ip)
        {
            var ipReq = new IpApiRequest(httpClient);
            var ipResult = await ipReq.PerformAsync(ip);
            CityModel city;
            try
            {
                var yaReq = new YaGeocoderRequest(httpClient);
                var cityName = await yaReq.GetCityNameAsync(ipResult.Latitude, ipResult.Longitude);

                city = await cityService.GetByNameAsync(cityName);
                if (city == null)
                    city = await cityService.GetByNameAsync("Москва");
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при определении города :" + e.Message);
                city = await cityService.GetByNameAsync("Москва");
            }

            return city;
        }
    }
}