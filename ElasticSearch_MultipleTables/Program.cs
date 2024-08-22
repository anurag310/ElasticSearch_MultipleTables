using Elasticsearch.Net;
using ElasticSearch_MultipleTables.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nest;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ProductDbContext>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Elasticsearch
var settings = new ConnectionSettings(new Uri("https://172.19.80.1:9200/"))
    .DefaultIndex("esMultipleTable").ServerCertificateValidationCallback(CertificateValidations.AllowAll)
        .BasicAuthentication("elastic", "Mn0F3LH2ikE4KkMYMdqT").RequestTimeout(TimeSpan.FromMinutes(2)); ;
var client = new ElasticClient(settings);
builder.Services.AddSingleton<IElasticClient>(client);
builder.Services.AddScoped<ProductService>();



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

app.Run();
