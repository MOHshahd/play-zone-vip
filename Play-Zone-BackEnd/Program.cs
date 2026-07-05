using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Play_Zone_BackEnd.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new 
        { 
            error = exception?.Message ?? "خطأ داخلي في الخادم",
            innerError = exception?.InnerException?.Message,
            stackTrace = exception?.StackTrace 
        });
    });
});

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try { db.Database.Migrate(); }
    catch { db.Database.EnsureCreated(); }
    try
    {
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ServiceRequests' AND xtype='U')
            CREATE TABLE ServiceRequests (
                Id NVARCHAR(450) PRIMARY KEY,
                SessionId NVARCHAR(450) NULL,
                DeviceId NVARCHAR(450) NOT NULL,
                DeviceName NVARCHAR(200) NOT NULL DEFAULT '',
                RequestType NVARCHAR(20) NOT NULL DEFAULT 'CallStaff',
                Description NVARCHAR(500) NOT NULL DEFAULT '',
                Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
            );
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_ServiceRequests_Status')
                CREATE INDEX IX_ServiceRequests_Status ON ServiceRequests (Status);
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_ServiceRequests_CreatedAt')
                CREATE INDEX IX_ServiceRequests_CreatedAt ON ServiceRequests (CreatedAt);
        ");
    }
    catch { }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
