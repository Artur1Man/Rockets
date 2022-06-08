namespace Controllers


open Microsoft.AspNetCore.Mvc


[<AutoOpen>]
module Common =
  let private actionResult<'T> (o: ActionResult) = ActionResult<'T>.op_Implicit o

  let ok<'T> (result: 'T) = OkObjectResult(result) |> actionResult<'T>
  let created<'T> (result: 'T) = CreatedAtActionResult(null, null, null, result) |> actionResult<'T>

  let badRequest<'T> (message: obj) = BadRequestObjectResult(message) |> actionResult<'T>