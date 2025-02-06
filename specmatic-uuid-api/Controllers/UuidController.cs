using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using specmatic_uuid_api.Data;
using specmatic_uuid_api.Models;
using specmatic_uuid_api.Models.Entity;

namespace specmatic_uuid_api.Controllers
{
    [Route("/uuid")]
    [ApiController]
    public class UuidController(AppDbContext dbContext) : ControllerBase
    {
        private readonly AppDbContext _dbContext = dbContext;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UuidRequest request)
        {
            var uuid = new UUID() {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Uuid = Guid.NewGuid(),
                UuidType = request.UuidType
            };

            await _dbContext.UUIDs.AddAsync(uuid);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByTypeAndUuid), new { uuid_type = uuid.UuidType, uuid = uuid.Uuid }, uuid);
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery(Name = "uuid_type")] UuidType? uuidType, [FromQuery] Guid? uuid)
        {
            var query = _dbContext.UUIDs.AsQueryable();

            if (uuidType.HasValue)
            {
                query = query.Where(u => u.UuidType == uuidType.Value);
            }

            if (uuid.HasValue)
            {
                query = query.Where(u => u.Uuid == uuid);
            }

            var result = await query.ToListAsync();
            return Ok(result);
        }

        [HttpGet("{uuid_type}/{uuid}")]
        public async Task<IActionResult> GetByTypeAndUuid([FromRoute(Name = "uuid_type")] UuidType uuid_type, [FromRoute] Guid uuid)
        {
            var entity = await _dbContext.UUIDs.FirstOrDefaultAsync(x => x.Uuid == uuid && x.UuidType == uuid_type);

            if (entity == null)
            {
                return NotFound($"UUID with type {uuid_type} and ID {uuid} not found.");
            }

            return Ok(entity);
        }

        [HttpPatch("{uuid_type}/{uuid}")]
        public async Task<IActionResult?> UpdateByTypeAndUuid([FromRoute(Name = "uuid_type")] UuidType uuid_type, [FromRoute] Guid uuid, [FromBody] Customer customer)
        {
            var entityResult = await GetByTypeAndUuid(uuid_type, uuid);

            if (entityResult is OkObjectResult okResult && okResult.Value is UUID entity)
            {
                entity.FirstName = customer.FirstName;
                entity.LastName = customer.LastName;
                entity.Email = customer.Email;
                await _dbContext.SaveChangesAsync();
                return Ok(entity);
            }

            return entityResult;
        }
    }
}
