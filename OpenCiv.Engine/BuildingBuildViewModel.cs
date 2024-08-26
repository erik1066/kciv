using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCiv.Engine
{
    public sealed class BuildingBuildViewModel : ObservableObject
    {
        public string _name = string.Empty;
        public string _description = string.Empty;
        public int _cost = 100;
        public bool _isResearched = false;
        public bool _isAvailable = true;
        public bool _isObsolete = false;
        public BuildingType _type = BuildingType.None;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged(nameof(Description));
            }
        }
        public int Cost
        {
            get { return _cost; }
            set
            {
                _cost = value;
                RaisePropertyChanged(nameof(Cost));
            }
        }
        public bool IsResearched
        {
            get { return _isResearched; }
            set
            {
                _isResearched = value;
                RaisePropertyChanged(nameof(IsResearched));
            }
        }
        public bool IsObsolete
        {
            get { return _isObsolete; }
            set
            {
                _isObsolete = value;
                RaisePropertyChanged(nameof(IsObsolete));
            }
        }
        public bool IsAvailable
        {
            get { return _isAvailable; }
            set
            {
                _isAvailable = value;
                RaisePropertyChanged(nameof(IsAvailable));
            }
        }
        public BuildingType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                RaisePropertyChanged(nameof(Type));
            }
        }

        public BuildingBuildViewModel(string name, int cost, BuildingType type, bool isAvailable = false, bool isResearched = false)
        {
            Name = name;
            Cost = cost;
            IsResearched = isResearched;
            IsAvailable = isAvailable;
            Type = type;

            if (type != BuildingType.None)
            {
                Description = Descriptions.ConvertBuildingTypeToDescription(type);
            }
        }
    }
}
