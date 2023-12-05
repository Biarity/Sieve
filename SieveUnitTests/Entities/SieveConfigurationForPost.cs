﻿using Sieve.Services;

namespace SieveUnitTests.Entities
{
    public class SieveConfigurationForPost : ISieveConfiguration
    {
        public void Configure(SievePropertyMapper mapper)
        {
            mapper.Property<Post>(p => p.ThisHasNoAttributeButIsAccessible)
                .CanSort()
                .CanFilter()
                .HasName("shortname");

            mapper.Property<Post>(typeof(Post).GetProperty(nameof(Post.ThisHasNoAttributeButIsAccessible2)))
                .CanSort()
                .CanFilter()
                .HasName("shortname2");

            mapper.Property<Post>(p => p.TopComment.Text)
                .CanFilter();

            mapper.Property<Post>(p => p.TopComment.Id)
                .CanSort();

            mapper.Property<Post>(p => p.OnlySortableViaFluentApi)
                .CanSort();

            mapper.Property<Post>(p => p.TopComment.Text)
                .CanFilter()
                .HasName("topc");

            mapper.Property<Post>(p => p.FeaturedComment.Text)
                .CanFilter()
                .HasName("featc");

            mapper
                .Property<Post>(p => p.DateCreated)
                .CanSort()
                .HasName("CreateDate");
        }
    }
}
