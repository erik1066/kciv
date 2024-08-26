using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCiv.Engine
{
    public sealed class UnitBuildViewModel : ObservableObject
    {
        public string _name = string.Empty;
        public int _cost = 100;
        public bool _isResearched = false;
        public bool _isAvailable = true;
        public bool _isObsolete = false;
        public UnitType _unitType = UnitType.Settler;
        public bool _showDetails = false;

        public string Name { get { return _name; }
        set
            {
                _name = value;
                RaisePropertyChanged(nameof(Name));
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
        public bool ShowDetails
        {
            get { return _showDetails; }
            set
            {
                _showDetails = value;
                RaisePropertyChanged(nameof(ShowDetails));
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
        public UnitType Type
        {
            get { return _unitType; }
            set
            {
                _unitType = value;
                RaisePropertyChanged(nameof(Type));
            }
        }
        public int MovePoints { get; set; }
        public double CombatPower
        {
            get
            {
                return ArchType == null ? 0.0 : ArchType.CombatPower;
            }
        }

        public double RangedPower
        {
            get
            {
                return ArchType == null ? 0.0 : ArchType.RangedPower;
            }
        }
        public AttackMethod AttackMethod
        {
            get
            {
                return ArchType == null ? AttackMethod.None : ArchType.AttackMethod;
            }
        }

        public double MaxMoves
        {
            get
            {
                return ArchType == null ? 0.0 : ArchType.MaxMoves;
            }
        }
        private Unit ArchType { get; set; }
        public string Bonuses
        {
            get
            {
                if (ArchType == null || ArchType.Bonuses.Count == 0) return string.Empty;
                return Descriptions.ConvertBonusToDescription(ArchType.Bonuses[0]);
            }
        }

        public UnitBuildViewModel(string name, int cost, UnitType type, bool isAvailable = false, bool isResearched = false)
        {
            Name = name;
            Cost = cost;
            IsResearched = isResearched;
            IsAvailable = isAvailable;
            Type = type;

            if (type != UnitType.None)
            {
                ShowDetails = true;
                UnitFactory factory = new UnitFactory();
                ArchType = factory.ProduceUnit(type, null);

                RaisePropertyChanged(nameof(MaxMoves));
                RaisePropertyChanged(nameof(CombatPower));
                RaisePropertyChanged(nameof(Bonuses));
            }
            else
            {
                ShowDetails = false;
            }
        }
    }
}
