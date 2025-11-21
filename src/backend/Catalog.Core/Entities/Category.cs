using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Core.Entities
{
    public class Category : BaseEntity
    {
        public Category()
        {
            Type = nameof(Category);
        }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; } = string.Empty;
    }
}
