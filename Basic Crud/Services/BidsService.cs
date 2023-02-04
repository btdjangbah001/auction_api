using Basic_Crud.Data;

namespace Basic_Crud.Services
{
    public class BidsService
    {
        private readonly AppDBContext context;
        private readonly UtilityService utilityService;

        public BidsService(AppDBContext context, UtilityService utilityService)
        {
            this.context = context;
            this.utilityService = utilityService;
        }
    }
}
