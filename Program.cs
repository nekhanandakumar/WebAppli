using EmployeeManagementAPI.Data;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<EmployeeRepository>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        // during development allow common frontend dev servers
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    // permissive policy useful for debugging CORS issues locally
    options.AddPolicy("AllowAnyLocal", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    // use permissive CORS in development to avoid CORS-related blocking while debugging
    app.UseCors("AllowAnyLocal");
}
// ensure CORS is enabled for non-development environments using the intended policy
if (!app.Environment.IsDevelopment())
{
    app.UseCors("AllowReact");

}
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
    RequestPath = "/Uploads"
});

app.UseAuthorization();
app.MapControllers();

app.Run();