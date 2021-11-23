module Result =

    let runEffect okFn errorFn =
        function
        | Ok value -> okFn value
        | Error failure -> errorFn failure

module Program =

    open Migrations.RuntimeMigrate

    let logSuccess () = printfn "Database migration succeeded."

    let logError =
        function
        | UnsuccessfulUpgrade msg -> eprintfn "Could not migrate the database due to: %s" msg
        | DeploymentError msg -> eprintfn "Error while performing database migration: %s" msg

    [<EntryPoint>]
    let main _ =
        (sprintf "Host=%s;Port=%s;Username=%s;Password=%s;Database=%s" "postgres" "5432" "postgres" "admin" "postgres")
        |> migrateToLatest
        |> Result.runEffect logSuccess logError

        0
