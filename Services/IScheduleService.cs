using HealthcareApp.Models;

namespace HealthcareApp.Services
{
    public interface IScheduleService
    {
        Task<MonthlyScheduleViewModel> GetMonthlyScheduleAsync(string doctorId, DateTime month);
        Task<bool> CreateSpecialScheduleAsync(SpecialSchedule specialSchedule);
        Task<bool> UpdateSpecialScheduleAsync(SpecialSchedule specialSchedule);
        Task<bool> DeleteSpecialScheduleAsync(int id, string doctorId);
        Task<bool> BulkUpdateScheduleAsync(string doctorId, BulkScheduleUpdateModel model);
        Task<bool> SaveScheduleTemplateAsync(string doctorId, string templateName, string? description);
        Task<bool> ApplyScheduleTemplateAsync(string doctorId, int templateId);
        Task<List<ScheduleTemplate>> GetScheduleTemplatesAsync(string doctorId);
    }
}