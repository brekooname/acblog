﻿using AcBlog.Data.Models;
using AcBlog.Data.Models.Actions;
using System.Net.Http;

namespace AcBlog.Sdk.Api
{
    internal class PageService : BaseRecordApiService<Page, PageQueryRequest>, IPageService
    {
        public PageService(IBlogService blog, HttpClient httpClient) : base(blog, httpClient)
        {
        }

        protected override string PrepUrl => "/Pages";
    }
}
