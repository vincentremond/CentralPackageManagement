namespace CentralPackageManagement

open System
open FsToolkit.ErrorHandling
open FParsec

[<CustomComparison>]
[<StructuralEquality>]
type Version =
    | Version4 of int * int * int * int
    | Version3 of int * int * int
    | Version3WithSuffix of int * int * int * string
    | Version2 of int * int

    member this.toString() =
        match this with
        | Version4(major, minor, build, revision) -> $"{major}.{minor}.{build}.{revision}"
        | Version3(major, minor, build) -> $"{major}.{minor}.{build}"
        | Version3WithSuffix(major, minor, build, suffix) -> $"{major}.{minor}.{build}-{suffix}"
        | Version2(major, minor) -> $"{major}.{minor}"

    member this.asTuple() =
        match this with
        | Version4(major, minor, build, revision) -> (major, minor, build, revision, "")
        | Version3(major, minor, build) -> (major, minor, build, 0, "")
        | Version3WithSuffix(major, minor, build, suffix) -> (major, minor, build, 0, suffix)
        | Version2(major, minor) -> (major, minor, 0, 0, "")

    static member parse versionString =

        let (?<|>) p1 p2 = attempt p1 <|> p2

        let pVersion4 =
            (((pint32 .>> pchar '.')
              .>>. (pint32 .>> pchar '.')
              .>>. (pint32 .>> pchar '.')
              .>>. pint32)
             .>> eof)
            |>> (fun (((a, b), c), d) -> Version4(a, b, c, d))

        let pVersion3 =
            (((pint32 .>> pchar '.') .>>. (pint32 .>> pchar '.') .>>. pint32) .>> eof)
            |>> (fun ((a, b), c) -> Version3(a, b, c))

        let pVersion3WithSuffix =
            (((pint32 .>> pchar '.')
              .>>. (pint32 .>> pchar '.')
              .>>. (pint32 .>> pchar '-')
              .>>. restOfLine false)
             .>> eof)
            |>> (fun (((a, b), c), suffix) -> Version3WithSuffix(a, b, c, suffix))

        let pVersion2 = (((pint32 .>> pchar '.') .>>. pint32) .>> eof) |>> Version2

        let parser = pVersion4 ?<|> pVersion3 ?<|> pVersion3WithSuffix ?<|> pVersion2

        match run parser versionString with
        | Success(result, _, _) -> result
        | Failure(errorMsg, _, _) -> failwith errorMsg

    interface IComparable with
        member this.CompareTo(obj: obj) =
            match obj with
            | :? Version as other ->
                let (major, minor, build, revision, suffix) = this.asTuple ()
                let (otherMajor, otherMinor, otherBuild, otherRevision, suffix) = other.asTuple ()

                match major.CompareTo(otherMajor) with
                | 0 ->
                    match minor.CompareTo(otherMinor) with
                    | 0 ->
                        match build.CompareTo(otherBuild) with
                        | 0 ->
                            match revision.CompareTo(otherRevision) with
                            | 0 ->
                                match suffix.CompareTo(suffix) with
                                | 0 -> 0
                                | result -> result
                            | result -> result
                        | result -> result
                    | result -> result
                | result -> result

            | _ -> failwith $"Cannot compare %A{this} with %A{obj}"
