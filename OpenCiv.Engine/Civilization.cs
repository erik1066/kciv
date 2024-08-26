using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using OpenCiv.Engine.Events;

namespace OpenCiv.Engine
{
    public class Civilization : ObservableObject
    {
        #region Members
        private readonly List<Unit> _units = new List<Unit>(50);
        private readonly List<City> _cities = new List<City>(50);
        private readonly List<Tech> _techs = new List<Tech>(50);
        private readonly ObservableCollection<UnitBuildViewModel> _constructableUnits = new ObservableCollection<UnitBuildViewModel>();
        private City _capital;
        private bool _isDefeated = false;
        private string _name = string.Empty;
        private string _nameSingular = string.Empty;
        private int _treasury = 50;
        private int _adjustedGoldPerTurn = 0;
        private double _totalCombatPower = 0;
        private int _id = -1;
        private int _estimatedResearchCompletion = 0;
        private Queue<string> _cityNames = new Queue<string>(20);
        #endregion // Members

        #region Events
        public event EventHandler<ResearchCompletedEventArgs> ResearchCompleted;
        #endregion // Events

        #region Properties

        public bool CanBuildFortress { get; set; } = false;

        public string ResearchTargetName => ResearchTarget == null ? string.Empty : ResearchTarget.Name;

        public int EstimatedResearchCompletion
        {
            get { return _estimatedResearchCompletion; }
            set { _estimatedResearchCompletion = value; RaisePropertyChanged(nameof(EstimatedResearchCompletion)); }
        }

        public Tech ResearchTarget { get; set; }

        public double ResearchProgress { get; set; }
        public double ResearchCost { get; set; }

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

        public string NameSingular
        {
            get
            {
                return _nameSingular;
            }
            set
            {
                _nameSingular = value;
                RaisePropertyChanged(nameof(NameSingular));
            }
        }

        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                RaisePropertyChanged(nameof(Id));
            }
        }

        public int AdjustedGoldPerTurn
        {
            get
            {
                return _adjustedGoldPerTurn;
            }
            set
            {
                if (value != _adjustedGoldPerTurn)
                {
                    _adjustedGoldPerTurn = value;
                    RaisePropertyChanged(nameof(AdjustedGoldPerTurn));
                }
            }
        }

        public int Treasury
        {
            get
            {
                return _treasury;
            }
            set
            {
                if (value != _treasury)
                {
                    _treasury = value;
                    RaisePropertyChanged(nameof(Treasury));
                }
            }
        }

        public int TaxRate { get; set; }
        public int ScienceRate { get; set; } = 50;
        public int LuxuryRate { get; set; } = 0;
        public bool IsDefeated => _isDefeated;

        public bool IsPlayer { get; set; } = false;

        public GovernmentType Government {get;set;} = GovernmentType.Dictatorship;

        public IEnumerable<Unit> Units => _units.AsReadOnly();

        public IEnumerable<City> Cities => _cities.AsReadOnly();

        public IEnumerable<Tech> Techs => _techs.AsReadOnly();

        #endregion // Properties

        public ICollectionView CitiesView { get; set; }
        
        public Civilization(string name, string singularName, bool isPlayer, int playerId)
        {
            Name = name;
            NameSingular = singularName;
            IsPlayer = isPlayer;
            Id = playerId;
            CitiesView = CollectionViewSource.GetDefaultView(this.Cities);

            foreach(var cityName in Descriptions.GetCityNames(this))
            {
                _cityNames.Enqueue(cityName);
            }
        }

        public bool CanBuild(UnitType type)
        {
            return _constructableUnits
                .Where(c => c.Type == type)
                .Where(c => c.IsAvailable)
                .Where(c => c.IsResearched)
                .FirstOrDefault() 
                == null ? false : true;
        }

        public void AddUnit(Unit newUnit)
        {
            if (_units.Contains(newUnit))
            {
                throw new InvalidOperationException("Cannot add this unit; unit already exists.");
            }
            else
            {
                _units.Add(newUnit);
            }
        }

        public void RemoveUnit(Unit unitToRemove)
        {
            if (!_units.Contains(unitToRemove))
            {
            }
            else
            {
                _units.Remove(unitToRemove);
            }
        }


        public UnitType GetBestUnitType(AttackMethod attackMethod)
        {
            if (attackMethod == AttackMethod.Melee)
            {
                UnitType meleeUnitType = _constructableUnits
                    .Where(u => u.AttackMethod == attackMethod)
                    .Where(u => u.IsAvailable)
                    .Where(u => u.IsResearched)
                    .OrderByDescending(u => u.CombatPower)
                    .FirstOrDefault().Type;

                if (this.IsPlayer == false && meleeUnitType == UnitType.Warrior) return UnitType.Axeman;
                else return meleeUnitType;
            }
            else
            {
                return _constructableUnits
                    .Where(u => u.AttackMethod == attackMethod)
                    .Where(u => u.IsAvailable)
                    .Where(u => u.IsResearched)
                    .OrderByDescending(u => u.RangedPower)
                    .FirstOrDefault().Type;
            }
        }

        public string GetNextCityName() => _cityNames.Dequeue();
        
        public Unit GetCostliestUnit() => Units
            .Where(u => u.Maintenance > 0)
            .OrderByDescending(u => u.Maintenance)
            .FirstOrDefault();

        public void AddCity(City newCity)
        {
            if (_cities.Contains(newCity))
            {
                throw new InvalidOperationException("Cannot add this unit; unit already exists.");
            }
            else
            {
                _cities.Add(newCity);
                CitiesView.Refresh();
            }
        }

        public void AddTech(Tech tech)
        {
            if (!_techs.Contains(tech))
            {
                _techs.Add(tech);

                if (Rules.HasTechForImprovement(_techs, ImprovementType.Fortress))
                {
                    CanBuildFortress = true;
                }
            }
        }

        public void RemoveCity(City cityToRemove)
        {
            if (!_cities.Contains(cityToRemove))
            {
                throw new InvalidOperationException("Cannot remove this unit; it doesn't exist.");
            }
            else {

                if (_capital == cityToRemove) {

                }

                _cities.Remove(cityToRemove);
                CitiesView.Refresh();
            }
        }

        public int ComputeBuildingMaintenance()
        {
            int maint = 0;
            foreach (var c in Cities)
            {
                foreach (var b in c.Buildings)
                {
                    maint += Rules.GetMaintenanceCost(b);
                    Treasury -= Rules.GetMaintenanceCost(b);
                }
            }
            return maint;
        }

        public int Population
        {
            get
            {
                int pop = 0;
                foreach (var city in Cities)
                {
                    pop += city.Population;
                }
                return pop;
            }
        }

        public void UpdateProperties()
        {
            RaisePropertyChanged(nameof(Population));
        }

        public int ComputeUnitMaintenance()
        {
            int maint = 0;
            foreach(var u in Units)
            {
                maint += u.Maintenance;
                Treasury -= u.Maintenance;
            }
            return maint;
        }

        public int GetResourceCount(ResourceType resource)
        {
            int count = 0;

            foreach(var c in Cities)
            {
                count += c.GetResourceCount(resource);
            }

            return count;
        }

        private void ChangeCapital(City oldCapital)
        {
            City oldestCity = null;

            foreach(var city in _cities.Where(c => c != oldCapital))
            {
                if (oldestCity != null)
                {
                    oldestCity = city;
                }
                else if (city.TurnFounded < oldestCity.TurnFounded)
                {
                    oldestCity = city;
                }
                else if (city.TurnFounded == oldestCity.TurnFounded)
                {
                    if (city.Size > oldestCity.Size)
                    {
                        oldestCity = city;
                    }
                }
            }

            if (oldestCity != null)
            {
                _capital = oldestCity;
            }
            else
            {
                _capital = null;
                _isDefeated = true;
            }
        }

        public void SetResearchTarget(Tech techToResearch)
        {
            ResearchTarget = techToResearch;
            ResearchCost = (double)techToResearch.Science;
            ResearchProgress = 0.0;
            ComputeEstimatedResearchCompletion();
            RaisePropertyChanged(nameof(ResearchTargetName));
        }

        private void ComputeEstimatedResearchCompletion()
        {
            double totalSciencePerTurn = 0.0;
            foreach (var city in Cities)
            {
                totalSciencePerTurn += city.SciencePerTurn;
            }

            EstimatedResearchCompletion = 1 + (int)((ResearchTarget.Science - ResearchProgress) / totalSciencePerTurn);
        }

        public void ComputeResearchProgress()
        {
            double totalSciencePerTurn = 0.0;
            foreach (var city in Cities)
            {
                ResearchProgress += city.SciencePerTurn;
                totalSciencePerTurn += city.SciencePerTurn;
            }

            if (ResearchTarget != null && ResearchProgress > ResearchCost)
            {
                AddTech(ResearchTarget);

                ResearchCompleted?.Invoke(this, new ResearchCompletedEventArgs(this, ResearchTarget));
                ResearchTarget = null;
                ResearchProgress = 0.0;
                ResearchCost = 0.0;

                RecomputeConstructableUnits();
                RecomputeConstructableBuildings();
                EstimatedResearchCompletion = 0;
            }
            else if (ResearchTarget != null)
            {
                ComputeEstimatedResearchCompletion();
            }
        }

        public void ComputeCombatPower()
        {

            double combatPower = 0.0f;

            foreach(var unit in _units)
            {
                combatPower = combatPower + (unit.CombatPower * unit.HitPoints / 100);
            }

            if (combatPower < 0) { System.Diagnostics.Debug.Fail("Combat power can't be less than zero"); } // wtf

            RaisePropertyChanged(nameof(TotalCombatPower));

            TotalCombatPower = combatPower;
        }

        public double TotalCombatPower
        {
            get
            {
                return _totalCombatPower;
            }
            set
            {
                if (value != _totalCombatPower)
                {
                    _totalCombatPower = value;
                    RaisePropertyChanged(nameof(TotalCombatPower));
                }
            }
        }

        public void RecomputeConstructableUnits()
        {
            IEnumerable<UnitBuildViewModel> unitBuildVMs = Rules.GetValidUnits(this);

            foreach(var u in unitBuildVMs)
            {
                AddConstructableUnit(u);
            }

            foreach (var city in Cities)
            {
                foreach (var u in _constructableUnits)
                {
                    city.AddConstructableUnit(u);
                }
            }
        }

        public void RecomputeConstructableBuildings()
        {
            IEnumerable<BuildingBuildViewModel> buildingVMs = Rules.GetValidBuildings(this);

            foreach (var city in Cities)
            {
                foreach (var b in buildingVMs)
                {
                    city.AddConstructableBuilding(b);
                }
            }
        }

        private void AddConstructableUnit(UnitBuildViewModel unitBuildVM)
        {
            UnitBuildViewModel currentUnitBuildVM = _constructableUnits.FirstOrDefault(t => t.Type == unitBuildVM.Type);

            if (currentUnitBuildVM == null)
            {
                _constructableUnits.Add(unitBuildVM);
            }
            else
            {
                currentUnitBuildVM.IsAvailable = unitBuildVM.IsAvailable;
                currentUnitBuildVM.IsResearched = unitBuildVM.IsResearched;
                currentUnitBuildVM.IsObsolete = unitBuildVM.IsObsolete;
            }
        }
    }
}
