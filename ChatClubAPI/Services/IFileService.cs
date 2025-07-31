namespace ChatClubAPI.Services
{
    public interface IFileService
    {
        Task<bool> SaveUserImageAsync(IFormFile file, Guid id);

    }
}
