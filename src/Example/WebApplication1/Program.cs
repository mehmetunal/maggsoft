using Maggsoft.Framework.Systems;
using Maggsoft.Npgsql.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();

app.CreateDatabase();
// Configure the HTTP request pipeline.
app.AddInfrastructure();