using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using xFood.Application.DTOs;
using xFood.Application.Interfaces; // Importante para usar o Repositório
using xFood.Web.Models;

namespace xFood.Web.Controllers
{
    public class HomeController : Controller
    {
        // Trocamos o DbContext direto pelo Repositório (Mais profissional)
        private readonly IProductRepository _productRepository;

        public HomeController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // --- HELPERS DE PERMISSÃO ---
        private string? GetRole() => HttpContext.Session.GetString("UserRole");
        private bool IsAdmin() => string.Equals(GetRole(), "Admin", StringComparison.OrdinalIgnoreCase);
        private bool IsManager() => string.Equals(GetRole(), "Manager", StringComparison.OrdinalIgnoreCase);

        // AQUI ESTÁ A MÁGICA: Adicionamos o parâmetro 'string? q'
        public async Task<IActionResult> Index(string? q)
        {
            // 1. Busca no banco usando o Repositório
            // Parâmetros: Categoria=null, Busca=q, Página=1, Tamanho=100 (traz bastante coisa pra Home)
            var (total, products) = await _productRepository.GetAllAsync(null, q, 1, 100);

            // 2. Manda o termo pesquisado para a View (para mostrar "Resultados para: ...")
            ViewBag.SearchTerm = q;

            // 3. A REGRA DE OURO DO ESTOQUE:
            // Se quem está acessando NÃO É o Admin e NEM o Gerente...
            if (!IsAdmin() && !IsManager())
            {
                // ... Esconde o que tem estoque zero.
                products = products.Where(p => p.Stock > 0).ToList();
            }

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // --- ADICIONE ESTE MÉTODO NO SEU HOME CONTROLLER ---

        [HttpGet]
        public async Task<IActionResult> SearchJson(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(new List<object>()); // Retorna vazio se digitar pouco

            // Usa o mesmo repositório, mas pedindo uma lista pequena (ex: 5 itens) para ser rápido
            var (total, products) = await _productRepository.GetAllAsync(null, q, 1, 5);

            // Seleciona só o necessário para o "Mini Card" do modal
            var results = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Price.ToString("C"),
                image = p.ImageUrl,
                category = p.CategoryName
            });

            return Json(results);
        }
    }
}


