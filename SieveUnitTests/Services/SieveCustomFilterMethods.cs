﻿using System;
using System.Linq;
using Sieve.Services;
using SieveUnitTests.Abstractions.Entity;
using SieveUnitTests.Entities;

namespace SieveUnitTests.Services
{
    public class SieveCustomFilterMethods : ISieveCustomFilterMethods
    {
        public IQueryable<Post> IsNew(IQueryable<Post> source, string op, string[] values)
        {
            var result = source.Where(p => p.LikeCount < 100);

            return result;
        }

        public IQueryable<Post> HasInTitle(IQueryable<Post> source, string op, string[] values)
        {
            var result = source.Where(p => p.Title.Contains(values[0]));

            return result;
        }

        public IQueryable<Comment> IsNew(IQueryable<Comment> source, string op, string[] values)
        {
            var result = source.Where(c => c.DateCreated > DateTimeOffset.UtcNow.AddDays(-2));

            return result;
        }

        public IQueryable<Comment> TestComment(IQueryable<Comment> source, string op, string[] values)
        {
            return source;
        }

        public IQueryable<T> Latest<T>(IQueryable<T> source, string op, string[] values) where T : BaseEntity
        {
            var result = source.Where(c => c.DateCreated > DateTimeOffset.UtcNow.AddDays(-14));
            return result;
        }

        public IQueryable<IPost> IsNew(IQueryable<IPost> source, string op, string[] values)
        {
            var result = source.Where(p => p.LikeCount < 100);

            return result;
        }

        public IQueryable<IPost> HasInTitle(IQueryable<IPost> source, string op, string[] values)
        {
            var result = source.Where(p => p.Title.Contains(values[0]));

            return result;
        }

        public IQueryable<IComment> IsNew(IQueryable<IComment> source, string op, string[] values)
        {
            var result = source.Where(c => c.DateCreated > DateTimeOffset.UtcNow.AddDays(-2));

            return result;
        }

        public IQueryable<IComment> TestComment(IQueryable<IComment> source, string op, string[] values)
        {
            return source;
        }
    }
}
