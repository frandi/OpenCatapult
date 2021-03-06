﻿// Copyright (c) Polyrific, Inc 2018. All rights reserved.

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Polyrific.Catapult.Shared.ApiClient
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;
        }
        
        public async Task<TResult> Get<TResult>(string path)
        {
            var response = await InvokeWithExceptionHandler(() => _httpClient.GetAsync(path));
            if (!response.IsSuccessStatusCode)
                await HandleResponseError(response);

            var result = await response.Content.ReadAsStringAsync();

            if (typeof(TResult) == typeof(string))
                return (TResult)Convert.ChangeType(result, typeof(TResult));

            return JsonConvert.DeserializeObject<TResult>(result);
        }
        
        public async Task<bool> Head(string path)
        {
            var response = await InvokeWithExceptionHandler(() => _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, path),
                HttpCompletionOption.ResponseHeadersRead, default(CancellationToken)));
            if (!response.IsSuccessStatusCode)
                await HandleResponseError(response);
            
            return response.IsSuccessStatusCode;
        }
        
        public async Task<TResult> Post<TContent, TResult>(string path, TContent content)
        {
            var response = await InvokeWithExceptionHandler(() => _httpClient.PostAsync(path, new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")));            if (!response.IsSuccessStatusCode)
                await HandleResponseError(response);

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TResult>(result);
        }

        public async Task<string> Post<TContent>(string path, TContent content)
        {
            var response = await InvokeWithExceptionHandler(() => _httpClient.PostAsync(path, new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")));
            if (!response.IsSuccessStatusCode)
                await HandleResponseError(response);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<bool> Put<TContent>(string path, TContent content)
        {
            var response = await InvokeWithExceptionHandler(() => _httpClient.PutAsync(path, new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")));
            if (!response.IsSuccessStatusCode)
                await HandleResponseError(response);

            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> Delete(string path)
        {
            var response = await InvokeWithExceptionHandler(() => _httpClient.DeleteAsync(path));
            if (!response.IsSuccessStatusCode)
                await HandleResponseError(response);

            return response.IsSuccessStatusCode;
        }

        private async Task<HttpResponseMessage> InvokeWithExceptionHandler(Func<Task<HttpResponseMessage>> httpClientInvocation)
        {
            try
            {
                return await httpClientInvocation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection error");
                throw new Exception($"Failed to connect to \"{_httpClient.BaseAddress.AbsoluteUri}\". Error: {ex.Message}");
            }
        }

        // TODO: Handle for NotFound, Unauthorized, Forbidden, BadRequest, and BadGateway
        private async Task HandleResponseError(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new Exception($"Error: {await response.Content.ReadAsStringAsync()}");
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.NotFound:
                    throw new Exception($"Failed to connect to \"{response.RequestMessage.RequestUri}\". Http Error {(int)response.StatusCode} - {response.ReasonPhrase}");
                case HttpStatusCode.Unauthorized:
                    throw new Exception($"Error: {response.ReasonPhrase}");
                default:
                    throw new Exception("Unknown error.");
            }
        }
    }
}
