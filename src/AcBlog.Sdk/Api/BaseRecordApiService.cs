﻿using AcBlog.Data.Models;
using AcBlog.Data.Models.Actions;
using AcBlog.Data.Repositories;
using AcBlog.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AcBlog.Sdk.Api
{
    internal abstract class BaseRecordApiService<T, TQuery> : IRecordRepository<T, string, TQuery> where T : RHasId<string> where TQuery : QueryRequest, new()
    {
        protected abstract string PrepUrl { get; }

        public BaseRecordApiService(IBlogService blog, HttpClient httpClient)
        {
            BlogService = blog;
            HttpClient = httpClient;
        }

        public IBlogService BlogService { get; private set; }

        public HttpClient HttpClient { get; }

        public RepositoryAccessContext Context { get; set; } = new RepositoryAccessContext();

        protected virtual void SetHeader()
        {
            (BlogService as ApiBlogService)?.SetHeader();
        }

        public virtual async IAsyncEnumerable<string> All([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            SetHeader();
            using var responseMessage = await HttpClient.GetAsync(PrepUrl, cancellationToken).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();
            var result = await responseMessage.Content.ReadFromJsonAsync<IEnumerable<string>>(cancellationToken: cancellationToken).ConfigureAwait(false);
            foreach (var item in result!)
            {
                yield return item;
            }
        }

        public virtual async Task<RepositoryStatus> GetStatus(CancellationToken cancellationToken = default)
        {
            SetHeader();
            using var responseMessage = await HttpClient.GetAsync(PrepUrl, cancellationToken).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();
            return (await responseMessage.Content.ReadFromJsonAsync<RepositoryStatus>(cancellationToken: cancellationToken).ConfigureAwait(false))!;
        }

        public virtual async Task<string?> Create(T value, CancellationToken cancellationToken = default)
        {
            SetHeader();

            using var responseMessage = await HttpClient.PostAsJsonAsync(PrepUrl, value, cancellationToken).ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
                return null;

            return await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<bool> Delete(string id, CancellationToken cancellationToken = default)
        {
            SetHeader();

            using var responseMessage = await HttpClient.DeleteAsync($"{PrepUrl}/{Uri.EscapeDataString(id)}", cancellationToken).ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
                return false;

            return await responseMessage.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<bool> Exists(string id, CancellationToken cancellationToken = default)
        {
            SetHeader();

            using var responseMessage = await HttpClient.GetAsync($"{PrepUrl}/{Uri.EscapeDataString(id)}", cancellationToken).ConfigureAwait(false);
            return responseMessage.IsSuccessStatusCode;
        }

        public virtual async Task<T?> Get(string id, CancellationToken cancellationToken = default)
        {
            SetHeader();

            using var responseMessage = await HttpClient.GetAsync($"{PrepUrl}/{Uri.EscapeDataString(id)}", cancellationToken).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();

            var result = await responseMessage.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (result is not null)
                result = result with { Id = id };

            return result;
        }

        public virtual async IAsyncEnumerable<string> Query(TQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            SetHeader();

            using var responseMessage = await HttpClient.PutAsJsonAsync($"{PrepUrl}/query", query, cancellationToken).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();

            var result = await responseMessage.Content.ReadFromJsonAsync<IEnumerable<string>>(cancellationToken: cancellationToken).ConfigureAwait(false);

            foreach (var item in result!)
            {
                yield return item;
            }
        }

        public virtual async Task<QueryStatistic> Statistic(TQuery query, CancellationToken cancellationToken = default)
        {
            SetHeader();
            using var responseMessage = await HttpClient.PutAsJsonAsync($"{PrepUrl}/stats", query, cancellationToken).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();
            return (await responseMessage.Content.ReadFromJsonAsync<QueryStatistic>(cancellationToken: cancellationToken).ConfigureAwait(false))!;
        }

        public virtual async Task<bool> Update(T value, CancellationToken cancellationToken = default)
        {
            SetHeader();

            using var responseMessage = await HttpClient.PutAsJsonAsync($"{PrepUrl}/{Uri.EscapeDataString(value.Id)}", value, cancellationToken).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();

            return await responseMessage.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
