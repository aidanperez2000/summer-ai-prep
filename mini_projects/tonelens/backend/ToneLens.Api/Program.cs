using ToneLens.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

 var ollamaBaseUrl = builder.Configuration.GetValue<string>("Ollama:BaseUrl") ?? "http://localhost:11434";
 var ollamaTimeoutMinutes = builder.Configuration.GetValue<double?>("Ollama:TimeoutMinutes") ?? 5;


builder.Services.AddHttpClient<IToneAnalysisService, OllamaToneAnalysisService>(client =>
{
    client.BaseAddress = new Uri(ollamaBaseUrl);
    client.Timeout = TimeSpan.FromMinutes(ollamaTimeoutMinutes);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
else
{
    app.MapGet("/", () => Results.Ok(new { message = "ToneLens API is running." }));
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();