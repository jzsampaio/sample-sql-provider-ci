module Program

open Migrations.RuntimeMigrate
open Server.Shared.Extensions
open Server.Shared.EnvironmentConfiguration

let logSuccess () = printfn "Database migration succeeded."

let logError =
    function
    | UnsuccessfulUpgrade msg -> eprintfn "Could not migrate the database due to: %s" msg
    | DeploymentError msg -> eprintfn "Error while performing database migration: %s" msg

[<EntryPoint>]
let main _ =
    readConnectionStringFromEnvVar ()
    |> migrateToLatest
    |> Result.runEffect logSuccess logError

    0
