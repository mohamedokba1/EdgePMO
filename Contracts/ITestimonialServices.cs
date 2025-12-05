using EdgePMO.API.Models;

namespace EdgePMO.API.Contracts
{
    public interface ITestimonialServices
    {
        Task<IEnumerable<Testimonial>> GetAllAsync();
        Task<Testimonial?> GetByIdAsync(Guid id);
        Task<Testimonial> CreateAsync(Testimonial testimonial);
        Task<bool> UpdateAsync(Testimonial testimonial);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<Testimonial>> GetByCourseIdAsync(Guid courseId);
    }
}