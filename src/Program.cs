using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Video.Frontend;
using Video.Frontend.Application.Interfaces;
using Video.Frontend.Application.Services;
using Video.Frontend.Infrastructure.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── Local Storage ──────────────────────────────────────────────────────────
builder.Services.AddBlazoredLocalStorage();

// ── Handler com JWT ────────────────────────────────────────────────────────
builder.Services.AddTransient<AuthenticatedHttpHandler>();

// ── AuthService (sem token) ────────────────────────────────────────────────
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Auth"] ?? "http://localhost:5000/");
});

// ── UploadService (com token) ──────────────────────────────────────────────
builder.Services.AddHttpClient("UploadService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Upload"] ?? "http://localhost:5001/");
}).AddHttpMessageHandler<AuthenticatedHttpHandler>();

// ── ProcessorService (futuro — com token) ─────────────────────────────────
// builder.Services.AddHttpClient("ProcessorService", client =>
// {
//     client.BaseAddress = new Uri(builder.Configuration["Services:Processor"] ?? "http://localhost:5002/");
// }).AddHttpMessageHandler<AuthenticatedHttpHandler>();

// ── NotificationService (futuro — com token) ──────────────────────────────
// builder.Services.AddHttpClient("NotificationService", client =>
// {
//     client.BaseAddress = new Uri(builder.Configuration["Services:Notification"] ?? "http://localhost:5003/");
// }).AddHttpMessageHandler<AuthenticatedHttpHandler>();

// ── DI ─────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService,   AuthService>();
builder.Services.AddScoped<IUploadService, UploadService>();

// ── ProcessorService e NotificationService (descomentar quando prontos) ────
// builder.Services.AddScoped<IProcessorService,    ProcessorService>();
// builder.Services.AddScoped<INotificationService, NotificationService>();

await builder.Build().RunAsync();
