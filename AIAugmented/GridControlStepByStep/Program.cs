using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GridControlStepByStep;
using GridControlStepByStep.Services;
using SampleDataRepository;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<GridStateService>();
builder.Services.AddScoped<IGridDataRepository<WeatherData>, WeatherDataRepository>();
builder.Services.AddScoped<IGridDataRepository<TBBurdenData>, TBBurdenDataRepository>();

await builder.Build().RunAsync();