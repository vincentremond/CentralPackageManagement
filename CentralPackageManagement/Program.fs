module Program

open CentralPackageManagement
open Pinicola.FSharp.SpectreConsole

[<EntryPoint>]
let main argv =
    if Array.contains "--rollback" argv then
        AnsiConsole.markupLineInterpolated $"[bold][yellow]Rolling back to individual package management[/][/]"
        Rollback.fromCentralPackageManagement ()
    else
        AnsiConsole.markupLineInterpolated $"[bold][yellow]Migrating to central package management[/][/]"
        Migrate.toCentralPackageManagement ()

    AnsiConsole.markupLineInterpolated $"[bold][green]Done[/][/]"

    0 // return an integer exit code¡
