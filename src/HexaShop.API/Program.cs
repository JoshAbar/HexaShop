﻿using Azure.Identity;
using System;
using HexaShop.API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


var builder = WebApplication.CreateBuilder(args);
var connectionString = "";
if (builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"] != null)
{
    var credential = new DefaultAzureCredential();
    builder.Configuration.AddAzureKeyVault(new Uri(builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"]), credential);
    connectionString = builder.Configuration[builder.Configuration["AZURE_SQL_CONNECTION_STRING_KEY"]];
}
else
{
    connectionString = builder.Configuration.GetConnectionString("HexaShopAPIContext");
}

builder.Services.AddDbContext<HexaShopAPIContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());
});

builder.Services.AddControllers();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HexaShopAPIContext>();
    await DbInitializer.Initialize(db);
}

//if (env.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Register the Swagger generator and the Swagger UI middlewares
app.UseOpenApi();
app.UseSwaggerUi3();

app.Run();
