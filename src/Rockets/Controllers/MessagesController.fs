namespace Controllers

open System

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open Newtonsoft.Json
open Microsoft.AspNetCore.Mvc


type MessageTypeDto =
  | RocketLaunched = 1
  | RocketSpeedIncreased = 2
  | RocketSpeedDecreased = 3
  | RocketExploded = 4
  | RocketMissionChanged = 5


type MetadataDto =
  {
    /// Unique channel for the rocket
    [<JsonRequired>] Channel : string
    /// The order of the message within the channel
    [<JsonRequired>] MessageNumber : int
    /// Message sent time
    [<JsonRequired>] MessageTime : DateTimeOffset
    /// Event description
    [<JsonRequired>] MessageType : MessageTypeDto
  }

type MessageDescriptionDto =
  {
    /// RocketLaunched event
    Type : string
    /// RocketLaunched event
    LaunchSpeed : Nullable<int>
    /// RocketLaunched event
    Mission : string
    /// RocketSpeedIncreased & RocketSpeedDecreased events. Speed change ammount
    By : Nullable<int>
    /// RocketExploded event
    Reason : string
    /// RocketMissionChanged event
    NewMission : string
  }


type MessageDto =
  {
    /// Exchange to submit to.
    [<JsonRequired>] Metadata : MetadataDto
    /// Area or Location name.
    [<JsonRequired>] Message: MessageDescriptionDto
  }
  member this.toMessage () =
   
    let messageAction =
      match this.Metadata.MessageType with
      | MessageTypeDto.RocketLaunched ->
        if String.IsNullOrEmpty this.Message.Type then
          invalidArg "message.type" "type field required for RocketLaunched message Type."
        elif String.IsNullOrEmpty this.Message.Mission then
          invalidArg "message.mission" "mission field required for RocketLaunched message Type."
        elif not this.Message.LaunchSpeed.HasValue then
          invalidArg "message.launchSpeed" "launchSpeed field required for RocketLaunched message Type."
        else
          Models.MessageAction.RocketLaunched (Type=this.Message.Type,LaunchSpeed=this.Message.LaunchSpeed.Value,Mission=this.Message.Mission)
      | MessageTypeDto.RocketSpeedIncreased ->
        if not this.Message.By.HasValue then
          invalidArg "message.by" "by field required for RocketSpeedIncreased message Type."
        else
          Models.MessageAction.RocketSpeedIncreased (By=this.Message.By.Value)
      | MessageTypeDto.RocketSpeedDecreased ->
        if not this.Message.By.HasValue then
          invalidArg "message.by" "by field required for RocketSpeedDecreased message Type."
        else
          Models.MessageAction.RocketSpeedDecreased (By=this.Message.By.Value)
      | MessageTypeDto.RocketExploded ->
        if String.IsNullOrEmpty this.Message.Reason then
          invalidArg "message.reason" "reason field required for RocketExploded message Type."
        else
          Models.MessageAction.RocketExploded (Reason=this.Message.Reason)
      | MessageTypeDto.RocketMissionChanged ->
        if String.IsNullOrEmpty this.Message.NewMission then
          invalidArg "message.reason" "reason field required for RocketMissionChanged message Type."
        else
          Models.MessageAction.RocketMissionChanged (NewMission=this.Message.NewMission)
      | _ -> invalidArg "metadata.messageType" "Invalid messageType"
    
    {
      Channel       = this.Metadata.Channel
      MessageNumber = this.Metadata.MessageNumber
      MessageTime   = this.Metadata.MessageTime
      MessageAction = messageAction
    }:Models.Message


[<ApiController>]
[<Route("[controller]")>]
type MessagesController (logger : ILogger<MessagesController>) =
    inherit ControllerBase()

    let summaries =
        [|
            "Freezing"
            "Bracing"
            "Chilly"
            "Cool"
            "Mild"
            "Warm"
            "Balmy"
            "Hot"
            "Sweltering"
            "Scorching"
        |]

    [<HttpGet>]
    member _.Get() =
       summaries

    [<HttpPost>]
    [<ProducesResponseType(StatusCodes.Status200OK)>]
    [<ProducesResponseType(StatusCodes.Status400BadRequest)>]
    member _.Post(message:MessageDto) =
      try
        let modelMessage = message.toMessage()
        logger.LogDebug $"He there {modelMessage}"

        ok modelMessage
      with
      | e ->
        logger.LogWarning(e,"Exception in handling Post message")
        badRequest e


