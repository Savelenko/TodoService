module DataAccess

open System
open Microsoft.Data.SqlClient
open Dapper
open FsToolkit.ErrorHandling

open Model

type ConnectionString = ConnectionString of string

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

    let dataAccessImplementation connectionString = { new IDataAccess with
        member _.LoadAllTodoItems = loadAllTodos connectionString
    }