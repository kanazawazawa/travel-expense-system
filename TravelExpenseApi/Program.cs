using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using TravelExpenseApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Azure AD認証の設定
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

// CORS設定 (フロントエンドからのアクセスを許可)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

// Travel Expense Service を DI コンテナに登録
builder.Services.AddSingleton<TravelExpenseService>();

// Fraud Detection Service を DI コンテナに登録
builder.Services.AddSingleton<FraudDetectionService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
// 本番環境でもSwaggerを有効化（テスト/開発目的）
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// CORS を有効化
app.UseCors("AllowAll");

// 認証・認可ミドルウェア
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
