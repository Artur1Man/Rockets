module RocketManager

open System.Threading
open Microsoft.Extensions.Logging
open Models

type private ManagerAction =
| AddMessage of Message
| GetRocketState of channel:string * AsyncReplyChannel<RocketState option>
| GetRocketStateAll of RocketSorting * AsyncReplyChannel<RocketState[]>
| GetMessage of channel:string * AsyncReplyChannel<Rocket option>
| GetMessageAll of RocketSorting * AsyncReplyChannel<Rocket[]>


type RocketManager(logger : ILogger<RocketManager>) =
  let mutable rockets : Map<string,Rocket> = Map.empty

  let cts = new CancellationTokenSource()

  let sortingFunct (rocketSorting) (rockets:Rocket[]) =
    match rocketSorting with
    | RocketSorting.ByChannel -> rockets |> Array.sortBy (fun x -> x.Channel)
    | RocketSorting.ByTime -> rockets |> Array.sortBy (fun x -> x.EarliestMessageTime())
    | RocketSorting.ByTimeDesc -> rockets |> Array.sortByDescending (fun x -> x.EarliestMessageTime())
    | _ -> rockets |> Array.sortBy (fun x -> x.Channel)

  let body (inbox : MailboxProcessor<ManagerAction>) =
    let loop () =
      async{
        let! action = inbox.Receive()
        match action with
        | AddMessage msg ->
          rockets <-
            rockets
            |> Map.change msg.Channel (fun r -> 
                match r with 
                | Some r -> r.AddMessage msg |> Some // If rocket exists, add message to it
                | None -> Rocket.FromMessage msg |> Some // If rocket does not exist, create new one with the provided message.
              )
        | GetRocketState (channel, replyChannel) ->
          let rocketState =
            match Map.tryFind channel rockets with
            | Some rocket -> rocket.CurrentState() |> Some
            | None -> None
          replyChannel.Reply rocketState
        | GetRocketStateAll (rocketSorting, replyChannel) ->
          let rocketStates =
            rockets
            |> Map.values
            |> Seq.toArray
            |> sortingFunct rocketSorting
            |> Array.choose (fun r -> 
                try
                  r.CurrentState() |> Some
                with
                | e ->
                  logger.LogWarning(e,"Could not get state for rocket")
                  None
              )
          replyChannel.Reply rocketStates
        | GetMessage (channel, replyChannel) ->
          let rocket = Map.tryFind channel rockets
          replyChannel.Reply rocket
        | GetMessageAll (rocketSorting, replyChannel) ->
          let alRockets = rockets |> Map.values |> Seq.toArray |> sortingFunct rocketSorting
          replyChannel.Reply alRockets
      }

    async{
      while  not cts.IsCancellationRequested do
        try 
          do! loop ()
        with
        | e -> 
          logger.LogError(e,"RocketManager crashed loop. Restarting.")
    }

  let rocketAgent = MailboxProcessor.Start(body, cancellationToken = cts.Token)

  member _.AddMessage (msg:Message) =
    rocketAgent.Post (AddMessage msg)

  member _.GetRocketState (channel:string) =
    rocketAgent.PostAndAsyncReply (fun reply -> GetRocketState (channel,reply))

  member _.GetRocketStateAll (sorting:RocketSorting) =
    rocketAgent.PostAndAsyncReply (fun reply -> GetRocketStateAll (sorting,reply))

  member _.GetMessage (channel:string) =
    rocketAgent.PostAndAsyncReply (fun reply -> GetMessage (channel,reply))

  member _.GetMessageAll (sorting:RocketSorting) =
    rocketAgent.PostAndAsyncReply (fun reply -> GetMessageAll (sorting,reply))
