namespace PharmacySystem
{
    partial class frmReport
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabManagement = new System.Windows.Forms.TabControl();
            this.tabProduct = new System.Windows.Forms.TabPage();
            this.dgdatasale = new System.Windows.Forms.DataGridView();
            this.txtenddate = new System.Windows.Forms.DateTimePicker();
            this.txtstartdate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.btnConsultSale = new System.Windows.Forms.Button();
            this.btnExportSale = new System.Windows.Forms.Button();
            this.lblSaleTotals = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tabCategory = new System.Windows.Forms.TabPage();
            this.dgdatapurchase = new System.Windows.Forms.DataGridView();
            this.btnConsultPurchase = new System.Windows.Forms.Button();
            this.cbosupplier = new System.Windows.Forms.ComboBox();
            this.txtenddatepurchase = new System.Windows.Forms.DateTimePicker();
            this.txtstartdatepurchase = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnExportPurchases = new System.Windows.Forms.Button();
            this.lblPurchaseTotals = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tabStore = new System.Windows.Forms.TabPage();
            this.dgdataproduct = new System.Windows.Forms.DataGridView();
            this.btnConsultProduct = new System.Windows.Forms.Button();
            this.cbocategory = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.btnExportProduct = new System.Windows.Forms.Button();
            this.lblProductTotals = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.tabAlertHistory = new System.Windows.Forms.TabPage();
            this.dgdataalerthistory = new System.Windows.Forms.DataGridView();
            this.txtenddatealerthistory = new System.Windows.Forms.DateTimePicker();
            this.txtstartdatealerthistory = new System.Windows.Forms.DateTimePicker();
            this.labelAlertHistoryEnd = new System.Windows.Forms.Label();
            this.labelAlertHistoryStart = new System.Windows.Forms.Label();
            this.labelAlertHistoryTitle = new System.Windows.Forms.Label();
            this.btnConsultAlertHistory = new System.Windows.Forms.Button();
            this.btnExportAlertHistory = new System.Windows.Forms.Button();
            this.labelAlertHistoryTopBorder = new System.Windows.Forms.Label();
            this.labelAlertHistoryBottomBorder = new System.Windows.Forms.Label();
            this.tabManagement.SuspendLayout();
            this.tabProduct.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdatasale)).BeginInit();
            this.tabCategory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdatapurchase)).BeginInit();
            this.tabStore.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdataproduct)).BeginInit();
            this.tabAlertHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdataalerthistory)).BeginInit();
            this.SuspendLayout();
            // 
            // tabManagement
            // 
            this.tabManagement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabManagement.Controls.Add(this.tabProduct);
            this.tabManagement.Controls.Add(this.tabCategory);
            this.tabManagement.Controls.Add(this.tabStore);
            this.tabManagement.Controls.Add(this.tabAlertHistory);
            this.tabManagement.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabManagement.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabManagement.Location = new System.Drawing.Point(12, 5);
            this.tabManagement.Name = "tabManagement";
            this.tabManagement.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tabManagement.SelectedIndex = 0;
            this.tabManagement.Size = new System.Drawing.Size(1245, 551);
            this.tabManagement.TabIndex = 1;
            // 
            // tabProduct
            // 
            this.tabProduct.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.tabProduct.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabProduct.Controls.Add(this.dgdatasale);
            this.tabProduct.Controls.Add(this.lblSaleTotals);
            this.tabProduct.Controls.Add(this.txtenddate);
            this.tabProduct.Controls.Add(this.txtstartdate);
            this.tabProduct.Controls.Add(this.label1);
            this.tabProduct.Controls.Add(this.label9);
            this.tabProduct.Controls.Add(this.label10);
            this.tabProduct.Controls.Add(this.btnConsultSale);
            this.tabProduct.Controls.Add(this.btnExportSale);
            this.tabProduct.Controls.Add(this.label3);
            this.tabProduct.Controls.Add(this.label2);
            this.tabProduct.Location = new System.Drawing.Point(4, 25);
            this.tabProduct.Name = "tabProduct";
            this.tabProduct.Padding = new System.Windows.Forms.Padding(3);
            this.tabProduct.Size = new System.Drawing.Size(1237, 522);
            this.tabProduct.TabIndex = 0;
            this.tabProduct.Text = "Ventas";
            // 
            // dgdatasale
            // 
            this.dgdatasale.AllowUserToAddRows = false;
            this.dgdatasale.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgdatasale.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgdatasale.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(1);
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdatasale.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgdatasale.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdatasale.EnableHeadersVisualStyles = false;
            this.dgdatasale.GridColor = System.Drawing.Color.DimGray;
            this.dgdatasale.Location = new System.Drawing.Point(26, 157);
            this.dgdatasale.MultiSelect = false;
            this.dgdatasale.Name = "dgdatasale";
            this.dgdatasale.ReadOnly = true;
            this.dgdatasale.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdatasale.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgdatasale.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this.dgdatasale.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgdatasale.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            this.dgdatasale.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgdatasale.RowTemplate.Height = 30;
            this.dgdatasale.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdatasale.Size = new System.Drawing.Size(1189, 326);
            this.dgdatasale.TabIndex = 44;
            //
            // lblSaleTotals
            //
            this.lblSaleTotals.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSaleTotals.BackColor = System.Drawing.Color.White;
            this.lblSaleTotals.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblSaleTotals.Location = new System.Drawing.Point(26, 490);
            this.lblSaleTotals.Name = "lblSaleTotals";
            this.lblSaleTotals.Size = new System.Drawing.Size(1189, 22);
            this.lblSaleTotals.TabIndex = 45;
            this.lblSaleTotals.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtenddate
            // 
            this.txtenddate.CustomFormat = "dd-MM-yyyy";
            this.txtenddate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.txtenddate.Location = new System.Drawing.Point(395, 45);
            this.txtenddate.Name = "txtenddate";
            this.txtenddate.Size = new System.Drawing.Size(187, 23);
            this.txtenddate.TabIndex = 43;
            // 
            // txtstartdate
            // 
            this.txtstartdate.CustomFormat = "dd-MM-yyyy";
            this.txtstartdate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.txtstartdate.Location = new System.Drawing.Point(107, 45);
            this.txtstartdate.Name = "txtstartdate";
            this.txtstartdate.Size = new System.Drawing.Size(187, 23);
            this.txtstartdate.TabIndex = 43;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(313, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 15);
            this.label1.TabIndex = 38;
            this.label1.Text = "Fecha Fin:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.White;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(25, 50);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(76, 15);
            this.label9.TabIndex = 38;
            this.label9.Text = "Fecha Inicio:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.Color.White;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(22, 14);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(161, 20);
            this.label10.TabIndex = 37;
            this.label10.Text = "Reporte de Ventas";
            // 
            // btnConsultSale
            // 
            this.btnConsultSale.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnConsultSale.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConsultSale.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConsultSale.Image = global::PharmacySystem.Properties.Resources.search16;
            this.btnConsultSale.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnConsultSale.Location = new System.Drawing.Point(611, 43);
            this.btnConsultSale.Name = "btnConsultSale";
            this.btnConsultSale.Size = new System.Drawing.Size(134, 25);
            this.btnConsultSale.TabIndex = 36;
            this.btnConsultSale.Text = "Consultar";
            this.btnConsultSale.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnConsultSale.UseVisualStyleBackColor = false;
            this.btnConsultSale.Click += new System.EventHandler(this.btnConsultSale_Click);
            // 
            // btnExportSale
            // 
            this.btnExportSale.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnExportSale.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportSale.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportSale.Image = global::PharmacySystem.Properties.Resources.excel;
            this.btnExportSale.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExportSale.Location = new System.Drawing.Point(26, 111);
            this.btnExportSale.Name = "btnExportSale";
            this.btnExportSale.Size = new System.Drawing.Size(134, 40);
            this.btnExportSale.TabIndex = 36;
            this.btnExportSale.Text = "Exportar";
            this.btnExportSale.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnExportSale.UseVisualStyleBackColor = false;
            this.btnExportSale.Click += new System.EventHandler(this.btnExportSale_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.BackColor = System.Drawing.Color.White;
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Location = new System.Drawing.Point(14, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(1215, 84);
            this.label3.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Location = new System.Drawing.Point(14, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(1215, 418);
            this.label2.TabIndex = 1;
            // 
            // tabCategory
            // 
            this.tabCategory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.tabCategory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabCategory.Controls.Add(this.dgdatapurchase);
            this.tabCategory.Controls.Add(this.lblPurchaseTotals);
            this.tabCategory.Controls.Add(this.btnConsultPurchase);
            this.tabCategory.Controls.Add(this.cbosupplier);
            this.tabCategory.Controls.Add(this.txtenddatepurchase);
            this.tabCategory.Controls.Add(this.txtstartdatepurchase);
            this.tabCategory.Controls.Add(this.label4);
            this.tabCategory.Controls.Add(this.label11);
            this.tabCategory.Controls.Add(this.label5);
            this.tabCategory.Controls.Add(this.label6);
            this.tabCategory.Controls.Add(this.btnExportPurchases);
            this.tabCategory.Controls.Add(this.label7);
            this.tabCategory.Controls.Add(this.label8);
            this.tabCategory.Location = new System.Drawing.Point(4, 25);
            this.tabCategory.Name = "tabCategory";
            this.tabCategory.Padding = new System.Windows.Forms.Padding(3);
            this.tabCategory.Size = new System.Drawing.Size(1237, 522);
            this.tabCategory.TabIndex = 1;
            this.tabCategory.Text = "Compras";
            // 
            // dgdatapurchase
            // 
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(239)))), ((int)(((byte)(249)))));
            this.dgdatapurchase.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgdatapurchase.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgdatapurchase.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgdatapurchase.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            this.dgdatapurchase.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.Padding = new System.Windows.Forms.Padding(1);
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdatapurchase.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgdatapurchase.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdatapurchase.EnableHeadersVisualStyles = false;
            this.dgdatapurchase.GridColor = System.Drawing.Color.DimGray;
            this.dgdatapurchase.Location = new System.Drawing.Point(22, 156);
            this.dgdatapurchase.MultiSelect = false;
            this.dgdatapurchase.Name = "dgdatapurchase";
            this.dgdatapurchase.ReadOnly = true;
            this.dgdatapurchase.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdatapurchase.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dgdatapurchase.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            this.dgdatapurchase.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dgdatapurchase.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgdatapurchase.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            this.dgdatapurchase.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgdatapurchase.RowTemplate.Height = 30;
            this.dgdatapurchase.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdatapurchase.Size = new System.Drawing.Size(1189, 326);
            this.dgdatapurchase.TabIndex = 56;
            //
            // lblPurchaseTotals
            //
            this.lblPurchaseTotals.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPurchaseTotals.BackColor = System.Drawing.Color.White;
            this.lblPurchaseTotals.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblPurchaseTotals.Location = new System.Drawing.Point(22, 489);
            this.lblPurchaseTotals.Name = "lblPurchaseTotals";
            this.lblPurchaseTotals.Size = new System.Drawing.Size(1189, 22);
            this.lblPurchaseTotals.TabIndex = 57;
            this.lblPurchaseTotals.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnConsultPurchase
            // 
            this.btnConsultPurchase.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnConsultPurchase.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConsultPurchase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConsultPurchase.Image = global::PharmacySystem.Properties.Resources.search16;
            this.btnConsultPurchase.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnConsultPurchase.Location = new System.Drawing.Point(886, 42);
            this.btnConsultPurchase.Name = "btnConsultPurchase";
            this.btnConsultPurchase.Size = new System.Drawing.Size(134, 25);
            this.btnConsultPurchase.TabIndex = 55;
            this.btnConsultPurchase.Text = "Consultar";
            this.btnConsultPurchase.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnConsultPurchase.UseVisualStyleBackColor = false;
            this.btnConsultPurchase.Click += new System.EventHandler(this.btnConsultPurchase_Click);
            // 
            // cbosupplier
            // 
            this.cbosupplier.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbosupplier.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbosupplier.FormattingEnabled = true;
            this.cbosupplier.Location = new System.Drawing.Point(91, 46);
            this.cbosupplier.Name = "cbosupplier";
            this.cbosupplier.Size = new System.Drawing.Size(189, 23);
            this.cbosupplier.TabIndex = 54;
            // 
            // txtenddatepurchase
            // 
            this.txtenddatepurchase.CustomFormat = "dd/MM/yyyy";
            this.txtenddatepurchase.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.txtenddatepurchase.Location = new System.Drawing.Point(669, 44);
            this.txtenddatepurchase.Name = "txtenddatepurchase";
            this.txtenddatepurchase.Size = new System.Drawing.Size(187, 23);
            this.txtenddatepurchase.TabIndex = 52;
            // 
            // txtstartdatepurchase
            // 
            this.txtstartdatepurchase.CustomFormat = "dd/MM/yyyy";
            this.txtstartdatepurchase.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.txtstartdatepurchase.Location = new System.Drawing.Point(381, 44);
            this.txtstartdatepurchase.Name = "txtstartdatepurchase";
            this.txtstartdatepurchase.Size = new System.Drawing.Size(187, 23);
            this.txtstartdatepurchase.TabIndex = 53;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(587, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 15);
            this.label4.TabIndex = 49;
            this.label4.Text = "Fecha Fin:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(19, 49);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(66, 15);
            this.label11.TabIndex = 50;
            this.label11.Text = "Proveedor:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(299, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 15);
            this.label5.TabIndex = 50;
            this.label5.Text = "Fecha Inicio:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(18, 13);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(175, 20);
            this.label6.TabIndex = 48;
            this.label6.Text = "Reporte de Compras";
            // 
            // btnExportPurchases
            // 
            this.btnExportPurchases.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnExportPurchases.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportPurchases.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportPurchases.Image = global::PharmacySystem.Properties.Resources.excel;
            this.btnExportPurchases.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExportPurchases.Location = new System.Drawing.Point(22, 110);
            this.btnExportPurchases.Name = "btnExportPurchases";
            this.btnExportPurchases.Size = new System.Drawing.Size(134, 40);
            this.btnExportPurchases.TabIndex = 47;
            this.btnExportPurchases.Text = "Exportar";
            this.btnExportPurchases.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnExportPurchases.UseVisualStyleBackColor = false;
            this.btnExportPurchases.Click += new System.EventHandler(this.btnExportPurchases_Click);
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label7.Location = new System.Drawing.Point(10, 4);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(1215, 84);
            this.label7.TabIndex = 45;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label8.Location = new System.Drawing.Point(10, 99);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(1215, 417);
            this.label8.TabIndex = 44;
            // 
            // tabStore
            // 
            this.tabStore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.tabStore.Controls.Add(this.dgdataproduct);
            this.tabStore.Controls.Add(this.lblProductTotals);
            this.tabStore.Controls.Add(this.btnConsultProduct);
            this.tabStore.Controls.Add(this.cbocategory);
            this.tabStore.Controls.Add(this.label12);
            this.tabStore.Controls.Add(this.label13);
            this.tabStore.Controls.Add(this.btnExportProduct);
            this.tabStore.Controls.Add(this.label14);
            this.tabStore.Controls.Add(this.label15);
            this.tabStore.Location = new System.Drawing.Point(4, 25);
            this.tabStore.Name = "tabStore";
            this.tabStore.Padding = new System.Windows.Forms.Padding(3);
            this.tabStore.Size = new System.Drawing.Size(1237, 522);
            this.tabStore.TabIndex = 2;
            this.tabStore.Text = "Productos";
            // 
            // dgdataproduct
            // 
            this.dgdataproduct.AllowUserToAddRows = false;
            this.dgdataproduct.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgdataproduct.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgdataproduct.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle7.Padding = new System.Windows.Forms.Padding(1);
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdataproduct.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgdataproduct.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdataproduct.EnableHeadersVisualStyles = false;
            this.dgdataproduct.GridColor = System.Drawing.Color.DimGray;
            this.dgdataproduct.Location = new System.Drawing.Point(23, 157);
            this.dgdataproduct.MultiSelect = false;
            this.dgdataproduct.Name = "dgdataproduct";
            this.dgdataproduct.ReadOnly = true;
            this.dgdataproduct.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdataproduct.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
            this.dgdataproduct.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this.dgdataproduct.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgdataproduct.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            this.dgdataproduct.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgdataproduct.RowTemplate.Height = 30;
            this.dgdataproduct.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdataproduct.Size = new System.Drawing.Size(1189, 326);
            this.dgdataproduct.TabIndex = 64;
            //
            // lblProductTotals
            //
            this.lblProductTotals.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProductTotals.BackColor = System.Drawing.Color.White;
            this.lblProductTotals.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblProductTotals.Location = new System.Drawing.Point(23, 490);
            this.lblProductTotals.Name = "lblProductTotals";
            this.lblProductTotals.Size = new System.Drawing.Size(1189, 22);
            this.lblProductTotals.TabIndex = 65;
            this.lblProductTotals.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnConsultProduct
            // 
            this.btnConsultProduct.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnConsultProduct.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConsultProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConsultProduct.Image = global::PharmacySystem.Properties.Resources.search16;
            this.btnConsultProduct.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnConsultProduct.Location = new System.Drawing.Point(313, 45);
            this.btnConsultProduct.Name = "btnConsultProduct";
            this.btnConsultProduct.Size = new System.Drawing.Size(134, 25);
            this.btnConsultProduct.TabIndex = 63;
            this.btnConsultProduct.Text = "Consultar";
            this.btnConsultProduct.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnConsultProduct.UseVisualStyleBackColor = false;
            this.btnConsultProduct.Click += new System.EventHandler(this.btnConsultProduct_Click);
            // 
            // cbocategory
            // 
            this.cbocategory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbocategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbocategory.FormattingEnabled = true;
            this.cbocategory.Location = new System.Drawing.Point(92, 47);
            this.cbocategory.Name = "cbocategory";
            this.cbocategory.Size = new System.Drawing.Size(189, 23);
            this.cbocategory.TabIndex = 62;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.BackColor = System.Drawing.Color.White;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(20, 50);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(63, 15);
            this.label12.TabIndex = 60;
            this.label12.Text = "Categoría:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.BackColor = System.Drawing.Color.White;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(19, 14);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(185, 20);
            this.label13.TabIndex = 59;
            this.label13.Text = "Reporte de Productos";
            // 
            // btnExportProduct
            // 
            this.btnExportProduct.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnExportProduct.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportProduct.Image = global::PharmacySystem.Properties.Resources.excel;
            this.btnExportProduct.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExportProduct.Location = new System.Drawing.Point(23, 111);
            this.btnExportProduct.Name = "btnExportProduct";
            this.btnExportProduct.Size = new System.Drawing.Size(134, 40);
            this.btnExportProduct.TabIndex = 58;
            this.btnExportProduct.Text = "Exportar";
            this.btnExportProduct.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnExportProduct.UseVisualStyleBackColor = false;
            this.btnExportProduct.Click += new System.EventHandler(this.btnExportProduct_Click);
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label14.BackColor = System.Drawing.Color.White;
            this.label14.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label14.Location = new System.Drawing.Point(11, 5);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(1215, 84);
            this.label14.TabIndex = 56;
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label15.BackColor = System.Drawing.Color.White;
            this.label15.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label15.Location = new System.Drawing.Point(11, 100);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(1215, 417);
            this.label15.TabIndex = 55;
            // 
            // tabAlertHistory
            //
            this.tabAlertHistory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.tabAlertHistory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabAlertHistory.Controls.Add(this.dgdataalerthistory);
            this.tabAlertHistory.Controls.Add(this.txtenddatealerthistory);
            this.tabAlertHistory.Controls.Add(this.txtstartdatealerthistory);
            this.tabAlertHistory.Controls.Add(this.labelAlertHistoryEnd);
            this.tabAlertHistory.Controls.Add(this.labelAlertHistoryStart);
            this.tabAlertHistory.Controls.Add(this.labelAlertHistoryTitle);
            this.tabAlertHistory.Controls.Add(this.btnConsultAlertHistory);
            this.tabAlertHistory.Controls.Add(this.btnExportAlertHistory);
            this.tabAlertHistory.Controls.Add(this.labelAlertHistoryTopBorder);
            this.tabAlertHistory.Controls.Add(this.labelAlertHistoryBottomBorder);
            this.tabAlertHistory.Location = new System.Drawing.Point(4, 25);
            this.tabAlertHistory.Name = "tabAlertHistory";
            this.tabAlertHistory.Padding = new System.Windows.Forms.Padding(3);
            this.tabAlertHistory.Size = new System.Drawing.Size(1237, 522);
            this.tabAlertHistory.TabIndex = 3;
            this.tabAlertHistory.Text = "Historial de Alertas";
            //
            // dgdataalerthistory
            //
            this.dgdataalerthistory.AllowUserToAddRows = false;
            this.dgdataalerthistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgdataalerthistory.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgdataalerthistory.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle9.Padding = new System.Windows.Forms.Padding(1);
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdataalerthistory.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.dgdataalerthistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdataalerthistory.EnableHeadersVisualStyles = false;
            this.dgdataalerthistory.GridColor = System.Drawing.Color.DimGray;
            this.dgdataalerthistory.Location = new System.Drawing.Point(23, 157);
            this.dgdataalerthistory.MultiSelect = false;
            this.dgdataalerthistory.Name = "dgdataalerthistory";
            this.dgdataalerthistory.ReadOnly = true;
            this.dgdataalerthistory.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdataalerthistory.RowHeadersDefaultCellStyle = dataGridViewCellStyle10;
            this.dgdataalerthistory.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this.dgdataalerthistory.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgdataalerthistory.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            this.dgdataalerthistory.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgdataalerthistory.RowTemplate.Height = 30;
            this.dgdataalerthistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdataalerthistory.Size = new System.Drawing.Size(1189, 352);
            this.dgdataalerthistory.TabIndex = 65;
            //
            // txtenddatealerthistory
            //
            this.txtenddatealerthistory.CustomFormat = "dd-MM-yyyy";
            this.txtenddatealerthistory.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.txtenddatealerthistory.Location = new System.Drawing.Point(395, 45);
            this.txtenddatealerthistory.Name = "txtenddatealerthistory";
            this.txtenddatealerthistory.Size = new System.Drawing.Size(187, 23);
            this.txtenddatealerthistory.TabIndex = 66;
            //
            // txtstartdatealerthistory
            //
            this.txtstartdatealerthistory.CustomFormat = "dd-MM-yyyy";
            this.txtstartdatealerthistory.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.txtstartdatealerthistory.Location = new System.Drawing.Point(107, 45);
            this.txtstartdatealerthistory.Name = "txtstartdatealerthistory";
            this.txtstartdatealerthistory.Size = new System.Drawing.Size(187, 23);
            this.txtstartdatealerthistory.TabIndex = 66;
            //
            // labelAlertHistoryEnd
            //
            this.labelAlertHistoryEnd.AutoSize = true;
            this.labelAlertHistoryEnd.BackColor = System.Drawing.Color.White;
            this.labelAlertHistoryEnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAlertHistoryEnd.Location = new System.Drawing.Point(313, 50);
            this.labelAlertHistoryEnd.Name = "labelAlertHistoryEnd";
            this.labelAlertHistoryEnd.Size = new System.Drawing.Size(64, 15);
            this.labelAlertHistoryEnd.TabIndex = 67;
            this.labelAlertHistoryEnd.Text = "Fecha Fin:";
            //
            // labelAlertHistoryStart
            //
            this.labelAlertHistoryStart.AutoSize = true;
            this.labelAlertHistoryStart.BackColor = System.Drawing.Color.White;
            this.labelAlertHistoryStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAlertHistoryStart.Location = new System.Drawing.Point(25, 50);
            this.labelAlertHistoryStart.Name = "labelAlertHistoryStart";
            this.labelAlertHistoryStart.Size = new System.Drawing.Size(76, 15);
            this.labelAlertHistoryStart.TabIndex = 67;
            this.labelAlertHistoryStart.Text = "Fecha Inicio:";
            //
            // labelAlertHistoryTitle
            //
            this.labelAlertHistoryTitle.AutoSize = true;
            this.labelAlertHistoryTitle.BackColor = System.Drawing.Color.White;
            this.labelAlertHistoryTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAlertHistoryTitle.Location = new System.Drawing.Point(22, 14);
            this.labelAlertHistoryTitle.Name = "labelAlertHistoryTitle";
            this.labelAlertHistoryTitle.Size = new System.Drawing.Size(230, 20);
            this.labelAlertHistoryTitle.TabIndex = 68;
            this.labelAlertHistoryTitle.Text = "Historial de Alertas";
            //
            // btnConsultAlertHistory
            //
            this.btnConsultAlertHistory.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnConsultAlertHistory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConsultAlertHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConsultAlertHistory.Image = global::PharmacySystem.Properties.Resources.search16;
            this.btnConsultAlertHistory.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnConsultAlertHistory.Location = new System.Drawing.Point(611, 43);
            this.btnConsultAlertHistory.Name = "btnConsultAlertHistory";
            this.btnConsultAlertHistory.Size = new System.Drawing.Size(134, 25);
            this.btnConsultAlertHistory.TabIndex = 69;
            this.btnConsultAlertHistory.Text = "Consultar";
            this.btnConsultAlertHistory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnConsultAlertHistory.UseVisualStyleBackColor = false;
            this.btnConsultAlertHistory.Click += new System.EventHandler(this.btnConsultAlertHistory_Click);
            //
            // btnExportAlertHistory
            //
            this.btnExportAlertHistory.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnExportAlertHistory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportAlertHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportAlertHistory.Image = global::PharmacySystem.Properties.Resources.excel;
            this.btnExportAlertHistory.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExportAlertHistory.Location = new System.Drawing.Point(23, 111);
            this.btnExportAlertHistory.Name = "btnExportAlertHistory";
            this.btnExportAlertHistory.Size = new System.Drawing.Size(134, 40);
            this.btnExportAlertHistory.TabIndex = 70;
            this.btnExportAlertHistory.Text = "Exportar";
            this.btnExportAlertHistory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnExportAlertHistory.UseVisualStyleBackColor = false;
            this.btnExportAlertHistory.Click += new System.EventHandler(this.btnExportAlertHistory_Click);
            //
            // labelAlertHistoryTopBorder
            //
            this.labelAlertHistoryTopBorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAlertHistoryTopBorder.BackColor = System.Drawing.Color.White;
            this.labelAlertHistoryTopBorder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelAlertHistoryTopBorder.Location = new System.Drawing.Point(14, 5);
            this.labelAlertHistoryTopBorder.Name = "labelAlertHistoryTopBorder";
            this.labelAlertHistoryTopBorder.Size = new System.Drawing.Size(1215, 84);
            this.labelAlertHistoryTopBorder.TabIndex = 71;
            //
            // labelAlertHistoryBottomBorder
            //
            this.labelAlertHistoryBottomBorder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAlertHistoryBottomBorder.BackColor = System.Drawing.Color.White;
            this.labelAlertHistoryBottomBorder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelAlertHistoryBottomBorder.Location = new System.Drawing.Point(14, 99);
            this.labelAlertHistoryBottomBorder.Name = "labelAlertHistoryBottomBorder";
            this.labelAlertHistoryBottomBorder.Size = new System.Drawing.Size(1215, 418);
            this.labelAlertHistoryBottomBorder.TabIndex = 72;
            //
            // frmReport
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(1269, 568);
            this.ControlBox = false;
            this.Controls.Add(this.tabManagement);
            this.Name = "frmReport";
            this.Text = "Reporteria";
            this.Load += new System.EventHandler(this.frmReport_Load);
            this.tabManagement.ResumeLayout(false);
            this.tabProduct.ResumeLayout(false);
            this.tabProduct.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdatasale)).EndInit();
            this.tabCategory.ResumeLayout(false);
            this.tabCategory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdatapurchase)).EndInit();
            this.tabStore.ResumeLayout(false);
            this.tabStore.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdataproduct)).EndInit();
            this.tabAlertHistory.ResumeLayout(false);
            this.tabAlertHistory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdataalerthistory)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabManagement;
        private System.Windows.Forms.TabPage tabProduct;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnExportSale;
        private System.Windows.Forms.Label lblSaleTotals;
        private System.Windows.Forms.Label lblPurchaseTotals;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabCategory;
        private System.Windows.Forms.TabPage tabStore;
        private System.Windows.Forms.DateTimePicker txtenddate;
        private System.Windows.Forms.DateTimePicker txtstartdate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConsultSale;
        private System.Windows.Forms.DateTimePicker txtenddatepurchase;
        private System.Windows.Forms.DateTimePicker txtstartdatepurchase;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnExportPurchases;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbosupplier;
        private System.Windows.Forms.ComboBox cbocategory;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnExportProduct;
        private System.Windows.Forms.Label lblProductTotals;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.DataGridView dgdatasale;
        private System.Windows.Forms.Button btnConsultPurchase;
        private System.Windows.Forms.Button btnConsultProduct;
        private System.Windows.Forms.DataGridView dgdatapurchase;
        private System.Windows.Forms.DataGridView dgdataproduct;
        private System.Windows.Forms.TabPage tabAlertHistory;
        private System.Windows.Forms.DataGridView dgdataalerthistory;
        private System.Windows.Forms.DateTimePicker txtenddatealerthistory;
        private System.Windows.Forms.DateTimePicker txtstartdatealerthistory;
        private System.Windows.Forms.Label labelAlertHistoryEnd;
        private System.Windows.Forms.Label labelAlertHistoryStart;
        private System.Windows.Forms.Label labelAlertHistoryTitle;
        private System.Windows.Forms.Button btnConsultAlertHistory;
        private System.Windows.Forms.Button btnExportAlertHistory;
        private System.Windows.Forms.Label labelAlertHistoryTopBorder;
        private System.Windows.Forms.Label labelAlertHistoryBottomBorder;
    }
}