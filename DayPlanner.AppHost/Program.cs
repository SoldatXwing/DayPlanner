
var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.DayPlanner_Api>("dayplanner-api");

var isAiEnabled = bool.TryParse(builder.Configuration["AiSettings:IsEnabled"], out var aiEnabled) && aiEnabled;

if (isAiEnabled)
{
    var ollama = builder.AddOllama("ollama")
    .WithGPUSupport()
    .WithDataVolume();

    var chat = ollama.AddModel("chat", builder.Configuration["AiSettings:Model"] ?? throw new NotImplementedException("Ai model isnt set in appsettings. Config path: 'AiSettings:Model'"));
    api.WithReference(chat)
    .WaitFor(chat);
}

builder.AddProject<Projects.DayPlanner_BackgroundServices>("dayplanner-backgroundservices");
    
//builder.AddProject<Projects.DayPlanner_Web>("dayplanner-web")
//    .WithReference(api);

builder.AddProject<Projects.DayPlanner_Web_Wasm>("dayplanner-web-wasm")
    .WithReference(api);

builder.Build().Run();
