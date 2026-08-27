namespace PharmacySystem
{
    partial class frmRoles
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
            this.lblRoles = new System.Windows.Forms.Label();
            this.lstRoles = new System.Windows.Forms.ListBox();
            this.txtRoleName = new System.Windows.Forms.TextBox();
            this.btnNewRole = new System.Windows.Forms.Button();
            this.btnRenameRole = new System.Windows.Forms.Button();
            this.btnDeleteRole = new System.Windows.Forms.Button();
            this.lblPermissions = new System.Windows.Forms.Label();
            this.tvPermissions = new System.Windows.Forms.TreeView();
            this.btnSavePermissions = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblRoles
            //
            this.lblRoles.AutoSize = true;
            this.lblRoles.Location = new System.Drawing.Point(12, 12);
            this.lblRoles.Name = "lblRoles";
            this.lblRoles.Size = new System.Drawing.Size(37, 13);
            this.lblRoles.TabIndex = 0;
            this.lblRoles.Text = "Roles";
            //
            // lstRoles
            //
            this.lstRoles.FormattingEnabled = true;
            this.lstRoles.IntegralHeight = false;
            this.lstRoles.Location = new System.Drawing.Point(12, 30);
            this.lstRoles.Name = "lstRoles";
            this.lstRoles.Size = new System.Drawing.Size(210, 300);
            this.lstRoles.TabIndex = 1;
            this.lstRoles.SelectedIndexChanged += new System.EventHandler(this.lstRoles_SelectedIndexChanged);
            //
            // txtRoleName
            //
            this.txtRoleName.Location = new System.Drawing.Point(12, 338);
            this.txtRoleName.Name = "txtRoleName";
            this.txtRoleName.Size = new System.Drawing.Size(210, 20);
            this.txtRoleName.TabIndex = 2;
            //
            // btnNewRole
            //
            this.btnNewRole.Location = new System.Drawing.Point(12, 366);
            this.btnNewRole.Name = "btnNewRole";
            this.btnNewRole.Size = new System.Drawing.Size(66, 26);
            this.btnNewRole.TabIndex = 3;
            this.btnNewRole.Text = "Nuevo";
            this.btnNewRole.UseVisualStyleBackColor = true;
            this.btnNewRole.Click += new System.EventHandler(this.btnNewRole_Click);
            //
            // btnRenameRole
            //
            this.btnRenameRole.Location = new System.Drawing.Point(84, 366);
            this.btnRenameRole.Name = "btnRenameRole";
            this.btnRenameRole.Size = new System.Drawing.Size(72, 26);
            this.btnRenameRole.TabIndex = 4;
            this.btnRenameRole.Text = "Renombrar";
            this.btnRenameRole.UseVisualStyleBackColor = true;
            this.btnRenameRole.Click += new System.EventHandler(this.btnRenameRole_Click);
            //
            // btnDeleteRole
            //
            this.btnDeleteRole.Location = new System.Drawing.Point(162, 366);
            this.btnDeleteRole.Name = "btnDeleteRole";
            this.btnDeleteRole.Size = new System.Drawing.Size(60, 26);
            this.btnDeleteRole.TabIndex = 5;
            this.btnDeleteRole.Text = "Eliminar";
            this.btnDeleteRole.UseVisualStyleBackColor = true;
            this.btnDeleteRole.Click += new System.EventHandler(this.btnDeleteRole_Click);
            //
            // lblPermissions
            //
            this.lblPermissions.AutoSize = true;
            this.lblPermissions.Location = new System.Drawing.Point(240, 12);
            this.lblPermissions.Name = "lblPermissions";
            this.lblPermissions.Size = new System.Drawing.Size(112, 13);
            this.lblPermissions.TabIndex = 6;
            this.lblPermissions.Text = "Permisos del rol";
            //
            // tvPermissions
            //
            this.tvPermissions.CheckBoxes = true;
            this.tvPermissions.Enabled = false;
            this.tvPermissions.FullRowSelect = true;
            this.tvPermissions.HideSelection = false;
            this.tvPermissions.Location = new System.Drawing.Point(243, 30);
            this.tvPermissions.Name = "tvPermissions";
            this.tvPermissions.ShowRootLines = false;
            this.tvPermissions.Size = new System.Drawing.Size(300, 300);
            this.tvPermissions.TabIndex = 7;
            this.tvPermissions.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvPermissions_AfterCheck);
            //
            // btnSavePermissions
            //
            this.btnSavePermissions.Enabled = false;
            this.btnSavePermissions.Location = new System.Drawing.Point(243, 338);
            this.btnSavePermissions.Name = "btnSavePermissions";
            this.btnSavePermissions.Size = new System.Drawing.Size(150, 30);
            this.btnSavePermissions.TabIndex = 8;
            this.btnSavePermissions.Text = "Guardar permisos";
            this.btnSavePermissions.UseVisualStyleBackColor = true;
            this.btnSavePermissions.Click += new System.EventHandler(this.btnSavePermissions_Click);
            //
            // frmRoles
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(555, 404);
            this.Controls.Add(this.btnSavePermissions);
            this.Controls.Add(this.tvPermissions);
            this.Controls.Add(this.lblPermissions);
            this.Controls.Add(this.btnDeleteRole);
            this.Controls.Add(this.btnRenameRole);
            this.Controls.Add(this.btnNewRole);
            this.Controls.Add(this.txtRoleName);
            this.Controls.Add(this.lstRoles);
            this.Controls.Add(this.lblRoles);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmRoles";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Roles y permisos";
            this.Load += new System.EventHandler(this.frmRoles_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblRoles;
        private System.Windows.Forms.ListBox lstRoles;
        private System.Windows.Forms.TextBox txtRoleName;
        private System.Windows.Forms.Button btnNewRole;
        private System.Windows.Forms.Button btnRenameRole;
        private System.Windows.Forms.Button btnDeleteRole;
        private System.Windows.Forms.Label lblPermissions;
        private System.Windows.Forms.TreeView tvPermissions;
        private System.Windows.Forms.Button btnSavePermissions;
    }
}
