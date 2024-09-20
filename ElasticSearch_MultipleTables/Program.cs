using Elasticsearch.Net;
using ElasticSearch_MultipleTables.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Elasticsearch
var settings = new ConnectionSettings(new Uri("https://10.8.18.105:9200/"))
        .DefaultIndex("products").ServerCertificateValidationCallback(CertificateValidations.AllowAll)
        .BasicAuthentication("elastic", "bm5MpO2w6J8lzIaMQoBA").RequestTimeout(TimeSpan.FromMinutes(2));

var client = new ElasticClient(settings);
builder.Services.AddSingleton<IElasticClient>(client);
builder.Services.AddScoped<ProductService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Initialize Elasticsearch
using (var scope = app.Services.CreateScope())
{
    var elasticClient = scope.ServiceProvider.GetRequiredService<IElasticClient>();
    var productService = scope.ServiceProvider.GetRequiredService<ProductService>();

    // Initialize Elasticsearch index
    await productService.InitializeElasticsearchAsync(elasticClient);

    // Optionally, you can start indexing products here if needed
    // await productService.IndexProductsAsync();
}

app.Run();
