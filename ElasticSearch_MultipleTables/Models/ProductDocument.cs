namespace ElasticSearch_MultipleTables.Models
{
    public class ProductDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public List<ProductReview> Reviews { get; set; }
        
    }
}
