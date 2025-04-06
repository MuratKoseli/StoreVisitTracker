using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreVisitTracker.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public UserRole Role { get; set; }



        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    }
}