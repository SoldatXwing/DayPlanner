
var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama")
    .WithGPUSupport()
    .WithDataVolume();

var chat = ollama.AddModel("chat", "deepseek-r1:8b");

var api = builder.AddProject<Projects.DayPlanner_Api>("dayplanner-api")
    .WithReference(chat)
    .WaitFor(chat);

builder.AddProject<Projects.DayPlanner_BackgroundServices>("dayplanner-backgroundservices");
    
builder.AddProject<Projects.DayPlanner_Web>("dayplanner-web")
    .WithReference(api);

builder.Build().Run();
