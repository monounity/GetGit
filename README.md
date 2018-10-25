# GetGit

> Migrate any TFS repository to a Git repository with simple but powerful branch mapping

- Flexible and configurable, map any TFS path or branch to any branch in Git
- Migrate tens of thousands of changesets from hundreds of branches, or just a selected range of changesets from a single branch
- Migrate plain text files or binary files with Git LFS (Large File Support)

## Prerequisites

- Git (and optionally Git LFS)
- Visual Studio 2017^
- Access to the TFS server, ie the Visual Studio 'Source Control Explorer' should be able to connect to the TFS repository
- Optional: an NUnit test runner, for example NUnit Console Runner, TestDriven.Net, ReSharper

## Installation

- Clone this repo, `git clone https://github.com/monounity/getgit.git`
- Open `src/GetGit.sln` in Visual Studio
- Restore all NuGet packages
- Build the project
- Optional: Run the unit and integration tests in the `src/Tests` project to make sure everything is working

## Workflow

GetGit is a command line application that is intended to be run in debug mode from inside Visual Studio, ie by simply hitting F5.

To setup the migration for the first run, GetGit must be configured correctly (also described more in-depth later in this document) and the first step is to configure at least the following variables:

- `Configuration.TfsServerUri`
- `Configuration.TfsProject`
- `Configuration.UserListingBaseHostName`
- `Configuration.EntryPoint`

The next step is to list all users for the configured server, project and entrypoint:

- Set `Configuration.ListUsers` to `true`
- Hit F5
- Copy the printed user items and insert them into the `Configuration.UserMappings` variable
- Edit as needed

In the next step, configure the Git repository path, the Git origin repository, the Git config templates and the branch mappings. Before starting the first migration run, also set the following variables: 

- Set `Configuration.ListUsers` to `false`
- Set `Configuration.InitializeRepo` to `true`
- Hit F5

While migrating, GetGit will most likely encounter files or changesets that shouldn't be migrated at all, or missing branches when merging etc.
When that happens, update the configuration and either start over from an empty repository (by setting `Configuration.InitializeRepo` to `true`) or by resuming the migration from a specified entrypoint.

### Resuming

If the appplication has to be stopped or stops because an error occurrs (missing branch or user mapping for example) it is possible to resume the migration from a specified entrypoint:

- Set `Configuration.ListUsers` to `false`
- Set `Configuration.InitializeRepo` to `false`
- Set `Configuration.EntryPoint` to the changeset id to start migrating from, the last migrated changeset id can be found last in the log
- Hit F5

## Mapping

GetGit employs a strategy called 'branch mapping', which is an easy and straightforward to produce accurate migration results from an intricate TFS history by telling GitGet which paths in TFS are considered source branches, and what their target branches should be called in Git.

The reason behind this choice of strategy is there are two big obstacles to overcome when translating what TFS considers a branch to what Git considers a branch:

1. A TFS directory might be used as a branch but the directory isn't actually configured as a branch, which makes it impossible to programmatically determine if the directory is a branch or not

2. The 'branch' concept is completely different in Git and TFS:

In Git, there is only one source tree for all branches and a branch is a label which points to a commit.

```bash
\---src
    +---app
    \---images
```

In TFS, there is one source tree per branch and branches are identfied by their paths. Branches are essentially deep copies of their parents.

```bash
+---Main
|   \---src
|       +---app
|       \---images
\---Release
    +---1.0
    |   \---src
    |       +---app
    |       \---images
    \---2.0
        \---src
            +---app
            \---images
```

Since there is no way to determine programmatically and reliably if a path is in fact a branch, it's also impossible to find the parent of a branch in a reliable way. This means it's impossible for GetGit to know that the contents of `Main/src` and `Release/1.0/src` should both be migrated to the same root, `/src`.
Branches could also have been renamed or moved at any point in time, which makes it even harder to keep track of what goes where.
In conclusion, any attempt at automatically resolving and migrating branches will most likely produce _garbage_.

Instead, GetGit uses a list of `BranchMapping` items to know which branches to create, from which TFS paths to fetch the branch content, and where to put the content in the Git repository. The `BranchMapping` object is quite simple, it has only one constructor with the following parameters:

|Name|Type|Description|Default|
|---|---|---|---|
|name|`string`|The name of the target Git branch||
|tfsPath|`string`|The path of the source branch in TFS||
|path|`string`|The target path in the Git repository||
|suspicious|`bool`|If a changeset has at least one item with a path marked as suspicious, GetGit will pause and ask if the changeset should be skipped|`false`|
|excluded|`bool`|Exclude a TFS branch from being migrated|`false`|

Using the Git vs TFS example from above, the branch mappings for a master branch and two release branches might look like this:

```csharp
    public static readonly IEnumerable<BranchMapping> BranchMappings = new List<BranchMapping>
    {
        new BranchMapping("master", "$/ProjectX/Main/src", SanePath.Combine(GitRepoPath, "src")),
        new BranchMapping("release/1.0.0", "$/ProjectX/Release/1.0/src", SanePath.Combine(GitRepoPath, "src")),
        new BranchMapping("release/2.0.0", "$/ProjectX/Release/2.0/src", SanePath.Combine(GitRepoPath, "src"))
    };
```

>Please note that because TFS uses forward slashes (`/`) as path separators, GetGit also uses forward slashes internally in all paths to make path matching easier. Because of this, GetGit ships with a utility class, `SanePath`, which helps combining two or more path segments into valid paths with forward slashes as separator.

>Please also note in the example above that branch mapping makes it possible to rename branches when migrating, for example the TFS source branch `$/ProjectX/Release/1.0/` will be migrated to the Git target branch `release/1.0.0`.

If branches has been moved around or renamed in the root, it might be a good idea to add a catch-all mapping to the end of the list and mark it as `suspicious`:

```csharp
    public static readonly IEnumerable<BranchMapping> BranchMappings = new List<BranchMapping>
    {
        new BranchMapping("master", "$/ProjectX/Main/src", SanePath.Combine(GitRepoPath, "src")),
        new BranchMapping("release/1.0.0", "$/ProjectX/Release/1.0/src", SanePath.Combine(GitRepoPath, "src")),
        new BranchMapping("release/2.0.0", "$/ProjectX/Release/2.0/src", SanePath.Combine(GitRepoPath, "src")),
        new BranchMapping("master", "$/ProjectX", SanePath.Combine(GitRepoPath, "src"), true) // DANGER
    };
```

When GetGit encounters a changeset with at least one file or directory on a path that is marked as suspicious, GetGit will pause and ask if the changeset should be downloaded and committed to Git or if it should be skipped. This way, changesets that should be migrated but are on branches that are hard to map in an intuitive and logical way can be caught and migrated interactively.

## Configuration

The configuration lives in a C# class in `src/GetGit/Configuration.cs` and has six sections:

- Variables like hostnames, paths, entry points etc
- Git command line config
- Excluded TFS paths
- Excluded changesets
- User mappings
- Branch mappings

### Variables

|Name|Value|Example|
|---|---|---|
|`TfsServerUri`|A URI pointing to a remote (or local) TFS server, for example|`https://example.com:8080/tfs/YOUR-ROOT`|
|`TfsProject`|The name of the TFS project to be migrated|ProjectX|
|`GitRepoPath`|A path in the local file system where GetGit will create a new, emtpy Git repository|`C:/ProjectX`|
|`GitOrigin`|The remote origin for the new Git repository|`https://example.com/path/to/YOUR-REPO.git`|
|`UserListingBaseHostName`|A hostname which is used to construct email adresses when listing users in the TFS project|`example.com`|
|`ListUsers`|If set to true, GetGit will print a formatted list of users, using the configured `EntryPoint` and exit|`{ @"DOMAIN\username", new UserMapping("User Name", "user.name@example.com") }`|
|`InitializeRepo`|If set to true, GetGit will create an empty Git repository in `GitRepoPath` with a `.gitattributes` and a `.gitignore`|   |
|`EntryPoint`|A point in time and space in TFS to start migrating from|A project path and a changeset id [`new EntryPoint("Main", 1234)`], just a changeset id [`new EntryPoint(1234)`], a range of changeset ids [`new EntryPoint(1234, 4567)`] etc|

### Git command line config

A list of `config` variables that will be passed to Git.

```csharp
    public static readonly string[] GitConfig =
    {
        "core.ignorecase true",
        "core.autocrlf true",
        "core.safecrlf false",
        ...
    };
```

### Excluded TFS paths

A list of regular expressions with path patterns that shouldn't be downloaded, for instance `/obj` folders that was mistakenly checked in, old scm settings files etc:

```csharp
    public static readonly Regex[] ExcludedPaths =
    {
        new Regex(@"/obj/", RegexOptions.IgnoreCase),
        new Regex(@"\.user$", RegexOptions.IgnoreCase),
        new Regex(@"\.vspscc$", RegexOptions.IgnoreCase),
        new Regex(@"\.vssscc$", RegexOptions.IgnoreCase),
        ...
    };
```

### Excluded changesets

A list of changesets that should be skipped when migrating:

```csharp
    public static readonly int[] ExcludedChangesets = 
    {
        1234,
        ...,
        ...
    };
```

### User mappings

Users are mapped in a lookup dictionary with their TFS user names as the key:

```csharp
    public static readonly Dictionary<string, UserMapping> UserMappings = new Dictionary<string, UserMapping>
    {
        { @"DOMAIN\username", new UserMapping("User Name", "user.name@example.com") },
        ...
    };
```

To generate a list of users for a TFS project, set `Configuration.ListUsers` to `true` and run GetGit. This will print a template list of all users who have checked in at least one changeset in `Configuration.TfsProject` between `versionFrom` and `versionTo` in `Configuration.EntryPoint`. The list is formatted as C# code and can be copied and pasted to `Configuration.UserMappings` as is. This list should contain all unique users, but email addresses are constructed with a simple algorithm using the users real name and `Configuration.UserListingBaseHostName` and should probably be edited before migrating.

>Please note that the username key is case sensitive!

>Please also note that the user who checked in a changeset will be both the author and the comitter of the corresponding Git commit.

### Branch mappings

A list of branch mapping items:

```csharp
    public static readonly IEnumerable<BranchMapping> BranchMappings = new List<BranchMapping>
    {
        new BranchMapping("master", "$/ProjectX/Main/src", SanePath.Combine(GitRepoPath, "src")),
        ...
    };
```

> Please note that all paths are case sensitive!

## Git configuration (ignored files and attributes)

Git can be configured locally in the files `.gitattributes` and `.gitignore`. GetGit will create a `.gitattributes` and a `.gitignore` in the root of the new Git repo when (and only when!) Configuration.InitializeRepo is set to `true`. The templates for creating these configuration files are in the directory `src/Application` and should be edited before migrating to suit the needs of the migrated project.

### Git LFS (Large File Support)

Patterns for which files and/or directories should migrated using LFS can be configured in the `.gitattributes` template in `src/Application`.
The template contains examples of common image and document formats.

## Tests

GetGit and its expected behavior is thoroughly tested in the project `src/Tests` using `NUnit`.
The tests are divided into two categories:

- Unit tests: fast, small and data independent tests
- Integration tests: slower tests that interacts with temporary local Git repositories

### Mocking the TFS server

All TFS server calls are abstracted away using a custom fluent API where it's possible to mock users, branches and complex changesets.

## Contributing

All pull requests with bug fixes and new features are welcome and will be considered as long as:

- The project compiles
- The old integration and unit tests pass
- The fix or the new feature is documented with one or several new integration or unit tests
- The pull request only fixes one issue or adds only one feature

Happy contributing :relaxed: :rocket:

## Licensing

This software is licensed with the MIT license.

© 2018 Monounity