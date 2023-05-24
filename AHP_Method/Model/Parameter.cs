using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHP_Method.Model
{
    internal class Parameter
    {
        public string Name { get; set; }
        public Parameter Parent { get; set; }

        public List<Alternativa> Alternative { get; set; }

        public double Weight { get; set; }
        private ObservableCollection<Parameter> _children;
        public ObservableCollection<Parameter> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                OnPropertyChanged(nameof(Children));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Parameter()
        {
            Children = new ObservableCollection<Parameter>();
            Alternative = new List<Alternativa>();

        }
        
        public Parameter(string name)
        {
            Name = name;
            Children = new ObservableCollection<Parameter>();
            Alternative = new List<Alternativa>();
        }

        public bool IsAncestor(Parameter potentialDescendant)
        {
            if (Children.Contains(potentialDescendant))
            {
                return true;
            }
            else
            {
                foreach (Parameter child in Children)
                {
                    if (child.IsAncestor(potentialDescendant))
                    {
                        return true;
                    }
                }
                return false;
            }
        }


    }
}
