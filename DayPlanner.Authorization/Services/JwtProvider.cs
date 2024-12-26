﻿using DayPlanner.Authorization.Exceptions;
using DayPlanner.Authorization.Interfaces;
using DayPlanner.Authorization.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Authorization.Services
{
    public partial class JwtProvider : IJwtProvider
    {
        private readonly HttpClient _httpClient;
        public JwtProvider(HttpClient httpClient) => _httpClient = httpClient;
        public async Task<string> GetForCredentialsAsync(string email, string password)
        {
            var request = new
            {
                email,
                password,
                returnSecureToken = true
            };
            var response = await _httpClient.PostAsJsonAsync("", request);
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                FireBaseError? error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"]!.ToObject<FireBaseError>();

                if (error != null)
                {
                    if (error.Message == "INVALID_LOGIN_CREDENTIALS")
                    {
                        throw new BadCredentialsException(error.Message);
                    }
                    else if (error.Message == "INVALID_EMAIL")
                    {
                        throw new InvalidEmailException(error.Message);
                    }
                    else
                    {
                        throw new Exception(error.Message); 
                    }
                }
                else
                {
                    throw new Exception("Unknown error occurred while processing the request.");
                }               
            }
            var authToken = await response.Content.ReadFromJsonAsync<AuthToken>();
            return authToken.IdToken;
        }
    }
}