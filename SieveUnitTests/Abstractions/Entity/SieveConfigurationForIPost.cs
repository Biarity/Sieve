using Sieve.Services;

namespace SieveUnitTests.Abstractions.Entity
{
    public class SieveConfigurationForIPost : ISieveConfiguration
    {
        public static void ConfigureStatic(SievePropertyMapper mapper)
        {
            mapper
                .Property<IPost>(p => p.Id)
                .CanSort();

            mapper.Property<IPost>(p => p.ThisHasNoAttributeButIsAccessible)
                .CanSort()
                .CanFilter()
                .HasName("shortname");

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

            mapper
                .Property<IPost>(post => post.DeletedBy)
                .CanSort()
                .HasName("DeletedBy");

            mapper
                .Property<IPost>(post => post.UpdatedBy)
                .CanFilter()
                .HasName("UpdatedBy");
        }

        public void Configure(SievePropertyMapper mapper)
        {
            ConfigureStatic(mapper);
        }
    }
}
