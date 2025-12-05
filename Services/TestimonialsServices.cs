using EdgePMO.API.Contracts;
using EdgePMO.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EdgePMO.API.Services
{
    public class TestimonialsServices : ITestimonialServices
    {
        private readonly EdgepmoDbContext _context;

        public TestimonialsServices(EdgepmoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Testimonial>> GetAllAsync()
        {
            return await _context.Testimonials
                .Include(t => t.Course)
                .ToListAsync();
        }

        public async Task<Testimonial?> GetByIdAsync(Guid id)
        {
            return await _context.Testimonials
                .Include(t => t.Course)
                .FirstOrDefaultAsync(t => t.TestimonialId == id);
        }

        public async Task<IEnumerable<Testimonial>> GetByCourseIdAsync(Guid courseId)
        {
            return await _context.Testimonials
                .Where(t => t.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<Testimonial> CreateAsync(Testimonial testimonial)
        {
            _context.Testimonials.Add(testimonial);
            await _context.SaveChangesAsync();
            return testimonial;
        }

        public async Task<bool> UpdateAsync(Testimonial testimonial)
        {
            var existing = await _context.Testimonials.FindAsync(testimonial.TestimonialId);
            if (existing == null)
                return false;

            _context.Entry(existing).CurrentValues.SetValues(testimonial);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _context.Testimonials.FindAsync(id);
            if (existing == null)
                return false;

            _context.Testimonials.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}