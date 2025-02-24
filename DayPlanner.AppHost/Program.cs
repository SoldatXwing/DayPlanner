var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.DayPlanner_Api>("dayplanner-api");

builder.AddProject<Projects.DayPlanner_BackgroundServices>("dayplanner-backgroundservices");
    
builder.AddProject<Projects.DayPlanner_Web>("dayplanner-web")
    .WithReference(api);

builder.Build().Run();
