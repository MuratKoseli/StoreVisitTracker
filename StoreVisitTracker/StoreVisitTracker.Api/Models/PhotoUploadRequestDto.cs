using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreVisitTracker.Api.Models
{
    public class PhotoUploadRequestDto
    {
        public int VisitId { get; set; }
        public int ProductId { get; set; }
        public string Base64Image { get; set; } = string.Empty;
    }
}