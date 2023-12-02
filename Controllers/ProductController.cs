using ControleEstoque.Data;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Controllers
{
    [Route("products")]
    public class ProductController : Controller
    {
        private readonly DataContext _context;
        public ProductController(DataContext context)
        {
            _context = context;
        }

        [Route("create")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Title");
            return View();
        }

        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Price,Quantity,Perishable,ExpirationDate,CategoryId")] Product model, string? addCategoryString, string price)
        {
            var baseC = new BaseController();
            model.Title = baseC.ToUpperFirst(model.Title!);

            if (model.Perishable == true && model.ExpirationDate == null)
                ModelState.AddModelError("ExpirationDate", "Produtos pereciveis devem possuir uma data de validade");

            if (model.CategoryId == 2147483647 && string.IsNullOrEmpty(addCategoryString))
                ModelState.AddModelError("Category.Title", "Campo Obrigatório");

            if (model.CategoryId == 2147483647 && !string.IsNullOrEmpty(addCategoryString))
            {
                if (addCategoryString!.Length < 3 || addCategoryString.Length > 60)
                    ModelState.AddModelError("Category.Title", "Este campo deve conter entre 3 e 60 caracteres");
            }

            if (model.CategoryId == 2147483647 && _context.Categories.ToList().Exists(x => x.Title!.ToUpper() == addCategoryString!.ToUpper()))
                ModelState.AddModelError("Category.Title", "Esta categoria já existe");


            if (!decimal.TryParse(model.Price.ToString(), out var parsedPrice))
            {
                ModelState.AddModelError("Price", "Preço invalido");
            }
            else
            {
                model.Price = parsedPrice;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(addCategoryString))
                    {
                        var category = new Category
                        {
                            Title = baseC.ToUpperFirst(addCategoryString)
                        };
                        _context.Categories.Add(category);
                        await _context.SaveChangesAsync();
                        model.CategoryId = category.Id;
                    }


                    if (model.Perishable == false)
                        model.ExpirationDate = null;

                    _context.Products.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    return BadRequest();
                }
            }
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Title");
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult<List<Product>>> Index(string searchString, string categorySearch, string searchStock)
        {
            var categoria = _context.Categories.FirstOrDefault(x => x.Id == Convert.ToInt32(categorySearch));

            var products = await _context.Products.Include(x => x.Category).AsNoTracking().ToListAsync();
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(x => x.Title!.ToUpper().StartsWith(searchString.ToUpper())).ToList();
            }
            if (!string.IsNullOrEmpty(searchStock))
            {
                if (searchStock.Equals("Em estoque"))
                    products = products.Where(x => x.Quantity > 0).ToList();

                if (searchStock.Equals("Esgotado"))
                    products = products.Where(x => x.Quantity == 0).ToList();

            }
            if (!string.IsNullOrEmpty(categorySearch))
            {
                products = products.Where(x => x.Category!.Title! == categoria!.Title).ToList();
            }
            products = products.OrderByDescending(x => x.ExpirationDate).ToList();
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Title");
            return View(products);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await _context.Products.Include(x => x.Category).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
                return NotFound(new { message = "Produto não encontrado" });

            return Ok(product);
        }

        [HttpGet]
        [Route("active")]
        public async Task<ActionResult<List<Product>>> GetActive()
        {
            var products = await _context.Products.Include(x => x.Category).Where(x => x.Quantity > 0).AsNoTracking().ToListAsync();
            return Ok(products);
        }

        [HttpGet]
        [Route("soldout")]
        public async Task<ActionResult<List<Product>>> GetSouldOut()
        {
            var products = await _context.Products.Include(x => x.Category).Where(x => x.Quantity < 1).AsNoTracking().ToListAsync();
            return Ok(products);
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
                return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Title");
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit/{id:int}")]
        public async Task<ActionResult<Product>> Edit(int id, int quantidade, Product model)
        {
            model = new Product
            {
                Title = null,
                Price = 0,
                Quantity = 0,
                Perishable = true,
                ExpirationDate = null
            };
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
                return NotFound();

            if (quantidade > 0)
            {

                try
                {
                    product.Quantity += quantidade;
                    _context.Products.Entry(product).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return BadRequest(new { message = "Não foi possivel atualizar o produto" });
                }
            }
            else
            {
                if (!ModelState.IsValid)
                    return View(model);
                return View(model);
            }
        }

        [HttpGet]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
                return NotFound();

            var product = await _context.Products.AsNoTracking().Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [Route("delete/{id:int}")]
        [HttpPost, ActionName("delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product != null)
                {
                    _context.Products.Remove(product);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}