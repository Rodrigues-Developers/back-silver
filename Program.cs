using dotenv.net; // Ensure you are using the correct namespace
using YourProject.Services;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from the .env file
DotEnv.Load(); // This will automatically load variables from a .env file in the root directory

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
