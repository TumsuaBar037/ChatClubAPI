namespace ChatClubAPI.Services
{
    public partial class CalculateService
    {
        private readonly DbService _dbService;

        public CalculateService(DbService dbService)
        {
            _dbService = dbService;
        }
    }
}
