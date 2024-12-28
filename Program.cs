using dotenv.net; // Ensure you are using the correct namespace
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
DotEnv.Load(); // Automatically load variables from a .env file in the root directory


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your Firebase Bearer token below:",
    });

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement {{
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }}
    );
});
builder.Services.AddSingleton<ProductService>();

// Configure CORS policy
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAngularApp", policy => {
        policy.WithOrigins("http://localhost:4200") // Angular's development server
              .AllowAnyMethod()                   // Allow all HTTP methods (GET, POST, etc.)
              .AllowAnyHeader()                   // Allow all headers
              .AllowCredentials();                // Allow cookies and authentication headers
    });
});


//Initialize Firebase Admin SDK
FirebaseInitializer.Initialize();


builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS globally
app.UseCors("AllowAngularApp");

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
