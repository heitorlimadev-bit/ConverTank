using System.Globalization;
using ConverTank.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

// Fora do ambiente de desenvolvimento (versão publicada), usa a pasta do próprio executável
// como raiz: assim wwwroot (CSS/JS) e appsettings.json são encontrados de onde quer que o
// ConverTank.exe seja iniciado (atalho, serviço, outra pasta no terminal).
var emDesenvolvimento = string.Equals(
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = emDesenvolvimento ? null : AppContext.BaseDirectory
});

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ConteudoBanco>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Aplica automaticamente as migrations pendentes ao iniciar (dispensa o "dotnet ef database update")
using (var scope = app.Services.CreateScope())
{
    var banco = scope.ServiceProvider.GetRequiredService<ConteudoBanco>();
    banco.Database.Migrate();
}

var defaultCulture = new CultureInfo("pt-BR");
defaultCulture.NumberFormat.NumberDecimalSeparator = ".";
defaultCulture.NumberFormat.CurrencyDecimalSeparator = ".";

var supportedCultures = new[] { defaultCulture, new CultureInfo("pt-BR"), new CultureInfo("en-US") };

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
