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
            this.timerNotification = new System.Windows.Forms.Timer(this.components);
            this.pnlTitleBar = new System.Windows.Forms.Panel();
            this.lblTitleBarText = new System.Windows.Forms.Label();
            this.btnCloseWin = new System.Windows.Forms.Button();
            this.btnMaximizeRestore = new System.Windows.Forms.Button();
            this.btnMinimizeWin = new System.Windows.Forms.Button();
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.pnlSidebarItems = new System.Windows.Forms.Panel();
            this.btnHome = new System.Windows.Forms.Button();
            this.lblGroupOperacion = new System.Windows.Forms.Label();
            this.btnSales = new System.Windows.Forms.Button();
            this.btnPurchases = new System.Windows.Forms.Button();
            this.lblGroupGestion = new System.Windows.Forms.Label();
            this.btnManagement = new System.Windows.Forms.Button();
            this.btnSuppliers = new System.Windows.Forms.Button();
            this.btnClients = new System.Windows.Forms.Button();
            this.btnUsers = new System.Windows.Forms.Button();
            this.btnRoles = new System.Windows.Forms.Button();
            this.lblGroupConsulta = new System.Windows.Forms.Label();
            this.btnReports = new System.Windows.Forms.Button();
            this.btnCashCount = new System.Windows.Forms.Button();
            this.btnAuditLog = new System.Windows.Forms.Button();
            this.btnAlerts = new System.Windows.Forms.Button();
            this.lblAlertBadge = new System.Windows.Forms.Label();
            this.pnlSidebarHeader = new System.Windows.Forms.Panel();
            this.lbluser = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblUserRole = new System.Windows.Forms.Label();
            this.pnlSidebarHeaderDivider = new System.Windows.Forms.Panel();
            this.pnlSidebarBottom = new System.Windows.Forms.Panel();
            this.btnExit = new System.Windows.Forms.Button();
            this.pnlSidebarBottomDivider = new System.Windows.Forms.Panel();
            this.pnlTitleBar.SuspendLayout();
            this.pnlSidebar.SuspendLayout();
            this.pnlSidebarItems.SuspendLayout();
            this.pnlSidebarHeader.SuspendLayout();
            this.pnlSidebarBottom.SuspendLayout();
            this.SuspendLayout();
            //
            // timerNotification
            //
            this.timerNotification.Tick += new System.EventHandler(this.timerNotification_Tick);
            //
            // pnlTitleBar
            //
            // Custom title bar (Fase 8): the Form has FormBorderStyle.None, so Windows draws none
            // of its own caption/buttons. Dragging is NOT done via WM_NCHITTEST on the Form - this
            // panel and the label are themselves real child windows, so Windows delivers mouse
            // messages to THEM first and MainForm's WndProc never sees WM_NCHITTEST for this area.
            // Instead, MouseDown here calls ReleaseCapture + SendMessage(WM_NCLBUTTONDOWN,
            // HTCAPTION) - the standard trick that hands the drag off to the OS exactly like a
            // native caption (Aero Snap included). WM_GETMINMAXINFO in MainForm.cs is unrelated to
            // that problem (it's a top-level-only message) and still works fine as an override.
            this.pnlTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.pnlTitleBar.Controls.Add(this.lblTitleBarText);
            this.pnlTitleBar.Controls.Add(this.btnCloseWin);
            this.pnlTitleBar.Controls.Add(this.btnMaximizeRestore);
            this.pnlTitleBar.Controls.Add(this.btnMinimizeWin);
            this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Location = new System.Drawing.Point(0, 0);
            this.pnlTitleBar.Name = "pnlTitleBar";
            this.pnlTitleBar.Size = new System.Drawing.Size(1500, 34);
            this.pnlTitleBar.TabIndex = 0;
            this.pnlTitleBar.DoubleClick += new System.EventHandler(this.pnlTitleBar_DoubleClick);
            this.pnlTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseDown);
            //
            // lblTitleBarText
            //
            // Anchor, not Dock=Fill: Fill claimed the whole panel width - including the 138px the
            // three buttons sit in - and being the first control added put it in front of them in
            // paint order, hiding them completely. Anchoring Left+Right instead stretches the
            // label with the window while keeping the same 138px gap on the right that its design
            // width already leaves (1500 - 1362), which is exactly the buttons' combined width.
            this.lblTitleBarText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitleBarText.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblTitleBarText.ForeColor = System.Drawing.Color.White;
            this.lblTitleBarText.Location = new System.Drawing.Point(0, 0);
            this.lblTitleBarText.Name = "lblTitleBarText";
            this.lblTitleBarText.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblTitleBarText.Size = new System.Drawing.Size(1362, 34);
            this.lblTitleBarText.TabIndex = 0;
            this.lblTitleBarText.Text = "Pharmacy System";
            this.lblTitleBarText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitleBarText.DoubleClick += new System.EventHandler(this.pnlTitleBar_DoubleClick);
            this.lblTitleBarText.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseDown);
            //
            // btnMinimizeWin
            //
            // Anchored, not Dock=Right: three same-edge Dock siblings order themselves by add
            // order in a way that's easy to get backwards, so this pins each button an explicit,
            // unambiguous distance from the right edge instead - Minimize, Maximize, Close from
            // left to right, same as every native Windows title bar.
            this.btnMinimizeWin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimizeWin.FlatAppearance.BorderSize = 0;
            this.btnMinimizeWin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(82)))));
            this.btnMinimizeWin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimizeWin.Font = new System.Drawing.Font("Segoe MDL2 Assets", 9.5F);
            this.btnMinimizeWin.ForeColor = System.Drawing.Color.White;
            this.btnMinimizeWin.Location = new System.Drawing.Point(1362, 0);
            this.btnMinimizeWin.Name = "btnMinimizeWin";
            this.btnMinimizeWin.Size = new System.Drawing.Size(46, 34);
            this.btnMinimizeWin.TabIndex = 1;
            this.btnMinimizeWin.Text = "";
            this.btnMinimizeWin.UseVisualStyleBackColor = false;
            this.btnMinimizeWin.Click += new System.EventHandler(this.btnMinimizeWin_Click);
            //
            // btnMaximizeRestore
            //
            this.btnMaximizeRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMaximizeRestore.FlatAppearance.BorderSize = 0;
            this.btnMaximizeRestore.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(82)))));
            this.btnMaximizeRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaximizeRestore.Font = new System.Drawing.Font("Segoe MDL2 Assets", 9.5F);
            this.btnMaximizeRestore.ForeColor = System.Drawing.Color.White;
            this.btnMaximizeRestore.Location = new System.Drawing.Point(1408, 0);
            this.btnMaximizeRestore.Name = "btnMaximizeRestore";
            this.btnMaximizeRestore.Size = new System.Drawing.Size(46, 34);
            this.btnMaximizeRestore.TabIndex = 2;
            this.btnMaximizeRestore.Text = "";
            this.btnMaximizeRestore.UseVisualStyleBackColor = false;
            this.btnMaximizeRestore.Click += new System.EventHandler(this.btnMaximizeRestore_Click);
            //
            // btnCloseWin
            //
            this.btnCloseWin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCloseWin.FlatAppearance.BorderSize = 0;
            this.btnCloseWin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(43)))), ((int)(((byte)(48)))));
            this.btnCloseWin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCloseWin.Font = new System.Drawing.Font("Segoe MDL2 Assets", 9.5F);
            this.btnCloseWin.ForeColor = System.Drawing.Color.White;
            this.btnCloseWin.Location = new System.Drawing.Point(1454, 0);
            this.btnCloseWin.Name = "btnCloseWin";
            this.btnCloseWin.Size = new System.Drawing.Size(46, 34);
            this.btnCloseWin.TabIndex = 3;
            this.btnCloseWin.Text = "";
            this.btnCloseWin.UseVisualStyleBackColor = false;
            this.btnCloseWin.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            //
            // pnlSidebar
            //
            // Replaces the old horizontal MenuStrip (Fase 7: navigation rework). A Panel instead
            // of a MenuStrip because the nav rows need to be grouped under section headers, host a
            // badge child on the Alertas button, and sit above a pinned bottom block (Salir +
            // usuario) - none of which the ToolStrip flow-layout model does cleanly.
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.pnlSidebar.Controls.Add(this.pnlSidebarItems);
            this.pnlSidebar.Controls.Add(this.pnlSidebarBottom);
            this.pnlSidebar.Controls.Add(this.pnlSidebarHeader);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(210, 861);
            this.pnlSidebar.TabIndex = 0;
            //
            // pnlSidebarItems
            //
            // Fill-docked BEFORE pnlSidebarBottom is added below with Dock=Bottom, so the bottom
            // block claims its fixed strip and this one takes whatever remains above it. Its
            // children are positioned by MainForm.LayoutSidebarItems() at runtime, skipping
            // whichever are hidden for the current user - not a fixed Designer layout, since which
            // items show depends on ApplySidebarPermissions.
            this.pnlSidebarItems.Controls.Add(this.btnAlerts);
            this.pnlSidebarItems.Controls.Add(this.btnCashCount);
            this.pnlSidebarItems.Controls.Add(this.btnAuditLog);
            this.pnlSidebarItems.Controls.Add(this.btnReports);
            this.pnlSidebarItems.Controls.Add(this.lblGroupConsulta);
            this.pnlSidebarItems.Controls.Add(this.btnRoles);
            this.pnlSidebarItems.Controls.Add(this.btnUsers);
            this.pnlSidebarItems.Controls.Add(this.btnClients);
            this.pnlSidebarItems.Controls.Add(this.btnSuppliers);
            this.pnlSidebarItems.Controls.Add(this.btnManagement);
            this.pnlSidebarItems.Controls.Add(this.lblGroupGestion);
            this.pnlSidebarItems.Controls.Add(this.btnPurchases);
            this.pnlSidebarItems.Controls.Add(this.btnSales);
            this.pnlSidebarItems.Controls.Add(this.lblGroupOperacion);
            this.pnlSidebarItems.Controls.Add(this.btnHome);
            this.pnlSidebarItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSidebarItems.Location = new System.Drawing.Point(0, 92);
            this.pnlSidebarItems.Name = "pnlSidebarItems";
            this.pnlSidebarItems.Size = new System.Drawing.Size(210, 699);
            this.pnlSidebarItems.TabIndex = 1;
            //
            // btnHome
            //
            this.btnHome.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnHome.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHome.FlatAppearance.BorderSize = 0;
            this.btnHome.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHome.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnHome.ForeColor = System.Drawing.Color.White;
            this.btnHome.Image = new System.Drawing.Bitmap(global::PharmacySystem.Properties.Resources.homeicon, new System.Drawing.Size(20, 20));
            this.btnHome.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnHome.Location = new System.Drawing.Point(10, 14);
            this.btnHome.Name = "btnHome";
            this.btnHome.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnHome.Size = new System.Drawing.Size(190, 40);
            this.btnHome.TabIndex = 0;
            this.btnHome.Text = "        Inicio";
            this.btnHome.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnHome.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnHome.UseVisualStyleBackColor = false;
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            //
            // lblGroupOperacion
            //
            this.lblGroupOperacion.AutoSize = true;
            this.lblGroupOperacion.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblGroupOperacion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(169)))), ((int)(((byte)(196)))));
            this.lblGroupOperacion.Location = new System.Drawing.Point(14, 68);
            this.lblGroupOperacion.Name = "lblGroupOperacion";
            this.lblGroupOperacion.Size = new System.Drawing.Size(76, 13);
            this.lblGroupOperacion.TabIndex = 1;
            this.lblGroupOperacion.Text = "OPERACIÓN";
            //
            // btnSales
            //
            this.btnSales.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnSales.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSales.FlatAppearance.BorderSize = 0;
            this.btnSales.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSales.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSales.ForeColor = System.Drawing.Color.White;
            this.btnSales.Image = new System.Drawing.Bitmap(global::PharmacySystem.Properties.Resources.SaleSectionIcon, new System.Drawing.Size(20, 20));
            this.btnSales.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSales.Location = new System.Drawing.Point(10, 96);
            this.btnSales.Name = "btnSales";
            this.btnSales.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnSales.Size = new System.Drawing.Size(190, 40);
            this.btnSales.TabIndex = 2;
            this.btnSales.Text = "        Ventas";
            this.btnSales.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSales.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSales.UseVisualStyleBackColor = false;
            this.btnSales.Click += new System.EventHandler(this.btnSales_Click);
            //
            // btnPurchases
            //
            this.btnPurchases.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnPurchases.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPurchases.FlatAppearance.BorderSize = 0;
            this.btnPurchases.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPurchases.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnPurchases.ForeColor = System.Drawing.Color.White;
            this.btnPurchases.Image = new System.Drawing.Bitmap(global::PharmacySystem.Properties.Resources.PurchaseSectionIcon, new System.Drawing.Size(20, 20));
            this.btnPurchases.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPurchases.Location = new System.Drawing.Point(10, 142);
            this.btnPurchases.Name = "btnPurchases";
            this.btnPurchases.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnPurchases.Size = new System.Drawing.Size(190, 40);
            this.btnPurchases.TabIndex = 3;
            this.btnPurchases.Text = "        Compras";
            this.btnPurchases.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPurchases.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPurchases.UseVisualStyleBackColor = false;
            this.btnPurchases.Click += new System.EventHandler(this.btnPurchases_Click);
            //
            // lblGroupGestion
            //
            this.lblGroupGestion.AutoSize = true;
            this.lblGroupGestion.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblGroupGestion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(169)))), ((int)(((byte)(196)))));
            this.lblGroupGestion.Location = new System.Drawing.Point(14, 196);
            this.lblGroupGestion.Name = "lblGroupGestion";
            this.lblGroupGestion.Size = new System.Drawing.Size(60, 13);
            this.lblGroupGestion.TabIndex = 4;
            this.lblGroupGestion.Text = "GESTIÓN";
            //
            // btnManagement
            //
            this.btnManagement.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnManagement.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManagement.FlatAppearance.BorderSize = 0;
            this.btnManagement.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManagement.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnManagement.ForeColor = System.Drawing.Color.White;
            this.btnManagement.Image = new System.Drawing.Bitmap(global::PharmacySystem.Properties.Resources.ManagementSectionIcon, new System.Drawing.Size(20, 20));
            this.btnManagement.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnManagement.Location = new System.Drawing.Point(10, 224);
            this.btnManagement.Name = "btnManagement";
            this.btnManagement.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnManagement.Size = new System.Drawing.Size(190, 40);
            this.btnManagement.TabIndex = 5;
            this.btnManagement.Text = "        Gestión";
            this.btnManagement.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnManagement.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnManagement.UseVisualStyleBackColor = false;
            this.btnManagement.Click += new System.EventHandler(this.btnManagement_Click);
            //
            // btnSuppliers
            //
            this.btnSuppliers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnSuppliers.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSuppliers.FlatAppearance.BorderSize = 0;
            this.btnSuppliers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSuppliers.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSuppliers.ForeColor = System.Drawing.Color.White;
            this.btnSuppliers.Image = new System.Drawing.Bitmap(global::PharmacySystem.Properties.Resources.proveedoricon, new System.Drawing.Size(20, 20));
            this.btnSuppliers.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSuppliers.Location = new System.Drawing.Point(10, 270);
            this.btnSuppliers.Name = "btnSuppliers";
            this.btnSuppliers.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnSuppliers.Size = new System.Drawing.Size(190, 40);
            this.btnSuppliers.TabIndex = 6;
            this.btnSuppliers.Text = "        Proveedores";
            this.btnSuppliers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSuppliers.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSuppliers.UseVisualStyleBackColor = false;
            this.btnSuppliers.Click += new System.EventHandler(this.btnSuppliers_Click);
            //
            // btnClients
            //
            this.btnClients.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnClients.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClients.FlatAppearance.BorderSize = 0;
            this.btnClients.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClients.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnClients.ForeColor = System.Drawing.Color.White;
            this.btnClients.Image = new System.Drawing.Bitmap(global::PharmacySystem.Properties.Resources.ClientSectionIcon, new System.Drawing.Size(20, 20));
            this.btnClients.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClients.Location = new System.Drawing.Point(10, 316);
            this.btnClients.Name = "btnClients";
            this.btnClients.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnClients.Size = new System.Drawing.Size(190, 40);
            this.btnClients.TabIndex = 7;
            this.btnClients.Text = "        Clientes";
            this.btnClients.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClients.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnClients.UseVisualStyleBackColor = false;
            this.btnClients.Click += new System.EventHandler(this.btnClients_Click);
            //
            // btnUsers
            //
            this.btnUsers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnUsers.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUsers.FlatAppearance.BorderSize = 0;
            this.btnUsers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUsers.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnUsers.ForeColor = System.Drawing.Color.White;
            this.btnUsers.Image = new System.Drawing.Bitmap(global::PharmacySystem.Properties.Resources.usuarioicon, new System.Drawing.Size(20, 20));
            this.btnUsers.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUsers.Location = new System.Drawing.Point(10, 362);
            this.btnUsers.Name = "btnUsers";
            this.btnUsers.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnUsers.Size = new System.Drawing.Size(190, 40);
            this.btnUsers.TabIndex = 8;
            this.btnUsers.Text = "        Usuarios";
            this.btnUsers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUsers.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnUsers.UseVisualStyleBackColor = false;
            this.btnUsers.Click += new System.EventHandler(this.btnUsers_Click);
            //
            // btnRoles
            //
            this.btnRoles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnRoles.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRoles.FlatAppearance.BorderSize = 0;
            this.btnRoles.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRoles.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnRoles.ForeColor = System.Drawing.Color.White;
            this.btnRoles.Image = new System.Drawing.Bitmap(global::PharmacySystem.Properties.Resources.configuracionicon, new System.Drawing.Size(20, 20));
            this.btnRoles.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRoles.Location = new System.Drawing.Point(10, 404);
            this.btnRoles.Name = "btnRoles";
            this.btnRoles.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnRoles.Size = new System.Drawing.Size(190, 40);
            this.btnRoles.TabIndex = 9;
            this.btnRoles.Text = "        Roles";
            this.btnRoles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRoles.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnRoles.UseVisualStyleBackColor = false;
            this.btnRoles.Click += new System.EventHandler(this.btnRoles_Click);
            //
            // lblGroupConsulta
            //
            this.lblGroupConsulta.AutoSize = true;
            this.lblGroupConsulta.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblGroupConsulta.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(169)))), ((int)(((byte)(196)))));
            this.lblGroupConsulta.Location = new System.Drawing.Point(14, 416);
            this.lblGroupConsulta.Name = "lblGroupConsulta";
            this.lblGroupConsulta.Size = new System.Drawing.Size(61, 13);
            this.lblGroupConsulta.TabIndex = 9;
            this.lblGroupConsulta.Text = "CONSULTA";
            //
            // btnReports
            //
            this.btnReports.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnReports.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReports.FlatAppearance.BorderSize = 0;
            this.btnReports.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReports.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnReports.ForeColor = System.Drawing.Color.White;
            this.btnReports.Image = new System.Drawing.Bitmap(global::PharmacySystem.Properties.Resources.reporteicon, new System.Drawing.Size(20, 20));
            this.btnReports.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReports.Location = new System.Drawing.Point(10, 444);
            this.btnReports.Name = "btnReports";
            this.btnReports.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnReports.Size = new System.Drawing.Size(190, 40);
            this.btnReports.TabIndex = 10;
            this.btnReports.Text = "        Reportería";
            this.btnReports.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReports.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnReports.UseVisualStyleBackColor = false;
            this.btnReports.Click += new System.EventHandler(this.btnReports_Click);
            //
            // btnCashCount
            //
            this.btnCashCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnCashCount.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCashCount.FlatAppearance.BorderSize = 0;
            this.btnCashCount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCashCount.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCashCount.ForeColor = System.Drawing.Color.White;
            this.btnCashCount.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCashCount.Location = new System.Drawing.Point(10, 490);
            this.btnCashCount.Name = "btnCashCount";
            this.btnCashCount.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnCashCount.Size = new System.Drawing.Size(190, 40);
            this.btnCashCount.TabIndex = 11;
            this.btnCashCount.Text = "        Arqueo de caja";
            this.btnCashCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCashCount.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCashCount.UseVisualStyleBackColor = false;
            this.btnCashCount.Click += new System.EventHandler(this.btnCashCount_Click);
            //
            // btnAuditLog
            //
            this.btnAuditLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnAuditLog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAuditLog.FlatAppearance.BorderSize = 0;
            this.btnAuditLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAuditLog.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnAuditLog.ForeColor = System.Drawing.Color.White;
            this.btnAuditLog.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAuditLog.Location = new System.Drawing.Point(10, 530);
            this.btnAuditLog.Name = "btnAuditLog";
            this.btnAuditLog.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnAuditLog.Size = new System.Drawing.Size(190, 40);
            this.btnAuditLog.TabIndex = 12;
            this.btnAuditLog.Text = "        Bitácora";
            this.btnAuditLog.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAuditLog.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAuditLog.UseVisualStyleBackColor = false;
            this.btnAuditLog.Click += new System.EventHandler(this.btnAuditLog_Click);
            //
            // btnAlerts
            //
            this.btnAlerts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnAlerts.Controls.Add(this.lblAlertBadge);
            this.btnAlerts.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAlerts.FlatAppearance.BorderSize = 0;
            this.btnAlerts.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnAlerts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAlerts.ForeColor = System.Drawing.Color.White;
            this.btnAlerts.Image = new System.Drawing.Bitmap(global::PharmacySystem.Properties.Resources.alertbellicon, new System.Drawing.Size(20, 20));
            this.btnAlerts.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAlerts.Location = new System.Drawing.Point(10, 490);
            this.btnAlerts.Name = "btnAlerts";
            this.btnAlerts.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnAlerts.Size = new System.Drawing.Size(190, 40);
            this.btnAlerts.TabIndex = 12;
            this.btnAlerts.Text = "        Alertas";
            this.btnAlerts.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAlerts.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAlerts.UseVisualStyleBackColor = false;
            this.btnAlerts.Click += new System.EventHandler(this.OpenAlertsCenter);
            //
            // lblAlertBadge
            //
            // Parented directly to btnAlerts instead of floating on the Form - it now moves
            // automatically whenever LayoutSidebarItems() repositions the button, no runtime
            // recomputation needed (unlike the old MenuStrip version of this badge).
            this.lblAlertBadge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.lblAlertBadge.Font = new System.Drawing.Font("Segoe UI", 7F, System.Drawing.FontStyle.Bold);
            this.lblAlertBadge.ForeColor = System.Drawing.Color.White;
            this.lblAlertBadge.Location = new System.Drawing.Point(164, 3);
            this.lblAlertBadge.Name = "lblAlertBadge";
            this.lblAlertBadge.Size = new System.Drawing.Size(20, 15);
            this.lblAlertBadge.TabIndex = 0;
            this.lblAlertBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblAlertBadge.Visible = false;
            //
            // pnlSidebarHeader
            //
            // The user's own identity - who's logged in - reads as a section of its own above
            // Inicio, not tucked at the bottom under Salir like before.
            this.pnlSidebarHeader.Controls.Add(this.label5);
            this.pnlSidebarHeader.Controls.Add(this.lbluser);
            this.pnlSidebarHeader.Controls.Add(this.lblUserRole);
            this.pnlSidebarHeader.Controls.Add(this.pnlSidebarHeaderDivider);
            this.pnlSidebarHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSidebarHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebarHeader.Name = "pnlSidebarHeader";
            this.pnlSidebarHeader.Size = new System.Drawing.Size(210, 92);
            this.pnlSidebarHeader.TabIndex = 0;
            //
            // label5
            //
            this.label5.AutoSize = true;
            // "Bienvenido" is now a small caption above the name, instead of the biggest line in
            // the block - the name is what actually identifies who's logged in, the role (added
            // below) is what identifies what they can do.
            this.label5.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(169)))), ((int)(((byte)(196)))));
            this.label5.Location = new System.Drawing.Point(14, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Bienvenido";
            //
            // lbluser
            //
            this.lbluser.AutoSize = true;
            this.lbluser.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbluser.ForeColor = System.Drawing.Color.White;
            this.lbluser.Location = new System.Drawing.Point(14, 29);
            this.lbluser.Name = "lbluser";
            this.lbluser.Size = new System.Drawing.Size(53, 21);
            this.lbluser.TabIndex = 1;
            this.lbluser.Text = "Usuario:";
            //
            // lblUserRole
            //
            this.lblUserRole.AutoSize = true;
            this.lblUserRole.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserRole.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(169)))), ((int)(((byte)(196)))));
            this.lblUserRole.Location = new System.Drawing.Point(14, 55);
            this.lblUserRole.Name = "lblUserRole";
            this.lblUserRole.Size = new System.Drawing.Size(30, 14);
            this.lblUserRole.TabIndex = 2;
            //
            // pnlSidebarHeaderDivider
            //
            this.pnlSidebarHeaderDivider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(82)))));
            this.pnlSidebarHeaderDivider.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlSidebarHeaderDivider.Location = new System.Drawing.Point(0, 91);
            this.pnlSidebarHeaderDivider.Name = "pnlSidebarHeaderDivider";
            this.pnlSidebarHeaderDivider.Size = new System.Drawing.Size(210, 1);
            this.pnlSidebarHeaderDivider.TabIndex = 3;
            //
            // pnlSidebarBottom
            //
            this.pnlSidebarBottom.Controls.Add(this.btnExit);
            this.pnlSidebarBottom.Controls.Add(this.pnlSidebarBottomDivider);
            this.pnlSidebarBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlSidebarBottom.Location = new System.Drawing.Point(0, 791);
            this.pnlSidebarBottom.Name = "pnlSidebarBottom";
            this.pnlSidebarBottom.Size = new System.Drawing.Size(210, 70);
            this.pnlSidebarBottom.TabIndex = 2;
            //
            // pnlSidebarBottomDivider
            //
            this.pnlSidebarBottomDivider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(55)))), ((int)(((byte)(82)))));
            this.pnlSidebarBottomDivider.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSidebarBottomDivider.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebarBottomDivider.Name = "pnlSidebarBottomDivider";
            this.pnlSidebarBottomDivider.Size = new System.Drawing.Size(210, 1);
            this.pnlSidebarBottomDivider.TabIndex = 0;
            //
            // btnExit
            //
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(37)))), ((int)(((byte)(69)))));
            this.btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Image = new System.Drawing.Bitmap(global::PharmacySystem.Properties.Resources.saliricon, new System.Drawing.Size(20, 20));
            this.btnExit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExit.Location = new System.Drawing.Point(10, 14);
            this.btnExit.Name = "btnExit";
            this.btnExit.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnExit.Size = new System.Drawing.Size(190, 40);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "        Salir";
            this.btnExit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(1500, 897);
            // pnlTitleBar (Top) is added BEFORE pnlSidebar (Left) so it spans the full width above
            // the sidebar, instead of the sidebar running up under it.
            this.Controls.Add(this.pnlSidebar);
            this.Controls.Add(this.pnlTitleBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.IsMdiContainer = true;
            this.MinimumSize = new System.Drawing.Size(1500, 736);
            this.Name = "MainForm";
            this.Text = "Pharmacy System";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.pnlTitleBar.ResumeLayout(false);
            this.pnlSidebar.ResumeLayout(false);
            this.pnlSidebarItems.ResumeLayout(false);
            this.pnlSidebarItems.PerformLayout();
            this.pnlSidebarHeader.ResumeLayout(false);
            this.pnlSidebarHeader.PerformLayout();
            this.pnlSidebarBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timerNotification;
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitleBarText;
        private System.Windows.Forms.Button btnMinimizeWin;
        private System.Windows.Forms.Button btnMaximizeRestore;
        private System.Windows.Forms.Button btnCloseWin;
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlSidebarItems;
        private System.Windows.Forms.Button btnHome;
        private System.Windows.Forms.Label lblGroupOperacion;
        private System.Windows.Forms.Button btnSales;
        private System.Windows.Forms.Button btnPurchases;
        private System.Windows.Forms.Label lblGroupGestion;
        private System.Windows.Forms.Button btnManagement;
        private System.Windows.Forms.Button btnSuppliers;
        private System.Windows.Forms.Button btnClients;
        private System.Windows.Forms.Button btnUsers;
        private System.Windows.Forms.Button btnRoles;
        private System.Windows.Forms.Label lblGroupConsulta;
        private System.Windows.Forms.Button btnReports;
        private System.Windows.Forms.Button btnCashCount;
        private System.Windows.Forms.Button btnAuditLog;
        private System.Windows.Forms.Button btnAlerts;
        private System.Windows.Forms.Label lblAlertBadge;
        private System.Windows.Forms.Panel pnlSidebarHeader;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lbluser;
        private System.Windows.Forms.Label lblUserRole;
        private System.Windows.Forms.Panel pnlSidebarHeaderDivider;
        private System.Windows.Forms.Panel pnlSidebarBottom;
        private System.Windows.Forms.Panel pnlSidebarBottomDivider;
        private System.Windows.Forms.Button btnExit;
    }
}
