
using System.Collections.Generic;
namespace ElasticSearch_MultipleTables.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public required Category Category { get; set; }
        public required ICollection<ProductReview> Reviews { get; set; }
    }
}
