namespace ChatClubAPI.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<bool> SaveUserImageAsync(IFormFile file, Guid id)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            var fileName = id + Path.GetExtension(file.FileName);

            var folderPath = Path.Combine(_env.WebRootPath, "uploads", "images", "users");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return true;
        }
    }
}
