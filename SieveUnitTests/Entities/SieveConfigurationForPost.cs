using Sieve.Services;

namespace SieveUnitTests.Entities
{
    public class SieveConfigurationForPost : ISieveConfiguration
    {
        public static void ConfigureStatic(SievePropertyMapper mapper)
        {
            mapper
                .Property<Post>(p => p.Id)
                .CanSort();

            mapper.Property<Post>(p => p.ThisHasNoAttributeButIsAccessible)
                .CanSort()
                .CanFilter()
                .HasName("shortname");

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

            mapper
                .Property<Post>(post => post.DeletedBy)
                .CanSort()
                .HasName("DeletedBy");

            mapper
                .Property<Post>(post => post.UpdatedBy)
                .CanFilter()
                .HasName("UpdatedBy");
        }

        public void Configure(SievePropertyMapper mapper)
        {
            ConfigureStatic(mapper);
        }
    }
}
