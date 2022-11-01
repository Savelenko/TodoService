module WebService

open System
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

open Application

(* Note that there is no dependency on the data access layer. Module order in the project emphasizes this. *)

module GetAllTodos =

    open GetAllTodos

    let private getAllTodos next (ctx: HttpContext) = task {
        // Retrieve data access implementation using .NET DI. This is the only place I use DI to avoid fighting against
        // the platform; this is an outer layer (the "shell") after all, so it's OK. Alternatively, passing data access
        // implementation as an argument to this function is also trivial and more explicit (and safe).
        let dataAccess = ctx.GetService<IDataAccess> ()
        // Invoke the use-case in the application layer
        let! todos = GetAllTodos.getAllTodos dataAccess
        // Serialize the result
        return! json todos next ctx
    }

    let httpHandler : HttpHandler =  GET >=> route "/todo" >=> getAllTodos

let errorHandler (ex: Exception) (log: ILogger) =
    log.LogError(EventId (), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500

let allServices : HttpHandler = choose [
    GetAllTodos.httpHandler
]