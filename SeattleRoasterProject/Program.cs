using Microsoft.AspNetCore.ResponseCompression;
using SeattleRoasterProject.Data;
using SeattleRoasterProject.Data.Services;
using System.IO.Compression;
using SeattleRoasterProject.Data.Middleware;
using Ljbc1994.Blazor.IntersectionObserver;

var builder = WebApplication.CreateBuilder(args);

if(!builder.Environment.IsProduction())
{
	builder.WebHost.UseStaticWebAssets();
}

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

IHostEnvironment env = builder.Environment;
builder.Configuration.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

builder.Services.Configure<AppSettingsModel>(builder.Configuration.GetSection(AppSettingsModel.SectionName));
var appSettings = builder.Configuration.GetSection(AppSettingsModel.SectionName).Get<AppSettingsModel>();

builder.Services.AddSingleton<EnvironmentSettings>(service => new EnvironmentSettings(appSettings ?? new AppSettingsModel()));

builder.Services.AddSingleton<RoasterService>();
builder.Services.AddSingleton<BeanService>();

builder.Services.AddSingleton<BeanFilterService>();
builder.Services.AddSingleton<BeanSortingService>();
builder.Services.AddSingleton<SearchBeanEncoderService>();
builder.Services.AddSingleton<FavoritesService>();
builder.Services.AddSingleton<SearchSuggestionService>();
builder.Services.AddSingleton<TastingNoteService>();

builder.Services.AddSingleton<TastingNoteCategoryService>();

builder.Services.AddResponseCompression(options =>
{
	options.Providers.Add<BrotliCompressionProvider>();
	options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
	options.Level = CompressionLevel.Optimal;
});

builder.Services.AddWebOptimizer();

builder.Services.AddIntersectionObserver();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseWebOptimizer();
app.UseResponseCompression();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
