/// The data access layer implements data access operations defined and required by the application layer. This
/// implementation contains only DB access, but a bigger application would also perform requests to external services
/// in this layer. This layer uses Dapper as internal implementation.
module DataAccess

open System
open Microsoft.Data.SqlClient
open Dapper
open FsToolkit.ErrorHandling

(* The data access layer depends on the application and model layers. *)

open Model

/// An explicitly-typed connection string.
type ConnectionString = ConnectionString of string

/// Once again, the continuation of the vertical (hence the name) component for the "retrieve all todo items"
/// functionality.
module GetAllTodos =

    open Application.GetAllTodos

    [<CLIMutable>]
    type DbTodo = {
        Id : Guid
        Title : string
        Description : Option<string> // This just works, but since when and how? Implicit conversions in F# Core? Nice.
        CreatedDate : DateTime
        CompletedDate : Option<DateTime>
    }

    /// A regular function which can load all todo items from the DB. It is public for the purpose of (unit) testing the
    /// data access layer in isolation.
    let loadAllTodos (ConnectionString connectionString) = async {
        use conn = new SqlConnection(connectionString)
        let! items = Async.AwaitTask (conn.QueryAsync<DbTodo> "SELECT * FROM dbo.Todo")
        return
            items
            |> Seq.map (fun dbItem -> result {
                let! identifier = TodoId.TryCreate dbItem.Id
                and! title = String255.TryCreate dbItem.Title
                and! description = Option.toResultOption String255.TryCreate dbItem.Description
                return {
                    Todo.Id = identifier
                    Title = title
                    Description = description
                    CreatedDate = dbItem.CreatedDate
                    CompletedDate = dbItem.CompletedDate
                }
            })
            |> Seq.sequenceResultM
            |> Result.map List.ofSeq
            |> function
                | Ok items -> items
                | Error message -> failwith $"Failed to load todo items from the DB: {message}"
    }

    /// The actual implementation of data access operation of the application layer. Mostly delegates to the normal
    /// functions. I'd not put the implementation directly in member because this interface is just a mechanism of
    /// abstraction for the application layer. In the Onion architecture this abstract is crucial, but the _interface_
    /// and its implementations are not really interesting as such.
    let dataAccessImplementation connectionString = { new IDataAccess with
        member _.LoadAllTodoItems = loadAllTodos connectionString
    }