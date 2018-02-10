# Sieve
⚗️ Sieve is a simple, clean, and extensible framework for .NET Core that **adds sorting, filtering, and pagination functionality out of the box**. 
Most common use case would be for serving ASP.NET Core GET queries.

[![NuGet Release](https://img.shields.io/nuget/v/Sieve.svg?style=flat-square)](https://www.nuget.org/packages/Sieve)

[Get Sieve on nuget](https://www.nuget.org/packages/Sieve/)

## Usage for ASP.NET Core

In this example, consider an app with a `Post` entity. 
We'll use Sieve to add sorting, filtering, and pagination capabilities when GET-ing all available posts.

### 1. Add required services

Inject the `SieveProcessor` service. So in `Startup.cs` add:
```
services.AddScoped<SieveProcessor>();
```

### 2. Tell Sieve which properties you'd like to sort/filter in your models

Sieve will only sort/filter properties that have the attribute `[Sieve(CanSort = true, CanFilter = true)]` on them (they don't have to be both true).
So for our `Post` entity model example:
```
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

### 3. Get sort/filter/page queries by using the Sieve model in your controllers

In the action that handles returning Posts, use `SieveModel` to get the sort/filter/page query. 
Apply it to your data by injecting `SieveProcessor` into the controller and using its `ApplyAll<TEntity>` method. So for instance:
```
[HttpGet]
public JsonResult GetPosts(SieveModel sieveModel) 
{
    var result = _dbContext.Posts.AsNoTracking(); // Makes read-only queries faster
    result = _sieveProcessor.ApplyAll(sieveModel, result); // Returns `result` after applying the sort/filter/page query in `SieveModel` to it
    return Json(result.ToList());
}
```
There are also `ApplySorting`, `ApplyFiltering`, and `ApplyPagination` methods.

### 4. Send a request

[Send a request](#send-a-request)

### Add custom sort/filter methods

If you want to add custom sort/filter methods, inject `ISieveCustomSortMethods` or `ISieveCustomFilterMethods` with the implementation being a class that has custom sort/filter methods that Sieve will through.

For instance:
```
services.AddScoped<ISieveCustomSortMethods, SieveCustomSortMethods>();
services.AddScoped<ISieveCustomFilterMethods, SieveCustomFilterMethods>();
```
Where `SieveCustomSortMethodsOfPosts` for example is:
```
public class SieveCustomSortMethods : ISieveCustomSortMethods
{
    public IQueryable<Post> Popularity(IQueryable<Post> source, bool useThenBy, bool desc) // The method is given an indicator of weather to use ThenBy(), and if the query is descending 
    {
        var result = useThenBy ?
            ((IOrderedQueryable<Post>)source).ThenBy(p => p.LikeCount) : // ThenBy only works on IOrderedQueryable<TEntity>
            source.OrderBy(p => p.LikeCount)
            .ThenBy(p => p.CommentCount)
            .ThenBy(p => p.DateCreated);

        return result; // Must return modified IQueryable<TEntity>
    }
}
```
And `SieveCustomFilterMethods`:
```
public class SieveCustomFilterMethods : ISieveCustomFilterMethods
{
    public IQueryable<Post> IsNew(IQueryable<Post> source, string op, string value) // The method is given the {Operator} & {Value}
    {
        var result = source.Where(p => p.LikeCount < 100 &&
                                        p.CommentCount < 5);

        return result; // Must return modified IQueryable<TEntity>
    }
}
```

### Configure Sieve
Use the [ASP.NET Core options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) with `SieveOptions` to tell Sieve where to look for configuration. For example:
```
services.Configure<SieveOptions>(Configuration.GetSection("Sieve"));
```
Then you can add the configuration:
```
{
    "Sieve": {
        "CaseSensitive": `boolean: should property names be case-sensitive? Defaults to false`,
        "DefaultPageSize": `number: optional number to fallback to when no page argument is given`
    }
}
```

## Send a request

With all the above in place, you can now send a GET request that includes a sort/filter/page query.
An example:
```
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
    * `{Operator}` is one of the [Operators](#operators)
    * `{Value}` is the value to use for filtering
* `page` is the number of page to return
* `pageSize` is the number of items returned per page 

Notes:
* Don't forget to remove commas from any `{Value}` fields
* You can have spaces anywhere except *within* `{Name}` or `{Operator}` fields
* Here's a [good example on how to work with arrays](https://github.com/Biarity/Sieve/issues/2)

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
| `@=*`      | Case-insensitive string Contains |
| `_=*`      | Case-insensitive string Starts with |
| `==*`      | Case-insensitive string Equals |

### Example project
You can find an example project incorporating most Sieve concepts in [SieveTests](https://github.com/Biarity/Sieve/tree/master/SieveTests).

## License & Contributing
Sieve is licensed under Apache 2.0. Any contributions highly appreciated!
