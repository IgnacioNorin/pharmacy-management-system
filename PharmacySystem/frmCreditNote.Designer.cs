namespace PharmacySystem
{
    partial class frmCreditNote
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
            this.lblType = new System.Windows.Forms.Label();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.lblNumber = new System.Windows.Forms.Label();
            this.txtNumber = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.lblDetail = new System.Windows.Forms.Label();
            this.lblReason = new System.Windows.Forms.Label();
            this.txtReason = new System.Windows.Forms.TextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblType
            //
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(12, 15);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(80, 15);
            this.lblType.Text = "Tipo documento";
            //
            // cboType
            //
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Location = new System.Drawing.Point(120, 12);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(140, 23);
            this.cboType.TabIndex = 0;
            //
            // lblNumber
            //
            this.lblNumber.AutoSize = true;
            this.lblNumber.Location = new System.Drawing.Point(12, 47);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(55, 15);
            this.lblNumber.Text = "N° compr.";
            //
            // txtNumber
            //
            this.txtNumber.Location = new System.Drawing.Point(120, 44);
            this.txtNumber.Name = "txtNumber";
            this.txtNumber.Size = new System.Drawing.Size(140, 21);
            this.txtNumber.TabIndex = 1;
            //
            // btnSearch
            //
            this.btnSearch.Location = new System.Drawing.Point(270, 42);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(90, 26);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Buscar";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            //
            // lblDetail
            //
            this.lblDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDetail.Location = new System.Drawing.Point(12, 82);
            this.lblDetail.Name = "lblDetail";
            this.lblDetail.Size = new System.Drawing.Size(348, 90);
            this.lblDetail.TabIndex = 3;
            this.lblDetail.Text = "Sin comprobante seleccionado.";
            //
            // lblReason
            //
            this.lblReason.AutoSize = true;
            this.lblReason.Location = new System.Drawing.Point(12, 185);
            this.lblReason.Name = "lblReason";
            this.lblReason.Size = new System.Drawing.Size(45, 15);
            this.lblReason.Text = "Motivo";
            //
            // txtReason
            //
            this.txtReason.Location = new System.Drawing.Point(12, 203);
            this.txtReason.Multiline = true;
            this.txtReason.Name = "txtReason";
            this.txtReason.Size = new System.Drawing.Size(348, 50);
            this.txtReason.TabIndex = 4;
            //
            // btnGenerate
            //
            this.btnGenerate.Enabled = false;
            this.btnGenerate.Location = new System.Drawing.Point(12, 263);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(200, 32);
            this.btnGenerate.TabIndex = 5;
            this.btnGenerate.Text = "Emitir nota de crédito";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            //
            // btnClose
            //
            this.btnClose.Location = new System.Drawing.Point(270, 263);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 32);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Cerrar";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // frmCreditNote
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 309);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.txtReason);
            this.Controls.Add(this.lblReason);
            this.Controls.Add(this.lblDetail);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtNumber);
            this.Controls.Add(this.lblNumber);
            this.Controls.Add(this.cboType);
            this.Controls.Add(this.lblType);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCreditNote";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Nota de crédito";
            this.Load += new System.EventHandler(this.frmCreditNote_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.TextBox txtNumber;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Label lblDetail;
        private System.Windows.Forms.Label lblReason;
        private System.Windows.Forms.TextBox txtReason;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnClose;
    }
}
