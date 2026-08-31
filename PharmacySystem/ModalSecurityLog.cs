using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem
{
    // "Bitácora" screen. Built entirely in code (no Designer file): a date range, a "Consultar"
    // button and a read-only grid of security_event rows, newest first. Read-only by design -
    // the audit trail is append-only and never edited from the app.
    public class ModalSecurityLog : Form, ISecurityLogView
    {
        private readonly SecurityLogPresenter _presenter;

        private readonly DateTimePicker _dtpFrom = new DateTimePicker();
        private readonly DateTimePicker _dtpTo = new DateTimePicker();
        private readonly DataGridView _grid = new DataGridView();
        private readonly Label _lblCount = new Label();

        public ModalSecurityLog()
        {
            BuildLayout();
            _presenter = CompositionRoot.CreateSecurityLogPresenter(this);
            Load += (s, e) => _presenter.OnConsult();
        }

        private void BuildLayout()
        {
            Text = "Bitácora de acciones";
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            ClientSize = new Size(900, 520);
            MinimumSize = new Size(680, 360);
            Font = new Font("Segoe UI", 9F);

            var lblFrom = new Label { Location = new Point(16, 18), AutoSize = true, Text = "Desde:" };
            _dtpFrom.Location = new Point(70, 14);
            _dtpFrom.Size = new Size(130, 24);
            _dtpFrom.Format = DateTimePickerFormat.Short;
            _dtpFrom.Value = DateTime.Today.AddDays(-30);

            var lblTo = new Label { Location = new Point(216, 18), AutoSize = true, Text = "Hasta:" };
            _dtpTo.Location = new Point(266, 14);
            _dtpTo.Size = new Size(130, 24);
            _dtpTo.Format = DateTimePickerFormat.Short;
            _dtpTo.Value = DateTime.Today;

            var btnConsult = new Button { Location = new Point(414, 13), Size = new Size(110, 26), Text = "Consultar" };
            btnConsult.Click += (s, e) => _presenter.OnConsult();

            _lblCount.Location = new Point(540, 18);
            _lblCount.AutoSize = true;
            _lblCount.ForeColor = Color.DimGray;

            _grid.Location = new Point(16, 48);
            _grid.Size = new Size(868, 456);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.ReadOnly = true;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.BackgroundColor = Color.White;

            _grid.Columns.Add(NewColumn("At", "Fecha y hora", 16));
            _grid.Columns.Add(NewColumn("ActorName", "Usuario", 14));
            _grid.Columns.Add(NewColumn("Action", "Acción", 14));
            _grid.Columns.Add(NewColumn("Entity", "Entidad", 10));
            _grid.Columns.Add(NewColumn("EntityId", "Id", 6));
            _grid.Columns.Add(NewColumn("Summary", "Detalle", 32));
            _grid.Columns.Add(NewColumn("Station", "Equipo", 12));

            Controls.AddRange(new Control[]
            {
                lblFrom, _dtpFrom, lblTo, _dtpTo, btnConsult, _lblCount, _grid
            });
        }

        private static DataGridViewTextBoxColumn NewColumn(string name, string header, int fillWeight) =>
            new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                FillWeight = fillWeight
            };

        #region ISecurityLogView

        public DateTime StartDate => _dtpFrom.Value.Date;
        public DateTime EndDate => _dtpTo.Value.Date;

        public void ShowEvents(IReadOnlyList<SecurityEventRow> events)
        {
            _grid.Rows.Clear();
            foreach (SecurityEventRow e in events)
            {
                _grid.Rows.Add(
                    e.At.ToString("dd/MM/yyyy HH:mm:ss"),
                    e.ActorName,
                    e.Action,
                    e.Entity,
                    e.EntityId?.ToString() ?? "",
                    e.Summary,
                    e.Station);
            }

            _lblCount.Text = events.Count == 0
                ? "Sin registros en el período."
                : $"{events.Count} registro(s).";
        }

        public void ShowError(string message) =>
            MessageBox.Show(message, "Bitácora", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        #endregion
    }
}
