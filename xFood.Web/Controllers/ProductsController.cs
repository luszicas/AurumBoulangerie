using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using xFood.Application.DTOs;
using xFood.Application.Interfaces;

namespace xFood.Web.Controllers
{
	public class ProductsController : Controller
	{
		private readonly IProductRepository _products;
		private readonly ICategoryRepository _categories;
		private readonly IWebHostEnvironment _env;

		public ProductsController(IProductRepository products, ICategoryRepository categories, IWebHostEnvironment env)
			=> (_products, _categories, _env) = (products, categories, env);

		// ============================================================
		// 🛡️ SISTEMA DE SEGURANÇA
		// ============================================================
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var userName = HttpContext.Session.GetString("UserName");
			if (string.IsNullOrEmpty(userName))
			{
				context.Result = new RedirectToActionResult("Login", "Account", null);
				return;
			}
			base.OnActionExecuting(context);
		}

		private string? GetRole() => HttpContext.Session.GetString("UserRole");
		private bool IsAdmin() => string.Equals(GetRole(), "Admin", StringComparison.OrdinalIgnoreCase);
		private bool IsManager() => string.Equals(GetRole(), "Manager", StringComparison.OrdinalIgnoreCase);
		private IActionResult Denied() => Forbid();

        // ============================================================

        // --- AQUI ESTÁ A MUDANÇA ---
        public async Task<IActionResult> Index(int? categoryId, string? q, int page = 1, int size = 50)
        {
            // 1. Carrega as categorias para o filtro lateral (se tiver)
            var cats = await _categories.GetAllAsync();

            // Importante: Manter o que o usuário digitou na caixa de busca da página de resultados
            ViewBag.CurrentSearch = q;
            ViewBag.Categories = new SelectList(cats, "Id", "Name", categoryId);

            // 2. Chama o Repositório passando o "q" que veio do modal
            var (total, items) = await _products.GetAllAsync(categoryId, q, page, size);

            // 3. Aplica a Regra de Ouro (Esconder sem estoque para clientes)
            if (!IsAdmin() && !IsManager())
            {
                items = items.Where(p => p.Stock > 0).ToList();
            }

            return View(items);
        }

        public async Task<IActionResult> Details(int id, string? @return = null)
		{
			var dto = await _products.GetByIdAsync(id);
			return dto is null ? NotFound() : View(dto);
		}

		public async Task<IActionResult> Create()
		{
			if (!(IsAdmin() || IsManager())) return Denied();

			var cats = await _categories.GetAllAsync();
			ViewBag.Categories = new SelectList(cats, "Id", "Name");
			return View(new ProductCreateUpdateDto { Stock = 0, Price = 0m });
		}

		[HttpPost]
		public async Task<IActionResult> Create(ProductCreateUpdateDto model, IFormFile? imageFile, string? @return)
		{
			if (!(IsAdmin() || IsManager())) return Denied();

			if (Request.Form.TryGetValue("Price", out var rawPrice))
			{
				var s = rawPrice.ToString().Trim();
				if (!string.IsNullOrEmpty(s))
				{
					var normalized = s.Replace("R$", "").Trim();
					if (s.Contains(',') && s.Contains('.')) normalized = s.Replace(".", "").Replace(",", ".");
					else if (s.Contains(',')) normalized = s.Replace(",", ".");

					if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
					{
						model.Price = parsed;
						if (ModelState.ContainsKey(nameof(model.Price))) ModelState[nameof(model.Price)]!.Errors.Clear();
					}
				}
			}

			if (!ModelState.IsValid)
			{
				var cats = await _categories.GetAllAsync();
				ViewBag.Categories = new SelectList(cats, "Id", "Name", model.CategoryId);
				return View(model);
			}

			if (imageFile is { Length: > 0 })
			{
				var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
				var folder = Path.Combine(_env.WebRootPath, "uploads");
				Directory.CreateDirectory(folder);
				using var fs = System.IO.File.Create(Path.Combine(folder, fileName));
				await imageFile.CopyToAsync(fs);
				model.ImageUrl = $"/uploads/{fileName}";
			}

			await _products.CreateAsync(model);
			TempData["Success"] = "Produto cadastrado com sucesso.";
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Edit(int id, string? @return = null)
		{
			if (!(IsAdmin() || IsManager())) return Denied();
			var dto = await _products.GetByIdAsync(id);
			if (dto is null) return NotFound();

			var cats = await _categories.GetAllAsync();
			ViewBag.Categories = new SelectList(cats, "Id", "Name", dto.CategoryId);

			var vm = new ProductCreateUpdateDto
			{
				Name = dto.Name,
				Description = dto.Description,
				Price = dto.Price,
				Stock = dto.Stock,
				ImageUrl = dto.ImageUrl,
				CategoryId = dto.CategoryId
			};
			ViewBag.ProductId = dto.Id;
			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(int id, ProductCreateUpdateDto model, IFormFile? imageFile, string? @return)
		{
			if (!(IsAdmin() || IsManager())) return Denied();

			if (Request.Form.TryGetValue("Price", out var rawPrice))
			{
				var s = rawPrice.ToString().Trim();
				if (!string.IsNullOrEmpty(s))
				{
					var normalized = s.Replace("R$", "").Trim();
					if (s.Contains(',') && s.Contains('.')) normalized = s.Replace(".", "").Replace(",", ".");
					else if (s.Contains(',')) normalized = s.Replace(",", ".");

					if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
					{
						model.Price = parsed;
						if (ModelState.ContainsKey(nameof(model.Price))) ModelState[nameof(model.Price)]!.Errors.Clear();
					}
				}
			}

			if (!ModelState.IsValid)
			{
				var cats = await _categories.GetAllAsync();
				ViewBag.Categories = new SelectList(cats, "Id", "Name", model.CategoryId);
				return View(model);
			}

			if (imageFile is { Length: > 0 })
			{
				var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
				var folder = Path.Combine(_env.WebRootPath, "uploads");
				Directory.CreateDirectory(folder);
				using var fs = System.IO.File.Create(Path.Combine(folder, fileName));
				await imageFile.CopyToAsync(fs);
				model.ImageUrl = $"/uploads/{fileName}";
			}
			else
			{
				var existing = await _products.GetByIdAsync(id);
				model.ImageUrl = existing?.ImageUrl;
			}

			await _products.UpdateAsync(id, model);
			TempData["Success"] = "Produto atualizado com sucesso.";
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int id, string? @return = null)
		{
			if (!IsAdmin()) return Denied();
			var dto = await _products.GetByIdAsync(id);
			return dto is null ? NotFound() : View(dto);
		}

		[HttpPost, ActionName("Delete")]
		public async Task<IActionResult> ConfirmDelete(int id, string? @return = null)
		{
			if (!IsAdmin()) return Denied();
			await _products.DeleteAsync(id);
			TempData["Success"] = "Produto excluído com sucesso.";
			return RedirectToAction(nameof(Index));
		}
	}
}