﻿using Microsoft.EntityFrameworkCore;

namespace AcBlog.Data.Repositories.SqlServer.Models
{
    public class BlogDataContext : DbContext
    {
        public BlogDataContext(DbContextOptions<BlogDataContext> options) : base(options)
        {
        }

        public DbSet<RawPost> Posts { get; set; }

        public DbSet<RawPage> Pages { get; set; }

        public DbSet<RawLayout> Layouts { get; set; }
    }
}
