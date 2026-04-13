namespace xFood.Desktop.Forms
{
    partial class FrmCozinha
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            flowPedidos = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // flowPedidos
            // 
            flowPedidos.AutoScroll = true;
            flowPedidos.BackColor = Color.FromArgb(30, 30, 30);
            flowPedidos.Dock = DockStyle.Fill;
            flowPedidos.Location = new Point(0, 0);
            flowPedidos.Margin = new Padding(3, 2, 3, 2);
            flowPedidos.Name = "flowPedidos";
            flowPedidos.Size = new Size(931, 557);
            flowPedidos.TabIndex = 0;
            flowPedidos.Paint += flowPedidos_Paint;
            // 
            // FrmCozinha
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(20, 20, 20);
            ClientSize = new Size(931, 557);
            Controls.Add(flowPedidos);
            ForeColor = Color.Coral;
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Margin = new Padding(3, 2, 3, 2);
            Name = "FrmCozinha";
            RightToLeftLayout = true;
            Text = "FrmCozinha";
            WindowState = FormWindowState.Minimized;
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowPedidos;
    }
}