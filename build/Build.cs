using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Serilog;

[GitHubActions(nameof(Compile),
    GitHubActionsImage.UbuntuLatest,
    InvokedTargets = new []{nameof(Clean)},
    On = new[] { GitHubActionsTrigger.Push }
    )]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Clean);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution solution;

    Target Clean => _ => _
        .Triggers(Restore)
        .Executes(() =>
        {
            Log.Information("Started executing our build");
            DotNetTasks.DotNetClean(x=>x.SetProject(solution));
            Log.Information("Finished executing our clean step");
        });

    Target Restore => _ => _
        .Triggers(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(x=>x.SetProjectFile(solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(x => x.SetProjectFile(solution));
        });

}
