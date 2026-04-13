using Microsoft.AspNetCore.Mvc;
using xFood.Application.Interfaces;

namespace xFood.Web.Controllers;

public class AccountController : Controller
{
    private readonly IUserRepository _userRepo;

    // 1. Injetamos o Repositório no construtor
    public AccountController(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // Se já estiver logado, joga pra home
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        // 2. Pergunta ao Banco de Dados se esse usuário existe
        var user = await _userRepo.AuthenticateAsync(email, password);

        if (user != null)
        {
            // ACHOU! Vamos salvar na sessão.
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetInt32("UserId", user.Id); // Útil para saber quem fez o pedido

            // 3. Traduzir o "Tipo do Banco" para o "Role do Sistema"
            // O banco pode estar escrito "Administrador", mas o sistema espera "Admin"
            string role = "User"; // Padrão

            if (user.TypeUserDescription != null)
            {
                var desc = user.TypeUserDescription.ToLower();
                if (desc.Contains("admin")) role = "Admin";
                else if (desc.Contains("gerente") || desc.Contains("manager")) role = "Manager";
            }

            HttpContext.Session.SetString("UserRole", role);

            return RedirectToAction("Index", "Home");
        }

        // Se chegou aqui, é porque não achou no banco ou senha está errada
        ViewBag.Error = "E-mail ou senha inválidos.";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login"); // Melhor voltar pro Login que pra Home
    }
}