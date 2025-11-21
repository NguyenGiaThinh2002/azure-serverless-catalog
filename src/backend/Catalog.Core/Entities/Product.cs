using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Core.Entities
{
    public class Product : BaseEntity
    {
        public Product()
        {
            Type = "Product";
        }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public string? ImageUrl { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public int Stock { get; set; } = 0;
        public bool IsPublished { get; set; } = false;
    }
}
