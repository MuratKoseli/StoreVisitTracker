using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreVisitTracker.Domain.Entities
{
    public class Photo
    {
        public int Id { get; set; }

        public int VisitId { get; set; }
        public Visit Visit { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Base64Image { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}