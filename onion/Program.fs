(* The most outer layer of the application. In this case a console program. *)

open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Giraffe
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Hosting

(* All layers are visible here. Normally the model layer should not be needed though. *)

open Application
open DataAccess

(* Normally the DB connection string would be in the configuration settings file. *)

let database = ConnectionString "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Todo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"

/// Assembles the ASP.NET "application" from various framework modules.
let private configureApp (app: IApplicationBuilder) =
    app
        .UseHsts()
        .UseGiraffeErrorHandler(WebService.errorHandler)
        .UseStaticFiles()
        .UseAuthentication()
        .UseAuthorization()
        // Provide Giraffe with combined HTTP handlers
        .UseGiraffe(WebService.allServices)

/// Prepares dependency injection consisting of framework and application-specific components.
let private configureServices (services: IServiceCollection) =
    services
        // Some of the following stuff is probably not needed, but I just pasted this from somewhere else
        .AddHsts(fun options -> options.MaxAge <- TimeSpan.FromDays(180.0))
        .AddAuthorization()
        .AddAuthentication(fun options -> options.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme)
        .Services
        // This is the place where the most outer layer chooses implementations of infrastructure layers (data access in
        // this case) which will end up being passed to the application layer. A.k.a the composition root.
        .AddSingleton<GetAllTodos.IDataAccess>(GetAllTodos.dataAccessImplementation database)
        .AddGiraffe()
        |> ignore

/// Go go go!
[<EntryPoint>]
let main argv =
    try
        Host
            .CreateDefaultBuilder(argv)
            .ConfigureWebHostDefaults(fun webHostBuilder ->
                webHostBuilder
                    .ConfigureServices(configureServices)
                    .Configure(configureApp)
                |> ignore
            )
            .UseConsoleLifetime(fun opts -> opts.SuppressStatusMessages <- true)
            .Build()
            .Run()
        0
    with
    | ex -> 1
