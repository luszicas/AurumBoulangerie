using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

using System.Globalization;

using xFood.Application.Interfaces;
using xFood.Infrastructure.Persistence;
using xFood.Infrastructure.Repositories;
using xFood.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Configuração principal do app ASP.NET Core:
// - Lê connection string e registra serviços (DbContext, repositórios, MVC, Swagger, CORS, Session)
// - Define middlewares globais (HTTPS, cultura pt-BR, roteamento, CORS, autorização, sessão)
// - Mapeia endpoints (API por atributos, MVC e assets estáticos)
// - Executa seed/migração do banco e inicia o host

#region 1) Configura��es (antes de registrar servi�os)
// Respons�vel por: ler appsettings, strings de conex�o, flags globais, etc.
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? "Server=(localdb)\\MSSQLLocalDB;Database=xFoodDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
#endregion

#region 2) Servi�os (Dependency Injection)
// Respons�vel por: registrar DbContext, reposit�rios, MVC, Swagger, CORS, Session, etc.

// Reposit�rios (Application -> Infrastructure)
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Reposit�rios (Application -> Infrastructure)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITypeUserRepository, TypeUserRepository>();


// EF Core
builder.Services.AddDbContext<xFoodDbContext>(options =>
    options.UseNpgsql(conn));

// MVC (controllers + views) e API Explorer/Swagger
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (liberado para estudos; restrinja em produ��o)
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

// Session (para login simples e sauda��o)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
#endregion

var app = builder.Build();

#region 3) Tratamento de erros / seguran�a por ambiente
// Respons�vel por: p�ginas de erro, HSTS e Swagger em Dev
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
#endregion

#region 4) Middlewares globais (ordem importa!)
// Respons�vel por: HTTPS, cultura, roteamento, CORS, autentica��o/autoriza��es, sess�o

app.UseHttpsRedirection();

// Cultura pt-BR
var defaultCulture = new CultureInfo("pt-BR");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new[] { defaultCulture },
    SupportedUICultures = new[] { defaultCulture }
};
app.UseRequestLocalization(localizationOptions);

app.UseRouting();

// CORS deve ficar entre UseRouting e os endpoints
app.UseCors("AllowAll");

// (futuro) app.UseAuthentication();
app.UseAuthorization();

// Session antes dos endpoints mapeados
app.UseSession();
#endregion

#region 5) Endpoints (rotas)
// Respons�vel por: mapear controllers de API e MVC, assets est�ticos
app.MapControllers();           // API por atributo
app.MapStaticAssets();          // novo pipeline de est�ticos (ASP.NET Core 8/9)
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
   .WithStaticAssets();
#endregion

#region 6) Seed/Migrate (cria/migra/popula banco)
// Respons�vel por: preparar o banco na primeira execu��o
await SeedService.EnsureSeedAsync(app.Services);
#endregion

#region 7) Run (bootstrap final)

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
app.Run();
#endregion
