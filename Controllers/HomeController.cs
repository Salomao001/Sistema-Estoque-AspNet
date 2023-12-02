using Microsoft.AspNetCore.Mvc;

namespace ControleEstoque.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}