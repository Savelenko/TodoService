/// The application layers as a single module. Normally this could be a separate project in the solution.
module Application

(* The application layer depends on the model layer. *)

open Model

/// A (simulated) vertical component for the "retrieve all todo items" functionality. The original example contains all
/// use-case functions in a single module. Here it is convenient to organize them in modules because there will be
/// additional definitions (data access interfaces specific to each use-case) and because this illustrates the structure
/// of a bigger application where vertical components would contain many modules and definitions per module.
module GetAllTodos =
    
    (* The application layer defines and therefore owns operations for accessing "external" data. These are only
    contracts, actual implementation details are not relevant here, intentionally. The interface is defined only in
    terms or the model (and perhaps application) layer types. No DTOs. *)

    type IDataAccess =

        abstract member LoadAllTodoItems : Async<List<Todo>>

    /// The actual use-case of retrieving all todo items.
    let getAllTodos (dataAccess : IDataAccess) =
        // This use-case is trivial as it serves only as a pass-through to the data access layer.
        dataAccess.LoadAllTodoItems