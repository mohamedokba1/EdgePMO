using AutoMapper;
using EdgePMO.API.Contracts;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EdgePMO.API.Services
{
    public class InstructorsServices : IInstructorServices
    {
        private readonly EdgepmoDbContext _context;
        private readonly IMapper _mapper;

        public InstructorsServices(EdgepmoDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response> GetAllAsync()
        {
            Response response = new Response();
            List<Instructor>? instructors = await _context.Instructors
                .Include(i => i.Courses)
                .ToListAsync();

            response.IsSuccess = true;
            response.Message = "Instructors retrieved successfully.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("instructors", JsonSerializer.SerializeToNode(_mapper.Map<IEnumerable<InstructorReadDto>>(instructors)) ?? JsonValue.Create(Array.Empty<object>()));
            return response;
        }

        public async Task<Response> GetByIdAsync(Guid id)
        {
            Response response = new Response();
            Instructor? instructor = await _context.Instructors
                .Include(i => i.Courses)
                .FirstOrDefaultAsync(i => i.InstructorId == id);

            if (instructor == null)
            {
                response.IsSuccess = false;
                response.Message = "Instructor not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }
            response.IsSuccess = true;
            response.Message = "Instructor retrieved successfully.";
            response.Code = HttpStatusCode.OK;
            response.Result.Add("instructor", JsonSerializer.SerializeToNode(instructor));
            return response;
        }

        public async Task<Response> CreateAsync(InstructorCreateDto dto)
        {
            Response response = new Response();
            Instructor? instructor = new Instructor
            {
                InstructorName = dto.InstructorName.Trim(),
                Profile = dto.Profile?.Trim(),
                ProfileImageUrl = dto.ProfileImageUrl?.Trim()
            };

            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Instructor created successfully.";
            response.Code = HttpStatusCode.Created;
            response.Result.Add("instructor", JsonSerializer.SerializeToNode(instructor));
            return response;
        }

        public async Task<Response> UpdateAsync(InstructorUpdateDto instructor)
        {
            Response response = new Response();
            Instructor? existing = await _context.Instructors.FindAsync(instructor.InstructorId);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Instructor not found.";
                response.Code = HttpStatusCode.NotFound;
                return response;
            }

            if (!string.IsNullOrEmpty(instructor.InstructorName))
            {
                existing.InstructorName = instructor.InstructorName.Trim();
            }
            if (!string.IsNullOrEmpty(instructor.Profile))
            {
                existing.Profile = instructor.Profile.Trim();
            }
            if (!string.IsNullOrEmpty(instructor.ProfileImageUrl))
            {
                existing.ProfileImageUrl = instructor.ProfileImageUrl.Trim();
            }

            await _context.SaveChangesAsync();
            response.IsSuccess = true;
            response.Message = "Instructor updated successfully.";
            response.Code = HttpStatusCode.OK;
            return response;
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            Response response = new Response();
            Instructor? existing = await _context.Instructors.FindAsync(id);
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Instructor not found.";
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            _context.Instructors.Remove(existing);
            await _context.SaveChangesAsync();
            response.IsSuccess = true;
            response.Message = "Instructor deleted successfully.";
            response.Code = HttpStatusCode.OK;
            return response;
        }
    }
}