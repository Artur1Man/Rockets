namespace Rockets

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open NSwag.AspNetCore


open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open NJsonSchema.Generation
open Microsoft.Extensions.Hosting

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =

      let builder = WebApplication.CreateBuilder(args)

      builder.Services
        .AddOpenApiDocument(fun s ->
          s.Title <- "Rockets documentation"
          s.Description <- """API documentation Rockets project."""
          s.SchemaNameGenerator <-
            { new ISchemaNameGenerator with member _.Generate(tp) = tp.Name.Replace("Dto", "") }
        )
        .AddControllers()
        .AddNewtonsoftJson()
      |> ignore

      let app = builder.Build()

      app
        .UseHttpsRedirection()
        .UseAuthorization()
        .UseOpenApi()
        .UseSwaggerUi3(fun s ->
          s.EnableTryItOut <- false
          s.Path <- ""
        )
        .UseRouting()
        .UseEndpoints(fun e -> e.MapControllers() |> ignore)
      |> ignore

      app.Run()

      exitCode
