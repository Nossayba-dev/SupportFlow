using Microsoft.AspNetCore.Mvc;

namespace SupportFlow.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
