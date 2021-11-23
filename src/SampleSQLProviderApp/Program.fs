open System

type Action =
    | NewUser of char
    | Print

let stateServer =
    MailboxProcessor.Start
        (fun inbox ->
            let rec messageLoop () =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | NewUser c ->
                        printfn "Creating new user"
                        //TODO create user w/ name = c
                    | Print ->
                        printfn "Printing all users"
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
