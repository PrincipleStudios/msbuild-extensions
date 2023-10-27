# PrincipleStudios.MsBuild.Mutex

Adds tasks to msbuild to protect certain actions with critical sections

## How to use

Add the following line to your msbuild project file inside of an `<ItemGroup>`:

```xml
<PackageReference Include="PrincipleStudios.MsBuild.Mutex" Version="0.1.0" PrivateAssets="All" />
```

In a target, you may now use the following tasks:

- `MutexExec` as [`Exec`](https://learn.microsoft.com/en-us/visualstudio/msbuild/exec-task)
- `MutexMSBuild` as [`MSBuild`](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-task)

See the above corresponding tasks for standard properties. Additional properties on the tasks are as follows:

- `MutexName` (string, required). The name of the mutex. May be re-used between multiple projects to have the critical section span multiple projects.
- `TimeoutSeconds` (number, optional, defaults to 300). The number of seconds to wait for the mutex.

Example:
```xml
<PropertyGroup>
	<PnpmInstallScript Condition=" '$(PnpmInstallScript)' == '' and $(Configuration) != 'Release' ">pnpm install</PnpmInstallScript>
	<PnpmInstallScript Condition=" '$(PnpmInstallScript)' == '' and $(Configuration) == 'Release' ">pnpm install --frozen-lockfile</PnpmInstallScript>

	<!-- pnpm allows an install for the entire solution -->
	<PnpmInstallRecordPath Condition=" '$(PnpmInstallRecordPath)' == '' ">$(PnpmStepRecordDir)_.install.$(Configuration)._</PnpmInstallRecordPath>
</PropertyGroup>

<ItemGroup>
	<!-- SolutionRoot is defined in a Directory.Build.props; see this project's source for example. -->
	<CompileConfig Include="$(SolutionRoot)package.json" />
	<CompileConfig Include="$(SolutionRoot)pnpm-lock.yaml" />
	<CompileConfig Include="package.json" />

	<PackageReference Include="PrincipleStudios.MsBuild.Mutex" Version="0.1.0" PrivateAssets="All" />
</ItemGroup>

<Target Name="_PnpmInstall" Inputs="@(CompileConfig)" Outputs="$(PnpmInstallRecordPath)">
	<!-- Launched via PnpmInstall to run in a critical section -->
	<Exec WorkingDirectory="$(ProjectDir)" Command="$(PnpmInstallScript)" />
	<Touch AlwaysCreate="true" ForceTouch="true" Files="$(PnpmInstallRecordPath)" />
</Target>

<Target Name="PnpmInstall" BeforeTargets="Build" Inputs="@(CompileConfig)" Outputs="$(PnpmInstallRecordPath)">
	<!-- Runs _PnpmInstall in a critical section, meaning only one  -->
	<MutexMsBuild MutexName="jotai-react-signals-pnpm-install"
		Projects="$(MSBuildProjectFullPath)"
		Targets="_PnpmInstall"
		BuildInParallel="true" />
</Target>
```
