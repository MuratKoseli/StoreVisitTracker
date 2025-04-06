using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreVisitTracker.Domain.Entities
{
    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    }
}