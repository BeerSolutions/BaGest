using BaGest;
using BaGest.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder();

// This will add the BaGest services and options to the container.
builder.Services.AddBaGestWebApplication(bagest =>
{
    bagest.AddSqliteDatabase();
    bagest.AddFileStorage();
});
var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

// Add BaGest's endpoints.
new BaGestEndpointBuilder().MapEndpoints(app);

await app.RunMigrationsAsync();
await app.RunAsync();
