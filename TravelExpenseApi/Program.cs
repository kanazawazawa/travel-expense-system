using TravelExpenseApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

app.UseAuthorization();

app.MapControllers();

app.Run();
