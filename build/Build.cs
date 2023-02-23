using System.Linq;
using GlobExpressions;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[ShutdownDotNetAfterServerBuild]
[GitHubActions("ci", GitHubActionsImage.Ubuntu2004,
    OnPullRequestBranches = new[] {"master", "releases/*"},
    AutoGenerate = true,
    InvokedTargets = new[] {nameof(Ci)},
    CacheKeyFiles = new string[0]
    )
]
[GitHubActions("ci_publish", GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "releases/*" },
    OnPushTags = new[] { "v*" },
    AutoGenerate = true,
    InvokedTargets = new[] {nameof(CiPublish)},
    CacheKeyFiles = new string[0],
    ImportSecrets = new[] {"NUGET_API_KEY"})]
class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository] readonly GitRepository GitRepository;

    [GitVersion(Framework = "netcoreapp3.1")] readonly GitVersion GitVersion;

    [Solution] readonly Solution Solution;

    // ReSharper disable once InconsistentNaming
    [Parameter] string NUGET_API_KEY;

    Project SieveProject => Solution.AllProjects.First(p => p.Name == "Sieve");

    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Executes(() =>
        {
            DotNetClean();
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution));
        });

    Target Package => _ => _
        .DependsOn(Test)
        .Executes(() =>
        
        {
            DotNetPack(s => s
                .SetProject(SieveProject)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(OutputDirectory)
                .SetVersion(GitVersion.NuGetVersionV2)
                .EnableNoRestore()
                .EnableNoBuild());
        });

    Target Publish => _ => _
        .DependsOn(Package)
        .Requires(() => IsServerBuild)
        .Requires(() => NUGET_API_KEY)
        .Requires(() => Configuration.Equals(Configuration.Release))
        .Executes(() =>
        {
            var files = Glob
                .Files(OutputDirectory, "*.nupkg")
                .NotNull()
                .ToList();

            Assert.NotEmpty(files);

            files
                .ForEach(x =>
                {
                    DotNetNuGetPush(s => s
                        .SetTargetPath(OutputDirectory / x)
                        .SetSource("https://api.nuget.org/v3/index.json")
                        .SetApiKey(NUGET_API_KEY));
                });
        });

    Target Ci => _ => _
        .DependsOn(Test);

    Target CiPublish => _ => _
        .DependsOn(Publish);

    public static int Main() => Execute<Build>(x => x.Package);
}
