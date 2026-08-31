using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PharmacySystem.Wpf
{
    // One node of the roles permission tree. IsChecked is a plain bindable flag; the cascade
    // ("no child without its parent") is applied by RolesWindow at the tree level, mirroring
    // frmRoles' AfterCheck handler.
    public class PermNodeVm : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public PermNodeVm Parent { get; set; }
        public ObservableCollection<PermNodeVm> Children { get; } = new ObservableCollection<PermNodeVm>();

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<PermNodeVm> DescendantsAndSelf()
        {
            yield return this;
            foreach (PermNodeVm child in Children)
                foreach (PermNodeVm d in child.DescendantsAndSelf())
                    yield return d;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
