using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using TestRPGGame.DataLoading;
using TestRPGGame.Interfaces;
using TestRPGGame;
using TestRPGGame.Blazor.Services;
using TestRPGGame.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add MudBlazor
builder.Services.AddMudServices();

// Add game services
builder.Services.AddScoped<GameStateService>();
builder.Services.AddSingleton<IDataRepository>(sp =>
{
    var repo = new JsonDataRepository();
    repo.LoadAllData(); // Load data once on startup
    return repo;
});
builder.Services.AddScoped<TestRPGGame.Interfaces.ILogger, ConsoleLogger>();
// RandomProvider is static, no need to register
builder.Services.AddScoped<IGameInterface>(sp =>
{
    var stateService = sp.GetRequiredService<GameStateService>();
    return new BlazorInterface(stateService);
});
builder.Services.AddScoped<GameCore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
