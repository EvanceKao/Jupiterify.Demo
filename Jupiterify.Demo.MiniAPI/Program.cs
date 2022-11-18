using Jupiterify.Demo.MiniAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddDbContext<DemoDB>(options => options.UseInMemoryDatabase("items"));
var connectionString = builder.Configuration.GetConnectionString("DemoDB") ?? "Data Source=Demo.db";  // Data Source 的值會是 SQLite 的檔案名稱
builder.Services.AddSqlite<DemoDB>(connectionString);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Member API",
        Description = "Demo with minimal API and EntityFramework Core",
        Version = "v1"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Member API V1");
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
});

app.MapGet("/memberprofiles", async (DemoDB db) => await db.MemberProfiles.ToListAsync());

app.MapPost("/memberprofiles", async (DemoDB db, MemberProfileEntity memberprofile) =>
{
    await db.MemberProfiles.AddAsync(memberprofile);
    await db.SaveChangesAsync();
    return Results.Created($"/memberprofiles/{memberprofile.Id}", memberprofile);
});

app.MapGet("/memberprofiles/{id}", async (DemoDB db, long id) => await db.MemberProfiles.FindAsync(id));

app.MapPut("/memberprofiles/{id}", async (DemoDB db, MemberProfileEntity updateMemberProfile, long id) =>
{
    var originMemberProfile = await db.MemberProfiles.FindAsync(id);
    if (originMemberProfile is null) return Results.NotFound();
    originMemberProfile.Name = updateMemberProfile.Name;
    originMemberProfile.State = updateMemberProfile.State;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/memberprofiles/{id}", async (DemoDB db, long id) =>
{
    var originMemberProfile = await db.MemberProfiles.FindAsync(id);
    if (originMemberProfile is null) return Results.NotFound();
    db.MemberProfiles.Remove(originMemberProfile);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}