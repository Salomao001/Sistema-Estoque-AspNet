using ControleEstoque.Data;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Controllers
{
    [Route("Clients")]
    public class ClientController : Controller
    {
        private readonly DataContext _context;

        public ClientController(DataContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<Client>> Create([Bind("Id,Name,Cpf")] Client model)
        {
            var baseC = new BaseController();
            model.Name = baseC.ToUpperFirst(model.Name!);

            foreach (var item in _context.Clients)
            {
                if (item.Cpf == model.Cpf && item.IsDeleted == false)
                {
                    ModelState.AddModelError("Cpf", "Este cpf já existe");
                    break;
                }
            }
            // var cpfExistente = _context.Clients.AsNoTracking().FirstOrDefault(x => x.Cpf == model.Cpf);
            // if (cpfExistente != null && cpfExistente.IsDeleted == false)
            //     ModelState.AddModelError("Cpf", "Este cpf já existe");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Clients.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return BadRequest(new { message = "Não foi possivel criar o usuário" });
                }
            }
            else
            {
                return View(model);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Client>>> Index(string nameSearchString, string cpfSearchString)
        {

            var clients = await _context.Clients.AsNoTracking().Where(x => x.IsDeleted != true).ToListAsync();

            if (!string.IsNullOrEmpty(cpfSearchString))
                clients = clients.Where(x => x.Cpf!.StartsWith(cpfSearchString)).ToList();

            if (!string.IsNullOrEmpty(nameSearchString))
            {
                clients = clients.Where(x => x.Name!.ToUpper().StartsWith(nameSearchString.ToUpper())).ToList();
            }

            return View(clients);
        }

        [HttpGet]
        [Route("id:int")]
        public async Task<ActionResult<Client>> GetById(int id)
        {
            var Client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (Client == null)
                return NotFound(new { message = "Usuário não encontrado" });

            return Ok(Client);
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        public async Task<ActionResult> Edit(int id)
        {
            var model = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return View(model);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<Client>> Edit(int id, [Bind("Id,Name,Cpf")] Client model)
        {
            var baseC = new BaseController();
            model.Name = baseC.ToUpperFirst(model.Name!);

            var cpfExistente = _context.Clients.AsNoTracking().FirstOrDefault(x => x.Cpf == model.Cpf);
            if (cpfExistente != null)
                ModelState.AddModelError("Cpf", "Este cpf já existe");

            var Client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (Client == null)
                return NotFound(new { message = "Usuário não encontrado" });
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Clients.Entry(model).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return BadRequest(new { message = "Usuário não encontrado" });
                }
            }
            else
            {
                return View(model);
            }
        }

        [HttpGet]
        [Route("delete/{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var model = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return View(model);
        }

        [HttpPost]
        [Route("delete/{id:int}")]
        [ActionName("delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<Client>> DeleteConfirmed(int id)
        {


            try
            {
                var cliente = await _context.Clients.FindAsync(id);

                var sales = _context.Sales.Where(s => s.ClienteId == cliente!.Id).ToList();
                foreach (var item in sales)
                {
                    if (item.ClienteId == cliente!.Id)
                    {

                        cliente.IsDeleted = true;
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));

                    }
                }
                if (cliente != null)
                {
                    _context.Clients.Remove(cliente);
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