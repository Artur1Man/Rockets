namespace Controllers

open System

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open RocketManager
open Models


[<ApiController>]
[<Route("[controller]")>]
type RocketsController (logger : ILogger<MessagesController>, rocketManager:RocketManager) =
  inherit ControllerBase()

  /// <summary>
  /// Get all valid rockets with their current state.
  /// Valid rockets are ones that received a launch command.
  /// </summary>
  /// <param name="sorting">Sorting of rockets. Time sorting by the timestamp of earliest message. Default value = ByChannel</param>
  [<HttpGet; Route("all")>]
  member _.GetAll(sorting:RocketSorting) =
    task{
      let! rocketStates = rocketManager.GetRocketStateAll(sorting)
      return ok rocketStates
    }

  /// <summary>
  /// Get the current state of the given rocket
  /// </summary>
  [<HttpGet("{channel}")>]
  member _.GetSingle(channel) =
    task{
      try
        let! rocketState = rocketManager.GetRocketState channel
        match rocketState with
        | Some r ->
          return ok rocketState
        | None ->
          return badRequest $"could not find rocket for channel: {channel}"
      with
      | e ->
        logger.LogWarning(e, "Failed to get rocket state")
        return badRequest e
    }






