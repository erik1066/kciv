using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenCiv.Engine
{
    [DebuggerDisplay("{Owner.Name} : {Type}")]
    public class Unit : ObservableObject
    {
        private readonly List<PromotionType> _promotions = new List<PromotionType>();
        private readonly List<CombatBonusType> _bonuses = new List<CombatBonusType>();
        private readonly List<UnitSpecialAction> _specialActions = new List<UnitSpecialAction>();
        private readonly UnitType _type = UnitType.Warrior;
        private readonly Civilization _owner;
        private int _kills = 0;
        private int _remainingMoves = 1;
        private int _maxMoves = 1;
        private double _hp = 100.0;
        private UnitStatus _status = UnitStatus.None;
        private int _turnFortified = -1;
        private int _turnBuildStarted = -1;
        private ImprovementType _building = ImprovementType.None;
        private int _turnBuildWillEnd = -1;
        private bool _wasAttackedThisTurn = false;
        private double _rangedPower = 0.0;
        private int _maintenance = 0;
        private int _experience = 0;

        public int Id { get; set; }

        public int RoadMoves { get; set; }
        public UnitType UpgradesTo { get; set; }

        public int TurnFortified => _turnFortified;

        public bool WasAttackedThisTurn
        {
            get
            {
                return _wasAttackedThisTurn;
            }
            set
            {
                _wasAttackedThisTurn = value;
                RaisePropertyChanged(nameof(WasAttackedThisTurn));
            }
        }

        public int Maintenance
        {
            get
            {
                return _maintenance;
            }
            set
            {
                _maintenance = value;
                RaisePropertyChanged(nameof(Maintenance));
            }
        }

        public int Experience
        {
            get
            {
                return _experience;
            }
            set
            {
                _experience = value;
                RaisePropertyChanged(nameof(Experience));
            }
        }

        public int TurnBuildStarted
        {
            get
            {
                return _turnBuildStarted;
            }
            private set
            {
                _turnBuildStarted = value;
                RaisePropertyChanged(nameof(TurnBuildStarted));
            }
        }

        public int TurnBuildWillEnd
        {
            get
            {
                return _turnBuildWillEnd;
            }
            private set
            {
                _turnBuildWillEnd = value;
                RaisePropertyChanged(nameof(TurnBuildWillEnd));
            }
        }

        public ImprovementType CurrentlyBuilding
        {
            get
            {
                return _building;
            }
            private set
            {
                _building = value;
                RaisePropertyChanged(nameof(CurrentlyBuilding));
            }
        }

        public double CombatPower { get; set; } = 0.0f;
        public double RangedPower
        {
            get
            {
                return _rangedPower;
            }
            set
            {
                _rangedPower = value;
                RaisePropertyChanged(nameof(RangedPower));
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
                RaisePropertyChanged("HitPoints");
            }
        }
        public UnitStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                RaisePropertyChanged(nameof(Status));
            }
        }

        public int Kills
        {
            get
            {
                return _kills;
            }
            set
            {
                _kills = value;
                RaisePropertyChanged(nameof(Kills));
            }
        }
        public int TurnCreated { get; set; } = 0;
        public int AttackRange { get; set; } = 0;
        public int MaxMoves
        {
            get
            {
                return _maxMoves;
            }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException();
                _maxMoves = value;
                RaisePropertyChanged(nameof(MaxMoves));
            }
        }

        public int RemainingMoves
        {
            get
            {
                return _remainingMoves;
            }
            set
            {
                _remainingMoves = value;
                RaisePropertyChanged(nameof(RemainingMoves));
            }
        }
        public int PromotionCount => Promotions.Count;

        public UnitLocomotion Locomotion { get; set; } = UnitLocomotion.Foot;
        public IList<PromotionType> Promotions
        {
            get
            {
                return _promotions.AsReadOnly();
            }
        }
        public IList<CombatBonusType> Bonuses
        {
            get
            {
                return _bonuses.AsReadOnly();
            }
        }
        public AttackMethod AttackMethod { get; set; }

        public string OwnerName => Owner.Name;

        public string Name { get; set; }

        public UnitType Type => _type;

        public Civilization Owner => _owner;

        public Unit(UnitType type, Civilization owner) 
        {
            _owner = owner;
            _type = type;
            RaisePropertyChanged(nameof(Type));
            RaisePropertyChanged(nameof(Owner));
            RaisePropertyChanged(nameof(OwnerName));
            HitPoints = 100.0;
            Kills = 0;
            Status = UnitStatus.None;

            RoadMoves = MaxMoves;
        }

        public bool HasSpecialAction(UnitSpecialAction action) => _specialActions == null ? false : _specialActions.Contains(action);

        public bool HasPromotion(PromotionType promotion) => _promotions == null ? false : _promotions.Contains(promotion);

        public bool HasCombatBonus(CombatBonusType combatBonus) => _bonuses == null ? false : _bonuses.Contains(combatBonus);

        public void Heal(int hpToHeal = 20)
        {
            double newHp = HitPoints + hpToHeal;
            if (newHp > 100) HitPoints = 100;
            else HitPoints = newHp;
        }

        public void Promote(PromotionType promotion)
        {
            if (_promotions.Contains(promotion))
            {
                throw new InvalidOperationException("Cannot add this promotion; unit already has it.");
            }

            if (promotion == PromotionType.Blitz)
            {
                MaxMoves++;
            }

            _promotions.Add(promotion);
            RaisePropertyChanged(nameof(PromotionCount));
        }

        public void AddCombatBonus(CombatBonusType bonus)
        {
            if (_bonuses.Contains(bonus))
            {
                throw new InvalidOperationException("Cannot add this bonus; unit already has it.");
            }

            _bonuses.Add(bonus);
        }

        public void AddSpecialAction(UnitSpecialAction specialAction)
        {
            if (_specialActions.Contains(specialAction))
            {
                throw new InvalidOperationException("Cannot add this special action; unit already has it.");
            }

            _specialActions.Add(specialAction);
        }

        public bool TryStartFortify(int turnNumber)
        {
            if (Status == UnitStatus.Fortifying || Status == UnitStatus.Fortified || Status == UnitStatus.Working) return false;

            _turnFortified = turnNumber;

            Status = UnitStatus.Fortifying;
            RemainingMoves = 0;

            return true;
        }

        public bool Activate()
        {
            if (Status == UnitStatus.Fortifying || Status == UnitStatus.Fortified || Status == UnitStatus.Working)
            {
                _turnFortified = -1;
                Status = UnitStatus.None;

                TurnBuildStarted = -1;
                TurnBuildWillEnd = -1;
                CurrentlyBuilding = ImprovementType.None;
                return true;
            }
            else
            {
                return true;
            }
        }

        public bool TryBeginBuilding(Tile tileToWork, int thisTurn, ImprovementType improvementType)
        {
            if (Status != UnitStatus.None) return false;

            TurnBuildStarted = thisTurn;
            CurrentlyBuilding = improvementType;
            TurnBuildWillEnd = thisTurn + Rules.GetImprovementBuildTime(tileToWork, improvementType);
            Status = UnitStatus.Working;
            RemainingMoves = 0;
            return true;
        }

        public bool TryBeginBuildingRoad(Tile tileToWork, int thisTurn)
        {
            if (Status != UnitStatus.None) return false;

            TurnBuildStarted = thisTurn;
            TurnBuildWillEnd = thisTurn + Rules.GetRoadBuildTime(tileToWork);
            Status = UnitStatus.Working;
            RemainingMoves = 0;
            return true;
        }
    }
}
