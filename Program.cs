using HotelPricingEngine.Models;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi.Models;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Add support for API controllers
builder.Services.AddRazorPages();  // Add support for Razor Pages
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<RoomAllocationService>();
builder.Services.AddSingleton<RoomAllocationService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Hotel Booking API",
        Description = "API documentation for Hotel Booking System",
        Contact = new OpenApiContact
        {
            Name = "Yassine Jlassi",
            Email = "yassinej696@gmail.com",
        }
    });
    c.EnableAnnotations();
    c.ExampleFilters();
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<BookingRequestExample>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    
    
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Booking API v1");
    c.RoutePrefix = string.Empty; // To serve Swagger UI at the app's root
});
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
// Map API controllers and Razor Pages
app.MapControllers();   // Maps API controllers
app.MapRazorPages();     // Maps Razor Pages

app.Run();
