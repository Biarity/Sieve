### 1. Pack:
Just build and it'll be in `.\Sieve\bin\Debug`
Update the version first though (in project properties)
### 3. Publish:
```
nuget push .\bin\Debug\Sieve.1.3.94.nupkg API_KEY -Source https://api.nuget.org/v3/index.json
```
Replace API_KEY with one you get from nuget's website.
Also don't forget to replace corresponding version.
