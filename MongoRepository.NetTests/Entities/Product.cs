using System;
using MongoRepository.Net;

namespace MongoRepository.NetTests.Entities
{
    /// <summary>
    /// Business Entity for Product
    /// </summary>
    public class Product : Entity
    {
        public Product()
        {
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }
    }
}
