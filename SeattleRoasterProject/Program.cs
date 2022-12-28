using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SeattleRoasterProject.Data;
using SeattleRoasterProject.Data.Services;

var builder = WebApplication.CreateBuilder(args);

if(!builder.Environment.IsProduction())
{
	builder.WebHost.UseStaticWebAssets();
}

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<RoasterService>();
builder.Services.AddSingleton<BeanService>();
builder.Services.AddSingleton<BeanFilterService>();
builder.Services.AddSingleton<BeanSortingService>();
builder.Services.AddSingleton<SearchBeanEncoderService>();
builder.Services.AddSingleton<EnviromentSettings>();
builder.Services.AddSingleton<FavoritesService>();
builder.Services.AddSingleton<SearchSuggestionService>();

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
