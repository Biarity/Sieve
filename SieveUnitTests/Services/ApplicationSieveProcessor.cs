using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;
using SieveUnitTests.Abstractions.Entity;
using SieveUnitTests.Entities;

namespace SieveUnitTests.Services
{
    public class ApplicationSieveProcessor : SieveProcessor
    {
        public ApplicationSieveProcessor(
            IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods,
            ISieveCustomFilterMethods customFilterMethods)
            : base(options, customSortMethods, customFilterMethods)
        {
        }

        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
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

            // interfaces
            mapper.Property<IPost>(p => p.ThisHasNoAttributeButIsAccessible)
                .CanSort()
                .CanFilter()
                .HasName("shortname");

            mapper.Property<IPost>(typeof(IPost).GetProperty(nameof(IPost.ThisHasNoAttributeButIsAccessible2)))
                .CanSort()
                .CanFilter()
                .HasName("shortname2");

            mapper.Property<IPost>(p => p.TopComment.Text)
                .CanFilter();

            mapper.Property<IPost>(p => p.TopComment.Id)
                .CanSort();

            mapper.Property<IPost>(p => p.OnlySortableViaFluentApi)
                .CanSort();

            mapper.Property<IPost>(p => p.TopComment.Text)
                .CanFilter()
                .HasName("topc");

            mapper.Property<IPost>(p => p.FeaturedComment.Text)
                .CanFilter()
                .HasName("featc");

            mapper
                .Property<IPost>(p => p.DateCreated)
                .CanSort()
                .HasName("CreateDate");

            return mapper;
        }
    }
}
