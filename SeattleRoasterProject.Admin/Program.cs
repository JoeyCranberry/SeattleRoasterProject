using RoasterBeansDataAccess.Services;
using SeattleRoasterProject.Admin.Models;
using SeattleRoasterProject.Admin.Services;
using SeattleRoasterProject.Core.Interfaces;
using SeattleRoasterProject.Core.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.Configure<IAppSettings>(builder.Configuration.GetSection(AdminAppSettings.SectionName));
var appSettings = builder.Configuration.GetSection(AdminAppSettings.SectionName).Get<AdminAppSettings>();

builder.Services.AddSingleton<EnvironmentSettings>(service =>
    new EnvironmentSettings(appSettings ?? new AdminAppSettings()));

builder.Services.AddScoped<JsInteropService>();

builder.Services.AddSingleton<RoasterService>();
builder.Services.AddSingleton<BeanService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
