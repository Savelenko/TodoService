[<AutoOpen>]
module Library

[<RequireQualifiedAccess>]
module Result =

    let ofTryParse originalValue v =
        match v with
        | true, v -> Ok v
        | false, _ -> Error $"Parse failed: '{originalValue}'"

[<RequireQualifiedAccess>]
module Option =

    /// If value is None, returns Ok None, otherwise runs the validator on Some value and wraps the result in Some. Use
    /// this if you want to handle the case for optional data, when you want to validate data *only if there is some*.
    let toResultOption validator value =
        value
        |> Option.map (validator >> Result.map Some)
        |> Option.defaultWith (fun () -> Ok None)