using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AssortEditor.Models
{
    public class Item : INotifyPropertyChanged
    {
        // Renamed the backing field to avoid conflict with the property name
        private string _idField;
        public string _id { get; set; }

        private string _tplField;
        public string _tpl
        {
            get => _tplField;
            set
            {
                if (_tplField != value)
                {
                    _tplField = value;
                    OnPropertyChanged(nameof(_tpl));
                }
            }
        }

        
        private string slotidField;
        public string slotid
        {
            get => slotidField;
            set
            {
                if (slotidField != value)
                {
                    slotidField = value;
                    OnPropertyChanged(nameof(slotid));
                }
            }
        }
        public string parentid { get; set; }
        
        public int? Location { get; set; }

        public Upd upd { get; set; } = new();
        public ObservableCollection<Item> Children { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Barter> Barters { get; set; } = new ObservableCollection<Barter>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Upd
    {
        public bool? unlimitedcount { get; set; }
        public int? stackobjectscount { get; set; }
        public int? buyrestrictioncurrent { get; set; }
        public int? buyrestrictionmax { get; set; }
        
        public Repairable? repairable { get; set; } = new();
        public Foldable? foldable { get; set; }
        public FireMode? firemode { get; set; }
    }

    public class Repairable
    {
        public int? durability { get; set; }  // Nullable int
        public int? maxdurability { get; set; }  // Nullable int
    }

    public class Foldable
    {
        public bool foldable { get; set; }
        public bool folded { get; set; }
    }

    public class FireMode
    {
        public string firemode { get; set; }
    }
}
