using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using OpenCiv.Engine.Events;

namespace OpenCiv.Engine
{
    [DebuggerDisplay("Name = {Name}, FPT = {FoodPerTurn}, PPT = {ProductionPerTurn}, GPT = {GoldPerTurn}")]
    public class City : ObservableObject 
    {
        #region Members
        private readonly Tile _centerTile;
        private int _size = 1;
        private int _turnFounded = -1;
        private int _food = 1;
        private Civilization _founder = null;
        private Civilization _owner = null;
        private string _name = string.Empty;
        private ObservableCollection<Tile> _ownedTiles = new ObservableCollection<Tile>();
        private ObservableCollection<BuildingType> _buildings = new ObservableCollection<BuildingType>();
        private CityPriority _priority = CityPriority.Balanced;
        private int _foodPerTurn = 1;
        private int _goldPerTurn = 1;
        private int _prodPerTurn = 1;
        private int _buildProgress = 0;
        private bool _isCapital = false;
        private double _hitPoints = 100.0;
        private double _maxHitPoints = 100.0;
        private readonly ObservableCollection<UnitBuildViewModel> _constructableUnits = new ObservableCollection<UnitBuildViewModel>();
        private readonly ObservableCollection<BuildingBuildViewModel> _constructableBuildings = new ObservableCollection<BuildingBuildViewModel>();
        private int _currentUnitProduction = -1;
        private int _currentBuildingProduction = -1;
        private int _ownerId = -1;
        private double _sciencePerTurn = 0.0;
        #endregion // Members

        #region Events
        public event EventHandler<UnitConstructedEventArgs> UnitConstructed;
        public event EventHandler<BuildingConstructedEventArgs> BuildingConstructed;
        #endregion // Events

        #region Properties
        public ICollectionView ConstructableUnitsView { get; set; }
        public ICollectionViewLiveShaping ConstructableUnitsViewLive { get; set; }
        public ICollectionView ConstructableBuildingsView { get; set; }
        public ICollectionViewLiveShaping ConstructableBuildingsViewLive { get; set; }

        public int OwnerId
        {
            get
            {
                return _ownerId;
            }
            set
            {
                _ownerId = value;
                RaisePropertyChanged(nameof(OwnerId));
            }
        }
        public bool IsCapital
        {
            get { return _isCapital; }
            set
            {
                _isCapital = value;
                RaisePropertyChanged(nameof(IsCapital));
            }
        }

        public double CombatPower
        {
            get
            {
                Unit best = null;

                foreach (var unit in Owner.Units)
                {
                    if (best == null) best = unit;
                    else if (best.CombatPower < unit.CombatPower)
                    {
                        best = unit;
                    }                    
                }

                if (best == null) return 20;

                return best.CombatPower;
            }
        }

        public double HitPoints
        {
            get
            {
                return _hitPoints;
            }
            set
            {
                _hitPoints = value;
                RaisePropertyChanged(nameof(HitPoints));
                RaisePropertyChanged(nameof(PercentRemainingHitPoints));
            }
        }

        public double MaxHitPoints
        {
            get
            {
                return _maxHitPoints;
            }
            set
            {
                if (value < 100.0 || value > 500000) throw new ArgumentOutOfRangeException();

                _maxHitPoints = value;
                RaisePropertyChanged(nameof(MaxHitPoints));
                RaisePropertyChanged(nameof(PercentRemainingHitPoints));
            }
        }

        public double PercentRemainingHitPoints
        {
            get
            {
                return HitPoints / MaxHitPoints;
            }
        }

        public bool HasWalls
        {
            get
            {
                return _buildings.Contains(BuildingType.Walls);
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (string.IsNullOrEmpty(value.Trim())) throw new InvalidOperationException();

                _name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public int CurrentUnitProduction
        {
            get { return _currentUnitProduction; }
            set
            {
                _currentUnitProduction = value;
                RaisePropertyChanged(nameof(CurrentUnitProduction));
            }
        }

        public int CurrentBuildingProduction
        {
            get { return _currentBuildingProduction; }
            set
            {
                _currentBuildingProduction = value;
                RaisePropertyChanged(nameof(CurrentBuildingProduction));
            }
        }

        public int FoodPerTurn
        {
            get { return _foodPerTurn; }
            set
            {
                _foodPerTurn = value;
                RaisePropertyChanged(nameof(FoodPerTurn));
            }
        }
        public int GoldPerTurn
        {
            get { return _goldPerTurn; }
            set
            {
                _goldPerTurn = value;
                RaisePropertyChanged(nameof(GoldPerTurn));
            }
        }
        public int AdjustedGoldPerTurn
        {
            get
            {
                return GoldPerTurn - Buildings.Count();
            }
        }

        public double SciencePerTurn
        {
            get
            {
                return _sciencePerTurn;
            }
            set
            {
                _sciencePerTurn = value;
                RaisePropertyChanged(nameof(SciencePerTurn));
            }
        }
        public int ProductionPerTurn
        {
            get { return _prodPerTurn; }
            set
            {
                _prodPerTurn = value;
                RaisePropertyChanged(nameof(ProductionPerTurn));
            }
        }
        public int Size
        {
            get
            {
                return _size;
            }
            private set
            {
                if (value >= 1 && value <= 100)
                {
                    _size = value;
                }
                else if (value < 1) _size = 1;
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
                RaisePropertyChanged(nameof(Size));
                RaisePropertyChanged(nameof(Population));
            }
        }
        public int TurnFounded
        {
            get
            {
                return _turnFounded;
            }
            private set
            {
                _turnFounded = value;
                RaisePropertyChanged(nameof(TurnFounded));
            }
        }
        public int Food
        {
            get
            {
                return _food;
            }
            set
            {
                _food = value;
                RaisePropertyChanged(nameof(Food));
            }
        }
        public Civilization Owner
        {
            get
            {
                return _owner;
            }
            set
            {
                _owner = value;
                RaisePropertyChanged(nameof(Owner));
            }
        }
        public CityPriority Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                _priority = value;
                RaisePropertyChanged(nameof(Priority));
            }
        }
        public ObservableCollection<BuildingType> Buildings
        {
            get
            {
                return _buildings;
            }
        }
        #endregion // Properties
        
        public City(string name, int turnFounded, Tile centerTile, Civilization owner, IEnumerable<Tile> ownedTiles, int ownerId, int startingSize = 1, bool isCapital = false)
        {
            if (string.IsNullOrEmpty(name.Trim())) throw new InvalidOperationException();

            Name = name;
            Owner = owner;
            _founder = owner;
            TurnFounded = turnFounded;
            Size = startingSize;
            Food = 1;
            _centerTile = centerTile;

            foreach (var tile in ownedTiles)
            {
                if (!_ownedTiles.Contains(tile))
                {
                    _ownedTiles.Add(tile);
                }
            }

            MaxHitPoints = Size * 100.0;
            HitPoints = MaxHitPoints;
            OwnerId = ownerId;

            RaisePropertyChanged(nameof(Size));
            RaisePropertyChanged(nameof(Name));

            //object current = ConstructableUnitsView.CurrentItem;
            // UNITS
            ConstructableUnitsView = CollectionViewSource.GetDefaultView(this._constructableUnits);
            ConstructableUnitsView.Filter = item => {
                UnitBuildViewModel vm = item as UnitBuildViewModel;
                return vm != null && vm.IsAvailable && vm.IsResearched;
            };

            //ConstructableUnitsView.MoveCurrentTo(current);

            ConstructableUnitsViewLive = ConstructableUnitsView as ICollectionViewLiveShaping;
            if (ConstructableUnitsViewLive.CanChangeLiveFiltering)
            {
                ConstructableUnitsViewLive.LiveFilteringProperties.Add("IsResearched");
                ConstructableUnitsViewLive.LiveFilteringProperties.Add("IsAvailable");
                ConstructableUnitsViewLive.LiveFilteringProperties.Add("IsObsolete");
                ConstructableUnitsViewLive.IsLiveFiltering = true;
            }

            // BUILDINGS
            ConstructableBuildingsView = CollectionViewSource.GetDefaultView(this._constructableBuildings);
            ConstructableBuildingsView.Filter = item => 
            {
                BuildingBuildViewModel vm = item as BuildingBuildViewModel;
                bool filter = (vm != null && vm.IsAvailable && vm.IsResearched && !Buildings.Contains(vm.Type));
                return filter;
            };

            ConstructableBuildingsViewLive = ConstructableBuildingsView as ICollectionViewLiveShaping;
            if (ConstructableBuildingsViewLive.CanChangeLiveFiltering)
            {
                ConstructableBuildingsViewLive.LiveFilteringProperties.Add("IsResearched");
                ConstructableBuildingsViewLive.LiveFilteringProperties.Add("IsAvailable");
                ConstructableBuildingsViewLive.IsLiveFiltering = true;
            }
        }

        public void AdjustForSettlerBuild()
        {
            Shrink();
            Food = 1;
        }

        public void Occupy()
        {
            Size = Size / 2;
            Food = 1;

            this.ComputeWorkedTiles();
            this.ComputeHitPoints();
        }

        public void AddConstructableUnit(UnitBuildViewModel unitBuildVM)
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

            //object current = ConstructableUnitsView.CurrentItem;

            //ConstructableUnitsView.Filter = null;
            //ConstructableUnitsView.Filter = item => {
            //    UnitBuildViewModel vm = item as UnitBuildViewModel;
            //    return vm != null && vm.IsAvailable && vm.IsResearched && !vm.IsObsolete;
            //};

            //ConstructableBuildingsView.MoveCurrentTo(current);
        }

        public void AddConstructableBuilding(BuildingBuildViewModel buildingVM)
        {
            BuildingBuildViewModel currentBuildVM = _constructableBuildings.FirstOrDefault(t => t.Type == buildingVM.Type);

            if (currentBuildVM == null)
            {
                _constructableBuildings.Add(buildingVM);
            }
            else
            {
                currentBuildVM.IsAvailable = buildingVM.IsAvailable;
                currentBuildVM.IsResearched = buildingVM.IsResearched;
                currentBuildVM.IsObsolete = buildingVM.IsObsolete;
            }

            //object current = ConstructableBuildingsView.CurrentItem;

            //ConstructableBuildingsView.Filter = null;
            //ConstructableBuildingsView.Filter = item =>
            //{
            //    BuildingBuildViewModel vm = item as BuildingBuildViewModel;
            //    bool filter = (vm != null && vm.IsAvailable && vm.IsResearched && !Buildings.Contains(vm.Type));
            //    return filter;
            //};

            //ConstructableBuildingsView.MoveCurrentTo(current);
        }

        public int GetResourceCount(ResourceType resource)
        {
            int count = _ownedTiles.Where(t => t.Resource == resource).Count();

            return count;
        }

        public void ProcessTurn()
        {
            ComputeWorkedTiles();
            ComputeFood();
            ComputeCurrentProduction();
        }

        public void Heal()
        {
            double newHp = HitPoints + 20;
            if (newHp > MaxHitPoints) HitPoints = MaxHitPoints;
            else HitPoints = newHp;
        }

        private void Grow()
        {
            Size++;
            ComputeHitPoints();
        }

        public int Population => (Size * 10000) + ((Size - 1) * (Size - 1) * 5000);

        private void ComputeHitPoints()
        {
            double remainingHpPct = HitPoints / MaxHitPoints;

            MaxHitPoints = Size * 100.0;

            if (Buildings.Contains(BuildingType.Walls))
            {
                MaxHitPoints += Rules.CITY_WALLS_HIT_POINT_BONUS; // add 50 hp for having walls
            }
            if (Buildings.Contains(BuildingType.Arsenal))
            {
                MaxHitPoints += Rules.ARSENAL_HIT_POINT_BONUS; // add 25 hp for having an arsenal
            }
            if (Buildings.Contains(BuildingType.Castle))
            {
                MaxHitPoints += 25.0; // add 25 hp for having a castle
            }

            // if we have max hp already, just down-adjust
            if (remainingHpPct >= 0.99)
            {
                HitPoints = MaxHitPoints;
            }
            else
            {
                // but if we were damaged, then scale the damage to the new HP level
                HitPoints = (MaxHitPoints * remainingHpPct);
            }
        }

        private void Shrink()
        {
            if (Size > 1)
            {
                Size--;
                ComputeHitPoints();
            }
        }

        public void AddBuilding(BuildingType buildingType)
        {
            if (!Buildings.Contains(buildingType))
            {
                Buildings.Add(buildingType);
                if (buildingType == BuildingType.Walls)
                {
                    RaisePropertyChanged(nameof(HasWalls));
                }

                ConstructableBuildingsView.MoveCurrentToFirst();
            }
        }

        private void ComputeWorkedTiles()
        {
            List<Tile> highestFoodPriority = new List<Tile>(19);
            List<Tile> highestProdPriority = new List<Tile>(19);
            List<Tile> highestGoldPriority = new List<Tile>(19);

            highestFoodPriority.AddRange(_ownedTiles);
            highestProdPriority.AddRange(_ownedTiles);
            highestGoldPriority.AddRange(_ownedTiles);

            highestFoodPriority.Sort(delegate (Tile t1, Tile t2) { return t1.Food.CompareTo(t2.Food); });
            highestProdPriority.Sort(delegate (Tile t1, Tile t2) { return t1.Production.CompareTo(t2.Production); });
            highestGoldPriority.Sort(delegate (Tile t1, Tile t2) { return t1.Gold.CompareTo(t2.Gold); });

            List<Tile> workedTiles = new List<Tile>(19);

            int j = 0;
            if (Priority == CityPriority.Balanced)
            {
                for (int i = 0; i < Size; i++) 
                {
                    if (i > 18) continue;

                    switch (i % 3)
                    {
                        case 0:
                            var foodTile = highestFoodPriority[Math.Abs(17 - j)];
                            if (!workedTiles.Contains(foodTile)) workedTiles.Add(foodTile);
                            break;
                        case 1:
                            var prodTile = highestProdPriority[Math.Abs(17 - j)];
                            if (!workedTiles.Contains(prodTile)) workedTiles.Add(prodTile);
                            break;
                        case 2:
                            var goldTile = highestGoldPriority[Math.Abs(17 - j)];
                            if (!workedTiles.Contains(goldTile)) workedTiles.Add(goldTile);
                            j++;
                            break;
                    }
                }
            }
            else if (Priority == CityPriority.Food)
            {
                for (int i = 0; i < Size; i++) 
                {
                    if (i > 18) continue;
                    workedTiles.Add(highestFoodPriority[Math.Abs(17 - i)]);
                }
            }
            else if (Priority == CityPriority.Production)
            {
                for (int i = 0; i < Size; i++) 
                {
                    if (i > 18) continue;
                    workedTiles.Add(highestFoodPriority[Math.Abs(17 - i)]);
                }
            }
            else if (Priority == CityPriority.Gold)
            {
                for (int i = 0; i < Size; i++) 
                {
                    if (i > 18) continue;
                    workedTiles.Add(highestGoldPriority[Math.Abs(17 - i)]);
                }
            }

            int tempFood = 1;
            int tempGold = 1;
            int tempProd = 1;
            double tempScience = 1;

            tempScience += Size;

            foreach (var tile in workedTiles)
            {
                tempFood += tile.Food;
                tempProd += tile.Production;
                tempGold += tile.Gold;
                tempScience += tile.Science;

                // Forge provides +1 prod for each IRON tile and 15% boost to land unit production
                if (tile.Resource == ResourceType.Iron && Buildings.Contains(BuildingType.Forge))
                {
                    tempProd += Rules.FORGE_TILE_IRON_PRODUCTION_POINTS;
                }

                if (tile.Resource == ResourceType.Wheat && Buildings.Contains(BuildingType.Granary))
                {
                    tempFood += Rules.GRANARY_TILE_WHEAT_FOOD_POINTS;
                }
            }

            if (Buildings.Contains(BuildingType.Library))
            {
                tempScience += (double)Size / 2; // increase by 1 science per 2 population
            }
            if (Buildings.Contains(BuildingType.University))
            {
                tempScience += (tempScience * Rules.UNIVERSITY_SCIENCE_MOD); // across the board 25% sci increase
                tempScience += 1; // plus one extra science
                tempScience = Math.Round(tempScience, 0);
            }

            if (Buildings.Contains(BuildingType.StoneWorks))
            {
                tempProd += Rules.STONEWORKS_PRODUCTION_POINTS;
            }
            if (Buildings.Contains(BuildingType.Workshop))
            {
                tempProd += Rules.WORKSHOP_PRODUCTION_POINTS;                    
            }

            if (Buildings.Contains(BuildingType.Granary))
            {
                tempFood += Rules.GRANARY_CITY_FOOD_POINTS; // increase base city food output by 2 for having a granary
            }

            if (Buildings.Contains(BuildingType.Market))
            {
                tempGold += Rules.MARKET_GOLD_POINTS;
                tempGold += (int)((double)tempGold * Rules.MARKET_GOLD_MOD);
            }

            FoodPerTurn = tempFood;
            ProductionPerTurn = tempProd;
            GoldPerTurn = tempGold;
            SciencePerTurn = tempScience;
        }

        private void ComputeFood()
        {
            Food = Food + (FoodPerTurn - (Size - 1));

            double foodNeededToGrow = (((double)Size + ((double)Size - 1)) * 30.0);

            if (Buildings.Contains(BuildingType.Aqueduct))
            {
                foodNeededToGrow = foodNeededToGrow / 1.6666667;
            }

            if (Food > foodNeededToGrow)
            {
                if (Size >= 8 && !Buildings.Contains(BuildingType.Aqueduct))
                {
                    Food = (int)foodNeededToGrow; // do not grow city if no adqueduct
                }
                else
                {
                    Grow();
                }
            }

            if (Food < 0)
            {
                Shrink();
            }
        }
        
        private void ComputeCurrentProduction()
        {
            UnitBuildViewModel vm = ConstructableUnitsView.CurrentItem as UnitBuildViewModel;

            if (vm != null)
            {
                if (vm.Type == UnitType.None) // special case, basically we're saying we want to produce gold
                {
                    GoldPerTurn += ProductionPerTurn;
                }
                else
                {
                    CurrentUnitProduction += ProductionPerTurn; // only 'produce' if there is a selected 'thing' to produce

                    // 15% unit production bonus for having a FORGE
                    if (Buildings.Contains(BuildingType.Forge))
                    {
                        CurrentUnitProduction = CurrentUnitProduction + (int)(ProductionPerTurn * Rules.FORGE_UNIT_PRODUCTION_MOD);
                    }

                    if (CurrentUnitProduction >= vm.Cost)
                    {
                        CurrentUnitProduction = 0;

                        int initialExperience = 0;

                        if (Buildings.Contains(BuildingType.Barracks)) initialExperience += 15;
                        if (Buildings.Contains(BuildingType.Armory)) initialExperience += 15;
                        if (Buildings.Contains(BuildingType.Stable) &&
                            vm.Type == UnitType.Chariot || vm.Type == UnitType.HorseArcher || vm.Type == UnitType.Horseman || vm.Type == UnitType.Knight || vm.Type == UnitType.Crusader
                            ) initialExperience += 15;

                        UnitConstructed?.Invoke(this, new UnitConstructedEventArgs(this, vm.Type, Owner, _centerTile, initialExperience));
                    }
                }
            }


            BuildingBuildViewModel buildingVM = ConstructableBuildingsView.CurrentItem as BuildingBuildViewModel;

            if (buildingVM != null)
            {
                if (buildingVM.Type == BuildingType.None) // special case, basically we're saying we want to produce science
                {
                    SciencePerTurn += ProductionPerTurn;
                }
                else
                {
                    CurrentBuildingProduction += ProductionPerTurn; // only 'produce' if there is a selected 'thing' to produce

                    if (CurrentBuildingProduction >= buildingVM.Cost)
                    {
                        CurrentBuildingProduction = 0;

                        object current = ConstructableBuildingsView.CurrentItem;

                        ConstructableBuildingsView.Filter = null;
                        ConstructableBuildingsView.Filter = item =>
                        {
                            BuildingBuildViewModel vm2 = item as BuildingBuildViewModel;
                            bool filter = (vm2 != null && vm2.IsAvailable && vm2.IsResearched && !Buildings.Contains(vm2.Type));
                            return filter;
                        };

                        ConstructableBuildingsView.MoveCurrentTo(current);

                        BuildingConstructed?.Invoke(this, new BuildingConstructedEventArgs(this, buildingVM.Type, Owner));
                        Buildings.Add(buildingVM.Type);
                    }
                }
            }
        }  
    }
}
