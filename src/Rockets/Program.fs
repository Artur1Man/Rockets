
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open NJsonSchema.Generation

open RocketManager

    
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
    .AddSingleton<RocketManager>(fun x -> x.GetService<ILoggerFactory>().CreateLogger<RocketManager>() |> RocketManager) 
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

  0
