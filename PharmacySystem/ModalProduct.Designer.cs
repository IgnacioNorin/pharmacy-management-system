namespace PharmacySystem
{
    partial class ModalProduct
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgdataproduct = new System.Windows.Forms.DataGridView();
            this.txtseachproduct = new System.Windows.Forms.TextBox();
            this.cbosearchproduct = new System.Windows.Forms.ComboBox();
            this.btnclear = new System.Windows.Forms.Button();
            this.btnsearch = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pblist = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgdataproduct)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pblist)).BeginInit();
            this.SuspendLayout();
            // 
            // dgdataproduct
            // 
            this.dgdataproduct.AllowUserToAddRows = false;
            this.dgdataproduct.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(1);
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdataproduct.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgdataproduct.ColumnHeadersHeight = 30;
            this.dgdataproduct.EnableHeadersVisualStyles = false;
            this.dgdataproduct.GridColor = System.Drawing.Color.DimGray;
            this.dgdataproduct.Location = new System.Drawing.Point(22, 111);
            this.dgdataproduct.MultiSelect = false;
            this.dgdataproduct.Name = "dgdataproduct";
            this.dgdataproduct.ReadOnly = true;
            this.dgdataproduct.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdataproduct.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgdataproduct.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgdataproduct.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            this.dgdataproduct.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgdataproduct.RowTemplate.Height = 30;
            this.dgdataproduct.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdataproduct.Size = new System.Drawing.Size(785, 340);
            this.dgdataproduct.TabIndex = 72;
            this.dgdataproduct.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgdataproduct_CellContentClick);
            this.dgdataproduct.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgdataproduct_CellMouseEnter);
            this.dgdataproduct.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgdataproduct_CellPainting);
            // 
            // txtseachproduct
            // 
            this.txtseachproduct.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtseachproduct.Location = new System.Drawing.Point(317, 50);
            this.txtseachproduct.Name = "txtseachproduct";
            this.txtseachproduct.Size = new System.Drawing.Size(195, 21);
            this.txtseachproduct.TabIndex = 71;
            // 
            // cbosearchproduct
            // 
            this.cbosearchproduct.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbosearchproduct.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbosearchproduct.FormattingEnabled = true;
            this.cbosearchproduct.Location = new System.Drawing.Point(106, 48);
            this.cbosearchproduct.Name = "cbosearchproduct";
            this.cbosearchproduct.Size = new System.Drawing.Size(194, 23);
            this.cbosearchproduct.TabIndex = 70;
            // 
            // btnclear
            // 
            this.btnclear.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnclear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnclear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnclear.Image = global::PharmacySystem.Properties.Resources.clear16;
            this.btnclear.Location = new System.Drawing.Point(571, 50);
            this.btnclear.Name = "btnclear";
            this.btnclear.Size = new System.Drawing.Size(37, 21);
            this.btnclear.TabIndex = 68;
            this.btnclear.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnclear.UseVisualStyleBackColor = false;
            this.btnclear.Click += new System.EventHandler(this.btnclear_Click);
            // 
            // btnsearch
            // 
            this.btnsearch.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnsearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnsearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnsearch.Image = global::PharmacySystem.Properties.Resources.search16;
            this.btnsearch.Location = new System.Drawing.Point(528, 50);
            this.btnsearch.Name = "btnsearch";
            this.btnsearch.Size = new System.Drawing.Size(37, 21);
            this.btnsearch.TabIndex = 69;
            this.btnsearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnsearch.UseVisualStyleBackColor = false;
            this.btnsearch.Click += new System.EventHandler(this.btnsearch_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(22, 53);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(69, 15);
            this.label9.TabIndex = 67;
            this.label9.Text = "Buscar por:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(20, 18);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(159, 20);
            this.label10.TabIndex = 66;
            this.label10.Text = "Lista de Productos";
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Location = new System.Drawing.Point(11, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(808, 79);
            this.label3.TabIndex = 65;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(248)))));
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Location = new System.Drawing.Point(11, 98);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(808, 365);
            this.label2.TabIndex = 64;
            // 
            // pblist
            // 
            this.pblist.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(241)))), ((int)(((byte)(244)))));
            this.pblist.Image = global::PharmacySystem.Properties.Resources.listaicon;
            this.pblist.Location = new System.Drawing.Point(185, 12);
            this.pblist.Name = "pblist";
            this.pblist.Size = new System.Drawing.Size(32, 32);
            this.pblist.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pblist.TabIndex = 73;
            this.pblist.TabStop = false;
            // 
            // ModalProduct
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(829, 473);
            this.Controls.Add(this.pblist);
            this.Controls.Add(this.dgdataproduct);
            this.Controls.Add(this.txtseachproduct);
            this.Controls.Add(this.cbosearchproduct);
            this.Controls.Add(this.btnclear);
            this.Controls.Add(this.btnsearch);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModalProduct";
            this.Text = "Producto";
            this.Load += new System.EventHandler(this.ModalProducto_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgdataproduct)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pblist)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgdataproduct;
        private System.Windows.Forms.TextBox txtseachproduct;
        private System.Windows.Forms.ComboBox cbosearchproduct;
        private System.Windows.Forms.Button btnclear;
        private System.Windows.Forms.Button btnsearch;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pblist;
    }
}