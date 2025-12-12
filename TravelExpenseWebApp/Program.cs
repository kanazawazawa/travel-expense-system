using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using TravelExpenseWebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Azure AD認証の追加 (ダウンストリームAPIへのアクセスを有効化)
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        // アカウント選択を常に表示
        options.Events.OnRedirectToIdentityProvider = context =>
        {
            context.ProtocolMessage.Prompt = "select_account";
            return Task.CompletedTask;
        };
    })
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Enable detailed errors for Blazor Server (development only)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddServerSideBlazor()
        .AddCircuitOptions(options =>
        {
            options.DetailedErrors = true;
        });
}

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

// HttpClientの設定
builder.Services.AddHttpClient();

// API サービスの登録
builder.Services.AddScoped<TravelExpenseWebApp.Services.TravelExpenseApiService>();

// AI Agent サービスの登録
builder.Services.AddSingleton<TravelExpenseWebApp.Services.TravelExpenseUIUpdateService>();
builder.Services.AddSingleton<TravelExpenseWebApp.Services.AzureAIAgentService>();
builder.Services.AddSingleton<TravelExpenseWebApp.Services.AgentModeService>();
builder.Services.AddScoped<TravelExpenseWebApp.Services.RealtimeAudioService>();

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
