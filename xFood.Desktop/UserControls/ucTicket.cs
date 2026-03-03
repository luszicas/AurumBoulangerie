//using System.Drawing;
//using System.Windows.Forms;
//using xFood.Domain.Entities; // Certifique-se que o xFood.Domain está referenciado

//namespace xFood.Desktop.UserControls
//{
//	public partial class ucTicket : UserControl
//	{
//		public Order Pedido { get; private set; }
//		public event EventHandler BtnAvancarClick; // Evento pro pai saber que clicou

//		public ucTicket(Order pedido)
//		{
//			InitializeComponent();
//			Pedido = pedido;
//			ConfigurarVisual();
//		}

//		private void ConfigurarVisual()
//		{
//			// 1. Configuração do Cartão
//			this.Size = new Size(280, 320);
//			this.BackColor = Color.White;
//			this.BorderStyle = BorderStyle.FixedSingle;
//			this.Margin = new Padding(10); // Espaço entre um e outro

//			// 2. Faixa Colorida do Status (Topo)
//			Panel pnlTopo = new Panel { Dock = DockStyle.Top, Height = 8 };
//			pnlTopo.BackColor = ObterCorStatus();
//			this.Controls.Add(pnlTopo);

//			// 3. Botão de Ação (Rodapé)
//			Button btnAcao = new Button
//			{
//				Text = "AVANÇAR STATUS >",
//				Dock = DockStyle.Bottom,
//				Height = 45,
//				BackColor = Color.FromArgb(30, 30, 30),
//				ForeColor = Color.White,
//				FlatStyle = FlatStyle.Flat,
//				Font = new Font("Segoe UI", 9, FontStyle.Bold),
//				Cursor = Cursors.Hand
//			};
//			btnAcao.FlatAppearance.BorderSize = 0;
//			btnAcao.Click += (s, e) => BtnAvancarClick?.Invoke(this, EventArgs.Empty);
//			this.Controls.Add(btnAcao);

//			// 4. Labels (Conteúdo)
//			Label lblId = new Label
//			{
//				Text = $"#{Pedido.OrderNumber}",
//				Font = new Font("Segoe UI", 18, FontStyle.Bold),
//				ForeColor = Color.Black,
//				Location = new Point(15, 25),
//				AutoSize = true
//			};

//			Label lblCliente = new Label
//			{
//				Text = Pedido.CustomerName.ToUpper(),
//				Font = new Font("Segoe UI", 10, FontStyle.Regular),
//				ForeColor = Color.Gray,
//				Location = new Point(15, 60),
//				AutoSize = true
//			};

//			// Lista de Produtos
//			string listaProdutos = "";
//			foreach (var item in Pedido.Items)
//			{
//				listaProdutos += $"{item.Quantity}x {item.ProductName}\n";
//			}

//			Label lblItens = new Label
//			{
//				Text = listaProdutos,
//				Font = new Font("Segoe UI", 11),
//				ForeColor = Color.FromArgb(50, 50, 50),
//				Location = new Point(15, 90),
//				AutoSize = true,
//				MaximumSize = new Size(250, 180) // Limita largura pra não estourar
//			};

//			this.Controls.Add(lblItens);
//			this.Controls.Add(lblCliente);
//			this.Controls.Add(lblId);
//		}

//		private Color ObterCorStatus()
//		{
//			return Pedido.Status switch
//			{
//				"Pendente" => Color.Orange,
//				"Em Preparo" => Color.DodgerBlue,
//				"Pronto" => Color.LimeGreen,
//				_ => Color.Gray
//			};
//		}
//	}
//}



using System.Drawing;
using System.Windows.Forms;
using xFood.Domain.Entities;

namespace xFood.Desktop.UserControls
{
	// Note: Mantivemos "partial" para o VS não reclamar, mas faremos tudo via código
	public partial class ucTicket : UserControl
	{
		public Order Pedido { get; private set; }
		public event EventHandler BtnAvancarClick;

		// --- PALETA DE CORES AURUM ---
		private readonly Color _corDourada = Color.FromArgb(197, 160, 89);
		private readonly Color _corEscura = Color.FromArgb(33, 33, 33);
		private readonly Color _corCinzaTexto = Color.FromArgb(100, 100, 100);

		// Cores dos Status
		private readonly Color _bgPendente = Color.FromArgb(255, 248, 225);
		private readonly Color _txtPendente = Color.FromArgb(245, 127, 23);

		private readonly Color _bgPreparo = Color.FromArgb(227, 242, 253);
		private readonly Color _txtPreparo = Color.FromArgb(21, 101, 192);

		private readonly Color _bgPronto = Color.FromArgb(232, 245, 233);
		private readonly Color _txtPronto = Color.FromArgb(46, 125, 50);

		public ucTicket(Order pedido)
		{
			// InitializeComponent(); <--- NÃO USE ISSO SE O DESIGNER ESTIVER VAZIO/QUEBRADO
			// Vamos desenhar tudo na mão abaixo:

			this.DoubleBuffered = true;
			Pedido = pedido;

			MontarVisualPremium();
		}

		private void MontarVisualPremium()
		{
			// 1. Configuração do Cartão
			this.Size = new Size(320, 380);
			this.BackColor = Color.White;
			this.Margin = new Padding(15);
			this.Padding = new Padding(20);

			this.Paint += UcTicket_Paint;

			// --- CABEÇALHO ---
			Label lblNumero = new Label
			{
				Text = $"#{Pedido.OrderNumber}",
				Font = new Font("Georgia", 22, FontStyle.Bold),
				ForeColor = _corEscura,
				Location = new Point(20, 20),
				AutoSize = true
			};

			Label lblHora = new Label
			{
				Text = "🕒 " + Pedido.OrderDate.ToString("HH:mm"),
				Font = new Font("Segoe UI", 9, FontStyle.Regular),
				ForeColor = _corCinzaTexto,
				Location = new Point(22, 55),
				AutoSize = true
			};

			// --- BADGE DE STATUS ---
			Label lblStatusBadge = new Label
			{
				Text = TraduzirStatus(Pedido.Status).ToUpper(),
				Font = new Font("Segoe UI", 7, FontStyle.Bold),
				AutoSize = false,
				Size = new Size(100, 24),
				TextAlign = ContentAlignment.MiddleCenter,
				Location = new Point(190, 25)
			};
			ConfigurarCoresBadge(lblStatusBadge, Pedido.Status);


			// --- CLIENTE ---
			Label lblLabelCliente = new Label
			{
				Text = "CLIENTE",
				Font = new Font("Segoe UI", 7, FontStyle.Bold),
				ForeColor = Color.Silver,
				Location = new Point(22, 90),
				AutoSize = true
			};

			Label lblNomeCliente = new Label
			{
				Text = Pedido.CustomerName,
				Font = new Font("Georgia", 14, FontStyle.Regular),
				ForeColor = _corEscura,
				Location = new Point(20, 105),
				AutoSize = true,
				MaximumSize = new Size(280, 0)
			};

			// --- ITENS ---
			Panel pnlItens = new Panel
			{
				Location = new Point(20, 140),
				Size = new Size(280, 130),
				AutoScroll = true
			};

			int yPos = 0;
			foreach (var item in Pedido.Items)
			{
				Label lblQtd = new Label
				{
					Text = item.Quantity.ToString(),
					Font = new Font("Segoe UI", 9, FontStyle.Bold),
					ForeColor = _corEscura,
					BackColor = Color.FromArgb(240, 240, 240),
					Size = new Size(25, 25),
					TextAlign = ContentAlignment.MiddleCenter,
					Location = new Point(0, yPos)
				};

				Label lblProd = new Label
				{
					Text = item.ProductName,
					Font = new Font("Segoe UI", 10, FontStyle.Regular),
					ForeColor = Color.FromArgb(60, 60, 60),
					Location = new Point(35, yPos + 3),
					AutoSize = true,
					MaximumSize = new Size(230, 0)
				};

				pnlItens.Controls.Add(lblQtd);
				pnlItens.Controls.Add(lblProd);

				yPos += Math.Max(30, lblProd.Height + 10);
			}

			// --- RODAPÉ ---
			Label lblTotalLabel = new Label
			{
				Text = "Total",
				ForeColor = Color.Gray,
				Font = new Font("Segoe UI", 9),
				Location = new Point(20, 290),
				AutoSize = true
			};

			Label lblTotalValor = new Label
			{
				Text = Pedido.TotalAmount.ToString("C"),
				ForeColor = _corDourada,
				Font = new Font("Segoe UI", 12, FontStyle.Bold),
				AutoSize = true
			};
			// Ajuste manual de posição
			lblTotalValor.Location = new Point(180, 288);


			// BOTÃO
			Button btnAcao = new Button
			{
				Dock = DockStyle.Bottom,
				Height = 45,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9, FontStyle.Bold),
				Cursor = Cursors.Hand
			};
			btnAcao.FlatAppearance.BorderSize = 0;

			ConfigurarBotaoAcao(btnAcao, Pedido.Status);

			btnAcao.Click += (s, e) => BtnAvancarClick?.Invoke(this, EventArgs.Empty);

			this.Controls.Add(lblStatusBadge);
			this.Controls.Add(lblHora);
			this.Controls.Add(lblNumero);
			this.Controls.Add(lblLabelCliente);
			this.Controls.Add(lblNomeCliente);
			this.Controls.Add(pnlItens);
			this.Controls.Add(lblTotalLabel);
			this.Controls.Add(lblTotalValor);
			this.Controls.Add(btnAcao);
		}

		private void ConfigurarCoresBadge(Label lbl, string status)
		{
			switch (status)
			{
				case "Pendente":
					lbl.BackColor = _bgPendente;
					lbl.ForeColor = _txtPendente;
					break;
				case "Em Preparo":
					lbl.BackColor = _bgPreparo;
					lbl.ForeColor = _txtPreparo;
					break;
				case "Pronto":
					lbl.BackColor = _bgPronto;
					lbl.ForeColor = _txtPronto;
					break;
				default:
					lbl.BackColor = Color.LightGray;
					lbl.ForeColor = Color.DarkGray;
					break;
			}
		}

		private void ConfigurarBotaoAcao(Button btn, string status)
		{
			switch (status)
			{
				case "Pendente":
					btn.Text = "INICIAR PREPARO →";
					btn.BackColor = _corEscura;
					btn.ForeColor = Color.White;
					break;
				case "Em Preparo":
					btn.Text = "FINALIZAR PEDIDO →";
					btn.BackColor = _corEscura;
					btn.ForeColor = Color.White;
					break;
				case "Pronto":
					btn.Text = "ENTREGAR →";
					btn.BackColor = _corDourada;
					btn.ForeColor = Color.White;
					break;
				default:
					btn.Text = "ARQUIVADO";
					btn.Enabled = false;
					break;
			}
		}

		private string TraduzirStatus(string status)
		{
			if (status == "Pendente") return "Aguardando";
			return status;
		}

		private void UcTicket_Paint(object sender, PaintEventArgs e)
		{
			ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle,
				Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
		}
	}
}