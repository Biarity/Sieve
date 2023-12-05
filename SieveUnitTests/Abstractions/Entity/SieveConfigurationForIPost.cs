using Sieve.Services;
using SieveUnitTests.Entities;

namespace SieveUnitTests.Abstractions.Entity
{
    public class SieveConfigurationForIPost : ISieveConfiguration
    {
        public void Configure(SievePropertyMapper mapper)
        {
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
        }
    }
}
