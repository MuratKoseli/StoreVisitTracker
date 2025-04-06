using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreVisitTracker.Domain.Entities
{
    public class Visit
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;

        public DateTime VisitDate { get; set; } = DateTime.UtcNow;

        public VisitStatus Status {get; set;} = VisitStatus.InProgress;

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
    }
}