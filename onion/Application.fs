module Application

open Model

module GetAllTodos =
    
    type IDataAccess =

        abstract member LoadAllTodoItems : Async<List<Todo>>

    let getAllTodos (dataAccess : IDataAccess) =
        // Just a pass-through to the DAL
        dataAccess.LoadAllTodoItems