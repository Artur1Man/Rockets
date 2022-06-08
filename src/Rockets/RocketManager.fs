module RocketManager

open System.Threading
open Microsoft.Extensions.Logging
open Models

type private ManagerAction =
| AddMessage of Message
| GetRocketState of channel:string * AsyncReplyChannel<RocketState option>
| GetAllRocketStates of AsyncReplyChannel<RocketState[]>
| GetAllRockets of AsyncReplyChannel<Rocket[]>


type RocketManager(logger : ILogger<RocketManager>) =
  let mutable rockets : Map<string,Rocket> = Map.empty

  let cts = new CancellationTokenSource()

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
        | GetAllRocketStates replyChannel ->
          let rocketStates =
            rockets
            |> Map.values
            |> Seq.choose (fun r -> 
                try
                  r.CurrentState() |> Some
                with
                | e ->
                  logger.LogWarning(e,"Could not get state for rocket")
                  None
              )
            |> Seq.toArray
          replyChannel.Reply rocketStates
        | GetAllRockets replyChannel ->
          let alRockets = rockets |> Map.values |> Seq.toArray
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

  member _.GetAllRocketStates() =
    rocketAgent.PostAndAsyncReply (fun reply -> GetAllRocketStates reply)

  member _.GetAllRockets() =
    rocketAgent.PostAndAsyncReply (fun reply -> GetAllRockets reply)