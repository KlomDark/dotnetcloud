using DotNetCloud.Server.UI.Components;
using DotNetCloud.Server.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.JSInterop;

namespace DotNetCloud.Server.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddHttpClient("Default", client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001/"); // API port (default)
            });
            builder.Services.AddHttpClient(); // Registers the default HttpClient for DI
            builder.Services.AddScoped<DotNetCloud.Server.UI.Services.AuthService>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("Default");
                var js = sp.GetRequiredService<IJSRuntime>();
                var nav = sp.GetRequiredService<NavigationManager>();
                return new DotNetCloud.Server.UI.Services.AuthService(httpClient, js, nav);
            });
            builder.Services.AddScoped<SessionService>();
            builder.Services.AddScoped<DotNetCloud.Server.UI.Services.TeamService>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("Default");
                return new DotNetCloud.Server.UI.Services.TeamService(httpClient);
            });
            builder.Services.AddSingleton<CircuitHandler, DiagnosticCircuitHandler>();

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
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }

    public class DiagnosticCircuitHandler : CircuitHandler
    {
        private readonly ILogger<DiagnosticCircuitHandler> _logger;
        public DiagnosticCircuitHandler(ILogger<DiagnosticCircuitHandler> logger)
        {
            _logger = logger;
        }
        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Blazor circuit opened: {circuit.Id}");
            return Task.CompletedTask;
        }
        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Blazor circuit closed: {circuit.Id}");
            return Task.CompletedTask;
        }
        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            _logger.LogWarning($"Blazor circuit connection down: {circuit.Id}");
            return Task.CompletedTask;
        }
        public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Blazor circuit connection up: {circuit.Id}");
            return Task.CompletedTask;
        }
    }
}
