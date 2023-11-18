using ChatApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options=>options.AddDefaultPolicy(policy=>policy.AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin()
    .SetIsOriginAllowed(x=>true)
    )
);

var app = builder.Build();

app.MapHub<ChatingHub>("/chatHub");
app.Run();
