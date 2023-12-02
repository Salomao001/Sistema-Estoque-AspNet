using ControleEstoque.Data;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Controllers
{

    public class CategoryController : Controller
    {
        private readonly DataContext _context;
        public CategoryController([FromServices] DataContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<Category>> Post([FromBody] Category model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Categories.Add(model);
                    await _context.SaveChangesAsync();
                    return Ok(model);
                }
                catch
                {
                    return BadRequest(new { message = "Não foi possivel criar a categoria" });
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            return View(categories);
        }

        [HttpGet]
        [Route("id:int")]
        public async Task<ActionResult<Category>> GetById(int id)
        {
            var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (category == null)
            {
                return NotFound(new { message = "Categoria não encontrada" });
            }
            return Ok(category);
        }

        [HttpPut]
        [Route("id:int")]
        public async Task<ActionResult<Category>> Put(int id, [FromBody] Category model)
        {
            if (model.Id != id)
                return NotFound(new { message = "Categoria não encontrada" });

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Categories.Entry(model).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return Ok(model);
                }
                catch
                {
                    return BadRequest(new { message = "Não foi possivel atualizar a categoria" });
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpDelete]
        [Route("id:int")]
        public async Task<ActionResult<Category>> Delete(int id)
        {
            var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if (category != null)
            {
                try
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Categoria removida com sucesso" });
                }
                catch
                {
                    return BadRequest(new { message = "Não foi possivel deletar a categoria" });
                }
            }
            else
            {
                return NotFound(new { message = "Categoria não encontrada" });
            }
        }
    }
}