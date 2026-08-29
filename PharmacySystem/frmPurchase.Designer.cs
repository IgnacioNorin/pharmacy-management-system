namespace PharmacySystem
{
    partial class frmPurchase
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label10 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtcodeproduct = new System.Windows.Forms.TextBox();
            this.txtdocumentsupplier = new System.Windows.Forms.TextBox();
            this.txtnamesupplier = new System.Windows.Forms.TextBox();
            this.txtnameproduct = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.txtpricepurchase = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtnumberdocument = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.cbotypedocument = new System.Windows.Forms.ComboBox();
            this.txtamount = new System.Windows.Forms.NumericUpDown();
            this.txtidsupplier = new System.Windows.Forms.TextBox();
            this.txtidproduct = new System.Windows.Forms.TextBox();
            this.dgdata = new System.Windows.Forms.DataGridView();
            this.btnSearchProduct = new System.Windows.Forms.Button();
            this.btnSearchSupplier = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnFinishPurchase = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.lbltotalamount = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.DTPexpireddate = new System.Windows.Forms.DateTimePicker();
            this.pblist = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.txtamount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgdata)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pblist)).BeginInit();
            this.SuspendLayout();
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label10.Location = new System.Drawing.Point(318, 30);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(938, 62);
            this.label10.TabIndex = 48;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(292, 568);
            this.label1.TabIndex = 29;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(17, 226);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.label2.Size = new System.Drawing.Size(143, 25);
            this.label2.TabIndex = 0;
            this.label2.Text = "Detalle Producto";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(17, 19);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(133, 20);
            this.label11.TabIndex = 0;
            this.label11.Text = "Detalle Compra";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(18, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "Tipo Documento:";
            // 
            // txtcodeproduct
            // 
            this.txtcodeproduct.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtcodeproduct.Location = new System.Drawing.Point(21, 272);
            this.txtcodeproduct.Name = "txtcodeproduct";
            this.txtcodeproduct.Size = new System.Drawing.Size(201, 21);
            this.txtcodeproduct.TabIndex = 6;
            this.txtcodeproduct.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCodeProduct_KeyDown);
            // 
            // txtdocumentsupplier
            // 
            this.txtdocumentsupplier.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtdocumentsupplier.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtdocumentsupplier.Location = new System.Drawing.Point(21, 157);
            this.txtdocumentsupplier.Name = "txtdocumentsupplier";
            this.txtdocumentsupplier.ReadOnly = true;
            this.txtdocumentsupplier.Size = new System.Drawing.Size(201, 21);
            this.txtdocumentsupplier.TabIndex = 3;
            // 
            // txtnamesupplier
            // 
            this.txtnamesupplier.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtnamesupplier.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtnamesupplier.Location = new System.Drawing.Point(21, 202);
            this.txtnamesupplier.Name = "txtnamesupplier";
            this.txtnamesupplier.ReadOnly = true;
            this.txtnamesupplier.Size = new System.Drawing.Size(244, 21);
            this.txtnamesupplier.TabIndex = 5;
            // 
            // txtnameproduct
            // 
            this.txtnameproduct.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtnameproduct.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtnameproduct.Location = new System.Drawing.Point(21, 314);
            this.txtnameproduct.Name = "txtnameproduct";
            this.txtnameproduct.ReadOnly = true;
            this.txtnameproduct.Size = new System.Drawing.Size(244, 21);
            this.txtnameproduct.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(18, 338);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 15);
            this.label5.TabIndex = 0;
            this.label5.Text = "Cantidad:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(18, 379);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(92, 15);
            this.label12.TabIndex = 0;
            this.label12.Text = "Precio Compra:";
            // 
            // txtpricepurchase
            // 
            this.txtpricepurchase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtpricepurchase.Location = new System.Drawing.Point(21, 397);
            this.txtpricepurchase.Name = "txtpricepurchase";
            this.txtpricepurchase.Size = new System.Drawing.Size(244, 21);
            this.txtpricepurchase.TabIndex = 10;
            this.txtpricepurchase.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPricePurchase_KeyPress);
            //
            // label6
            //
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(18, 95);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(122, 15);
            this.label6.TabIndex = 0;
            this.label6.Text = "Número Documento:";
            // 
            // txtnumberdocument
            // 
            this.txtnumberdocument.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtnumberdocument.Location = new System.Drawing.Point(21, 112);
            this.txtnumberdocument.Name = "txtnumberdocument";
            this.txtnumberdocument.Size = new System.Drawing.Size(244, 21);
            this.txtnumberdocument.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label7.Location = new System.Drawing.Point(318, 99);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(938, 455);
            this.label7.TabIndex = 47;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(328, 52);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(177, 20);
            this.label9.TabIndex = 0;
            this.label9.Text = "Resumen de Compra";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(18, 251);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "Código Producto:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(18, 139);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(168, 15);
            this.label14.TabIndex = 0;
            this.label14.Text = "Documento Proveedor:";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(18, 184);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(196, 15);
            this.label15.TabIndex = 0;
            this.label15.Text = "Nombre / Razón Social Proveedor:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(18, 296);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(107, 15);
            this.label8.TabIndex = 0;
            this.label8.Text = "Nombre Producto:";
            // 
            // cbotypedocument
            // 
            this.cbotypedocument.BackColor = System.Drawing.Color.White;
            this.cbotypedocument.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbotypedocument.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbotypedocument.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbotypedocument.FormattingEnabled = true;
            this.cbotypedocument.Location = new System.Drawing.Point(21, 66);
            this.cbotypedocument.Name = "cbotypedocument";
            this.cbotypedocument.Size = new System.Drawing.Size(244, 23);
            this.cbotypedocument.TabIndex = 1;
            // 
            // txtamount
            // 
            this.txtamount.Location = new System.Drawing.Point(21, 356);
            this.txtamount.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.txtamount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtamount.Name = "txtamount";
            this.txtamount.Size = new System.Drawing.Size(244, 20);
            this.txtamount.TabIndex = 9;
            this.txtamount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // txtidsupplier
            // 
            this.txtidsupplier.Location = new System.Drawing.Point(192, 134);
            this.txtidsupplier.Name = "txtidsupplier";
            this.txtidsupplier.Size = new System.Drawing.Size(16, 20);
            this.txtidsupplier.TabIndex = 0;
            this.txtidsupplier.Text = "0";
            this.txtidsupplier.Visible = false;
            // 
            // txtidproduct
            // 
            this.txtidproduct.Location = new System.Drawing.Point(206, 246);
            this.txtidproduct.Name = "txtidproduct";
            this.txtidproduct.Size = new System.Drawing.Size(16, 20);
            this.txtidproduct.TabIndex = 0;
            this.txtidproduct.Text = "0";
            this.txtidproduct.Visible = false;
            // 
            // dgdata
            // 
            this.dgdata.AllowUserToAddRows = false;
            this.dgdata.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgdata.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgdata.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle7.Padding = new System.Windows.Forms.Padding(1);
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdata.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgdata.ColumnHeadersHeight = 30;
            this.dgdata.EnableHeadersVisualStyles = false;
            this.dgdata.GridColor = System.Drawing.Color.DimGray;
            this.dgdata.Location = new System.Drawing.Point(332, 112);
            this.dgdata.MultiSelect = false;
            this.dgdata.Name = "dgdata";
            this.dgdata.ReadOnly = true;
            this.dgdata.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdata.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
            this.dgdata.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgdata.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            this.dgdata.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgdata.RowTemplate.Height = 30;
            this.dgdata.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdata.Size = new System.Drawing.Size(914, 393);
            this.dgdata.TabIndex = 65;
            this.dgdata.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgdata_CellContentClick);
            this.dgdata.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgdata_CellMouseEnter);
            this.dgdata.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgdata_CellPainting);
            // 
            // btnSearchProduct
            // 
            this.btnSearchProduct.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnSearchProduct.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearchProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchProduct.Image = global::PharmacySystem.Properties.Resources.search16;
            this.btnSearchProduct.Location = new System.Drawing.Point(228, 272);
            this.btnSearchProduct.Name = "btnSearchProduct";
            this.btnSearchProduct.Size = new System.Drawing.Size(37, 21);
            this.btnSearchProduct.TabIndex = 7;
            this.btnSearchProduct.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSearchProduct.UseVisualStyleBackColor = false;
            this.btnSearchProduct.Click += new System.EventHandler(this.btnSearchProduct_Click);
            // 
            // btnSearchSupplier
            // 
            this.btnSearchSupplier.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnSearchSupplier.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearchSupplier.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchSupplier.Image = global::PharmacySystem.Properties.Resources.search16;
            this.btnSearchSupplier.Location = new System.Drawing.Point(228, 157);
            this.btnSearchSupplier.Name = "btnSearchSupplier";
            this.btnSearchSupplier.Size = new System.Drawing.Size(37, 21);
            this.btnSearchSupplier.TabIndex = 4;
            this.btnSearchSupplier.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSearchSupplier.UseVisualStyleBackColor = false;
            this.btnSearchSupplier.Click += new System.EventHandler(this.btnSearchSupplier_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Image = global::PharmacySystem.Properties.Resources.add24;
            this.btnAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAdd.Location = new System.Drawing.Point(21, 518);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(244, 38);
            this.btnAdd.TabIndex = 12;
            this.btnAdd.Text = "Agregar producto";
            this.btnAdd.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnFinishPurchase
            // 
            this.btnFinishPurchase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFinishPurchase.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnFinishPurchase.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFinishPurchase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFinishPurchase.Image = global::PharmacySystem.Properties.Resources.check24;
            this.btnFinishPurchase.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnFinishPurchase.Location = new System.Drawing.Point(1051, 44);
            this.btnFinishPurchase.Name = "btnFinishPurchase";
            this.btnFinishPurchase.Size = new System.Drawing.Size(195, 38);
            this.btnFinishPurchase.TabIndex = 13;
            this.btnFinishPurchase.Text = "Terminar Compra";
            this.btnFinishPurchase.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnFinishPurchase.UseVisualStyleBackColor = false;
            this.btnFinishPurchase.Click += new System.EventHandler(this.btnFinishPurchase_Click);
            // 
            // label16
            // 
            this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label16.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(1048, 521);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(99, 17);
            this.label16.TabIndex = 0;
            this.label16.Text = "Monto Total:";
            // 
            // lbltotalamount
            // 
            this.lbltotalamount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lbltotalamount.AutoSize = true;
            this.lbltotalamount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.lbltotalamount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbltotalamount.Location = new System.Drawing.Point(1153, 521);
            this.lbltotalamount.Name = "lbltotalamount";
            this.lbltotalamount.Size = new System.Drawing.Size(17, 17);
            this.lbltotalamount.TabIndex = 0;
            this.lbltotalamount.Text = "0";
            this.lbltotalamount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label17.Location = new System.Drawing.Point(21, 467);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(116, 13);
            this.label17.TabIndex = 66;
            this.label17.Text = "Fecha de Vencimiento:";
            // 
            // backgroundWorker1
            // 

            // 
            // DTPexpireddate
            // 
            this.DTPexpireddate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.DTPexpireddate.Location = new System.Drawing.Point(24, 485);
            this.DTPexpireddate.Name = "DTPexpireddate";
            this.DTPexpireddate.Size = new System.Drawing.Size(241, 20);
            this.DTPexpireddate.TabIndex = 67;
            // 
            // pblist
            // 
            this.pblist.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            this.pblist.Image = global::PharmacySystem.Properties.Resources.listaicon;
            this.pblist.Location = new System.Drawing.Point(511, 40);
            this.pblist.Name = "pblist";
            this.pblist.Size = new System.Drawing.Size(32, 32);
            this.pblist.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pblist.TabIndex = 68;
            this.pblist.TabStop = false;
            // 
            // frmPurchase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1269, 568);
            this.ControlBox = false;
            this.Controls.Add(this.pblist);
            this.Controls.Add(this.DTPexpireddate);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.dgdata);
            this.Controls.Add(this.txtidproduct);
            this.Controls.Add(this.txtidsupplier);
            this.Controls.Add(this.txtamount);
            this.Controls.Add(this.btnSearchProduct);
            this.Controls.Add(this.btnSearchSupplier);
            this.Controls.Add(this.cbotypedocument);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.lbltotalamount);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnFinishPurchase);
            this.Controls.Add(this.txtnumberdocument);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtpricepurchase);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtnameproduct);
            this.Controls.Add(this.txtnamesupplier);
            this.Controls.Add(this.txtdocumentsupplier);
            this.Controls.Add(this.txtcodeproduct);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label10);
            this.Name = "frmPurchase";
            this.Text = "Compra";
            this.Load += new System.EventHandler(this.frmPurchase_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txtamount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgdata)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pblist)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtcodeproduct;
        private System.Windows.Forms.TextBox txtdocumentsupplier;
        private System.Windows.Forms.TextBox txtnamesupplier;
        private System.Windows.Forms.TextBox txtnameproduct;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtpricepurchase;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtnumberdocument;
        private System.Windows.Forms.Button btnFinishPurchase;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbotypedocument;
        private System.Windows.Forms.Button btnSearchSupplier;
        private System.Windows.Forms.Button btnSearchProduct;
        private System.Windows.Forms.NumericUpDown txtamount;
        private System.Windows.Forms.TextBox txtidsupplier;
        private System.Windows.Forms.TextBox txtidproduct;
        private System.Windows.Forms.DataGridView dgdata;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label lbltotalamount;
        private System.Windows.Forms.Label label17;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.DateTimePicker DTPexpireddate;
        private System.Windows.Forms.PictureBox pblist;
    }
}