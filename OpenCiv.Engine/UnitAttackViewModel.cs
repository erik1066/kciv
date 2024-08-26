using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCiv.Engine
{
    public sealed class UnitAttackViewModel : ObservableObject
    {
        private UnitType _type = UnitType.None;
        private TerrainType _terrain = TerrainType.Grassland;
        private AttackMethod _attackMethod = AttackMethod.None;
        private string _ownerName = string.Empty;
        private int _rp = 0;
        private int _cp = 0;
        private bool _isShowing = false;
        private double _hpPct = 0.0;
        private double _hp = 0.0;
        private string _name = string.Empty;

        private double _attackerHpLoss = 0.0;
        private double _defenderHpLoss = 0.0;

        public void Reset()
        {
            AttackMethod = AttackMethod.None;
            Terrain = TerrainType.Grassland;
            Type = UnitType.None;
            OwnerName = string.Empty;
            RangedPower = 0;
            CombatPower = 0;
            IsShowing = false;
            HitPointsRemainingPct = 0;
            HitPoints = 0;
            AttackerHitPointLoss = 0;
            DefenderHitPointLoss = 0;
            Name = string.Empty;
        }

        public UnitAttackViewModel()
        {

        }

        public void Set(Unit unit, Tile tile, CombatReport report)
        {
            AttackMethod = unit.AttackMethod;
            CombatPower = (int)unit.CombatPower;
            HitPoints = unit.HitPoints;
            HitPointsRemainingPct = 0;
            IsShowing = true;
            OwnerName = unit.OwnerName;
            RangedPower = (int)unit.RangedPower;
            Terrain = tile.Terrain;
            Type = unit.Type;
            Name = unit.Name;

            AttackerHitPointLoss = report.AttackerHitPointLoss;
            DefenderHitPointLoss = report.DefenderHitPointLoss;
        }

        public double AttackerHitPointLoss
        {
            get
            {
                return _attackerHpLoss;
            }
            set
            {
                _attackerHpLoss = value;
                RaisePropertyChanged(nameof(AttackerHitPointLoss));
            }
        }

        public double DefenderHitPointLoss
        {
            get
            {
                return _defenderHpLoss;
            }
            set
            {
                _defenderHpLoss = value;
                RaisePropertyChanged(nameof(DefenderHitPointLoss));
            }
        }

        public AttackMethod AttackMethod
        {
            get
            {
                return _attackMethod;
            }
            set
            {
                _attackMethod = value;
                RaisePropertyChanged(nameof(AttackMethod));
            }
        }

        public double HitPoints
        {
            get
            {
                return _hp;
            }
            set
            {
                _hp = value;
                RaisePropertyChanged(nameof(HitPoints));
            }
        }
        public double HitPointsRemainingPct
        {
            get
            {
                return _hpPct;
            }
            set
            {
                _hpPct = value;
                RaisePropertyChanged(nameof(HitPointsRemainingPct));
            }
        }

        public bool IsShowing
        {
            get
            {
                return _isShowing;
            }
            set
            {
                _isShowing = value;
                RaisePropertyChanged(nameof(IsShowing));
            }
        }

        public UnitType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                RaisePropertyChanged(nameof(Type));
            }
        }

        public TerrainType Terrain
        {
            get
            {
                return _terrain;
            }
            set
            {
                _terrain = value;
                RaisePropertyChanged(nameof(Terrain));
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public string OwnerName
        {
            get
            {
                return _ownerName;
            }
            set
            {
                _ownerName = value;
                RaisePropertyChanged(nameof(OwnerName));
            }
        }

        public int RangedPower
        {
            get
            {
                return _rp;
            }
            set
            {
                _rp = value;
                RaisePropertyChanged(nameof(RangedPower));
            }
        }

        public int CombatPower
        {
            get
            {
                return _cp;
            }
            set
            {
                _cp = value;
                RaisePropertyChanged(nameof(CombatPower));
            }
        }
    }
}
