using Microsoft.AspNetCore.Mvc;
using SampleWebApiApp.BusinessLogic;

namespace SampleWebApiApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly ILogger<ItemsController> _logger;
        private readonly IItemsManager _itemsManager;

        public ItemsController(IItemsManager itemsManager, ILogger<ItemsController> logger)
        {
            _itemsManager = itemsManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return Ok("Items Controller");
        }

        [HttpGet("action1")]
        public async Task<IActionResult> Action1(int departmentId, int param1)
        {
            var response = await _itemsManager.HandleAction1Async(departmentId, param1);
            if (response.IsValid)
            {
                return Ok(response.Response!);
            }
            else
            {
                return BadRequest(response.ErrorMessage);
            }
        }
    }
}
