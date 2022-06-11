#r "nuget: FSharp.Data"

open FSharp.Data
open FSharp.Data.HttpRequestHeaders
open System

module Messages = 
  let createLaunchMessage channel rocketType mission = 
    sprintf """
  {
    "metadata": {
        "channel": "%s",
        "messageNumber": 1,    
        "messageTime": "2022-02-02T19:39:05.86337+01:00",                                          
        "messageType": "RocketLaunched"                             
    },
    "message": {                                                    
        "type": "%s",
        "launchSpeed": 500,
        "mission": "%s"  
    }
  }
  """ channel rocketType mission

  let createSpeedIncreaseMessage channel messageNumber ammount = 
    sprintf """
  {
      "metadata": {
          "channel": "%s",
          "messageNumber": %i,    
          "messageTime": "2022-02-02T19:39:05.86337+01:00",                                          
          "messageType": "RocketSpeedIncreased"                             
      },
      "message": {
          "by": %i
      }
  }
  """ channel messageNumber ammount

  let createSpeedDecreaseMessage channel messageNumber ammount = 
    sprintf """
  {
      "metadata": {
          "channel": "%s",
          "messageNumber": %i,    
          "messageTime": "2022-02-02T19:39:05.86337+01:00",                                          
          "messageType": "RocketSpeedDecreased"                             
      },
      "message": {
          "by": %i
      }
  }
  """ channel messageNumber ammount

  let createExplodeMessage channel messageNumber = 
    sprintf """
  {
      "metadata": {
          "channel": "%s",
          "messageNumber": %i,    
          "messageTime": "2022-02-02T19:39:05.86337+01:00",                                          
          "messageType": "RocketExploded"                             
      },
      "message": {
          "reason": "PRESSURE_VESSEL_FAILURE"
      }
  }""" channel messageNumber

  let createMissionChangeMessage channel messageNumber mission =
      sprintf """
  {
      "metadata": {
          "channel": "%s",
          "messageNumber": %i,    
          "messageTime": "2022-02-02T19:39:05.86337+01:00",                                          
          "messageType": "RocketMissionChanged"                             
      },
      "message": {
          "newMission":"%s"
      }
  }""" channel messageNumber mission

let rockets = [|
  "rocket 1", "Falcon"
  "rocket 2", "Drowsy"
  "rocket 3", "Looney"
  "rocket 4", "Johny"
  "rocket 5", "Carl VI"
  "rocket 6", "Ingognito"
  "rocket 7", "Woody"
|]

let missions = [|
  "Mercury"
  "Venus"
  "Earth"
  "Mars"
  "Big Boy"
  "Ring Boy"
  "LoL"
  "Roman poseidon"
  "One of us"
|]

let getRandomMission() =
  let rnd = System.Random()
  missions.[rnd.Next(0,missions.Length - 1)]

let sendMessage (msg:string) = 
  let url = "https://localhost:8088/messages"
  Http.Request(
    url,
    httpMethod="POST",
    headers = [ContentType HttpContentTypes.Json],
    body = TextRequest msg
  ) |> ignore


// ---------------------------------------------


// create and send create Rocket messages
let createRockets () =
  rockets
  |> Array.iter (fun (channel, rocketType) -> 
      let mission = getRandomMission ()
      let msg = Messages.createLaunchMessage channel rocketType mission
      sendMessage msg
    )

let worldTour (channel:string) =
  missions
  |> Array.iteri (fun indx mission ->
    let msg = Messages.createMissionChangeMessage channel (indx + 2) mission
    sendMessage msg
  )

let speeeeeeed (channel:string) =
  [| 2 .. 10 |]
  |> Array.iter ( fun indx -> 
    let msg = Messages.createSpeedIncreaseMessage channel indx (indx*100)
    sendMessage msg
  )

// messages are duplicate so only 1 should be stored
let repeatSpeed (channel:string) =
  [| 2 .. 10 |]
  |> Array.iter ( fun indx -> 
    let msg = Messages.createSpeedIncreaseMessage channel 2 (indx*100)
    sendMessage msg
  )

let multipleMsg (channel:string) =
  [| 2 .. 10 |]
  |> Array.iter ( fun indx -> 
    let msg = 
      match indx with
      | 2 | 3 | 5 ->
        Messages.createSpeedIncreaseMessage channel indx (indx*100)
      | 4 | 8 ->
        let newMission = missions.[indx % missions.Length]
        Messages.createMissionChangeMessage channel indx newMission
      | 6 | 7 | 9 ->
        Messages.createSpeedDecreaseMessage channel indx (indx*100)
      | 10 ->
        Messages.createExplodeMessage channel indx
      | _ -> Messages.createSpeedIncreaseMessage channel indx (indx*100) // never happens
    sendMessage msg
  )

let multipleMsgReverseOrder (channel:string) =
  [| 2 .. 10 |]
  |> Array.rev
  |> Array.iter ( fun indx -> 
    let msg = 
      match indx with
      | 2 | 3 | 5 ->
        Messages.createSpeedIncreaseMessage channel indx (indx*100)
      | 4 | 8 ->
        let newMission = missions.[indx % missions.Length]
        Messages.createMissionChangeMessage channel indx newMission
      | 6 | 7 | 9 ->
        Messages.createSpeedDecreaseMessage channel indx (indx*100)
      | 10 ->
        Messages.createExplodeMessage channel indx
      | _ -> Messages.createSpeedIncreaseMessage channel indx (indx*100) // never happens
    sendMessage msg
  )


let earlyDestroy (channel:string) =
  [| 2 .. 5 |]
  |> Array.iter ( fun indx -> 
    let msg = 
      match indx with
      | 3 | 5 ->
        Messages.createSpeedIncreaseMessage channel indx (indx*100)
      | 4 ->
        let newMission = missions.[indx % missions.Length]
        Messages.createMissionChangeMessage channel indx newMission
      | 2 ->
        Messages.createExplodeMessage channel indx
      | _ -> Messages.createSpeedIncreaseMessage channel indx (indx*100) // never happens
    sendMessage msg
  )


// **************
createRockets ()
// **************

// rockets.[0] Control one

// **************
// Mission changes
worldTour (fst rockets.[1])
// **************

// **************
// speed increase a lot
speeeeeeed (fst rockets.[2])
// **************

// **************
// speed increase once only
repeatSpeed (fst rockets.[3])
// **************

// **************
// different message
multipleMsg (fst rockets.[4])
// **************

// **************
// different message in reverse order (should be same as prev res)
multipleMsgReverseOrder (fst rockets.[5])
// **************

// **************
// early destroy so following messages have no effect
earlyDestroy (fst rockets.[6])
// **************
