namespace PharmacySystem
{
    partial class frmHome
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlTileSales = new System.Windows.Forms.Panel();
            this.lblSalesSub = new System.Windows.Forms.Label();
            this.lblSalesValue = new System.Windows.Forms.Label();
            this.lblSalesKey = new System.Windows.Forms.Label();
            this.pnlTileSalesBorder = new System.Windows.Forms.Panel();
            this.pnlTileAlerts = new System.Windows.Forms.Panel();
            this.lblAlertsSub = new System.Windows.Forms.Label();
            this.lblAlertsValue = new System.Windows.Forms.Label();
            this.lblAlertsKey = new System.Windows.Forms.Label();
            this.pnlTileAlertsBorder = new System.Windows.Forms.Panel();
            this.pnlTileExpiring = new System.Windows.Forms.Panel();
            this.lblExpiringSub = new System.Windows.Forms.Label();
            this.lblExpiringValue = new System.Windows.Forms.Label();
            this.lblExpiringKey = new System.Windows.Forms.Label();
            this.pnlTileExpiringBorder = new System.Windows.Forms.Panel();
            this.pnlTileStock = new System.Windows.Forms.Panel();
            this.lblStockSub = new System.Windows.Forms.Label();
            this.lblStockValue = new System.Windows.Forms.Label();
            this.lblStockKey = new System.Windows.Forms.Label();
            this.pnlTileStockBorder = new System.Windows.Forms.Panel();
            this.pnlAttention = new System.Windows.Forms.Panel();
            this.dgAttention = new System.Windows.Forms.DataGridView();
            this.lblAttentionTitle = new System.Windows.Forms.Label();
            this.pnlQuickActions = new System.Windows.Forms.Panel();
            this.btnManageProducts = new System.Windows.Forms.Button();
            this.btnNewPurchase = new System.Windows.Forms.Button();
            this.btnNewSale = new System.Windows.Forms.Button();
            this.lblQuickActionsTitle = new System.Windows.Forms.Label();
            this.pnlTileSales.SuspendLayout();
            this.pnlTileAlerts.SuspendLayout();
            this.pnlTileExpiring.SuspendLayout();
            this.pnlTileStock.SuspendLayout();
            this.pnlAttention.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgAttention)).BeginInit();
            this.pnlQuickActions.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlTileSales
            //
            this.pnlTileSales.BackColor = System.Drawing.Color.White;
            this.pnlTileSales.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTileSales.Controls.Add(this.lblSalesSub);
            this.pnlTileSales.Controls.Add(this.lblSalesValue);
            this.pnlTileSales.Controls.Add(this.lblSalesKey);
            this.pnlTileSales.Controls.Add(this.pnlTileSalesBorder);
            this.pnlTileSales.Location = new System.Drawing.Point(20, 20);
            this.pnlTileSales.Name = "pnlTileSales";
            this.pnlTileSales.Size = new System.Drawing.Size(290, 90);
            this.pnlTileSales.TabIndex = 0;
            //
            // lblSalesSub
            //
            this.lblSalesSub.AutoSize = true;
            this.lblSalesSub.ForeColor = System.Drawing.Color.Gray;
            this.lblSalesSub.Location = new System.Drawing.Point(14, 66);
            this.lblSalesSub.Name = "lblSalesSub";
            this.lblSalesSub.Size = new System.Drawing.Size(0, 15);
            this.lblSalesSub.TabIndex = 2;
            //
            // lblSalesValue
            //
            this.lblSalesValue.AutoSize = true;
            this.lblSalesValue.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblSalesValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.lblSalesValue.Location = new System.Drawing.Point(12, 34);
            this.lblSalesValue.Name = "lblSalesValue";
            this.lblSalesValue.Size = new System.Drawing.Size(28, 32);
            this.lblSalesValue.TabIndex = 1;
            this.lblSalesValue.Text = "-";
            //
            // lblSalesKey
            //
            this.lblSalesKey.AutoSize = true;
            this.lblSalesKey.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblSalesKey.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(116)))), ((int)(((byte)(138)))));
            this.lblSalesKey.Location = new System.Drawing.Point(14, 14);
            this.lblSalesKey.Name = "lblSalesKey";
            this.lblSalesKey.Size = new System.Drawing.Size(89, 13);
            this.lblSalesKey.TabIndex = 0;
            this.lblSalesKey.Text = "VENTAS DE HOY";
            //
            // pnlTileSalesBorder
            //
            this.pnlTileSalesBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(169)))), ((int)(((byte)(196)))));
            this.pnlTileSalesBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTileSalesBorder.Location = new System.Drawing.Point(0, 0);
            this.pnlTileSalesBorder.Name = "pnlTileSalesBorder";
            this.pnlTileSalesBorder.Size = new System.Drawing.Size(288, 4);
            this.pnlTileSalesBorder.TabIndex = 3;
            //
            // pnlTileAlerts
            //
            this.pnlTileAlerts.BackColor = System.Drawing.Color.White;
            this.pnlTileAlerts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTileAlerts.Controls.Add(this.lblAlertsSub);
            this.pnlTileAlerts.Controls.Add(this.lblAlertsValue);
            this.pnlTileAlerts.Controls.Add(this.lblAlertsKey);
            this.pnlTileAlerts.Controls.Add(this.pnlTileAlertsBorder);
            this.pnlTileAlerts.Location = new System.Drawing.Point(326, 20);
            this.pnlTileAlerts.Name = "pnlTileAlerts";
            this.pnlTileAlerts.Size = new System.Drawing.Size(290, 90);
            this.pnlTileAlerts.TabIndex = 1;
            //
            // lblAlertsSub
            //
            this.lblAlertsSub.AutoSize = true;
            this.lblAlertsSub.ForeColor = System.Drawing.Color.Gray;
            this.lblAlertsSub.Location = new System.Drawing.Point(14, 66);
            this.lblAlertsSub.Name = "lblAlertsSub";
            this.lblAlertsSub.Size = new System.Drawing.Size(0, 15);
            this.lblAlertsSub.TabIndex = 2;
            //
            // lblAlertsValue
            //
            this.lblAlertsValue.AutoSize = true;
            this.lblAlertsValue.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblAlertsValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(62)))), ((int)(((byte)(56)))));
            this.lblAlertsValue.Location = new System.Drawing.Point(12, 34);
            this.lblAlertsValue.Name = "lblAlertsValue";
            this.lblAlertsValue.Size = new System.Drawing.Size(28, 32);
            this.lblAlertsValue.TabIndex = 1;
            this.lblAlertsValue.Text = "-";
            //
            // lblAlertsKey
            //
            this.lblAlertsKey.AutoSize = true;
            this.lblAlertsKey.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblAlertsKey.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(116)))), ((int)(((byte)(138)))));
            this.lblAlertsKey.Location = new System.Drawing.Point(14, 14);
            this.lblAlertsKey.Name = "lblAlertsKey";
            this.lblAlertsKey.Size = new System.Drawing.Size(103, 13);
            this.lblAlertsKey.TabIndex = 0;
            this.lblAlertsKey.Text = "ALERTAS ABIERTAS";
            //
            // pnlTileAlertsBorder
            //
            this.pnlTileAlertsBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(62)))), ((int)(((byte)(56)))));
            this.pnlTileAlertsBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTileAlertsBorder.Location = new System.Drawing.Point(0, 0);
            this.pnlTileAlertsBorder.Name = "pnlTileAlertsBorder";
            this.pnlTileAlertsBorder.Size = new System.Drawing.Size(288, 4);
            this.pnlTileAlertsBorder.TabIndex = 3;
            //
            // pnlTileExpiring
            //
            this.pnlTileExpiring.BackColor = System.Drawing.Color.White;
            this.pnlTileExpiring.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTileExpiring.Controls.Add(this.lblExpiringSub);
            this.pnlTileExpiring.Controls.Add(this.lblExpiringValue);
            this.pnlTileExpiring.Controls.Add(this.lblExpiringKey);
            this.pnlTileExpiring.Controls.Add(this.pnlTileExpiringBorder);
            this.pnlTileExpiring.Location = new System.Drawing.Point(632, 20);
            this.pnlTileExpiring.Name = "pnlTileExpiring";
            this.pnlTileExpiring.Size = new System.Drawing.Size(290, 90);
            this.pnlTileExpiring.TabIndex = 2;
            //
            // lblExpiringSub
            //
            this.lblExpiringSub.AutoSize = true;
            this.lblExpiringSub.ForeColor = System.Drawing.Color.Gray;
            this.lblExpiringSub.Location = new System.Drawing.Point(14, 66);
            this.lblExpiringSub.Name = "lblExpiringSub";
            this.lblExpiringSub.Size = new System.Drawing.Size(129, 15);
            this.lblExpiringSub.TabIndex = 2;
            this.lblExpiringSub.Text = "productos con fecha próxima";
            //
            // lblExpiringValue
            //
            this.lblExpiringValue.AutoSize = true;
            this.lblExpiringValue.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblExpiringValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(106)))), ((int)(((byte)(34)))));
            this.lblExpiringValue.Location = new System.Drawing.Point(12, 34);
            this.lblExpiringValue.Name = "lblExpiringValue";
            this.lblExpiringValue.Size = new System.Drawing.Size(28, 32);
            this.lblExpiringValue.TabIndex = 1;
            this.lblExpiringValue.Text = "-";
            //
            // lblExpiringKey
            //
            this.lblExpiringKey.AutoSize = true;
            this.lblExpiringKey.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblExpiringKey.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(116)))), ((int)(((byte)(138)))));
            this.lblExpiringKey.Location = new System.Drawing.Point(14, 14);
            this.lblExpiringKey.Name = "lblExpiringKey";
            this.lblExpiringKey.Size = new System.Drawing.Size(65, 13);
            this.lblExpiringKey.TabIndex = 0;
            this.lblExpiringKey.Text = "POR VENCER";
            //
            // pnlTileExpiringBorder
            //
            this.pnlTileExpiringBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(106)))), ((int)(((byte)(34)))));
            this.pnlTileExpiringBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTileExpiringBorder.Location = new System.Drawing.Point(0, 0);
            this.pnlTileExpiringBorder.Name = "pnlTileExpiringBorder";
            this.pnlTileExpiringBorder.Size = new System.Drawing.Size(288, 4);
            this.pnlTileExpiringBorder.TabIndex = 3;
            //
            // pnlTileStock
            //
            this.pnlTileStock.BackColor = System.Drawing.Color.White;
            this.pnlTileStock.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTileStock.Controls.Add(this.lblStockSub);
            this.pnlTileStock.Controls.Add(this.lblStockValue);
            this.pnlTileStock.Controls.Add(this.lblStockKey);
            this.pnlTileStock.Controls.Add(this.pnlTileStockBorder);
            this.pnlTileStock.Location = new System.Drawing.Point(938, 20);
            this.pnlTileStock.Name = "pnlTileStock";
            this.pnlTileStock.Size = new System.Drawing.Size(290, 90);
            this.pnlTileStock.TabIndex = 3;
            //
            // lblStockSub
            //
            this.lblStockSub.AutoSize = true;
            this.lblStockSub.ForeColor = System.Drawing.Color.Gray;
            this.lblStockSub.Location = new System.Drawing.Point(14, 66);
            this.lblStockSub.Name = "lblStockSub";
            this.lblStockSub.Size = new System.Drawing.Size(107, 15);
            this.lblStockSub.TabIndex = 2;
            this.lblStockSub.Text = "por debajo del umbral";
            //
            // lblStockValue
            //
            this.lblStockValue.AutoSize = true;
            this.lblStockValue.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblStockValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(62)))), ((int)(((byte)(56)))));
            this.lblStockValue.Location = new System.Drawing.Point(12, 34);
            this.lblStockValue.Name = "lblStockValue";
            this.lblStockValue.Size = new System.Drawing.Size(28, 32);
            this.lblStockValue.TabIndex = 1;
            this.lblStockValue.Text = "-";
            //
            // lblStockKey
            //
            this.lblStockKey.AutoSize = true;
            this.lblStockKey.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblStockKey.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(116)))), ((int)(((byte)(138)))));
            this.lblStockKey.Location = new System.Drawing.Point(14, 14);
            this.lblStockKey.Name = "lblStockKey";
            this.lblStockKey.Size = new System.Drawing.Size(76, 13);
            this.lblStockKey.TabIndex = 0;
            this.lblStockKey.Text = "STOCK CRÍTICO";
            //
            // pnlTileStockBorder
            //
            this.pnlTileStockBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(62)))), ((int)(((byte)(56)))));
            this.pnlTileStockBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTileStockBorder.Location = new System.Drawing.Point(0, 0);
            this.pnlTileStockBorder.Name = "pnlTileStockBorder";
            this.pnlTileStockBorder.Size = new System.Drawing.Size(288, 4);
            this.pnlTileStockBorder.TabIndex = 3;
            //
            // pnlAttention
            //
            this.pnlAttention.BackColor = System.Drawing.Color.White;
            this.pnlAttention.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAttention.Controls.Add(this.dgAttention);
            this.pnlAttention.Controls.Add(this.lblAttentionTitle);
            this.pnlAttention.Location = new System.Drawing.Point(20, 130);
            this.pnlAttention.Name = "pnlAttention";
            this.pnlAttention.Size = new System.Drawing.Size(780, 418);
            this.pnlAttention.TabIndex = 4;
            //
            // dgAttention
            //
            this.dgAttention.AllowUserToAddRows = false;
            this.dgAttention.AllowUserToResizeColumns = false;
            this.dgAttention.AllowUserToResizeRows = false;
            this.dgAttention.BackgroundColor = System.Drawing.Color.White;
            this.dgAttention.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(116)))), ((int)(((byte)(138)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(116)))), ((int)(((byte)(138)))));
            this.dgAttention.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgAttention.ColumnHeadersHeight = 28;
            this.dgAttention.EnableHeadersVisualStyles = false;
            this.dgAttention.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(245)))));
            this.dgAttention.Location = new System.Drawing.Point(1, 41);
            this.dgAttention.MultiSelect = false;
            this.dgAttention.Name = "dgAttention";
            this.dgAttention.ReadOnly = true;
            this.dgAttention.RowHeadersVisible = false;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(249)))), ((int)(((byte)(252)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            this.dgAttention.RowTemplate.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgAttention.RowTemplate.Height = 32;
            this.dgAttention.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgAttention.Size = new System.Drawing.Size(776, 374);
            this.dgAttention.TabIndex = 1;
            this.dgAttention.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgAttention_CellContentClick);
            this.dgAttention.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgAttention_CellMouseEnter);
            //
            // lblAttentionTitle
            //
            this.lblAttentionTitle.AutoSize = true;
            this.lblAttentionTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblAttentionTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.lblAttentionTitle.Location = new System.Drawing.Point(14, 12);
            this.lblAttentionTitle.Name = "lblAttentionTitle";
            this.lblAttentionTitle.Size = new System.Drawing.Size(115, 19);
            this.lblAttentionTitle.TabIndex = 0;
            this.lblAttentionTitle.Text = "Requiere atención";
            //
            // pnlQuickActions
            //
            this.pnlQuickActions.BackColor = System.Drawing.Color.White;
            this.pnlQuickActions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlQuickActions.Controls.Add(this.btnManageProducts);
            this.pnlQuickActions.Controls.Add(this.btnNewPurchase);
            this.pnlQuickActions.Controls.Add(this.btnNewSale);
            this.pnlQuickActions.Controls.Add(this.lblQuickActionsTitle);
            this.pnlQuickActions.Location = new System.Drawing.Point(808, 130);
            this.pnlQuickActions.Name = "pnlQuickActions";
            this.pnlQuickActions.Size = new System.Drawing.Size(408, 418);
            this.pnlQuickActions.TabIndex = 5;
            //
            // btnManageProducts
            //
            this.btnManageProducts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(249)))), ((int)(((byte)(252)))));
            this.btnManageProducts.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManageProducts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageProducts.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnManageProducts.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnManageProducts.Location = new System.Drawing.Point(14, 128);
            this.btnManageProducts.Name = "btnManageProducts";
            this.btnManageProducts.Size = new System.Drawing.Size(380, 46);
            this.btnManageProducts.TabIndex = 3;
            this.btnManageProducts.Text = "Alta de producto";
            this.btnManageProducts.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnManageProducts.UseVisualStyleBackColor = false;
            this.btnManageProducts.Click += new System.EventHandler(this.btnManageProducts_Click);
            //
            // btnNewPurchase
            //
            this.btnNewPurchase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(249)))), ((int)(((byte)(252)))));
            this.btnNewPurchase.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewPurchase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewPurchase.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNewPurchase.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnNewPurchase.Location = new System.Drawing.Point(14, 70);
            this.btnNewPurchase.Name = "btnNewPurchase";
            this.btnNewPurchase.Size = new System.Drawing.Size(380, 46);
            this.btnNewPurchase.TabIndex = 2;
            this.btnNewPurchase.Text = "Registrar compra";
            this.btnNewPurchase.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNewPurchase.UseVisualStyleBackColor = false;
            this.btnNewPurchase.Click += new System.EventHandler(this.btnNewPurchase_Click);
            //
            // btnNewSale
            //
            this.btnNewSale.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(249)))), ((int)(((byte)(252)))));
            this.btnNewSale.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewSale.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewSale.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNewSale.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnNewSale.Location = new System.Drawing.Point(14, 12);
            this.btnNewSale.Name = "btnNewSale";
            this.btnNewSale.Size = new System.Drawing.Size(380, 46);
            this.btnNewSale.TabIndex = 1;
            this.btnNewSale.Text = "Nueva venta";
            this.btnNewSale.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNewSale.UseVisualStyleBackColor = false;
            this.btnNewSale.Click += new System.EventHandler(this.btnNewSale_Click);
            // Header row lives directly on the panel, above the three action buttons, matching the
            // "Requiere atención" panel's title placement.
            //
            // lblQuickActionsTitle
            //
            this.lblQuickActionsTitle.AutoSize = true;
            this.lblQuickActionsTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblQuickActionsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.lblQuickActionsTitle.Location = new System.Drawing.Point(14, 12);
            this.lblQuickActionsTitle.Name = "lblQuickActionsTitle";
            this.lblQuickActionsTitle.Size = new System.Drawing.Size(113, 19);
            this.lblQuickActionsTitle.TabIndex = 0;
            this.lblQuickActionsTitle.Text = "Accesos rápidos";
            //
            // frmHome
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(1269, 568);
            this.ControlBox = false;
            this.Controls.Add(this.pnlQuickActions);
            this.Controls.Add(this.pnlAttention);
            this.Controls.Add(this.pnlTileStock);
            this.Controls.Add(this.pnlTileExpiring);
            this.Controls.Add(this.pnlTileAlerts);
            this.Controls.Add(this.pnlTileSales);
            this.Name = "frmHome";
            this.Text = "Inicio";
            this.Load += new System.EventHandler(this.frmHome_Load);
            this.pnlTileSales.ResumeLayout(false);
            this.pnlTileSales.PerformLayout();
            this.pnlTileAlerts.ResumeLayout(false);
            this.pnlTileAlerts.PerformLayout();
            this.pnlTileExpiring.ResumeLayout(false);
            this.pnlTileExpiring.PerformLayout();
            this.pnlTileStock.ResumeLayout(false);
            this.pnlTileStock.PerformLayout();
            this.pnlAttention.ResumeLayout(false);
            this.pnlAttention.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgAttention)).EndInit();
            this.pnlQuickActions.ResumeLayout(false);
            this.pnlQuickActions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTileSales;
        private System.Windows.Forms.Label lblSalesSub;
        private System.Windows.Forms.Label lblSalesValue;
        private System.Windows.Forms.Label lblSalesKey;
        private System.Windows.Forms.Panel pnlTileSalesBorder;
        private System.Windows.Forms.Panel pnlTileAlerts;
        private System.Windows.Forms.Label lblAlertsSub;
        private System.Windows.Forms.Label lblAlertsValue;
        private System.Windows.Forms.Label lblAlertsKey;
        private System.Windows.Forms.Panel pnlTileAlertsBorder;
        private System.Windows.Forms.Panel pnlTileExpiring;
        private System.Windows.Forms.Label lblExpiringSub;
        private System.Windows.Forms.Label lblExpiringValue;
        private System.Windows.Forms.Label lblExpiringKey;
        private System.Windows.Forms.Panel pnlTileExpiringBorder;
        private System.Windows.Forms.Panel pnlTileStock;
        private System.Windows.Forms.Label lblStockSub;
        private System.Windows.Forms.Label lblStockValue;
        private System.Windows.Forms.Label lblStockKey;
        private System.Windows.Forms.Panel pnlTileStockBorder;
        private System.Windows.Forms.Panel pnlAttention;
        private System.Windows.Forms.DataGridView dgAttention;
        private System.Windows.Forms.Label lblAttentionTitle;
        private System.Windows.Forms.Panel pnlQuickActions;
        private System.Windows.Forms.Button btnManageProducts;
        private System.Windows.Forms.Button btnNewPurchase;
        private System.Windows.Forms.Button btnNewSale;
        private System.Windows.Forms.Label lblQuickActionsTitle;
    }
}
