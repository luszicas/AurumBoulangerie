using Microsoft.EntityFrameworkCore;
using xFood.Desktop.UserControls;
using xFood.Domain.Entities;
using xFood.Infrastructure.Persistence;

namespace xFood.Desktop.Forms
{
    public partial class FrmCozinha : Form
    {
        private readonly xFoodDbContext _context;
        private System.Windows.Forms.Timer _timer;

        // O Construtor padrão do Windows Forms
        public FrmCozinha(xFoodDbContext context)
        {
            InitializeComponent(); // <--- ISSO AQUI DESENHA O QUE VOCÊ FEZ NO DESIGNER
            _context = context;

            // Configura o timer para atualizar a cada 5 segundos
            ConfigurarTimer();

            // Carrega a primeira vez
            CarregarPedidos();
        }

        private void ConfigurarTimer()
        {
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 5000; // 5 segundos
            _timer.Tick += (s, e) => CarregarPedidos();
            _timer.Start();
        }

        private async void CarregarPedidos()
        {
            try
            {
                // 1. Busca no Banco
                var pedidos = await _context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.Status != "Entregue")
                    .OrderBy(o => o.OrderDate)
                    .AsNoTracking()
                    .ToListAsync();

                // 2. Limpa o painel VISUAL que você criou (flowPedidos)
                flowPedidos.Controls.Clear();

                // 3. Cria os tickets e joga dentro do painel
                foreach (var pedido in pedidos)
                {
                    // Aqui chamamos aquele UserControl que criamos (o papelzinho)
                    var ticket = new ucTicket(pedido);

                    // Programamos o clique do botão do ticket
                    ticket.BtnAvancarClick += async (s, e) => await AvancarStatus(pedido.Id);

                    // Adiciona VISUALMENTE na tela
                    flowPedidos.Controls.Add(ticket);
                }
            }
            catch (Exception ex)
            {
                // Se der erro, mostra num MessageBox pra gente saber
                MessageBox.Show("Erro ao buscar pedidos: " + ex.Message);
            }
        }

        private async Task AvancarStatus(int pedidoId)
        {
            var pedido = await _context.Orders.FindAsync(pedidoId);

            if (pedido != null)
            {
                // Lógica de avançar status
                if (pedido.Status == "Pendente") pedido.Status = "Em Preparo";
                else if (pedido.Status == "Em Preparo") pedido.Status = "Pronto";
                else if (pedido.Status == "Pronto") pedido.Status = "Entregue";

                await _context.SaveChangesAsync();

                // Atualiza a tela na hora
                CarregarPedidos();
            }
        }

        private void flowPedidos_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}