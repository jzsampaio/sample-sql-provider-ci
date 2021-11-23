open System

open FSharp.Data.Sql

type Action =
    | NewUser of char
    | Print

[<Literal>]
let private CompileTimeConnection =
    "Host=127.0.0.1;Port=5432;Username=postgres;Password=admin;Database=postgres;"

type Schema =
    SqlDataProvider<ConnectionString=CompileTimeConnection, DatabaseVendor=Common.DatabaseProviderTypes.POSTGRESQL>

type DataSource = Schema.dataContext

let createDBContext (connection: string) = Schema.GetDataContext connection

let stateServer =
    MailboxProcessor.Start
        (fun inbox ->
            let rec messageLoop () =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | NewUser c -> printfn "Creating new user"
                    // list of users available at: DataSource.``public.appuserEntity``
                    //TODO create user w/ name = c
                    | Print -> printfn "Printing all users"
                    // TODO print all users on database

                    return! messageLoop ()
                }

            messageLoop ())

let rec client () =
    async {
        printfn "Options:  any char to create new user; 1 to print; 0 to quit"
        let key = Console.ReadKey(true)

        return!
            match key.KeyChar with
            | '1' ->
                stateServer.Post(Print)
                client ()
            | '0' ->
                printfn "Shutting down!"
                async.Return()
            | c ->
                stateServer.Post(NewUser c)
                client ()
    }

[<EntryPoint>]
let main argv =
    client () |> Async.RunSynchronously
    0
