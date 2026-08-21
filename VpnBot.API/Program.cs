using Infrastructure.Implementation.DataAccess;
using Infrastructure.Implementation.HappSpoofer;
using Infrastructure.Implementation.SingBox;
using Infrastructure.Implementation.Telegram;
using Infrastructure.Implementation.XUI;
using Microsoft.EntityFrameworkCore;
using UseCases;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccess(builder.Configuration);

builder.Services.AddOptions<XUISettings>().BindConfiguration(nameof(XUISettings));
builder.Services.AddXUIClient();

builder.Services.AddOptions<TelegramSettings>().BindConfiguration(nameof(TelegramSettings));
builder.Services.AddTelegramServices();

builder.Services.AddHappSpoofer();
builder.Services.AddMemoryCache();

builder.Services.AddXrayToSingBoxConverter();

builder.Services.AddOptions<VpnSettings>().BindConfiguration(nameof(VpnSettings));
builder.Services.AddOptions<SubscriptionsSettings>().BindConfiguration(nameof(SubscriptionsSettings));
builder.Services.AddUseCases();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseRouting();
app.UseEndpoints(e =>
{
    e.MapControllers();
});
app.Run();
