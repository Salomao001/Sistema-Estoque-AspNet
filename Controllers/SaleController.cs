using ControleEstoque.Data;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ControleEstoque.Controllers
{
    [Route("Sales")]
    public class SaleController : Controller
    {
        private readonly DataContext _context;

        public SaleController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create(int contador = 0)
        {
            var error = "";
            if (contador > 50)
                contador = 50;
            ViewBag.Clients = new SelectList(_context.Clients.Where(x => x.IsDeleted != true).ToList(), "Id", "Name");
            ViewBag.Products = new SelectList(_context.Products.ToList(), "Id", "Title");
            ViewBag.Contador = contador;
            ViewBag.Error = error;
            return View();
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<Sale>> Create(Sale model, int contador)
        {
            model.Value = 0;
            var error = "";
            if (model.SaleProducts.IsNullOrEmpty())
            {
                error = "Deve adicionar um produto a venda";
                ModelState.AddModelError("", "");
            }
            foreach (var item in model.SaleProducts)
            {
                var product = _context.Products.AsNoTracking().Where(x => x.Id == item.ProductId).FirstOrDefault();
                model.Value += item.Quantity * product!.Price;
                item.title = product.Title!;

                if (product.Quantity - item.Quantity < 0)
                {
                    error = "Quantidade inválida";
                    ModelState.AddModelError("Quantity", "Quantidade inválida");
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Sales.Add(model);
                    model.SellDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return BadRequest();
                }
            }
            else
            {
                ViewBag.Error = error;
                ViewBag.Clients = new SelectList(_context.Clients.Where(x => x.IsDeleted != true).ToList(), "Id", "Name");
                ViewBag.Products = new SelectList(_context.Products.ToList(), "Id", "Title");
                ViewBag.Contador = contador;
                return View(model);
            }
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<List<Sale>>> Index(string nameSearchString, string cpfSearchString)
        {
            var sales = await _context.Sales.Include(x => x.Cliente).Include(x => x.SaleProducts).AsNoTracking().ToListAsync();


            if (!string.IsNullOrEmpty(nameSearchString))
            {
                sales = sales.Where(x => x.Cliente!.Name!.ToUpper().StartsWith(nameSearchString.ToUpper())).ToList();
            }

            if (!string.IsNullOrEmpty(cpfSearchString))
                sales = sales.Where(x => x.Cliente!.Cpf!.StartsWith(cpfSearchString)).ToList();

            return View(sales);
        }

        [HttpGet]
        [Route("id:int")]
        public async Task<ActionResult<Sale>> GetById(int id)
        {
            var Sale = await _context.Sales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (Sale == null)
                return NotFound(new { message = "Venda não encontrado" });

            return Ok(Sale);
        }

        [HttpGet]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
                return NotFound();

            var sale = await _context.Sales.AsNoTracking().Include(x => x.Cliente).FirstOrDefaultAsync(x => x.Id == id);

            if (sale == null)
                return NotFound();

            return View(sale);
        }

        [HttpPost]
        [ActionName("delete")]
        [Route("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<Sale>> DeleteConfirmed(int id)
        {
            var sale = await _context.Sales.Include(x => x.SaleProducts).FirstOrDefaultAsync(x => x.Id == id);
            var products = await _context.Products.ToListAsync();

            foreach (var item in sale!.SaleProducts)
            {
                foreach (var _item in products)
                {
                    if (item.ProductId == _item.Id)
                    {
                        _item.Quantity += item.Quantity;
                    }
                }
            }

            if (sale != null)
            {
                _context.Sales.Remove(sale);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}