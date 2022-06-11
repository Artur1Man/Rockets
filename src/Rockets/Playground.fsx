
type maTp = {
  Text : string
  Nr : int
}

let n = 
  [|
    {Text="adfdf32"; Nr=2}
    {Text="12312dsaf"; Nr=3}
    {Text="a123wd1231"; Nr= (-12)}
  |] |> Set

for item in n do
  printfn $"{item}"


n.MinimumElement


let arr = [|1;2;3|] |> Set


let a = arr.Add 12

a.MaximumElement


let mp = 
  [|
    (2,"two")
    (3,"the")
    (12,"tw")
    (4,"fr")
    (0,"zo")
  |] |> Map.ofArray

mp |> Map.iter (fun k i -> printfn $"{k} {i}")

let newMp = 
  mp 
  |> Map.change 5 (fun it ->
        match it with
        | Some v -> Some "Updated val"
        | None -> Some "Was none, now some"
      )