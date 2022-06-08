module Models

open System


type MessageAction = 
  | RocketLaunched of Type:string * LaunchSpeed:int * Mission:string
  | RocketSpeedIncreased of By:int
  | RocketSpeedDecreased of By:int
  | RocketExploded of Reason:string
  | RocketMissionChanged of NewMission:string

[<CustomEquality; CustomComparison>]
type Message = 
  {
    Channel : string
    MessageNumber : int
    MessageTime : DateTimeOffset
    MessageAction : MessageAction
  }
  // Custom Comparison for easy 
  override this.Equals other =
    match other with
    | :? Message as p ->
      (p.Channel.Equals this.Channel)
      && (p.MessageNumber.Equals this.MessageNumber)
    | _ -> false
  override this.GetHashCode () = (this.Channel,this.MessageNumber).GetHashCode()
  interface IComparable<Message> with
    member this.CompareTo other = this.MessageNumber.CompareTo other.MessageNumber
  interface IComparable with
    member this.CompareTo other =
        match other with
        | :? Message as p -> (this :> IComparable<_>).CompareTo p
        | _ -> -1

type RocketStatus =
| Operational
| Exploded of reason:string

type RocketState = {
  Channel : string
  Type : string
  CurrentSpeed : int
  CurrentMission : string
  CurrentStatus : RocketStatus
}

type Rocket =
  {
    Messages : Set<Message>
    Channel : string
  }
  member this.AddMessage (msg:Message) =
    {
      Messages = this.Messages.Add msg
      Channel = this.Channel
    }
  member this.CurrentState () =
    /// Return new state if currently operational, otherwise keep the old one.
    let updateIfOperational (currentState:RocketState) (newState:RocketState) =
      match currentState.CurrentStatus with
      | Operational -> newState
      | Exploded _ -> currentState
    /// Apply the changes from the new message to the current state
    let updateState (currentState:RocketState) (msg:Message) =
      match msg.MessageAction with
      | RocketLaunched _ -> currentState // Lauched should always be first. Handled in init step
      | RocketSpeedIncreased by -> 
        {currentState with CurrentSpeed = currentState.CurrentSpeed + by}
        |> updateIfOperational currentState
      | RocketSpeedDecreased by -> 
        {currentState with CurrentSpeed = currentState.CurrentSpeed - by}
        |> updateIfOperational currentState
      | RocketMissionChanged newMission -> 
        {currentState with CurrentMission = newMission}
        |> updateIfOperational currentState
      | RocketExploded reason ->
        {currentState with CurrentStatus = Exploded reason}
        |> updateIfOperational currentState

    let firstMessage = this.Messages.MinimumElement
    match firstMessage.MessageAction with
    | RocketLaunched (rocketType,launchSpeed,mission) ->
      let initialState = {
        Channel = this.Channel
        Type = rocketType
        CurrentSpeed = launchSpeed
        CurrentMission = mission
        CurrentStatus = Operational
      }
      // We defined custom comparison for Message type so the set will keep them in order of MessageNumber
      this.Messages
      |> Set.fold updateState initialState

    | _ -> failwith $"No launch message received for rocket {this.Channel} yet."

  static member FromMessage (msg:Message) =
    {
      Messages = Set.empty.Add msg
      Channel = msg.Channel
    }


