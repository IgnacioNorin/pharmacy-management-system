namespace PharmacySystem
{
    partial class frmManagement
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
            this.tabManagement = new System.Windows.Forms.TabControl();
            this.tabProduct = new System.Windows.Forms.TabPage();
            this.pblist = new System.Windows.Forms.PictureBox();
            this.txtindexproduct = new System.Windows.Forms.TextBox();
            this.txtidproduct = new System.Windows.Forms.TextBox();
            this.dgdataproduct = new System.Windows.Forms.DataGridView();
            this.txtsearchproduct = new System.Windows.Forms.TextBox();
            this.cbosearchproduct = new System.Windows.Forms.ComboBox();
            this.btnclear = new System.Windows.Forms.Button();
            this.btnsearch = new System.Windows.Forms.Button();
            this.cbocategory = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.btnDeleteProduct = new System.Windows.Forms.Button();
            this.btnCleanProduct = new System.Windows.Forms.Button();
            this.btnSaveProduct = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtdescriptionproduct = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtnameproduct = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtcodeproduct = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabCategory = new System.Windows.Forms.TabPage();
            this.pblist2 = new System.Windows.Forms.PictureBox();
            this.txtindexcategory = new System.Windows.Forms.TextBox();
            this.txtidcategory = new System.Windows.Forms.TextBox();
            this.dgdatacategory = new System.Windows.Forms.DataGridView();
            this.btnDeleteCategory = new System.Windows.Forms.Button();
            this.btnCleanCategory = new System.Windows.Forms.Button();
            this.btnSaveCategory = new System.Windows.Forms.Button();
            this.txtdescriptioncategory = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.tabStore = new System.Windows.Forms.TabPage();
            this.txtaddress = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.btnSaveStore = new System.Windows.Forms.Button();
            this.label22 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.txtphone = new System.Windows.Forms.TextBox();
            this.txtemail = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.txtlegalName = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.txttaxid = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.cbocurrency = new System.Windows.Forms.ComboBox();
            this.label24 = new System.Windows.Forms.Label();
            this.txttaxrate = new System.Windows.Forms.TextBox();
            this.lbltaxrate = new System.Windows.Forms.Label();
            this.chkTaxAffected = new System.Windows.Forms.CheckBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tabManagement.SuspendLayout();
            this.tabProduct.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pblist)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgdataproduct)).BeginInit();
            this.tabCategory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pblist2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgdatacategory)).BeginInit();
            this.tabStore.SuspendLayout();
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
            this.tabManagement.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabManagement.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabManagement.Location = new System.Drawing.Point(12, 12);
            this.tabManagement.Name = "tabManagement";
            this.tabManagement.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tabManagement.SelectedIndex = 0;
            this.tabManagement.Size = new System.Drawing.Size(1245, 544);
            this.tabManagement.TabIndex = 0;
            // 
            // tabProduct
            // 
            this.tabProduct.BackColor = System.Drawing.Color.White;
            this.tabProduct.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabProduct.Controls.Add(this.pblist);
            this.tabProduct.Controls.Add(this.txtindexproduct);
            this.tabProduct.Controls.Add(this.txtidproduct);
            this.tabProduct.Controls.Add(this.dgdataproduct);
            this.tabProduct.Controls.Add(this.txtsearchproduct);
            this.tabProduct.Controls.Add(this.cbosearchproduct);
            this.tabProduct.Controls.Add(this.btnclear);
            this.tabProduct.Controls.Add(this.btnsearch);
            this.tabProduct.Controls.Add(this.cbocategory);
            this.tabProduct.Controls.Add(this.chkTaxAffected);
            this.tabProduct.Controls.Add(this.label9);
            this.tabProduct.Controls.Add(this.label10);
            this.tabProduct.Controls.Add(this.btnDeleteProduct);
            this.tabProduct.Controls.Add(this.btnCleanProduct);
            this.tabProduct.Controls.Add(this.btnSaveProduct);
            this.tabProduct.Controls.Add(this.label6);
            this.tabProduct.Controls.Add(this.txtdescriptionproduct);
            this.tabProduct.Controls.Add(this.label5);
            this.tabProduct.Controls.Add(this.txtnameproduct);
            this.tabProduct.Controls.Add(this.label4);
            this.tabProduct.Controls.Add(this.txtcodeproduct);
            this.tabProduct.Controls.Add(this.label7);
            this.tabProduct.Controls.Add(this.label8);
            this.tabProduct.Controls.Add(this.label3);
            this.tabProduct.Controls.Add(this.label2);
            this.tabProduct.Controls.Add(this.label1);
            this.tabProduct.Location = new System.Drawing.Point(4, 25);
            this.tabProduct.Name = "tabProduct";
            this.tabProduct.Padding = new System.Windows.Forms.Padding(3);
            this.tabProduct.Size = new System.Drawing.Size(1237, 515);
            this.tabProduct.TabIndex = 0;
            this.tabProduct.Text = "Productos";
            // 
            // pblist
            // 
            this.pblist.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            this.pblist.Image = global::PharmacySystem.Properties.Resources.listaicon;
            this.pblist.Location = new System.Drawing.Point(490, 22);
            this.pblist.Name = "pblist";
            this.pblist.Size = new System.Drawing.Size(32, 32);
            this.pblist.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pblist.TabIndex = 66;
            this.pblist.TabStop = false;
            // 
            // txtindexproduct
            // 
            this.txtindexproduct.Location = new System.Drawing.Point(230, 69);
            this.txtindexproduct.Name = "txtindexproduct";
            this.txtindexproduct.Size = new System.Drawing.Size(22, 23);
            this.txtindexproduct.TabIndex = 64;
            this.txtindexproduct.Text = "0";
            this.txtindexproduct.Visible = false;
            // 
            // txtidproduct
            // 
            this.txtidproduct.Location = new System.Drawing.Point(256, 69);
            this.txtidproduct.Name = "txtidproduct";
            this.txtidproduct.Size = new System.Drawing.Size(22, 23);
            this.txtidproduct.TabIndex = 65;
            this.txtidproduct.Text = "0";
            this.txtidproduct.Visible = false;
            // 
            // dgdataproduct
            // 
            this.dgdataproduct.AllowUserToAddRows = false;
            this.dgdataproduct.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgdataproduct.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgdataproduct.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(1);
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdataproduct.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgdataproduct.ColumnHeadersHeight = 30;
            this.dgdataproduct.EnableHeadersVisualStyles = false;
            this.dgdataproduct.GridColor = System.Drawing.Color.DimGray;
            this.dgdataproduct.Location = new System.Drawing.Point(324, 112);
            this.dgdataproduct.MultiSelect = false;
            this.dgdataproduct.Name = "dgdataproduct";
            this.dgdataproduct.ReadOnly = true;
            this.dgdataproduct.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdataproduct.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgdataproduct.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgdataproduct.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            this.dgdataproduct.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgdataproduct.RowTemplate.Height = 30;
            this.dgdataproduct.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdataproduct.Size = new System.Drawing.Size(893, 384);
            this.dgdataproduct.TabIndex = 63;
            this.dgdataproduct.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgdataProduct_CellContentClick);
            this.dgdataproduct.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgdataProduct_CellMouseEnter);
            this.dgdataproduct.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgdataProduct_CellPainting);
            // 
            // txtsearchproduct
            // 
            this.txtsearchproduct.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtsearchproduct.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtsearchproduct.Location = new System.Drawing.Point(926, 36);
            this.txtsearchproduct.Name = "txtsearchproduct";
            this.txtsearchproduct.Size = new System.Drawing.Size(195, 21);
            this.txtsearchproduct.TabIndex = 62;
            // 
            // cbosearchproduct
            // 
            this.cbosearchproduct.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbosearchproduct.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbosearchproduct.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbosearchproduct.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbosearchproduct.FormattingEnabled = true;
            this.cbosearchproduct.Location = new System.Drawing.Point(715, 34);
            this.cbosearchproduct.Name = "cbosearchproduct";
            this.cbosearchproduct.Size = new System.Drawing.Size(194, 23);
            this.cbosearchproduct.TabIndex = 61;
            // 
            // btnclear
            // 
            this.btnclear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnclear.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnclear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnclear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnclear.Image = global::PharmacySystem.Properties.Resources.clear16;
            this.btnclear.Location = new System.Drawing.Point(1180, 36);
            this.btnclear.Name = "btnclear";
            this.btnclear.Size = new System.Drawing.Size(37, 21);
            this.btnclear.TabIndex = 59;
            this.btnclear.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnclear.UseVisualStyleBackColor = false;
            this.btnclear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnsearch
            // 
            this.btnsearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnsearch.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnsearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnsearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnsearch.Image = global::PharmacySystem.Properties.Resources.search16;
            this.btnsearch.Location = new System.Drawing.Point(1137, 36);
            this.btnsearch.Name = "btnsearch";
            this.btnsearch.Size = new System.Drawing.Size(37, 21);
            this.btnsearch.TabIndex = 60;
            this.btnsearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnsearch.UseVisualStyleBackColor = false;
            this.btnsearch.Click += new System.EventHandler(this.btnsearch_Click);
            // 
            // cbocategory
            // 
            this.cbocategory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbocategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbocategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbocategory.FormattingEnabled = true;
            this.cbocategory.Location = new System.Drawing.Point(35, 266);
            this.cbocategory.Name = "cbocategory";
            this.cbocategory.Size = new System.Drawing.Size(243, 23);
            this.cbocategory.TabIndex = 41;
            //
            // chkTaxAffected
            //
            this.chkTaxAffected.AutoSize = true;
            this.chkTaxAffected.Checked = true;
            this.chkTaxAffected.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTaxAffected.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkTaxAffected.Location = new System.Drawing.Point(35, 298);
            this.chkTaxAffected.Name = "chkTaxAffected";
            this.chkTaxAffected.Size = new System.Drawing.Size(100, 19);
            this.chkTaxAffected.TabIndex = 42;
            this.chkTaxAffected.Text = "Afecto a IVA";
            this.chkTaxAffected.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(631, 39);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(69, 15);
            this.label9.TabIndex = 38;
            this.label9.Text = "Buscar por:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(325, 32);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(159, 20);
            this.label10.TabIndex = 37;
            this.label10.Text = "Lista de Productos";
            // 
            // btnDeleteProduct
            // 
            this.btnDeleteProduct.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnDeleteProduct.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteProduct.Image = global::PharmacySystem.Properties.Resources.delete32;
            this.btnDeleteProduct.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnDeleteProduct.Location = new System.Drawing.Point(34, 411);
            this.btnDeleteProduct.Name = "btnDeleteProduct";
            this.btnDeleteProduct.Size = new System.Drawing.Size(244, 38);
            this.btnDeleteProduct.TabIndex = 35;
            this.btnDeleteProduct.Text = "Eliminar";
            this.btnDeleteProduct.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDeleteProduct.UseVisualStyleBackColor = false;
            this.btnDeleteProduct.Click += new System.EventHandler(this.btnDeleteProduct_Click);
            // 
            // btnCleanProduct
            // 
            this.btnCleanProduct.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnCleanProduct.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCleanProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCleanProduct.Image = global::PharmacySystem.Properties.Resources.clear32;
            this.btnCleanProduct.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCleanProduct.Location = new System.Drawing.Point(34, 367);
            this.btnCleanProduct.Name = "btnCleanProduct";
            this.btnCleanProduct.Size = new System.Drawing.Size(244, 38);
            this.btnCleanProduct.TabIndex = 34;
            this.btnCleanProduct.Text = "Limpiar";
            this.btnCleanProduct.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCleanProduct.UseVisualStyleBackColor = false;
            this.btnCleanProduct.Click += new System.EventHandler(this.btnCleanProduct_Click);
            // 
            // btnSaveProduct
            // 
            this.btnSaveProduct.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnSaveProduct.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveProduct.Image = global::PharmacySystem.Properties.Resources.save32;
            this.btnSaveProduct.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSaveProduct.Location = new System.Drawing.Point(34, 323);
            this.btnSaveProduct.Name = "btnSaveProduct";
            this.btnSaveProduct.Size = new System.Drawing.Size(244, 38);
            this.btnSaveProduct.TabIndex = 36;
            this.btnSaveProduct.Text = "Guardar";
            this.btnSaveProduct.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSaveProduct.UseVisualStyleBackColor = false;
            this.btnSaveProduct.Click += new System.EventHandler(this.btnSaveProduct_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(31, 247);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 15);
            this.label6.TabIndex = 29;
            this.label6.Text = "Categoría:";
            // 
            // txtdescriptionproduct
            // 
            this.txtdescriptionproduct.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtdescriptionproduct.Location = new System.Drawing.Point(34, 206);
            this.txtdescriptionproduct.Name = "txtdescriptionproduct";
            this.txtdescriptionproduct.Size = new System.Drawing.Size(244, 21);
            this.txtdescriptionproduct.TabIndex = 32;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(31, 188);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 15);
            this.label5.TabIndex = 28;
            this.label5.Text = "Descripción:";
            // 
            // txtnameproduct
            // 
            this.txtnameproduct.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtnameproduct.Location = new System.Drawing.Point(34, 149);
            this.txtnameproduct.Name = "txtnameproduct";
            this.txtnameproduct.Size = new System.Drawing.Size(244, 21);
            this.txtnameproduct.TabIndex = 31;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(31, 131);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 15);
            this.label4.TabIndex = 27;
            this.label4.Text = "Nombre:";
            // 
            // txtcodeproduct
            // 
            this.txtcodeproduct.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtcodeproduct.Location = new System.Drawing.Point(34, 95);
            this.txtcodeproduct.Name = "txtcodeproduct";
            this.txtcodeproduct.Size = new System.Drawing.Size(244, 21);
            this.txtcodeproduct.TabIndex = 30;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(31, 77);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 15);
            this.label7.TabIndex = 26;
            this.label7.Text = "Código:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(67, 30);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(168, 20);
            this.label8.TabIndex = 25;
            this.label8.Text = "Detalle de Producto";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Location = new System.Drawing.Point(313, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(916, 59);
            this.label3.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Location = new System.Drawing.Point(313, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(916, 416);
            this.label2.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(304, 507);
            this.label1.TabIndex = 0;
            // 
            // tabCategory
            // 
            this.tabCategory.BackColor = System.Drawing.Color.White;
            this.tabCategory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabCategory.Controls.Add(this.pblist2);
            this.tabCategory.Controls.Add(this.txtindexcategory);
            this.tabCategory.Controls.Add(this.txtidcategory);
            this.tabCategory.Controls.Add(this.dgdatacategory);
            this.tabCategory.Controls.Add(this.btnDeleteCategory);
            this.tabCategory.Controls.Add(this.btnCleanCategory);
            this.tabCategory.Controls.Add(this.btnSaveCategory);
            this.tabCategory.Controls.Add(this.txtdescriptioncategory);
            this.tabCategory.Controls.Add(this.label11);
            this.tabCategory.Controls.Add(this.label12);
            this.tabCategory.Controls.Add(this.label13);
            this.tabCategory.Controls.Add(this.label14);
            this.tabCategory.Controls.Add(this.label16);
            this.tabCategory.Controls.Add(this.label17);
            this.tabCategory.Location = new System.Drawing.Point(4, 25);
            this.tabCategory.Name = "tabCategory";
            this.tabCategory.Padding = new System.Windows.Forms.Padding(3);
            this.tabCategory.Size = new System.Drawing.Size(1237, 515);
            this.tabCategory.TabIndex = 1;
            this.tabCategory.Text = "Categorias";
            // 
            // pblist2
            // 
            this.pblist2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            this.pblist2.Image = global::PharmacySystem.Properties.Resources.listaicon;
            this.pblist2.Location = new System.Drawing.Point(496, 20);
            this.pblist2.Name = "pblist2";
            this.pblist2.Size = new System.Drawing.Size(32, 32);
            this.pblist2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pblist2.TabIndex = 61;
            this.pblist2.TabStop = false;
            // 
            // txtindexcategory
            // 
            this.txtindexcategory.Location = new System.Drawing.Point(221, 57);
            this.txtindexcategory.Name = "txtindexcategory";
            this.txtindexcategory.Size = new System.Drawing.Size(22, 23);
            this.txtindexcategory.TabIndex = 59;
            this.txtindexcategory.Text = "0";
            this.txtindexcategory.Visible = false;
            // 
            // txtidcategory
            // 
            this.txtidcategory.Location = new System.Drawing.Point(247, 57);
            this.txtidcategory.Name = "txtidcategory";
            this.txtidcategory.Size = new System.Drawing.Size(22, 23);
            this.txtidcategory.TabIndex = 60;
            this.txtidcategory.Text = "0";
            this.txtidcategory.Visible = false;
            // 
            // dgdatacategory
            // 
            this.dgdatacategory.AllowUserToAddRows = false;
            this.dgdatacategory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgdatacategory.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgdatacategory.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(1);
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdatacategory.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgdatacategory.ColumnHeadersHeight = 30;
            this.dgdatacategory.EnableHeadersVisualStyles = false;
            this.dgdatacategory.GridColor = System.Drawing.Color.DimGray;
            this.dgdatacategory.Location = new System.Drawing.Point(325, 109);
            this.dgdatacategory.MultiSelect = false;
            this.dgdatacategory.Name = "dgdatacategory";
            this.dgdatacategory.ReadOnly = true;
            this.dgdatacategory.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdatacategory.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgdatacategory.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgdatacategory.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            this.dgdatacategory.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgdatacategory.RowTemplate.Height = 30;
            this.dgdatacategory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdatacategory.Size = new System.Drawing.Size(889, 387);
            this.dgdatacategory.TabIndex = 58;
            this.dgdatacategory.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgdataCategory_CellContentClick);
            this.dgdatacategory.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgdataCategory_CellMouseEnter);
            this.dgdatacategory.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgdataCategory_CellPainting);
            // 
            // btnDeleteCategory
            // 
            this.btnDeleteCategory.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnDeleteCategory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteCategory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteCategory.Image = global::PharmacySystem.Properties.Resources.delete32;
            this.btnDeleteCategory.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnDeleteCategory.Location = new System.Drawing.Point(26, 217);
            this.btnDeleteCategory.Name = "btnDeleteCategory";
            this.btnDeleteCategory.Size = new System.Drawing.Size(244, 38);
            this.btnDeleteCategory.TabIndex = 56;
            this.btnDeleteCategory.Text = "Eliminar";
            this.btnDeleteCategory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDeleteCategory.UseVisualStyleBackColor = false;
            this.btnDeleteCategory.Click += new System.EventHandler(this.btnDeleteCategory_Click);
            // 
            // btnCleanCategory
            // 
            this.btnCleanCategory.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnCleanCategory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCleanCategory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCleanCategory.Image = global::PharmacySystem.Properties.Resources.clear32;
            this.btnCleanCategory.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCleanCategory.Location = new System.Drawing.Point(26, 173);
            this.btnCleanCategory.Name = "btnCleanCategory";
            this.btnCleanCategory.Size = new System.Drawing.Size(244, 38);
            this.btnCleanCategory.TabIndex = 55;
            this.btnCleanCategory.Text = "Limpiar";
            this.btnCleanCategory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCleanCategory.UseVisualStyleBackColor = false;
            this.btnCleanCategory.Click += new System.EventHandler(this.btnCleanCategory_Click);
            // 
            // btnSaveCategory
            // 
            this.btnSaveCategory.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnSaveCategory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveCategory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveCategory.Image = global::PharmacySystem.Properties.Resources.save32;
            this.btnSaveCategory.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSaveCategory.Location = new System.Drawing.Point(26, 129);
            this.btnSaveCategory.Name = "btnSaveCategory";
            this.btnSaveCategory.Size = new System.Drawing.Size(244, 38);
            this.btnSaveCategory.TabIndex = 57;
            this.btnSaveCategory.Text = "Guardar";
            this.btnSaveCategory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSaveCategory.UseVisualStyleBackColor = false;
            this.btnSaveCategory.Click += new System.EventHandler(this.btnSaveCategory_Click);
            // 
            // txtdescriptioncategory
            // 
            this.txtdescriptioncategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtdescriptioncategory.Location = new System.Drawing.Point(26, 83);
            this.txtdescriptioncategory.Name = "txtdescriptioncategory";
            this.txtdescriptioncategory.Size = new System.Drawing.Size(244, 21);
            this.txtdescriptioncategory.TabIndex = 47;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(23, 65);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(75, 15);
            this.label11.TabIndex = 46;
            this.label11.Text = "Descripción:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(59, 18);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(174, 20);
            this.label12.TabIndex = 45;
            this.label12.Text = "Detalle de Categoría";
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label13.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label13.Location = new System.Drawing.Point(312, 94);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(916, 416);
            this.label13.TabIndex = 42;
            // 
            // label14
            // 
            this.label14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label14.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label14.Dock = System.Windows.Forms.DockStyle.Left;
            this.label14.Location = new System.Drawing.Point(3, 3);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(287, 507);
            this.label14.TabIndex = 41;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(325, 32);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(165, 20);
            this.label16.TabIndex = 51;
            this.label16.Text = "Lista de Categorías";
            // 
            // label17
            // 
            this.label17.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label17.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label17.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label17.Location = new System.Drawing.Point(312, 15);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(916, 59);
            this.label17.TabIndex = 43;
            // 
            // tabStore
            // 
            this.tabStore.BackColor = System.Drawing.Color.White;
            this.tabStore.Controls.Add(this.txtaddress);
            this.tabStore.Controls.Add(this.label23);
            this.tabStore.Controls.Add(this.btnSaveStore);
            this.tabStore.Controls.Add(this.label22);
            this.tabStore.Controls.Add(this.label21);
            this.tabStore.Controls.Add(this.txtphone);
            this.tabStore.Controls.Add(this.txtemail);
            this.tabStore.Controls.Add(this.label18);
            this.tabStore.Controls.Add(this.txtlegalName);
            this.tabStore.Controls.Add(this.label19);
            this.tabStore.Controls.Add(this.txttaxid);
            this.tabStore.Controls.Add(this.label20);
            this.tabStore.Controls.Add(this.cbocurrency);
            this.tabStore.Controls.Add(this.label24);
            this.tabStore.Controls.Add(this.txttaxrate);
            this.tabStore.Controls.Add(this.lbltaxrate);
            this.tabStore.Controls.Add(this.label15);
            this.tabStore.Location = new System.Drawing.Point(4, 25);
            this.tabStore.Name = "tabStore";
            this.tabStore.Padding = new System.Windows.Forms.Padding(3);
            this.tabStore.Size = new System.Drawing.Size(1237, 515);
            this.tabStore.TabIndex = 2;
            this.tabStore.Text = "Tienda";
            // 
            // txtaddress
            // 
            this.txtaddress.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.txtaddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtaddress.Location = new System.Drawing.Point(513, 364);
            this.txtaddress.Name = "txtaddress";
            this.txtaddress.Size = new System.Drawing.Size(247, 21);
            this.txtaddress.TabIndex = 60;
            this.txtaddress.Tag = "Dirección";
            // 
            // label23
            // 
            this.label23.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label23.AutoSize = true;
            this.label23.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label23.Location = new System.Drawing.Point(510, 337);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(59, 15);
            this.label23.TabIndex = 59;
            this.label23.Text = "Dirección";
            //
            // label24
            //
            this.label24.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label24.AutoSize = true;
            this.label24.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label24.Location = new System.Drawing.Point(510, 397);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(55, 15);
            this.label24.TabIndex = 61;
            this.label24.Text = "Moneda:";
            //
            // cbocurrency
            //
            this.cbocurrency.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cbocurrency.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbocurrency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbocurrency.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbocurrency.FormattingEnabled = true;
            this.cbocurrency.Location = new System.Drawing.Point(513, 415);
            this.cbocurrency.Name = "cbocurrency";
            this.cbocurrency.Size = new System.Drawing.Size(247, 23);
            this.cbocurrency.TabIndex = 62;
            //
            // lbltaxrate
            //
            this.lbltaxrate.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lbltaxrate.AutoSize = true;
            this.lbltaxrate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lbltaxrate.Location = new System.Drawing.Point(510, 448);
            this.lbltaxrate.Name = "lbltaxrate";
            this.lbltaxrate.Size = new System.Drawing.Size(80, 15);
            this.lbltaxrate.TabIndex = 63;
            this.lbltaxrate.Text = "Tasa IVA (%)";
            //
            // txttaxrate
            //
            this.txttaxrate.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.txttaxrate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txttaxrate.Location = new System.Drawing.Point(513, 466);
            this.txttaxrate.Name = "txttaxrate";
            this.txttaxrate.Size = new System.Drawing.Size(80, 21);
            this.txttaxrate.TabIndex = 64;
            //
            // btnSaveStore
            //
            this.btnSaveStore.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnSaveStore.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnSaveStore.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveStore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveStore.Image = global::PharmacySystem.Properties.Resources.save32;
            this.btnSaveStore.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSaveStore.Location = new System.Drawing.Point(513, 458);
            this.btnSaveStore.Name = "btnSaveStore";
            this.btnSaveStore.Size = new System.Drawing.Size(247, 38);
            this.btnSaveStore.TabIndex = 58;
            this.btnSaveStore.Text = "Guardar";
            this.btnSaveStore.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSaveStore.UseVisualStyleBackColor = false;
            this.btnSaveStore.Click += new System.EventHandler(this.btnSaveStore_Click);
            // 
            // label22
            // 
            this.label22.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label22.AutoSize = true;
            this.label22.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.Location = new System.Drawing.Point(509, 61);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(173, 20);
            this.label22.TabIndex = 34;
            this.label22.Text = "Detalle de mi Tienda";
            // 
            // label21
            // 
            this.label21.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label21.AutoSize = true;
            this.label21.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(510, 276);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(58, 15);
            this.label21.TabIndex = 33;
            this.label21.Text = "Teléfono:";
            // 
            // txtphone
            // 
            this.txtphone.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.txtphone.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtphone.Location = new System.Drawing.Point(513, 304);
            this.txtphone.Name = "txtphone";
            this.txtphone.Size = new System.Drawing.Size(247, 21);
            this.txtphone.TabIndex = 31;
            this.txtphone.Tag = "Teléfono";
            // 
            // txtemail
            // 
            this.txtemail.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.txtemail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtemail.Location = new System.Drawing.Point(513, 242);
            this.txtemail.Name = "txtemail";
            this.txtemail.Size = new System.Drawing.Size(247, 21);
            this.txtemail.TabIndex = 30;
            this.txtemail.Tag = "Correo";
            // 
            // label18
            // 
            this.label18.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label18.AutoSize = true;
            this.label18.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(510, 215);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(47, 15);
            this.label18.TabIndex = 27;
            this.label18.Text = "Correo:";
            // 
            // txtlegalName
            // 
            this.txtlegalName.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.txtlegalName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtlegalName.Location = new System.Drawing.Point(513, 182);
            this.txtlegalName.Name = "txtlegalName";
            this.txtlegalName.Size = new System.Drawing.Size(247, 21);
            this.txtlegalName.TabIndex = 29;
            this.txtlegalName.Tag = "Razón Social";
            // 
            // label19
            // 
            this.label19.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label19.AutoSize = true;
            this.label19.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(510, 155);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(83, 15);
            this.label19.TabIndex = 26;
            this.label19.Text = "Razón Social:";
            // 
            // txttaxid
            // 
            this.txttaxid.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.txttaxid.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txttaxid.Location = new System.Drawing.Point(513, 121);
            this.txttaxid.Name = "txttaxid";
            this.txttaxid.Size = new System.Drawing.Size(247, 21);
            this.txttaxid.TabIndex = 28;
            this.txttaxid.Tag = "Número Documento";
            // 
            // label20
            // 
            this.label20.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label20.AutoSize = true;
            this.label20.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.Location = new System.Drawing.Point(510, 91);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(122, 15);
            this.label20.TabIndex = 25;
            this.label20.Text = "Número Documento:";
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.label15.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label15.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label15.Location = new System.Drawing.Point(333, 15);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(578, 479);
            this.label15.TabIndex = 0;
            // 
            // frmManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(1269, 568);
            this.ControlBox = false;
            this.Controls.Add(this.tabManagement);
            this.Name = "frmManagement";
            this.Text = "Gestion";
            this.Load += new System.EventHandler(this.frmManagement_Load);
            this.tabManagement.ResumeLayout(false);
            this.tabProduct.ResumeLayout(false);
            this.tabProduct.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pblist)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgdataproduct)).EndInit();
            this.tabCategory.ResumeLayout(false);
            this.tabCategory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pblist2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgdatacategory)).EndInit();
            this.tabStore.ResumeLayout(false);
            this.tabStore.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabManagement;
        private System.Windows.Forms.TabPage tabProduct;
        private System.Windows.Forms.TabPage tabCategory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnDeleteProduct;
        private System.Windows.Forms.Button btnCleanProduct;
        private System.Windows.Forms.Button btnSaveProduct;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtdescriptionproduct;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtnameproduct;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtcodeproduct;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbocategory;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtdescriptioncategory;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Button btnDeleteCategory;
        private System.Windows.Forms.Button btnCleanCategory;
        private System.Windows.Forms.Button btnSaveCategory;
        private System.Windows.Forms.TabPage tabStore;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtphone;
        private System.Windows.Forms.TextBox txtemail;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txtlegalName;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox txttaxid;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.ComboBox cbocurrency;
        private System.Windows.Forms.TextBox txttaxrate;
        private System.Windows.Forms.Label lbltaxrate;
        private System.Windows.Forms.CheckBox chkTaxAffected;
        private System.Windows.Forms.Button btnSaveStore;
        private System.Windows.Forms.DataGridView dgdatacategory;
        private System.Windows.Forms.TextBox txtindexcategory;
        private System.Windows.Forms.TextBox txtidcategory;
        private System.Windows.Forms.TextBox txtsearchproduct;
        private System.Windows.Forms.ComboBox cbosearchproduct;
        private System.Windows.Forms.Button btnclear;
        private System.Windows.Forms.Button btnsearch;
        private System.Windows.Forms.DataGridView dgdataproduct;
        private System.Windows.Forms.TextBox txtindexproduct;
        private System.Windows.Forms.TextBox txtidproduct;
        private System.Windows.Forms.PictureBox pblist;
        private System.Windows.Forms.PictureBox pblist2;
        private System.Windows.Forms.TextBox txtaddress;
        private System.Windows.Forms.Label label23;
    }
}