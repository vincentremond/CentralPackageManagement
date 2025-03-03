namespace CentralPackageManagement

open System

[<RequireQualifiedAccess>]
module String =
    let split (separator: string) (value: string) =
        value.Split([| separator |], StringSplitOptions.RemoveEmptyEntries)
        |> Array.toList
