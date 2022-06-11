
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open NJsonSchema.Generation
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open Newtonsoft.Json.Serialization
open Microsoft.FSharpLu.Json

open RocketManager

module Serializer =
  let adjustSettings (settings: JsonSerializerSettings) =
    settings.ContractResolver <- CamelCasePropertyNamesContractResolver()
    settings.NullValueHandling <- NullValueHandling.Ignore
    settings.DefaultValueHandling <- DefaultValueHandling.Populate
    settings.Converters.Add(StringEnumConverter(AllowIntegerValues = false))
    settings.Converters.Add(CompactUnionJsonConverter())
    settings

    
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
    .AddNewtonsoftJson(fun options -> Serializer.adjustSettings options.SerializerSettings |> ignore)
  |> ignore

  let app = builder.Build()

  app
    .UseHttpsRedirection()
    .UseAuthorization()
    .UseOpenApi()
    .UseSwaggerUi3(fun s ->
      s.DocumentTitle <- "Rockets"
      s.EnableTryItOut <- false
      s.Path <- ""
    )
    .UseRouting()
    .UseEndpoints(fun e -> e.MapControllers() |> ignore)
  |> ignore

  app.Run()

  0
