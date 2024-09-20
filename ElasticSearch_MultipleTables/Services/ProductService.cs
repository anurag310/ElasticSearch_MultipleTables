using ElasticSearch_MultipleTables.Data;
using ElasticSearch_MultipleTables.Models;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
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
    // Initialize Elasticsearch by creating an index with mappings
    public async Task InitializeElasticsearchAsync(IElasticClient client)
    {
        // Check if the index exists
        var indexExistsResponse = await client.Indices.ExistsAsync("esmultipletable");

        if (!indexExistsResponse.Exists)
        {
            // Create the index with mappings
            var createIndexResponse = await client.Indices.CreateAsync("esmultipletable", c => c
                .Map<ProductDocument>(m => m
                    .AutoMap() // Automatically map properties
                    .Properties(props => props
                        .Text(t => t
                            .Name(n => n.Name)
                        )
                        .Text(t => t
                            .Name(n => n.Description)
                        )
                        .Text(t => t
                            .Name(n => n.Category)
                        )
                        .Nested<ProductReview>(n => n
                            .Name(r => r.Reviews)
                            .AutoMap()
                        )
                    )
                )
            );

            if (!createIndexResponse.IsValid)
            {
                throw new Exception($"Failed to create index: {createIndexResponse.OriginalException?.Message}");
            }
        }
    }

    // Index data from SQL Server to Elasticsearch
    public async Task<List<IndexResult>> IndexProductsAsync()
    {
        var results = new List<IndexResult>();

        try
        {
            var products = await _dbContext.Products
                                           .Include(p => p.Category)
                                           .Include(p => p.Reviews)
                                           .ToListAsync();

            foreach (var product in products)
            {
                var productDocument = new ProductDocument
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Category = product.Category?.Name,
                    Reviews = product.Reviews?.Select(r => new ProductReview
                    {
                        Id = r.Id,
                        ProductId = r.ProductId,
                        ReviewText = r.ReviewText,
                        Rating = r.Rating
                    }).ToList()
                };

                try
                {
                    var response = await _elasticClient.IndexDocumentAsync(productDocument);

                    if (response.IsValid)
                    {
                        results.Add(new IndexResult
                        {
                            ProductId = product.Id,
                            Success = true,
                            Message = "Indexed successfully."
                        });
                    }
                    else
                    {
                        results.Add(new IndexResult
                        {
                            ProductId = product.Id,
                            Success = false,
                            Message = response.OriginalException?.Message ?? "Unknown error"
                        });
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new IndexResult
                    {
                        ProductId = product.Id,
                        Success = false,
                        Message = ex.Message
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while indexing products: {ex.Message}");
        }

        Console.WriteLine("Indexing operation completed.");
        return results;
    }
    // Search products in Elasticsearch
    public async Task<IEnumerable<ProductDocument>> SearchProductsAsync(string searchTerm)
    {
        var searchResponse = await _elasticClient.SearchAsync<ProductDocument>(s => s
            .Query(q => q
                .MultiMatch(m => m
                    .Fields(f => f
                        .Field(p => p.Name)
                        .Field(p => p.Description)
                        .Field(p => p.Category))
                    .Query(searchTerm)
                )
            )
        );

        if (!searchResponse.IsValid)
        {
            Console.WriteLine($"Search failed: {searchResponse.OriginalException?.Message}");
            return Enumerable.Empty<ProductDocument>();
        }
        return searchResponse.Documents;
    }
}
