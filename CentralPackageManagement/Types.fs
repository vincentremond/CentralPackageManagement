module CentralPackageManagement.Types

open System
open FsToolkit.ErrorHandling

type PackageId =
    | PackageId of string

    member this.Value =
        match this with
        | PackageId value -> value

[<RequireQualifiedAccess>]
module String =
    let split (separator: string) (value: string) =
        value.Split([| separator |], StringSplitOptions.RemoveEmptyEntries)
        |> Array.toList

[<CustomComparison>]
[<StructuralEquality>]
type Version =
    | Version4 of int * int * int * int
    | Version3 of int * int * int
    | Version2 of int * int

    member this.toString() =
        match this with
        | Version4(major, minor, build, revision) -> $"{major}.{minor}.{build}.{revision}"
        | Version3(major, minor, build) -> $"{major}.{minor}.{build}"
        | Version2(major, minor) -> $"{major}.{minor}"

    member this.asTuple() =
        match this with
        | Version4(major, minor, build, revision) -> (major, minor, build, revision)
        | Version3(major, minor, build) -> (major, minor, build, 0)
        | Version2(major, minor) -> (major, minor, 0, 0)

    static member parse versionString =

        let parseInt (s: string) =
            match Int32.TryParse(s) with
            | true, value -> Some value
            | false, _ -> None

        let segments = versionString |> String.split "." |> List.traverseOptionM parseInt

        match segments with
        | Some [ major; minor; build; revision ] -> Version4(major, minor, build, revision)
        | Some [ major; minor; build ] -> Version3(major, minor, build)
        | Some [ major; minor ] -> Version2(major, minor)
        | _ -> failwith $"Invalid version string: %A{versionString}"

    interface IComparable with
        member this.CompareTo(obj: obj) =
            match obj with
            | :? Version as other ->
                let (major, minor, build, revision) = this.asTuple ()
                let (otherMajor, otherMinor, otherBuild, otherRevision) = other.asTuple ()

                match major.CompareTo(otherMajor) with
                | 0 ->
                    match minor.CompareTo(otherMinor) with
                    | 0 ->
                        match build.CompareTo(otherBuild) with
                        | 0 -> revision.CompareTo(otherRevision)
                        | result -> result
                    | result -> result
                | result -> result

            | _ -> failwith $"Cannot compare %A{this} with %A{obj}"

type CentralPackageReference = {
    PackageId: PackageId
    Version: Version
}
