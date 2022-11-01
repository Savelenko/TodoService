/// A service layer exposes use-cases of the application layer to the outside world. This service layer makes them
/// available as HTTP Web-services. It uses Giraffe as internal implementation.
module WebService

open System
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

(* The service layer depends on the application and model layers. There is no dependency on the data access layer;
module order in the project emphasizes this. *)

open Model
open Application

/// Continuation of the vertical (hence the name) component for the "retrieve all todo items" functionality. Module name
/// is deliberately same for the module merging effect: as we move towards the outer layers, each layer contributes more
/// to the vertical component and in the end everything related to retrieving of all todo items is available in the
/// single "module" `GetAllTodos`. See the main program file.
module GetAllTodos =

    open GetAllTodos

    /// Dispatches an HTTP request to the application layer use-case.
    let private getAllTodos next (ctx: HttpContext) = task {
        // Retrieve data access implementation using .NET DI. This is the only place I use DI to avoid fighting against
        // the platform; this is an outer layer (the "shell") after all, so it's OK. Alternatively, passing data access
        // implementation as an argument to this function is also trivial and more explicit (and safe).
        let dataAccess = ctx.GetService<IDataAccess> ()
        // Invoke the use-case in the application layer
        let! todos = GetAllTodos.getAllTodos dataAccess
        // Serialize the result. Default JSON serialization will do fine for now. I will switch to Thoth later, because
        // that is the way.
        return! json todos next ctx
    }

    /// "Routing" for the "retrieve all todo items". This is a tiny vertical component so there is only one URL. In a
    /// large application one vertical component might have multiple URLs associated with it; they would be combined in
    /// this single function using the `choose` combinator.
    let httpHandler : HttpHandler =  GET >=> route "/todo" >=> getAllTodos

/// A generic Giraffe error handles.
let errorHandler (ex: Exception) (log: ILogger) =
    log.LogError(EventId (), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500

/// Combines HTTP handlers of all vertical components into one handler which effectively represents the complete
/// contribution of the service layer.
let allServices : HttpHandler = choose [
    GetAllTodos.httpHandler
]