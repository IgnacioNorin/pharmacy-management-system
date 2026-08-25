namespace PharmacySystem
{
    partial class MainForm
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.msMenu = new System.Windows.Forms.MenuStrip();
            this.clientsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.managementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.salesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.purchasesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.suppliersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.usersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reportsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notificationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lbluser = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblnotifystock = new System.Windows.Forms.Label();
            this.lblnotifyexpireddate = new System.Windows.Forms.Label();
            this.timerNotification = new System.Windows.Forms.Timer(this.components);
            this.pictureBoxStock = new System.Windows.Forms.PictureBox();
            this.pictureBoxExpiredDate = new System.Windows.Forms.PictureBox();
            this.msMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxExpiredDate)).BeginInit();
            this.SuspendLayout();
            // 
            // msMenu
            // 
            this.msMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.msMenu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.msMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clientsToolStripMenuItem,
            this.managementToolStripMenuItem,
            this.salesToolStripMenuItem,
            this.purchasesToolStripMenuItem,
            this.suppliersToolStripMenuItem,
            this.usersToolStripMenuItem,
            this.reportsToolStripMenuItem,
            this.notificationsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.msMenu.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.msMenu.Location = new System.Drawing.Point(0, 0);
            this.msMenu.Name = "msMenu";
            this.msMenu.Size = new System.Drawing.Size(1484, 91);
            this.msMenu.TabIndex = 1;
            this.msMenu.Text = "menuStrip1";
            // 
            // clientsToolStripMenuItem
            // 
            this.clientsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.clientsToolStripMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.clientsToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.clientsToolStripMenuItem.Image = global::PharmacySystem.Properties.Resources.ClientSectionIcon;
            this.clientsToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.clientsToolStripMenuItem.Name = "clientsToolStripMenuItem";
            this.clientsToolStripMenuItem.Size = new System.Drawing.Size(76, 87);
            this.clientsToolStripMenuItem.Text = "Clientes";
            this.clientsToolStripMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.clientsToolStripMenuItem.Click += new System.EventHandler(this.clientsToolStripMenuItem_Click);
            // 
            // managementToolStripMenuItem
            // 
            this.managementToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.managementToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.managementToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("managementToolStripMenuItem.Image")));
            this.managementToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.managementToolStripMenuItem.Name = "managementToolStripMenuItem";
            this.managementToolStripMenuItem.Size = new System.Drawing.Size(76, 87);
            this.managementToolStripMenuItem.Text = "Gestión";
            this.managementToolStripMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.managementToolStripMenuItem.Click += new System.EventHandler(this.managementToolStripMenuItem_Click);
            // 
            // salesToolStripMenuItem
            // 
            this.salesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.salesToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.salesToolStripMenuItem.Image = global::PharmacySystem.Properties.Resources.SaleSectionIcon;
            this.salesToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.salesToolStripMenuItem.Name = "salesToolStripMenuItem";
            this.salesToolStripMenuItem.Size = new System.Drawing.Size(76, 87);
            this.salesToolStripMenuItem.Text = "Ventas";
            this.salesToolStripMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.salesToolStripMenuItem.Click += new System.EventHandler(this.salesToolStripMenuItem_Click);
            // 
            // purchasesToolStripMenuItem
            // 
            this.purchasesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.purchasesToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.purchasesToolStripMenuItem.Image = global::PharmacySystem.Properties.Resources.PurchaseSectionIcon;
            this.purchasesToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.purchasesToolStripMenuItem.Name = "purchasesToolStripMenuItem";
            this.purchasesToolStripMenuItem.Size = new System.Drawing.Size(81, 87);
            this.purchasesToolStripMenuItem.Text = "Compras";
            this.purchasesToolStripMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.purchasesToolStripMenuItem.Click += new System.EventHandler(this.purchasesToolStripMenuItem_Click);
            // 
            // suppliersToolStripMenuItem
            // 
            this.suppliersToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.suppliersToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.suppliersToolStripMenuItem.Image = global::PharmacySystem.Properties.Resources.proveedoricon;
            this.suppliersToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.suppliersToolStripMenuItem.Name = "suppliersToolStripMenuItem";
            this.suppliersToolStripMenuItem.Size = new System.Drawing.Size(107, 87);
            this.suppliersToolStripMenuItem.Text = "Proveedores";
            this.suppliersToolStripMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.suppliersToolStripMenuItem.Click += new System.EventHandler(this.suppliersToolStripMenuItem_Click);
            // 
            // usersToolStripMenuItem
            // 
            this.usersToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.usersToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.usersToolStripMenuItem.Image = global::PharmacySystem.Properties.Resources.usuarioicon;
            this.usersToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.usersToolStripMenuItem.Name = "usersToolStripMenuItem";
            this.usersToolStripMenuItem.Size = new System.Drawing.Size(78, 87);
            this.usersToolStripMenuItem.Text = "Usuarios";
            this.usersToolStripMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.usersToolStripMenuItem.Click += new System.EventHandler(this.usersToolStripMenuItem_Click);
            // 
            // reportsToolStripMenuItem
            // 
            this.reportsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.reportsToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.reportsToolStripMenuItem.Image = global::PharmacySystem.Properties.Resources.reporteicon;
            this.reportsToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.reportsToolStripMenuItem.Name = "reportsToolStripMenuItem";
            this.reportsToolStripMenuItem.Size = new System.Drawing.Size(93, 87);
            this.reportsToolStripMenuItem.Text = "Reportería";
            this.reportsToolStripMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.reportsToolStripMenuItem.Click += new System.EventHandler(this.reportsToolStripMenuItem_Click);
            // 
            // notificationsToolStripMenuItem
            // 
            this.notificationsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.notificationsToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.notificationsToolStripMenuItem.Image = global::PharmacySystem.Properties.Resources.notificacionconfiguracionicon;
            this.notificationsToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.notificationsToolStripMenuItem.Name = "notificationsToolStripMenuItem";
            this.notificationsToolStripMenuItem.Size = new System.Drawing.Size(116, 87);
            this.notificationsToolStripMenuItem.Text = "Notificaciones";
            this.notificationsToolStripMenuItem.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.notificationsToolStripMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.notificationsToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem1_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.exitToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.exitToolStripMenuItem.Image = global::PharmacySystem.Properties.Resources.saliricon;
            this.exitToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(76, 87);
            this.exitToolStripMenuItem.Text = "Salir";
            this.exitToolStripMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // lbluser
            // 
            this.lbluser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbluser.AutoSize = true;
            this.lbluser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.lbluser.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbluser.ForeColor = System.Drawing.Color.White;
            this.lbluser.Location = new System.Drawing.Point(1294, 49);
            this.lbluser.Name = "lbluser";
            this.lbluser.Size = new System.Drawing.Size(61, 17);
            this.lbluser.TabIndex = 3;
            this.lbluser.Text = "Usuario:";
            this.lbluser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(1292, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(119, 25);
            this.label5.TabIndex = 3;
            this.label5.Text = "Bienvenido";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.BackColor = System.Drawing.Color.White;
            this.label10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label10.Location = new System.Drawing.Point(810, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(457, 75);
            this.label10.TabIndex = 49;
            // 
            // lblnotifystock
            //
            this.lblnotifystock.AutoSize = true;
            this.lblnotifystock.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblnotifystock.Location = new System.Drawing.Point(830, 20);
            this.lblnotifystock.Name = "lblnotifystock";
            this.lblnotifystock.Size = new System.Drawing.Size(0, 13);
            this.lblnotifystock.TabIndex = 51;
            this.lblnotifystock.Click += new System.EventHandler(this.OpenAlertsCenter);
            //
            // lblnotifyexpireddate
            //
            this.lblnotifyexpireddate.AutoSize = true;
            this.lblnotifyexpireddate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblnotifyexpireddate.Location = new System.Drawing.Point(833, 51);
            this.lblnotifyexpireddate.Name = "lblnotifyexpireddate";
            this.lblnotifyexpireddate.Size = new System.Drawing.Size(0, 13);
            this.lblnotifyexpireddate.TabIndex = 52;
            this.lblnotifyexpireddate.Click += new System.EventHandler(this.OpenAlertsCenter);
            //
            // timerNotification
            //
            this.timerNotification.Tick += new System.EventHandler(this.timerNotification_Tick);
            //
            // pictureBoxStock
            //
            this.pictureBoxStock.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxStock.Image = global::PharmacySystem.Properties.Resources.advertenciaicon;
            this.pictureBoxStock.Location = new System.Drawing.Point(817, 20);
            this.pictureBoxStock.Name = "pictureBoxStock";
            this.pictureBoxStock.Size = new System.Drawing.Size(16, 16);
            this.pictureBoxStock.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxStock.TabIndex = 54;
            this.pictureBoxStock.TabStop = false;
            this.pictureBoxStock.Click += new System.EventHandler(this.OpenAlertsCenter);
            //
            // pictureBoxExpiredDate
            //
            this.pictureBoxExpiredDate.BackColor = System.Drawing.Color.White;
            this.pictureBoxExpiredDate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxExpiredDate.Image = global::PharmacySystem.Properties.Resources.advertenciaicon;
            this.pictureBoxExpiredDate.Location = new System.Drawing.Point(817, 49);
            this.pictureBoxExpiredDate.Name = "pictureBoxExpiredDate";
            this.pictureBoxExpiredDate.Size = new System.Drawing.Size(16, 16);
            this.pictureBoxExpiredDate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxExpiredDate.TabIndex = 56;
            this.pictureBoxExpiredDate.TabStop = false;
            this.pictureBoxExpiredDate.Click += new System.EventHandler(this.OpenAlertsCenter);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(1484, 861);
            this.Controls.Add(this.pictureBoxExpiredDate);
            this.Controls.Add(this.pictureBoxStock);
            this.Controls.Add(this.lblnotifyexpireddate);
            this.Controls.Add(this.lblnotifystock);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.lbluser);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.msMenu);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.msMenu;
            this.MinimumSize = new System.Drawing.Size(1300, 700);
            this.Name = "MainForm";
            this.RightToLeftLayout = true;
            this.Text = "Pharmacy System";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.msMenu.ResumeLayout(false);
            this.msMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxExpiredDate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip msMenu;
        private System.Windows.Forms.ToolStripMenuItem clientsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem managementToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem salesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem purchasesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem suppliersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reportsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem usersToolStripMenuItem;
        private System.Windows.Forms.Label lbluser;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripMenuItem notificationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lblnotifystock;
        private System.Windows.Forms.Label lblnotifyexpireddate;
        private System.Windows.Forms.Timer timerNotification;
        private System.Windows.Forms.PictureBox pictureBoxStock;
        private System.Windows.Forms.PictureBox pictureBoxExpiredDate;
    }
}

