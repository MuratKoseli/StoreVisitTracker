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
    public class VisitController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VisitController(AppDbContext context)
        {
            _context = context;
        }


        //  Yeni bir ziyaret oluşturur (JWT’den userId alır).
        [HttpPost]
        public async Task<IActionResult> CreateVisit([FromBody] VisitCreateRequestDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Girilen StoreId'nin var olup olmadığını kontrol ediyoruz. Eğer böyle bir değer yoksa geri bildirim veriyoruz.
            var storeExists = await _context.Stores.AnyAsync(s => s.Id == request.StoreId);
            if (!storeExists)
            {
                return BadRequest($"Geçersiz mağaza ID: {request.StoreId}. Böyle bir mağaza bulunamadı.");
            }

            var visit = new Visit
            {
                StoreId = request.StoreId,
                VisitDate = DateTime.UtcNow,
                Status = VisitStatus.InProgress,
                UserId = userId
            };

            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            return Ok(visit);
        }



        [HttpGet]
        public async Task<IActionResult> GetAllVisits([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var username = User.Identity?.Name;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı.");

            IQueryable<Visit> query = _context.Visits
                .Include(v => v.Store)
                .Include(v => v.Photos);

            if (user.Role != UserRole.Admin)
            {
                query = query.Where(v => v.UserId == userId);
            }

            var pagedVisits = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new
                {
                    v.Id,
                    v.UserId, // Burada UserId'yi getiriyoruz, CurrentUserId' eklediğimiz için.
                    v.VisitDate,
                    v.Status,
                    Store = new
                    {
                        v.Store.Id,
                        v.Store.Name,
                        v.Store.Location
                    },
                    Photos = v.Photos.Select(p => new
                    {
                        p.Id,
                        p.ProductId,
                        p.Base64Image,
                        p.UploadedAt
                    })
                })
                .ToListAsync();

            return Ok(new
            {
                // CurrentUserId sayesinde Frontend ya da test tarafında kimin verisine baktığını görebiliyoruz.
                CurrentUserId = userId,
                Visits = pagedVisits
            });
        }




        
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteVisit(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var visit = await _context.Visits.FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

            if (visit == null)
                return NotFound("Ziyaret bulunamadı veya yetkiniz yok.");

            if (visit.Status == VisitStatus.Completed)
                return BadRequest("Ziyaret zaten tamamlandı.");

            visit.Status = VisitStatus.Completed;
            await _context.SaveChangesAsync();

            return Ok("Ziyaret başarıyla tamamlandı.");
        }
    }
}
