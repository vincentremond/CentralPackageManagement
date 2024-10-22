module Program

open CentralPackageManagement

[<EntryPoint>]
let main argv =
    if Array.contains "--rollback" argv then
        Rollback.fromCentralPackageManagement()
    else
        Migrate.toCentralPackageManagement()
    
    0 // return an integer exit code
