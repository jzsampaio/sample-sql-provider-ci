module Migrations.RuntimeMigrate

open System.Reflection

open DbUp

type MigrationFailure =
    | UnsuccessfulUpgrade of string
    | DeploymentError of string

let migrateDb assembly (connection: string) =
    try
        let upgrade =
            DeployChanges
                .To
                .PostgresqlDatabase(connection)
                .WithScriptsEmbeddedInAssembly(assembly)
                .LogToConsole()
                .Build()
                .PerformUpgrade()

        if upgrade.Successful then
            Ok()
        else
            Error(UnsuccessfulUpgrade upgrade.Error.Message)
    with
    | ex -> Error(DeploymentError ex.Message)

let migrateToLatest connection =
    let assembly = Assembly.GetExecutingAssembly()

    connection |> migrateDb assembly
