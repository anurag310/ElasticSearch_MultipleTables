using ElasticSearch_MultipleTables.Data;
using ElasticSearch_MultipleTables.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Nest;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ProductService
{
    private readonly ProductDbContext _dbContext;
    private readonly IElasticClient _elasticClient;

    public ProductService(ProductDbContext dbContext, IElasticClient elasticClient)
    {
        _dbContext = dbContext;
        _elasticClient = elasticClient;
    }

    // Index data from SQL Server to Elasticsearch
    public async Task IndexProductsAsync()
    {
        try
        {
            var products = await _dbContext.Products
                                           .Include(p => p.Category)
                                           .Include(p => p.Reviews)
                                           .ToListAsync();

            foreach (var product in products)
            {
                // Map the product entity to the ProductDocument
                var productDocument = new ProductDocument
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Category = product.Category.Name,
                    Reviews = product.Reviews.ToList()
                };
                try
                {

                    // Index the product document in Elasticsearch
                    var response = await _elasticClient.IndexDocumentAsync(productDocument);
                    if (!response.IsValid)
                    {

                        Console.WriteLine($"Failed to index product {product.Id}: {response.OriginalException?.Message}");
                        continue; // Skip the current product and move to the next
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message );
                }
            }

            Console.WriteLine("All products indexed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while indexing products: {ex.Message}");
        }
    }

    // Search products in Elasticsearch
    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        var searchResponse = await _elasticClient.SearchAsync<Product>(s => s
            .Query(q => q
                .MultiMatch(m => m
                    .Fields(f => f
                        .Field(p => p.Name)
                        .Field(p => p.Description)
                        .Field(p => p.Category.Name)
                        .Field(p => p.Reviews.Select(r => r.ReviewText)))
                    .Query(searchTerm)
                )
            )
        );

        return searchResponse.Documents;
    }
}
