# 🎛️ Sieve
Sieve is a simple and extensible framework for .NET Core that adds sorting, filtering, and pagination functionality out of the box. 
Most common use case would be for serving ASP.NET Core GET queries.

## Usage for ASP.NET Core

In this example, consider an app with a `Post` entity. 
We'll use Sieve to add sorting, filtering, and pagination capabilities when GET-ing all available posts.

#### 1. Add required services (`SieveProcessor<TEntity>`)

Inject the `SieveProcessor<TEntity>` service for each entity you'd like to use Sieve with.
So to use Sieve with `Post`s, go to `ConfigureServices` in `Startup.cs` and add:
```
services.AddScoped<SieveProcessor<Post>>();
```

#### 2. Add `Sieve` attributes on properties you'd like to sort/filter in your models

Sieve will only sort/filter properties that have the attribute `[Sieve(CanSort = true, CanFilter = true)]` on them (they don't have to be both true).
So for our `Post` entity model:
```
public int Id { get; set; }

[Sieve(CanFilter = true, CanSort = true)]
public string Title { get; set; }

[Sieve(CanFilter = true, CanSort = true)]
public int LikeCount { get; set; }

[Sieve(CanFilter = true, CanSort = true)]
public int CommentCount { get; set; }

[Sieve(CanFilter = true, CanSort = true, name = "created")]
public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;

```
There is also the `name` parameter that you can use to have a different name for use by clients.

#### 3. Use `SieveModel` in your controllers

In the action handling returning Posts, use the `SieveModel` to get the sort/filter/paginate query. 
Apply it by to your data by injecting `SieveProcessor<Post>` into the controller and using its `ApplyAll` method.
For instance: 
```
[HttpGet]
public JsonResult GetPosts(SieveModel sieveModel) 
{
    var result = _dbContext.Posts.AsNoTracking(); // Makes read-only queries faster
    result = _sieveProcessor.ApplyAll(sieveModel, result); // Returns `result` after applying the sort/filter/paginate query in `SieveModel` to it
    return Json(result.ToList());
}
```
There are also `ApplySorting`, `ApplyFiltering`, and `ApplyPagination` methods.

#### 4. Send a request

[Send a request](#Send%20a%20request)

#### Add custom sort/filter methods

#### Customize Sieve

## Usage for other than ASP.NET Core


## Send a request

With all the above in place, you can now send a GET request that includes a sort/filter/paginate query:
```
GET /GetPosts

?sorts=     LikeCount,CommentCount,-created     // 
&filters=   LikeCount > 10, Title contains 
&page=      1
&pageSize=  10

```



