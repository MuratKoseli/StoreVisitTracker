using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StoreVisitTracker.Api.Models;
using StoreVisitTracker.Domain.Entities;
using StoreVisitTracker.Infrastructure.Db;

namespace StoreVisitTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PhotoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PhotoController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet("visit/{visitId}")]
        public async Task<IActionResult> GetPhotosByVisit(int visitId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var visit = await _context.Visits
                .FirstOrDefaultAsync(v => v.Id == visitId && v.UserId == userId);

            if (visit == null)
                return Forbid("Bu ziyarete ait fotoğrafları görüntüleyemezsiniz.");

            var photos = await _context.Photos
                .Where(p => p.VisitId == visitId)
                .ToListAsync();

            return Ok(photos);
        }



        [HttpPost]
        public async Task<IActionResult> UploadPhoto([FromBody] PhotoCreateRequestDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Visit bu kullanıcıya ait mi kontrol ediyor
            var visit = await _context.Visits
                .FirstOrDefaultAsync(v => v.Id == request.VisitId && v.UserId == userId);

            // Eğer visit kullanıcıya ait değilse kimlik doğrulanmış olsa da Forbid() kullanarak yetkisinin olmadığını belirtiyoruz.
            if (visit == null)
            {
                return Forbid("Bu ziyarete fotoğraf yükleme izniniz yok.");
            }


            var photo = new Photo
            {
                VisitId = request.VisitId,
                ProductId = request.ProductId,
                Base64Image = request.Base64Image,
                UploadedAt = DateTime.UtcNow
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return Ok(photo);
        }

    }
}