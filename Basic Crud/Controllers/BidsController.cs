using Basic_Crud.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Basic_Crud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidsController : ControllerBase
    {
        private readonly BidsService service;

        public BidsController(BidsService service)
        {
            this.service = service;
        }
    }
}
