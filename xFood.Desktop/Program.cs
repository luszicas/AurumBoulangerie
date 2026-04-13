using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using xFood.Desktop.Forms;
using xFood.Infrastructure.Persistence;

namespace xFood.Desktop
{
	internal static class Program
	{
		// Inicializamos com null! para evitar o aviso amarelo (CS8618)
		public static IServiceProvider ServiceProvider { get; private set; } = null!;

		[STAThread]
		static void Main()
		{
			// --- CORREÇÃO 1: Configuração Manual (Funciona sempre) ---
			// Substituímos o ApplicationConfiguration.Initialize() por estes 3 comandos
			// Usamos "System.Windows.Forms." na frente para garantir que é do Windows
			System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.SystemAware);
			System.Windows.Forms.Application.EnableVisualStyles();
			System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

			// Configuração da Injeção de Dependência
			var services = new ServiceCollection();
			ConfigureServices(services);
			ServiceProvider = services.BuildServiceProvider();

			// Pega o formulário pronto do injetor
			var form = ServiceProvider.GetRequiredService<FrmCozinha>();

			// --- CORREÇÃO 2: Caminho Completo ---
			// Escrevendo o nome completo, o C# para de confundir com o seu projeto xFood.Application
			System.Windows.Forms.Application.Run(form);
		}

		private static void ConfigureServices(ServiceCollection services)
		{
			// COLE SUA STRING DE CONEXÃO AQUI
			string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=xFoodDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

			services.AddDbContext<xFoodDbContext>(options =>
				options.UseSqlServer(connectionString));

			// Registra o Formulário
			services.AddTransient<FrmCozinha>();
		}
	}
}