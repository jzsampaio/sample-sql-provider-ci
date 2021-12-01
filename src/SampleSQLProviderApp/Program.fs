open System

open FSharp.Data.Sql

type Action =
    | NewUser of string
    | Clear
    | Print

// The compile time connection remain hard coded. Even on production grade application.
// The compile time connection must be available to your compiler.
[<Literal>]
let private CompileTimeConnection =
    "Host=127.0.0.1;Port=5432;Username=postgres;Password=admin;Database=postgres;"

type Schema =
    SqlDataProvider<ConnectionString=CompileTimeConnection, DatabaseVendor=Common.DatabaseProviderTypes.POSTGRESQL>

type DataSource = Schema.dataContext

// The following is the runtime connection.
// On a production grade application, you should read it from a secret location.
let connString = sprintf "Host=%s;Port=%s;Username=%s;Password=%s;Database=%s" "127.0.0.1" "5432" "postgres" "admin" "postgres"

let createDBContext (connection: string) = Schema.GetDataContext connection

let createUser (ctx: DataSource) name =
    let newUser = ctx.Public.Appuser.``Create(name)``(name)
    ctx.SubmitUpdates()
    printfn "A new user was created: %A" newUser.Name

let deleteUsers (ctx: DataSource) =
    ctx.Public.Appuser
        |> Seq.``delete all items from single table``
        |> Async.RunSynchronously
        |> ignore
    ctx.SubmitUpdates()
    printfn "Users deleted"

let readUsers (ctx: DataSource) =
    let users = query {
        for user in ctx.Public.Appuser do
            select (user)
    }
    match users |> Seq.toList with
    | [] -> printfn "Empty list of users )'="
    | users -> users |> List.iter (fun user -> printfn "User: %s" user.Name)

let stateServer =
    MailboxProcessor.Start
        (fun inbox ->
            let rec messageLoop () =
                async {
                    let! msg = inbox.Receive()
                    let ctx = createDBContext connString

                    match msg with
                    | NewUser name -> createUser ctx name
                    | Clear -> deleteUsers ctx
                    | Print -> readUsers ctx

                    return! messageLoop ()
                }

            messageLoop ())

let rec client () =
    async {
        printfn "Options: a string to create new user; 1 to print; 2 to delete; 0 to quit"
        return!
            match Console.ReadLine() with
            | "0" ->
                printfn "Shutting down!"
                async.Return()
            | "1" ->
                stateServer.Post(Print)
                client ()
            | "2" ->
                stateServer.Post(Clear)
                client ()
            | name ->
                stateServer.Post(NewUser name)
                client ()
    }

[<EntryPoint>]
let main argv =
    client () |> Async.RunSynchronously
    0
