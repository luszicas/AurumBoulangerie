using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters; // Necessário para o filtro
using xFood.Application.DTOs;
using xFood.Application.Interfaces;

namespace xFood.Web.Controllers
{
	public class CategoriesController : Controller
	{
		private readonly ICategoryRepository _repo;
		public CategoriesController(ICategoryRepository repo) => _repo = repo;

		// ============================================================
		// 🛡️ SISTEMA DE SEGURANÇA (O PORTEIRO)
		// ============================================================
		// Esse método roda ANTES de qualquer página carregar.
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			// Verifica se a sessão "UserName" existe
			var userName = HttpContext.Session.GetString("UserName");

			if (string.IsNullOrEmpty(userName))
			{
				// Se não tiver logado, chuta para o Login
				context.Result = new RedirectToActionResult("Login", "Account", null);
			}

			base.OnActionExecuting(context);
		}
		// ============================================================

		public async Task<IActionResult> Index()
			=> View(await _repo.GetAllAsync());

		public IActionResult Create() => View(new CategoryDto(0, "", null));

		[HttpPost]
		public async Task<IActionResult> Create(CategoryDto model)
		{
			if (!ModelState.IsValid) return View(model);

			await _repo.CreateAsync(model);

			// --- ADICIONE ESTA LINHA ---
			TempData["Success"] = "Categoria criada com sucesso.";

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Edit(int id)
		{
			var dto = await _repo.GetByIdAsync(id);
			if (dto is null) return NotFound();
			return View(dto);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(CategoryDto model)
		{
			if (!ModelState.IsValid) return View(model);

			await _repo.UpdateAsync(model);

			// --- ADICIONE ESTA LINHA ---
			TempData["Success"] = "Categoria atualizada com sucesso.";

			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> Delete(int id)
		{
			var dto = await _repo.GetByIdAsync(id);
			if (dto is null) return NotFound();

			var hasProducts = await _repo.AnyProductsAsync(id);
			ViewBag.HasProducts = hasProducts;
			return View(dto);
		}

		[HttpPost, ActionName("Delete")]
		public async Task<IActionResult> ConfirmDelete(int id)
		{
			if (await _repo.AnyProductsAsync(id))
			{
				TempData["Error"] = "Não é possível excluir: existem produtos vinculados.";
				return RedirectToAction(nameof(Index));
			}
			await _repo.DeleteAsync(id);
			TempData["Success"] = "Categoria excluída com sucesso.";
			return RedirectToAction(nameof(Index));
		}
	}
}