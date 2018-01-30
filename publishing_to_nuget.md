### 1. Pack:
```
dotnet pack Sieve.csproj -c Release -o . /p:PackageVersion=1.3.4
```
Don't forget to change version since nuget packages are immutable (add one to the nuget current).

### 2. Manually add nuspec:
For some reason `dotnet pack` chooses to ignore my Sieve.nuspec.
So unpack the Sieve.1.2.0.nupkg, and replace the nuspec in there with the local one.
Don't forget that if you add new dependencies, you'll need to declare them in the nuspec.
Also don't forget updating the version number in nuspec.
Also don't forget updaing `releaseNotes` in nuspec.

### 3. Publish:
```
nuget push Sieve.1.2.0.nupkg API_KEY -Source https://api.nuget.org/v3/index.json
```
Replace API_KEY with one you get from nuget's website.
Also don't forget to replace corresponding version.
