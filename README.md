test

# Sieve
⚗️ Sieve is a simple, clean, and extensible framework for .NET Core that **adds sorting, filtering, and pagination functionality out of the box**. 
Most common use case would be for serving ASP.NET Core GET queries.

[![NuGet Release](https://img.shields.io/nuget/v/Sieve?style=for-the-badge)](https://www.nuget.org/packages/Sieve)
[![NuGet Pre-Release](https://img.shields.io/nuget/vpre/Sieve?style=for-the-badge)](https://www.nuget.org/packages/Sieve)

[Get Sieve on nuget](https://www.nuget.org/packages/Sieve/)

## Usage for ASP.NET Core

In this example, consider an app with a `Post` entity. 
We'll use Sieve to add sorting, filtering, and pagination capabilities when GET-ing all available posts.

### 1. Add required services

Inject the `SieveProcessor` service. So in `Startup.cs` add:
```C#
services.AddScoped<SieveProcessor>();
```

### 2. Tell Sieve which properties you'd like to sort/filter in your models

Sieve will only sort/filter properties that have the attribute `[Sieve(CanSort = true, CanFilter = true)]` on them (they don't have to be both true).
So for our `Post` entity model example:
```C#
public int Id { get; set; }

[Sieve(CanFilter = true, CanSort = true)]
public string Title { get; set; }

[Sieve(CanFilter = true, CanSort = true)]
public int LikeCount { get; set; }

[Sieve(CanFilter = true, CanSort = true)]
public int CommentCount { get; set; }

[Sieve(CanFilter = true, CanSort = true, Name = "created")]
public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;

```
There is also the `Name` parameter that you can use to have a different name for use by clients.

Alternatively, you can use [Fluent API](#fluent-api) to do the same. This is especially useful if you don't want to use attributes or have multiple APIs. 

### 3. Get sort/filter/page queries by using the Sieve model in your controllers

In the action that handles returning Posts, use `SieveModel` to get the sort/filter/page query. 
Apply it to your data by injecting `SieveProcessor` into the controller and using its `Apply<TEntity>` method. So for instance:
```C#
[HttpGet]
public JsonResult GetPosts(SieveModel sieveModel) 
{
    var result = _dbContext.Posts.AsNoTracking(); // Makes read-only queries faster
    result = _sieveProcessor.Apply(sieveModel, result); // Returns `result` after applying the sort/filter/page query in `SieveModel` to it
    return Json(result.ToList());
}
```
You can also explicitly specify if only filtering, sorting, and/or pagination should be applied via optional arguments.

### 4. Send a request

[Send a request](#send-a-request)

### Add custom sort/filter methods

If you want to add custom sort/filter methods, inject `ISieveCustomSortMethods` or `ISieveCustomFilterMethods` with the implementation being a class that has custom sort/filter methods that Sieve will search through.

For instance:
```C#
services.AddScoped<ISieveCustomSortMethods, SieveCustomSortMethods>();
services.AddScoped<ISieveCustomFilterMethods, SieveCustomFilterMethods>();
```
Where `SieveCustomSortMethodsOfPosts` for example is:
```C#
public class SieveCustomSortMethods : ISieveCustomSortMethods
{
    public IQueryable<Post> Popularity(IQueryable<Post> source, bool useThenBy, bool desc) // The method is given an indicator of whether to use ThenBy(), and if the query is descending 
    {
        var result = useThenBy ?
            ((IOrderedQueryable<Post>)source).ThenBy(p => p.LikeCount) : // ThenBy only works on IOrderedQueryable<TEntity>
            source.OrderBy(p => p.LikeCount)
            .ThenBy(p => p.CommentCount)
            .ThenBy(p => p.DateCreated);

        return result; // Must return modified IQueryable<TEntity>
    }

    public IQueryable<T> Oldest<T>(IQueryable<T> source, bool useThenBy, bool desc) where T : BaseEntity // Generic functions are allowed too
    {
        var result = useThenBy ?
            ((IOrderedQueryable<T>)source).ThenByDescending(p => p.DateCreated) :
            source.OrderByDescending(p => p.DateCreated);

        return result;
    }
}
```
And `SieveCustomFilterMethods`:
```C#
public class SieveCustomFilterMethods : ISieveCustomFilterMethods
{
    public IQueryable<Post> IsNew(IQueryable<Post> source, string op, string[] values) // The method is given the {Operator} & {Value}
    {
        var result = source.Where(p => p.LikeCount < 100 &&
                                        p.CommentCount < 5);

        return result; // Must return modified IQueryable<TEntity>
    }

    public IQueryable<T> Latest<T>(IQueryable<T> source, string op, string[] values) where T : BaseEntity // Generic functions are allowed too
    {
        var result = source.Where(c => c.DateCreated > DateTimeOffset.UtcNow.AddDays(-14));
        return result;
    }
}
```

## Configure Sieve
Use the [ASP.NET Core options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) with `SieveOptions` to tell Sieve where to look for configuration. For example:
```C#
services.Configure<SieveOptions>(Configuration.GetSection("Sieve"));
```
Then you can add the configuration:
```json
{
    "Sieve": {
        "CaseSensitive": "boolean: should property names be case-sensitive? Defaults to false",
        "DefaultPageSize": "int number: optional number to fallback to when no page argument is given. Set <=0 to disable paging if no pageSize is specified (default).",
        "MaxPageSize": "int number: maximum allowed page size. Set <=0 to make infinite (default)",
        "ThrowExceptions": "boolean: should Sieve throw exceptions instead of silently failing? Defaults to false",
        "IgnoreNullsOnNotEqual": "boolean: ignore null values when filtering using is not equal operator? Defaults to true",
        "DisableNullableTypeExpressionForSorting": "boolean: disable the creation of nullable type expression for sorting. Some databases do not handle it (yet). Defaults to false"
    }
}
```

## Send a request

With all the above in place, you can now send a GET request that includes a sort/filter/page query.
An example:
```curl
GET /GetPosts

?sorts=     LikeCount,CommentCount,-created         // sort by likes, then comments, then descendingly by date created 
&filters=   LikeCount>10, Title@=awesome title,     // filter to posts with more than 10 likes, and a title that contains the phrase "awesome title"
&page=      1                                       // get the first page...
&pageSize=  10                                      // ...which contains 10 posts

```
More formally:
* `sorts` is a comma-delimited ordered list of property names to sort by. Adding a `-` before the name switches to sorting descendingly.
* `filters` is a comma-delimited list of `{Name}{Operator}{Value}` where
    * `{Name}` is the name of a property with the Sieve attribute or the name of a custom filter method for TEntity
        * You can also have multiple names (for OR logic) by enclosing them in brackets and using a pipe delimiter, eg. `(LikeCount|CommentCount)>10` asks if `LikeCount` or `CommentCount` is `>10`
    * `{Operator}` is one of the [Operators](#operators)
    * `{Value}` is the value to use for filtering
        * You can also have multiple values (for OR logic) by using a pipe delimiter, eg. `Title@=new|hot` will return posts with titles that contain the text "`new`" or "`hot`"
* `page` is the number of page to return
* `pageSize` is the number of items returned per page 

Notes:
* You can use backslashes to escape special characters and sequences:
    * commas: `Title@=some\,title` makes a match with "some,title"
    * pipes: `Title@=some\|title` makes a match with "some|title"
    * null values: `Title@=\null` will search for items with title equal to "null" (not a missing value, but "null"-string literally)
* You can have spaces anywhere except *within* `{Name}` or `{Operator}` fields
* If you need to look at the data before applying pagination (eg. get total count), use the optional paramters on `Apply` to defer pagination (an [example](https://github.com/Biarity/Sieve/issues/34))
* Here's a [good example on how to work with enumerables](https://github.com/Biarity/Sieve/issues/2)
* Another example on [how to do OR logic](https://github.com/Biarity/Sieve/issues/8)

### Nested objects
You can filter/sort on a nested object's property by marking the property using the Fluent API. 
Marking via attributes not currently supported.

For example, using this object model:

```C#
public class Post {
    public User Creator { get; set; }
}

public class User {
    public string Name { get; set; }
}
```

Mark `Post.User` to be filterable:
```C#
// in MapProperties
mapper.Property<Post>(p => p.Creator.Name)
    .CanFilter();
```

Now you can make requests such as: `filters=User.Name==specific_name`.

### Creating your own DSL
You can replace this DSL with your own (eg. use JSON instead) by implementing an [ISieveModel](https://github.com/Biarity/Sieve/blob/master/Sieve/Models/ISieveModel.cs). You can use the default [SieveModel](https://github.com/Biarity/Sieve/blob/master/Sieve/Models/SieveModel.cs) for reference.

### Operators
| Operator   | Meaning                  |
|------------|--------------------------|
| `==`       | Equals                   |
| `!=`       | Not equals               |
| `>`        | Greater than             |
| `<`        | Less than                |
| `>=`       | Greater than or equal to |
| `<=`       | Less than or equal to    |
| `@=`       | Contains                 |
| `_=`       | Starts with              |
| `_-=`      | Ends with                |
| `!@=`      | Does not Contains        |
| `!_=`      | Does not Starts with     |
| `!_-=`     | Does not Ends with       |
| `@=*`      | Case-insensitive string Contains |
| `_=*`      | Case-insensitive string Starts with |
| `_-=*`     | Case-insensitive string Ends with |
| `==*`      | Case-insensitive string Equals |
| `!=*`      | Case-insensitive string Not equals |
| `!@=*`     | Case-insensitive string does not Contains |
| `!_=*`     | Case-insensitive string does not Starts with |

### Handle Sieve's exceptions

Sieve will silently fail unless `ThrowExceptions` in the configuration is set to true. 3 kinds of custom exceptions can be thrown:

* `SieveMethodNotFoundException` with a `MethodName`
* `SieveIncompatibleMethodException` with a `MethodName`, an `ExpectedType` and an `ActualType`
* `SieveException` which encapsulates any other exception types in its `InnerException`

It is recommended that you write exception-handling middleware to globally handle Sieve's exceptions when using it with ASP.NET Core.


### Example project
You can find an example project incorporating most Sieve concepts in [SieveTests](https://github.com/Biarity/Sieve/tree/master/SieveTests).

## Fluent API
To use the Fluent API instead of attributes in marking properties, setup an alternative `SieveProcessor` that overrides `MapProperties`. For [example](https://github.com/Biarity/Sieve/blob/master/Sieve.Sample/Services/ApplicationSieveProcessor.cs):

```C#
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
        mapper.Property<Post>(p => p.Title)
            .CanFilter()
            .HasName("a_different_query_name_here");

        mapper.Property<Post>(p => p.CommentCount)
            .CanSort();

        mapper.Property<Post>(p => p.DateCreated)
            .CanSort()
            .CanFilter()
            .HasName("created_on");

        return mapper;
    }
}
```



Now you should inject the new class instead:
```C#
services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
```
Find More on Sieve's Fluent API [here](https://github.com/Biarity/Sieve/issues/4#issuecomment-364629048).

### Modular Fluent API configuration
Adding all fluent mappings directly in the processor can become unwieldy on larger projects.
It can also clash with vertical architectures.
To enable functional grouping of mappings the `ISieveConfiguration` interface was created together with extensions to the default mapper.
```C#
public class SieveConfigurationForPost : ISieveConfiguration
{
    public void Configure(SievePropertyMapper mapper)
    {
        mapper.Property<Post>(p => p.Title)
            .CanFilter()
            .HasName("a_different_query_name_here");

        mapper.Property<Post>(p => p.CommentCount)
            .CanSort();

        mapper.Property<Post>(p => p.DateCreated)
            .CanSort()
            .CanFilter()
            .HasName("created_on");

        return mapper;
    }
}
```
With the processor simplified to:
```C#
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
        return mapper
            .ApplyConfiguration<SieveConfigurationForPost>()
            .ApplyConfiguration<SieveConfigurationForComment>();       
    }
}
```
There is also the option to scan and add all configurations for a given assembly
```C#
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
        return mapper.ApplyConfigurationsFromAssembly(typeof(ApplicationSieveProcessor).Assembly);            
    }
}
```

## Upgrading to v2.2.0

2.2.0 introduced OR logic for filter values. This means your custom filters will need to accept multiple values rather than just the one.

* In all your custom filter methods, change the last argument to be a `string[] values` instead of `string value`
* The first value can then be found to be `values[0]` rather than `value`
* Multiple values will be present if the client uses OR logic

## Upgrading from v1.* to v2.*

* Changes to the `SieveProcessor` API:
    * `ApplyAll` is now `Apply`
    * `ApplyFiltering`, `ApplySorting`, and `ApplyPagination` are now depricated - instead you can use optional arguments on `Apply` to achieve the same
* Instead of just removing commas from `{Value}`s, [you'll also need to remove brackets and pipes](#send-a-request)


## License & Contributing
Sieve is licensed under Apache 2.0. Any contributions highly appreciated!
