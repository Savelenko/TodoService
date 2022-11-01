//
open Application
open DataAccess

let database = ConnectionString "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Todo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"

[<EntryPoint>]
let main argv =    
    printfn "%A" (Async.RunSynchronously (GetAllTodos.getAllTodos (GetAllTodos.dataAccessImplementation database)))
    0
