using Infrastructure.Implementation.DataAccess;
using Infrastructure.Implementation.Telegram;
using Infrastructure.Implementation.XUI;
using UseCases;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccess(builder.Configuration);

builder.Services.AddOptions<XUISettings>().BindConfiguration(nameof(XUISettings));
builder.Services.AddXUIClient();

builder.Services.AddOptions<TelegramSettings>().BindConfiguration(nameof(TelegramSettings));
builder.Services.AddTelegramServices();

builder.Services.AddOptions<VpnSettings>().BindConfiguration(nameof(VpnSettings));
builder.Services.AddUseCases();


var app = builder.Build();

app.Run();
