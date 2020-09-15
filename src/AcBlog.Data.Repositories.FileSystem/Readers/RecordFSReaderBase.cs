﻿using AcBlog.Data.Models;
using AcBlog.Data.Models.Actions;
using StardustDL.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AcBlog.Data.Repositories.FileSystem.Readers
{
    public abstract class RecordFSReaderBase<T, TId, TQuery> : RecordFSRepository<T, TId, TQuery> where TId : class where T : class, IHasId<TId> where TQuery : QueryRequest, new()
    {
        protected RecordFSReaderBase(string rootPath, IFileProvider fileProvider) : base(rootPath, fileProvider)
        {
            PagingProvider = new PagingProvider<TId>(Paths.GetPaginationRoot(RootPath), FileProvider);
        }

        protected PagingProvider<TId> PagingProvider { get; }

        readonly Lazy<RepositoryStatus> _status = new Lazy<RepositoryStatus>(new RepositoryStatus
        {
            CanRead = true,
            CanWrite = false,
        });

        protected virtual string GetPath(TId id) => Path.Join(RootPath, $"{id}.json");

        public override async IAsyncEnumerable<TId> All([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in Query(new TQuery(), cancellationToken).ConfigureAwait(false))
            {
                yield return item;
            }
        }

        public override async IAsyncEnumerable<TId> Query(TQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            query.Pagination ??= new Pagination();

            await PagingProvider.FillPagination(query.Pagination);

            var res = new QueryResponse<TId>(
                await PagingProvider.GetPaging(query.Pagination).ConfigureAwait(false),
                query.Pagination);
            return res;
        }

        public override async Task<QueryStatistic> Statistic(TQuery query, CancellationToken cancellationToken = default)
        {

        }

        public override Task<TId?> Create(T value, CancellationToken cancellationToken = default) => Task.FromResult<TId?>(null);

        public override Task<bool> Delete(TId id, CancellationToken cancellationToken = default) => Task.FromResult(false);

        public override async Task<bool> Exists(TId id, CancellationToken cancellationToken = default)
        {
            return await (await FileProvider.GetFileInfo(GetPath(id)).ConfigureAwait(false)).Exists();
        }

        public override async Task<T?> Get(TId id, CancellationToken cancellationToken = default)
        {
            await using var fs = await (await FileProvider.GetFileInfo(GetPath(id)).ConfigureAwait(false)).CreateReadStream().ConfigureAwait(false);
            var result = await JsonSerializer.DeserializeAsync<T?>(fs, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (result is not null)
                result.Id = id;
            return result;
        }

        public override Task<bool> Update(T value, CancellationToken cancellationToken = default) => Task.FromResult(false);

        public override Task<RepositoryStatus> GetStatus(CancellationToken cancellationToken = default) => Task.FromResult(_status.Value);
    }
}
