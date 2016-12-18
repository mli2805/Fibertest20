#r @"packages\build\FAKE\tools\FakeLib.dll"
open Fake
open Fake.Paket
open Fake.Testing
open Fake.AssemblyInfoFile
open Fake.DotCover

Restore id

!! @"packages\sqlite\build\**\SQLite.props"
|> ReplaceInFiles [("(Exists('packages.config') Or Exists('packages.$(MSBuildProjectName).config')) And", "")]


let major = "0.1"
let minor = getBuildParamOrDefault "minor" "0.0"
let version = major + "." + minor

Target "PatchAssemblyInfo" (fun _ ->
    for file in !! "**/AssemblyInfo.cs" -- "packages/**"  do
        UpdateAttributes file [ 
            Attribute.Version version
            Attribute.FileVersion version]
)

Target "Clean" (fun _ ->
    for dir in !! "**/bin/" ++ "**/obj/"  do
        CleanDir dir
)

Target "Build" (fun _ ->
    !! "*.sln"
    |> MSBuildRelease "" "Build"
    |> Log "Build-Output: "
)
Target "Test" (fun _ ->
    !! "**/bin/release/Tests.dll"
    |> Seq.distinct
    |> xUnit2 (fun x -> { x with Parallel = ParallelMode.All })
)


"Clean"
==> "PatchAssemblyInfo"
==> "Build"
==> "Test"

RunTargetOrDefault "Test"