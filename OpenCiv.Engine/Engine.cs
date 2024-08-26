using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using OpenCiv.Engine.Events;

namespace OpenCiv.Engine
{
    public class Engine : ObservableObject
    {
        #region Private Members
        private World _world = new World();
        private readonly ObservableCollection<Civilization> _civilizations = new ObservableCollection<Civilization>();
        private int _turn = 0;
        private readonly UnitFactory _unitFactory = new UnitFactory();
        private bool _isProcessingTurn = false;
        private bool _isProcessing = false;
        private bool _isInRangedMode = false;
        private bool _hasPlayerLost = false;
        private bool _hasPlayerWon = false;
        private readonly PopupMessageViewModel _popupViewModel = new PopupMessageViewModel();
        private readonly PromotionManager _promotionManager = new PromotionManager();
        private readonly INetworkAdapter _netAdapter;
        private readonly Dictionary<Unit, ulong> _barbarianUnits = new Dictionary<Unit, ulong>();
        private int _nextUnitId = 0;
        private int _barbarianNodesToAddNextTurn = 0;
        private Pathfinder _pathfinder;
        #endregion // Private Members

        #region Properties
        public UnitAttackViewModel UnitAttackVM { get; set; } = new UnitAttackViewModel();
        public bool IsProcessing { get { return _isProcessing; } set { _isProcessing = value; RaisePropertyChanged(nameof(IsProcessing)); } }
        public PopupMessageViewModel PopupVM => _popupViewModel;
        public bool HasPlayerLost
        {
            get
            {
                return _hasPlayerLost;
            }
            set
            {
                _hasPlayerLost = value;
                RaisePropertyChanged(nameof(HasPlayerLost));
            }
        }
        public bool HasPlayerWon
        {
            get
            {
                return _hasPlayerWon;
            }
            set
            {
                _hasPlayerWon = value;
                RaisePropertyChanged(nameof(HasPlayerWon));
            }
        }
        public int Turn
        {
            get { return _turn;  }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("Turn cannot be less than or equal to zero.");
                
                _turn = value;
                RaisePropertyChanged("Turn");                
            }
        }
        public bool IsProcessingTurn
        {
            get
            {
                return _isProcessingTurn;
            }
            set
            {
                _isProcessingTurn = value;
                RaisePropertyChanged(nameof(IsProcessingTurn));
            }
        }
        public bool IsInRangedMode
        {
            get
            {
                return _isInRangedMode;
            }
            set
            {
                _isInRangedMode = value;
                RaisePropertyChanged(nameof(IsInRangedMode));
            }
        }
        public Unit SelectedUnit => World.GetSelectedUnit();
        public int PlayerTreasury
        {
            get
            {
                if (_civilizations.Count == 0)
                {
                    return 0;
                }
                else
                {
                    foreach(var c in _civilizations)
                    {
                        if (c.IsPlayer)
                        {
                            return c.Treasury;
                        }
                    }

                    throw new InvalidOperationException();
                }
            }
        }
        public Civilization PlayerCivilization => _civilizations.FirstOrDefault(c => c.IsPlayer);
        public Civilization BarbarianCivilization => _civilizations.FirstOrDefault(c => c.IsPlayer == false);
        public TechTree TechTree { get; set; }
        public ObservableCollection<StatusMessage> StatusMessages { get; } = new ObservableCollection<StatusMessage>();
        public ObservableCollection<NodeMessageViewModel> NodeMessages { get; } = new ObservableCollection<NodeMessageViewModel>();
        public World World
        {
            get
            {
                return _world;
            }
            set
            {
                _world = value;
                RaisePropertyChanged(nameof(World));
            }
        }
        public ICollectionView CivilizationsView { get; set; }
        public bool CanFortify => CanSelectedUnitFortify();
        public bool CanActivate => CanSelectedUnitActivate();
        public bool CanFoundCity => CanSelectedUnitFoundCity();
        public bool CanBuildFarm => CanSelectedUnitBuildFarm();
        public bool CanBuildRoad => CanSelectedUnitBuildRoad();
        public bool CanBuildFortress => CanSelectedUnitBuildFortress();
        public bool CanBuildMine => CanSelectedUnitBuildMine();
        public bool CanRangedAttack => CanSelectedUnitRangedAttack();
        public bool CanBombard => CanSelectedUnitBombard();
        public bool CanDisband => CanSelectedUnitDisband();
        public bool CanUpgrade => CanSelectedUnitUpgrade();
        public bool IsPlayerOutOfMoves
        {
            get
            {
                int unitsWithRemainingMoves = PlayerCivilization.Units.Where(u => u.RemainingMoves > 0).Count();
                return unitsWithRemainingMoves == 0;
            }
        }
        #endregion // Properties

        #region Constructors
        public Engine()
        {
            List<Civilization> civilizations = new List<Civilization>();
            Civilization barbarians = new Civilization("Barbarians", "Barbarian", false, 1);
            Civilization romans = new Civilization("Romans", "Roman", true, 2);
            civilizations.Add(barbarians);
            civilizations.Add(romans);

            //{
            //    System.Diagnostics.Debug.Print("-- MOCK NETWORK ADAPTER INSTANTIATED --");
            //    _netAdapter = new Net.MockNetworkAdapter();
            //}
            _netAdapter = new XBeeNetworkAdapter(Properties.Settings.Default.ComPort, Properties.Settings.Default.NodeId);

            Turn = 1;
            foreach (var c in civilizations)
            {
                if (!_civilizations.Contains(c))
                {
                    _civilizations.Add(c);
                }

                c.ComputeCombatPower();
            }

            RaisePropertyChanged(nameof(PlayerTreasury));
            RaisePropertyChanged(nameof(PlayerCivilization));
            CivilizationsView = CollectionViewSource.GetDefaultView(this._civilizations);
            StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.Generic, Message = "Engine initialized" });

            _pathfinder = new Pathfinder(World);

            SetupGame();
        }
        #endregion // Constructors
        
        public void SetEnemyDataContext(Unit unit)
        {
            if (unit == null || unit.Owner == PlayerCivilization || unit.HitPoints <= 0.0 || SelectedUnit == null || SelectedUnit.Owner != PlayerCivilization)
            {
                UnitAttackVM.Reset();
            }
            else
            {
                Tile tile = World.GetTileForUnit(unit);
                CombatReport report = new CombatReport(SelectedUnit, unit, tile, false);
                UnitAttackVM.Set(unit,tile,report);
            }
        }

        private Unit AddUnit(UnitType type, Civilization civ, Tile tile)
        {
            Unit unit = _unitFactory.ProduceUnit(type, civ);

            if (tile.HasUnit)
            {
                IEnumerable<Tile> surroundingTiles = World.GetCityOwnedTiles(tile);

                bool hasMoved = false;

                foreach (var t in surroundingTiles)
                {
                    if (t.HasUnit == false)
                    {
                        if (Rules.IsValidTerrainForMove(unit, t))
                        {
                            World.AddOrMoveUnitToTile(unit, t);
                            hasMoved = true;
                            break;
                        }
                    }
                }

                if (!hasMoved) return null; // unit can't be added...
            }
            else
            {
                World.AddOrMoveUnitToTile(unit, tile);
            }
            civ.AddUnit(unit);

            _nextUnitId++;

            return unit;
        }

        private City AddCity(string name, Civilization civ, Tile tile, int startingSize, bool isCapital)
        {
            City newCity = new City(name, Turn, tile, civ, World.GetCityOwnedTiles(tile), civ.Id, startingSize, isCapital); // barbarian capital                    
            World.AddCityToTile(newCity, tile);
            civ.AddCity(newCity);
            civ.CitiesView.Refresh();

            if (_netAdapter != null && civ != BarbarianCivilization)
            {
                _netAdapter.AddLongTermObjective(tile.X, tile.Y);
            }

            return newCity;
        }

        private void SetupGame()
        {
            TechTree = new TechTree();
            RaisePropertyChanged(nameof(TechTree));

            if (_netAdapter != null)
            {
                _netAdapter.Start();
                System.Threading.Thread.Sleep(1000);
            }
            else
            {
                throw new InvalidOperationException("ZigBee adapter cannot be null");
            }

            foreach (var c in _civilizations)
            {
                if (c == BarbarianCivilization)
                {
                    Tile startingTile = World.GetStartingLocationForPlayer(BarbarianCivilization.Id);
                    Tile kvenlandTile = World.GetTileAt(new Coords(5, 7));
                    Tile sigurdTile = World.GetTileAt(new Coords(2, 7));

                    City nidaros = AddCity(c.GetNextCityName(), c, startingTile, 3, true);
                    AddCity(c.GetNextCityName(), c, kvenlandTile, 2, false);
                    AddCity(c.GetNextCityName(), c, sigurdTile, 2, false);
                    
                    nidaros.AddBuilding(BuildingType.Walls);

                    c.AddTech(TechTree.GetNext(c));
                    c.AddTech(TechTree.GetNext(c));
                    c.AddTech(TechTree.GetNext(c));
                    c.AddTech(TechTree.GetNext(c));

                    c.AddTech(TechTree.GetNext(c));
                    c.AddTech(TechTree.GetNext(c));
                }
                else
                {
                    Tile startingTile = World.GetStartingLocationForPlayer(PlayerCivilization.Id);
                    //IEnumerable<Tile> cityOwnedTiles = World.GetCityOwnedTiles(startingTile);

                    Tile c2 = World.GetTileAt(new Coords(26, 6));
                    //IEnumerable<Tile> c2OwnedTiles = World.GetCityOwnedTiles(c2);

                    Coords warriorCoords = new Coords(startingTile.X, startingTile.Y + 1);
                    Coords builderCoords = new Coords(startingTile.X + 1, startingTile.Y + 2);
                    Coords slingerCoords = new Coords(startingTile.X + 2, startingTile.Y + 2);
                    Coords settlerCoords = new Coords(startingTile.X + 3, startingTile.Y + 3);
                    //Coords catapultCoords = new Coords(startingTile.X + 3, startingTile.Y + 3);

                    Tile warriorTile = World.GetTileAt(warriorCoords);
                    Tile builderTile = World.GetTileAt(builderCoords);
                    Tile slingerTile = World.GetTileAt(slingerCoords);
                    Tile settlerTile = World.GetTileAt(settlerCoords);
                    //Tile catapultTile = World.GetTileAt(catapultCoords);

                    Unit w = AddUnit(UnitType.Warrior, c, warriorTile);
                    AddUnit(UnitType.Builder, c, builderTile);
                    AddUnit(UnitType.Slinger, c, slingerTile);
                    //AddUnit(UnitType.Settler, c, settlerTile);

                    //_promotionManager.PromoteUnit(w);

                    c.IsPlayer = true;

                    AddCity(c.GetNextCityName(), c, startingTile, 2, true);
                    AddCity(c.GetNextCityName(), c, c2, 1, true);

                    //City capital = new City(c.GetNextCityName(), Turn, startingTile, c, cityOwnedTiles, c.Id, 2, true);
                    //World.AddCityToTile(capital, startingTile);
                    //c.AddCity(capital);

                    //City secondCity = new City(c.GetNextCityName(), Turn, c2, c, c2OwnedTiles, c.Id, 1, false);
                    //World.AddCityToTile(secondCity, c2);
                    //c.AddCity(secondCity);

                    c.AddTech(TechTree.GetNext(c));
                    c.AddTech(TechTree.GetNext(c));

                    CivilizationsView.MoveCurrentTo(c);
                }

                c.ComputeCombatPower();

                foreach(var city in c.Cities)
                {
                    city.UnitConstructed += City_UnitConstructed;
                }

                c.RecomputeConstructableUnits();
                c.RecomputeConstructableBuildings();
                c.ResearchCompleted += CivilizationResearchCompleted;
            }

            World.TileRefreshComplete += World_TileRefreshComplete;

            RaisePropertyChanged(nameof(PlayerTreasury));
            RaisePropertyChanged(nameof(PlayerCivilization));

            StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.GameStarted, Message = "Game started" });
            StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.Generic, Message = "The barbarians are restless and seek revenge for old grievances..." });

            IsInRangedMode = false;

            World.ForceRefresh();
        }

        private void CivilizationResearchCompleted(object sender, ResearchCompletedEventArgs e)
        {
            if (e.Civilization == PlayerCivilization)
            {
                PopupVM.Message = $"Research into {e.TechResearched.Name} has been completed!";
                PopupVM.Title = "RESEARCH COMPLETE";
                PopupVM.IsShowing = true;
            }
        }

        private void City_UnitConstructed(object sender, UnitConstructedEventArgs e)
        {
            Tile tileToAddUnitTo = e.StartTile;

            if (e.StartTile.HasUnit)
            {
                var tiles = World.GetBorderingTiles(tileToAddUnitTo);
                tileToAddUnitTo = tiles.FirstOrDefault(t => t.HasUnit == false);
                if (tileToAddUnitTo == null) return; // TODO: Handle this better
            }

            Unit u = AddUnit(e.Type, e.Owner, e.StartTile);
            u.Experience = e.InitialExperience;

            if (e.Type == UnitType.Settler)
            {
                e.City.AdjustForSettlerBuild();
            }

            StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.UnitConstructed, Message = $"{e.City.Name} produced {u.Type}" });
        }

        private void World_TileRefreshComplete(object sender, EventArgs e)
        {
            UpdateActions();
        }

        private void UpdateActions()
        {
            RaisePropertyChanged(nameof(CanFortify));
            RaisePropertyChanged(nameof(CanActivate));
            RaisePropertyChanged(nameof(CanFoundCity));
            RaisePropertyChanged(nameof(CanBuildFarm));
            RaisePropertyChanged(nameof(CanBuildRoad));
            RaisePropertyChanged(nameof(CanBuildMine));
            RaisePropertyChanged(nameof(CanRangedAttack));
            RaisePropertyChanged(nameof(CanBombard));
            RaisePropertyChanged(nameof(CanDisband));
            RaisePropertyChanged(nameof(CanUpgrade));
            RaisePropertyChanged(nameof(CanBuildFortress));
        }

        public void ExitRangedMode()
        {
            IsInRangedMode = false;
            World.HideTargeting();
            //World.ForceRefresh();
        }

        #region Commands

        #region Farm Build
        private bool CanSelectedUnitBuildFarm()
        {
            if (SelectedUnit == null || SelectedUnit.RemainingMoves == 0 || SelectedUnit.Status == UnitStatus.Working) return false;
            if (SelectedUnit.Type != UnitType.Builder) return false;

            Tile tile = World.GetTileForUnit(SelectedUnit);
            if (tile.Improvement == ImprovementType.Farms) return false;

            if (!Rules.IsImprovementValidForTerrain(ImprovementType.Farms, tile.Terrain)) return false;

            return true;
        }
        public ICommand BuildFarmSelectedUnitCommand { get { return new RelayCommand(BuildFarmSelectedUnit, CanSelectedUnitBuildFarm); } }
        private void BuildFarmSelectedUnit()
        {
            BuildFarm(SelectedUnit);
            //Unit selected = SelectedUnit;
            //World.ForceRefresh();            
        }
        private void BuildFarm(Unit unit)
        {
            Tile tile = World.GetTileForUnit(unit);
            if (unit == null || unit.RemainingMoves == 0 || unit.Status != UnitStatus.None || !Rules.IsImprovementValidForTerrain(ImprovementType.Farms, tile.Terrain)) return;
            unit.TryBeginBuilding(tile, Turn, ImprovementType.Farms);
            UpdateActions();
        }
        #endregion // Farm Build

        #region Mine Build
        private bool CanSelectedUnitBuildMine()
        {
            if (SelectedUnit == null || SelectedUnit.RemainingMoves == 0 || SelectedUnit.Status == UnitStatus.Working) return false;
            if (SelectedUnit.Type != UnitType.Builder) return false;

            Tile tile = World.GetTileForUnit(SelectedUnit);

            if (tile.Improvement == ImprovementType.Mines) return false;

            if (SelectedUnit.Owner.Techs.FirstOrDefault(t => t.Name.Equals("Mining", StringComparison.OrdinalIgnoreCase)) == null) return false;

            if (!Rules.IsImprovementValidForTerrain(ImprovementType.Mines, tile.Terrain)) return false;

            return true;
        }
        public ICommand BuildMineSelectedUnitCommand { get { return new RelayCommand(BuildMineSelectedUnit, CanSelectedUnitBuildMine); } }
        private void BuildMineSelectedUnit()
        {
            BuildMine(SelectedUnit);
            //Unit selected = SelectedUnit;
            //World.ForceRefresh();
        }
        private void BuildMine(Unit unit)
        {
            Tile tile = World.GetTileForUnit(unit);
            if (unit == null || unit.RemainingMoves == 0 || unit.Status != UnitStatus.None || !Rules.IsImprovementValidForTerrain(ImprovementType.Mines, tile.Terrain)) return;
            unit.TryBeginBuilding(tile, Turn, ImprovementType.Mines);
            UpdateActions();
        }
        #endregion // Mine Build

        #region Road
        private bool CanSelectedUnitBuildRoad()
        {
            if (SelectedUnit == null || SelectedUnit.RemainingMoves == 0 || SelectedUnit.Status == UnitStatus.Working) return false;
            if (SelectedUnit.Type != UnitType.Builder) return false;

            Tile tile = World.GetTileForUnit(SelectedUnit);
            if (tile.Terrain == TerrainType.Coast || tile.Terrain == TerrainType.Ocean || tile.HasRoad) return false;

            return true;
        }
        public ICommand BuildRoadSelectedUnitCommand { get { return new RelayCommand(BuildRoadSelectedUnit, CanSelectedUnitBuildRoad); } }
        private void BuildRoadSelectedUnit()
        {
            BuildRoad(SelectedUnit);          
        }
        private void BuildRoad(Unit unit)
        {
            Tile tile = World.GetTileForUnit(unit);
            if (unit == null || unit.RemainingMoves == 0 || unit.Status != UnitStatus.None) return;
            unit.TryBeginBuildingRoad(tile, Turn);
            UpdateActions();
        }
        #endregion // Road

        #region Fortress
        private bool CanSelectedUnitBuildFortress()
        {
            if (SelectedUnit == null || SelectedUnit.RemainingMoves == 0 || SelectedUnit.Status == UnitStatus.Working) return false;
            if (SelectedUnit.Type != UnitType.Builder) return false;

            Tile tile = World.GetTileForUnit(SelectedUnit);
            if (tile.Terrain == TerrainType.Coast || tile.Terrain == TerrainType.Ocean || tile.Improvement == ImprovementType.Fortress || tile.Terrain == TerrainType.Mountains) return false;

            if (!PlayerCivilization.CanBuildFortress) return false;

            return true;
        }
        public ICommand BuildFortressSelectedUnitCommand { get { return new RelayCommand(BuildFortressSelectedUnit, CanSelectedUnitBuildFortress); } }
        private void BuildFortressSelectedUnit()
        {
            BuildFortress(SelectedUnit);
        }
        private void BuildFortress(Unit unit)
        {
            Tile tile = World.GetTileForUnit(unit);
            if (unit == null || unit.RemainingMoves == 0 || unit.Status != UnitStatus.None) return;
            unit.TryBeginBuilding(tile, Turn, ImprovementType.Fortress);
            UpdateActions();
        }
        #endregion // Road

        #region City
        private bool CanSelectedUnitFoundCity()
        {
            if (SelectedUnit == null || SelectedUnit.RemainingMoves == 0 || !SelectedUnit.HasSpecialAction(UnitSpecialAction.BuildCity)) return false;

            Tile tile = World.GetTileForUnit(SelectedUnit);
            if (tile.Terrain == TerrainType.Coast || tile.Terrain == TerrainType.Ocean || tile.Terrain == TerrainType.Mountains) return false;

            return true;
        }
        public ICommand FoundCitySelectedUnitCommand { get { return new RelayCommand(FoundCitySelectedUnit, CanSelectedUnitFoundCity); } }
        private void FoundCitySelectedUnit()
        {
            FoundCity(SelectedUnit);
        }
        private void FoundCity(Unit unit)
        {
            Tile tile = World.GetTileForUnit(unit);
            if (unit == null || unit.RemainingMoves == 0 || !unit.HasSpecialAction(UnitSpecialAction.BuildCity)) return;

            Civilization owner = unit.Owner;

            AddCity(owner.GetNextCityName(), owner, tile, 1, false);

            RemoveUnit(unit);

            UpdateActions();

            World.ForceRefresh();
        }
        #endregion // Road

        #region Disband
        private bool CanSelectedUnitDisband()
        {
            if (SelectedUnit == null || SelectedUnit.RemainingMoves == 0 || SelectedUnit.Status == UnitStatus.Working) return false;
            return true;
        }
        public ICommand DisbandSelectedUnitCommand { get { return new RelayCommand(DisbandSelectedUnit, CanSelectedUnitDisband); } }
        private void DisbandSelectedUnit()
        {
            if (SelectedUnit != null)
            {
                DisbandUnit(SelectedUnit);
            }          
        }
        private void DisbandUnit(Unit unit)
        {
            int gold = Rules.GetProductionCostForUnit(unit.Type);
            unit.Owner.Treasury += gold;
            RemoveUnit(unit);
        }
        #endregion // Disband

        #region Upgrade
        private bool CanSelectedUnitUpgrade()
        {
            if (SelectedUnit == null || SelectedUnit.RemainingMoves == 0 || SelectedUnit.Status == UnitStatus.Fortified || SelectedUnit.Status == UnitStatus.Fortifying) return false;
            if (SelectedUnit.Type == UnitType.BarbarianLeader) return false;

            if (Rules.GetUpgradeCostForUnit(SelectedUnit.Type) > PlayerCivilization.Treasury) return false;

            if (Rules.UpgradePath(SelectedUnit) == SelectedUnit.Type) return false;

            return true;
        }
        public ICommand UpgradeSelectedUnitCommand { get { return new RelayCommand(UpgradeSelectedUnit, CanSelectedUnitUpgrade); } }
        private void UpgradeSelectedUnit()
        {
            if (SelectedUnit != null)
            {
                UpgradeUnit(SelectedUnit);
            }
        }
        private void UpgradeUnit(Unit unit)
        {
            UnitType newType = Rules.UpgradePath(SelectedUnit);

            Tile tile = World.GetTileForUnit(unit);

            RemoveUnit(unit);
            Unit newUnit = AddUnit(newType, unit.Owner, tile);
            newUnit.RemainingMoves = 0;
            _promotionManager.TransferPromotions(unit, newUnit);

            Civilization civ = unit.Owner;
            civ.Treasury -= Rules.GetUpgradeCostForUnit(newType);
        }
        #endregion // Upgrade

        #region Ranged Attack
        private bool CanSelectedUnitRangedAttack()
        {
            if (SelectedUnit == null || SelectedUnit.RemainingMoves == 0) return false;
            if (SelectedUnit.AttackMethod == AttackMethod.Ranged || SelectedUnit.AttackMethod == AttackMethod.PrecisionStrike) return true;
            return false;
        }
        public ICommand RangedAttackSelectedUnitCommand { get { return new RelayCommand(RangedAttackSelectedUnit, CanSelectedUnitRangedAttack); } }
        private void RangedAttackSelectedUnit()
        {
            IsInRangedMode = true;
            World.ShowTargeting(World.GetTileForUnit(SelectedUnit), SelectedUnit.AttackRange);
        }
        public void TryRangedAttackWithSelectedUnit(Tile defenderTile)
        {
            if (defenderTile.HasUnit == false && defenderTile.HasCity == false) return; 

            Unit attacker = SelectedUnit;
            if (defenderTile.HasUnit)
            {
                Unit defender = defenderTile.CurrentUnit;
                DoCombat(attacker, defender, defenderTile);
            }
            ExitRangedMode();
        }
        #endregion // Ranged Attack

        #region Bombardment
        private bool CanSelectedUnitBombard()
        {
            if (SelectedUnit == null || SelectedUnit.RemainingMoves < SelectedUnit.MaxMoves) return false;
            if (SelectedUnit.AttackMethod == AttackMethod.IndiscriminateBombardment) return true;
            return false;
        }
        public ICommand BombardAttackSelectedUnitCommand { get { return new RelayCommand(RangedAttackSelectedUnit, CanSelectedUnitBombard); } }
        #endregion // Bombardment

        #region Fortify
        private bool CanSelectedUnitFortify()
        {
            // unit must have moves left and not be already fortified/fortifying
            if (SelectedUnit == null || SelectedUnit.RemainingMoves == 0 || SelectedUnit.Status == UnitStatus.Fortified || SelectedUnit.Status == UnitStatus.Fortifying || SelectedUnit.AttackMethod == AttackMethod.None) return false;
            return true;
        }
        public ICommand FortifySelectedUnitCommand { get { return new RelayCommand(FortifySelectedUnit, CanSelectedUnitFortify); } }
        private void FortifySelectedUnit()
        {
            Fortify(SelectedUnit);

            Unit selected = SelectedUnit;
            //World.ForceDefocus();
            //World.ForceFocus(selected);
            //World.ForceRefresh();// TODO Re-enable?
        }
        private void Fortify(Unit unit)
        {
            if (unit == null || unit.RemainingMoves == 0 || unit.Status == UnitStatus.Fortified || unit.Status == UnitStatus.Fortifying) return;
            unit.TryStartFortify(Turn);
            UpdateActions();
        }
        #endregion // Fortify

        #region Activate
        private bool CanSelectedUnitActivate()
        {
            if (SelectedUnit == null || SelectedUnit.Status == UnitStatus.None) return false;
            return true;
        }
        public ICommand ActivateSelectedUnitCommand { get { return new RelayCommand(ActivateSelectedUnit, CanSelectedUnitActivate); } }
        private void ActivateSelectedUnit()
        {
            Activate(SelectedUnit);
            Unit selected = SelectedUnit;            
            //World.ForceRefresh();
            World.ForceDefocus();
            World.ForceFocus(selected);
        }
        private void Activate(Unit unit)
        {
            if (unit == null) return;
            unit.Status = UnitStatus.None;
            UpdateActions();
        }
        #endregion Activate

        private bool CanExecuteDoTurn() { return !IsProcessing && !IsProcessingTurn; }
        public ICommand DoTurnCommand { get { return new RelayCommand(DoTurn, CanExecuteDoTurn); } }
        private void DoTurn()
        {
            IsInRangedMode = false;

            IsProcessingTurn = true;

            NodeUpdate?.Invoke(this, new EventArgs());

            if (Turn == 2)
            {
                Tile axemanTile = World.GetTileAt(new Coords(5, 15));
                Tile leaderTile = World.GetTileAt(new Coords(5, 7));

                Civilization c = BarbarianCivilization;

                AddUnit(UnitType.Axeman, c, axemanTile);
                AddUnit(UnitType.BarbarianLeader, c, leaderTile);
                AddUnit(UnitType.Axeman, c, World.GetTileAt(new Coords(7, 14)));
                AddUnit(UnitType.Axeman, c, World.GetTileAt(new Coords(6, 9)));
                AddUnit(UnitType.Axeman, c, World.GetTileAt(new Coords(8, 8)));
                AddUnit(UnitType.Archer, c, World.GetTileAt(new Coords(8, 12)));
                AddUnit(UnitType.Archer, c, World.GetTileAt(new Coords(5, 14)));
                AddUnit(UnitType.HorseArcher, c, World.GetTileAt(new Coords(7, 6)));
                AddUnit(UnitType.Horseman, c, World.GetTileAt(new Coords(7, 7)));
                AddUnit(UnitType.Horseman, c, World.GetTileAt(new Coords(7, 8)));
                AddUnit(UnitType.Horseman, c, World.GetTileAt(new Coords(7, 9)));
                AddUnit(UnitType.Horseman, c, World.GetTileAt(new Coords(7, 10)));
                AddUnit(UnitType.HorseArcher, c, World.GetTileAt(new Coords(7, 5)));
                AddUnit(UnitType.Horseman, c, World.GetTileAt(new Coords(7, 17)));
                AddUnit(UnitType.Catapult, c, World.GetTileAt(new Coords(6, 13)));
                AddUnit(UnitType.Catapult, c, World.GetTileAt(new Coords(6, 15)));

                List<Unit> barbUnits = BarbarianCivilization.Units.ToList();

                IDictionary<ulong, string> nodes = _netAdapter.GetNetworkNodes();

                int i = 0;
                foreach (var node in nodes)
                {
                    if (i >= barbUnits.Count) break;
                    _barbarianUnits.Add(barbUnits[i], node.Key);
                    Tile t = World.GetTileForUnit(barbUnits[i]);
                    _netAdapter.AssignUnitToNode(node.Key, barbUnits[i], t.X, t.Y);

                    StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.Generic, Message = $"Node {node.Value} assigned to {barbUnits[i].Type}" });

                    i++;
                }
            }

            Turn = Turn + 1;

            // Heal all unmoved units
            // Finish fortifying units
            // Restore unit move points
            foreach(var civ in _civilizations)
            {
                if (BarbarianCivilization == civ)
                {
                    _netAdapter.BroadcastStartTurn((uint)Turn);
                    
                    DoCheckForNetworkUpdates();

                    _netAdapter.BroadcastEndTurn();
                }

                // figure out which units can now be built; based on new techs researched, resources available or pillaged, etc
                //civ.RecomputeConstructableUnits();

                foreach (var unit in civ.Units)
                {
                    // check for promotions
                    bool needsPromotion = _promotionManager.DoesUnitNeedPromotion(unit);
                    if (needsPromotion)
                    {
                        PromotionType promotedTo = _promotionManager.PromoteUnit(unit);

                        if (promotedTo != PromotionType.None && unit.Owner == PlayerCivilization)
                        {
                            StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.UnitPromotion, Message = $"{unit.Name} was promoted to {promotedTo}" });
                        }
                    }

                    // heal unmoved units
                    if (unit.RemainingMoves == unit.MaxMoves && unit.HitPoints < 100)
                    {
                        var unitTile = World.GetTileForUnit(unit);

                        // Heal when: The unit wasn't attacked this turn, OR we have the March promotion, OR if we're in a friendly city, 
                        // but never heal under any circumstance when surrounded - even if the above conditions are met
                        if (!unit.WasAttackedThisTurn || unit.Promotions.Contains(PromotionType.March) || unitTile.HasCity) 
                        {
                            var tiles = World.GetBorderingTiles(unitTile);

                            int adjacentEnemiesCount = tiles
                                .Where(e => e.HasUnit)
                                .Where(e => e.CurrentUnit.Owner != unit.Owner)
                                .Where(e => e.CurrentUnit.AttackMethod == AttackMethod.Melee)
                                .Count();

                            if (adjacentEnemiesCount < 3)
                            {
                                unit.Heal();

                                if (unit.Owner == PlayerCivilization)
                                {
                                    double healedHp = (100.0 - unit.HitPoints) > 20.0 ? 20 : (100.0 - unit.HitPoints);
                                    StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.Generic, Message = $"{unit.Type} healed {healedHp.ToString("N0")} hp" });
                                }
                            }
                            else if (unit.Owner == PlayerCivilization)
                            {
                                StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.Generic, Message = $"{unit.Type} at {unitTile.Coordinates.X},{unitTile.Coordinates.Y} is surrounded!" });
                            }
                        }
                        else
                        {
                            unit.WasAttackedThisTurn = false;
                        }

                        // Medic heals
                        if (unit.Promotions.Contains(PromotionType.Medic))
                        {
                            unit.Heal(5);

                            var borderingTiles = World.GetBorderingTiles(World.GetTileForUnit(unit));
                            foreach(var t in borderingTiles)
                            {
                                if (t.TileN.HasPlayerUnit) t.TileN.CurrentUnit.Heal(5);
                                if (t.TileNE.HasPlayerUnit) t.TileNE.CurrentUnit.Heal(5);
                                if (t.TileSE.HasPlayerUnit) t.TileSE.CurrentUnit.Heal(5);
                                if (t.TileS.HasPlayerUnit) t.TileS.CurrentUnit.Heal(5);
                                if (t.TileSW.HasPlayerUnit) t.TileSW.CurrentUnit.Heal(5);
                                if (t.TileNW.HasPlayerUnit) t.TileNW.CurrentUnit.Heal(5);
                            }
                        }
                    }

                    // handle fortifying units
                    if (unit.Status == UnitStatus.Fortifying && (unit.TurnFortified + 1) < Turn)
                    {
                        unit.Status = UnitStatus.Fortified;
                        if (unit.Owner == PlayerCivilization)
                        {
                            StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.Generic, Message = $"{unit.Type} finished fortifying" });
                        }
                    }

                    // handle workers/builders
                    if (unit.Type == UnitType.Builder && unit.Status == UnitStatus.Working)
                    {
                        if (unit.TurnBuildWillEnd <= Turn)
                        {
                            Tile tileToImprove = World.GetTileForUnit(unit);

                            if (unit.CurrentlyBuilding == ImprovementType.None)
                            {
                                // road
                                tileToImprove.HasRoad = true;
                                if (unit.Owner == PlayerCivilization)
                                {
                                    StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.ImprovementBuilt, Message = $"Road finished" });
                                }
                            }
                            else
                            {
                                World.ImproveTile(tileToImprove, unit.CurrentlyBuilding);
                                if (unit.Owner == PlayerCivilization)
                                {
                                    StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.ImprovementBuilt, Message = $"{unit.CurrentlyBuilding} finished" });
                                }
                            }

                            unit.Activate();
                        }
                    }

                    // restore move points
                    unit.RemainingMoves = unit.MaxMoves;

                    // restore road bonus move points
                    unit.RoadMoves = unit.MaxMoves;
                }

                int gold = 0;

                foreach (var city in civ.Cities)
                {
                    city.ProcessTurn();
                    gold = gold + city.AdjustedGoldPerTurn;

                    var cityTile = World.GetTileForCity(city);
                    var tiles = World.GetBorderingTiles(cityTile);

                    int adjacentEnemiesCount = tiles
                        .Where(e => e.HasUnit)
                        .Where(e => e.CurrentUnit.Owner != city.Owner)
                        .Where(e => e.CurrentUnit.AttackMethod == AttackMethod.Melee)
                        .Count();

                    if (adjacentEnemiesCount < 3)
                    {
                        city.Heal();
                    }
                    else if (city.Owner == PlayerCivilization)
                    {
                        StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.Generic, Message = $"{city} is under siege!" });
                    }
                }

                civ.Treasury += gold;

                gold -= civ.ComputeUnitMaintenance();
                gold -= civ.ComputeBuildingMaintenance();

                civ.AdjustedGoldPerTurn = gold;

                // recompute combat power
                civ.ComputeCombatPower();
                civ.ComputeResearchProgress();

                if (civ.ResearchTarget == null)
                {
                    civ.SetResearchTarget(TechTree.GetNext(civ));
                }

                if (civ.Treasury < -10 && civ != BarbarianCivilization && PopupVM.IsShowing == false)
                {
                    Unit nextUnitToDisband = civ.GetCostliestUnit();

                    if (civ.IsPlayer)
                    {
                        PopupVM.Title = "UNIT DISBANDED";
                        PopupVM.Message = $"A {nextUnitToDisband.Type} has been disbanded due to a lack of funds in your treasury.";
                        PopupVM.IsShowing = true;
                    }

                    DisbandUnit(nextUnitToDisband);
                }

                civ.UpdateProperties();
            }

            if (_barbarianNodesToAddNextTurn > 0)
            {
                for (int i = 0; i < _barbarianNodesToAddNextTurn; i++)
                {
                    AddNextBarbarian();
                }

                System.Threading.Thread.Sleep(1000);

                _barbarianNodesToAddNextTurn = 0;
            }

            StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.TurnComplete, Message = $"Turn {Turn - 1} complete" });

            //World.ForceRefresh();

            if (PlayerCivilization.Cities.Count() == 0)
            {
                HasPlayerLost = true;
            }

            if (BarbarianCivilization.Cities.Count() == 0)
            {
                HasPlayerWon = true;
            }

            World.ForceDefocus();

            IsProcessingTurn = false;
        }
        #endregion // Commands

        private void DoCheckForNetworkUpdates()
        {
            System.Threading.Thread.Sleep(250);
            
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            uint TIMEOUT = 15000;

            Dictionary<ulong, bool> nodesCompletionStatus = new Dictionary<ulong, bool>();

            foreach(var node in _netAdapter.GetNetworkNodes())
            {
                nodesCompletionStatus.Add(node.Key, false);
            }

            while (_netAdapter.HasNextAction)
            {
                if (nodesCompletionStatus.Where(n => n.Value == true).Count() == nodesCompletionStatus.Count) break;
                if (_netAdapter.HasNextAction == false)
                {
                    System.Threading.Thread.Sleep(5);
                    continue;
                }

                if (sw.Elapsed.TotalMilliseconds > TIMEOUT) break;

                NodeAction nextAction = _netAdapter.ReadNextAction();
                ulong address = nextAction.NodeAddress;

                if (!_barbarianUnits.ContainsValue(address))
                {
                    continue;
                }

                Unit nodeUnit = _barbarianUnits.FirstOrDefault(u => u.Value == address).Key; //currentUnitTile.CurrentUnit;
                ulong nodeAddress = _barbarianUnits.FirstOrDefault(u => u.Value == address).Value;
                string nodeName = _netAdapter.GetNetworkNodes()[nodeAddress];

                Tile currentUnitTile = World.GetTileAt(new Coords(nextAction.CurrentX, nextAction.CurrentY));
                Tile actionUnitTile = World.GetTileAt(new Coords(nextAction.ActionTargetX, nextAction.ActionTargetY));

                if (currentUnitTile.HasUnit == false) continue;

                NodeActionAcknowledgement ack = null;
                NodeActionType actionType = nextAction.Type;

                bool shouldUpdateUI = false;

                System.Diagnostics.Debug.Print("Turn: " + Turn.ToString() + " : " + actionType.ToString() + " : " + nodeName);

                NodeMessageViewModel nodeMessageVM = new NodeMessageViewModel() { Addr64 = address, Addr16 = nextAction.NodeAddress16, Type = actionType, NodeId = nodeName };
                string displayMessage = $"({actionUnitTile.Y}, {actionUnitTile.X})";

                switch (actionType)
                {
                    case NodeActionType.Activate:
                        Activate(nodeUnit);
                        ack = new NodeActionAcknowledgement(actionType, ServerUpdate.None, true);
                        break;
                    case NodeActionType.Fortify:
                        Fortify(nodeUnit);
                        ack = new NodeActionAcknowledgement(actionType, ServerUpdate.None, true);
                        break;
                    case NodeActionType.Move:
                        bool wasMoveSuccessful = TryMoveUnit(nodeUnit, actionUnitTile);
                        ack = new NodeActionAcknowledgement(actionType, ServerUpdate.MovesRemaining, wasMoveSuccessful);
                        shouldUpdateUI = true;
                        displayMessage = "Move";
                        break;
                    case NodeActionType.None:
                        // no-op
                        continue; ;
                    case NodeActionType.EndTurn:
                        if (nodesCompletionStatus.ContainsKey(nodeAddress))
                        {
                            nodesCompletionStatus[nodeAddress] = true;
                        }
                        break;
                    case NodeActionType.Pillage:
                        Pillage(nodeUnit);
                        ack = new NodeActionAcknowledgement(actionType, ServerUpdate.MovesRemaining, true);
                        break;
                    case NodeActionType.Report:
                        displayMessage = "Reported enemy";
                        // server doesn't need to do anything with this other than add to node message output
                        break;
                    case NodeActionType.RequestSitrep:

                        displayMessage = string.Empty;

                        ack = new NodeActionAcknowledgement(actionType, ServerUpdate.AckSitrep, true);
                        // send SITREP response
                        break;
                    case NodeActionType.RequestCombatReport:

                        displayMessage = string.Empty;

                        CombatReport report = null;

                        if (actionUnitTile.HasUnit && nodeUnit.Owner != actionUnitTile.CurrentUnit.Owner)
                        {
                            report = new CombatReport(nodeUnit, actionUnitTile.CurrentUnit, actionUnitTile, false);
                        }
                        else if (actionUnitTile.HasCity && nodeUnit.Owner != actionUnitTile.City.Owner)
                        {
                            report = new CombatReport(nodeUnit, actionUnitTile.City, actionUnitTile);
                        }

                        if (report != null)
                        {
                            ack = new NodeActionAcknowledgement(actionType, ServerUpdate.AckCombatReport, true);
                            ack.CombatReport = report;
                        }

                        break;

                    case NodeActionType.RequestExtendedSitrep:

                        List<Tile> twoRangeTiles = new List<Tile>();
                        List<Tile> oneRangeTiles = new List<Tile>();

                        oneRangeTiles.AddRange(World.GetBorderingTiles(currentUnitTile));

                        foreach (var tile in oneRangeTiles)
                        {
                            List<Tile> twoRangeSet = World.GetBorderingTiles(tile).ToList();

                            foreach (var twoTile in twoRangeSet)
                            {
                                if (twoTile == currentUnitTile) continue;
                                if (oneRangeTiles.Contains(twoTile)) continue;
                                if (twoRangeTiles.Contains(twoTile)) continue;

                                twoRangeTiles.Add(twoTile);
                            }
                        }

                        if (twoRangeTiles.Count != 12)
                        {
                            // wtf
                        }

                        ack = new NodeActionAcknowledgement(actionType, ServerUpdate.AckExtendedSitrep, true);
                        ack.AddTwoRangeTiles(twoRangeTiles);

                        break;

                    case NodeActionType.Attack:

                        CombatReport outcome = null;

                        bool wasAttackAttemptSuccess = true;
                        bool wasKilled = false;

                        try
                        {
                            if (actionUnitTile.HasUnit && nodeUnit.Owner != actionUnitTile.CurrentUnit.Owner)
                            {
                                displayMessage = $"Attack {actionUnitTile.CurrentUnit.Name} at ({actionUnitTile.Y}, {actionUnitTile.Y})";
                                outcome = DoCombat(nodeUnit, actionUnitTile.CurrentUnit, actionUnitTile);
                            }
                            else if (actionUnitTile.HasCity && nodeUnit.Owner != actionUnitTile.City.Owner)
                            {
                                displayMessage = $"Attack city of {actionUnitTile.City.Name} at ({actionUnitTile.Y}, {actionUnitTile.Y})";
                                outcome = DoCityCombat(nodeUnit, actionUnitTile.City, actionUnitTile);
                            }

                            if (outcome.AttackerHitPointLoss > nodeUnit.HitPoints)
                            {
                                displayMessage += $" [was killed in action!]";
                                wasKilled = true;
                            }

                            if (outcome != null)
                            {
                                ack = new NodeActionAcknowledgement(actionType, ServerUpdate.AckCombatReport, wasAttackAttemptSuccess, wasKilled);
                                ack.CombatReport = outcome;
                                shouldUpdateUI = true;
                            }
                        }
                        catch
                        {
                            displayMessage = "Requested an attack, but target may have been killed before attack could be executed by the server";
                            wasAttackAttemptSuccess = false;
                            ack = new NodeActionAcknowledgement(actionType, ServerUpdate.MovesRemaining, wasAttackAttemptSuccess, wasKilled);
                        }

                        break;

                    case NodeActionType.RequestPath:

                        displayMessage = $"Endpoint: ({actionUnitTile.Y}, {actionUnitTile.X})";

                        IEnumerable<Tile> path = _pathfinder.FindPath(currentUnitTile, actionUnitTile);

                        if (path == null) return;

                        ack = new NodeActionAcknowledgement(actionType, ServerUpdate.AckPath, true);
                        ack.Path = path;
                        

                        break;
                }

                if (ack != null)
                {
                    ack.DestinationAddress = nextAction.NodeAddress;
                    ack.RemainingMoves = nodeUnit.RemainingMoves;
                    ack.RemainingHitPoints = (byte)nodeUnit.HitPoints;
                    ack.SitrepTile = currentUnitTile;
                    ack.NewX = World.GetTileForUnit(nodeUnit) == null ? 0 : World.GetTileForUnit(nodeUnit).X;
                    ack.NewY = World.GetTileForUnit(nodeUnit) == null ? 0 : World.GetTileForUnit(nodeUnit).Y;
                    var res = _netAdapter.SendAck(ack);

                    if (res.ApiID == MFToolkit.Net.XBee.XBeeApiType.ZNetTxStatus)
                    {
                        MFToolkit.Net.XBee.ZNetTxStatusResponse response = res as MFToolkit.Net.XBee.ZNetTxStatusResponse;
                        if (response != null)
                        {
                            if (response.DeliveryStatus != MFToolkit.Net.XBee.DeliveryStatusType.Success)
                            {
                                System.Diagnostics.Debug.Print("Delivery failure to " + nodeName);
                            }
                        }
                    }
                    else
                    {
                        nodeMessageVM.ServerAck = true;
                    }

                    if (shouldUpdateUI)
                    {
                        NodeUpdate?.Invoke(this, new EventArgs());
                    }
                }

                NodeMessages.Add(nodeMessageVM);
            }
        }

        public event EventHandler NodeUpdate;

        public void Pillage(Unit unit)
        {
            Tile tileToPillage = World.GetTileForUnit(unit);
            if (tileToPillage.Improvement != ImprovementType.None && tileToPillage.Improvement != ImprovementType.Fortress)
            {
                StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.Generic, Message = $"{unit.Name} pillaged {tileToPillage.Improvement}" });
                tileToPillage.Improvement = ImprovementType.None;
            }
        }

        private void RemoveUnit(Unit unit)
        {
            World.RemoveUnit(unit);
            var civ = unit.Owner;
            civ.RemoveUnit(unit);

            if (_barbarianUnits.ContainsKey(unit))
            {
                _netAdapter.KillUnit(_barbarianUnits[unit]);
                _barbarianUnits.Remove(unit);
                _barbarianNodesToAddNextTurn++;
            }
        }

        private void AddNextBarbarian()
        {
            Tile start = World.GetStartingLocationForPlayer(BarbarianCivilization.Id);

            var r = new System.Random();
            int type = r.Next(1, 3);

            int tileRandomizer = r.Next(1, 15);

            if (tileRandomizer == 1)
            { 
                start = World.GetTileAt(new Coords(22, 5));
            }
            else if (tileRandomizer == 2)
            {
                start = World.GetTileAt(new Coords(15, 19));
            }

            UnitType nextUnitType = BarbarianCivilization.GetBestUnitType((AttackMethod)type);
            Unit nextUnit = null;

            if (!start.HasUnit)
            {
                nextUnit = AddUnit(nextUnitType, BarbarianCivilization, start);                
            }
            else
            {
                IEnumerable<Tile> startBorders = World.GetBorderingTiles(start);
                Tile preferredTile = null;
                foreach(var tile in startBorders.Where(t => t.HasUnit == false))
                {
                    preferredTile = tile;                    
                }

                if (preferredTile != null)
                {
                    nextUnit = AddUnit(nextUnitType, BarbarianCivilization, preferredTile);
                }
            }

            IDictionary<ulong, string> nodes = _netAdapter.GetNetworkNodes();

            if (nextUnit != null)
            {
                nextUnit.Experience += 20;

                _barbarianUnits.Add(nextUnit, 0);
                
                foreach(var node in nodes.Where(n => !n.Value.Equals("MUFFLEY", StringComparison.OrdinalIgnoreCase)))
                {
                   if (!_barbarianUnits.ContainsValue(node.Key))
                    {
                        _barbarianUnits[nextUnit] = node.Key;
                        
                        _netAdapter.AssignUnitToNode(node.Key, nextUnit, World.GetTileForUnit(nextUnit).X, World.GetTileForUnit(nextUnit).Y);
                        StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.Generic, Message = $"Node {node.Value} assigned to {nextUnit.Type}" });

                        int barbCount = BarbarianCivilization.Units.Count();

                        if (barbCount < 3)
                        {
                            nextUnit.Experience += 15;
                        }
                    }
                }
            }
        }

        public bool TryMoveUnit(Unit unit, Tile destination)
        {
            if (unit.RemainingMoves <= 0) return false;
            if (unit.Status != UnitStatus.None) return false;
            if (IsProcessingTurn && unit.Owner.IsPlayer) return false;            
            
            if (!Rules.IsValidTerrainForMove(unit, destination)) return false;

            int distance = World.GetValidLandDistanceForMove(unit, destination);
            if (false /*distance > 1*//* || distance < 0*/) return false; // TODO: Fix - but for now just allow 1 move at a time

            if (destination.HasUnit)
            {
                if (destination.CurrentUnit == unit) return false;
                if (unit.AttackMethod == AttackMethod.None) return false; // do not allow this unit to attack!
                if (unit.Owner == destination.CurrentUnit.Owner) return false; // don't attack your own units

                Unit defender = destination.CurrentUnit;
                DoCombat(unit, defender, destination);

                //World.ForceRefresh();
            }
            else if (destination.HasCity && destination.City.Owner != unit.Owner)
            {
                if (unit.AttackMethod == AttackMethod.None) return false;
                DoCityCombat(unit, destination.City, destination);
            }
            else
            {
                int roadBonus = Rules.ComputeOneMoveRoadBonus(unit, World.GetTileForUnit(unit), destination);

                int moveCost = 1;

                if (roadBonus > 0)
                {
                    moveCost = 0;
                    unit.RoadMoves--;
                }
                else
                {
                    moveCost = Rules.ComputeMovementCost(unit, destination);
                }

                World.AddOrMoveUnitToTile(unit, destination, true);

                unit.RemainingMoves = (unit.RemainingMoves - moveCost < 0) ? 0 : unit.RemainingMoves - moveCost;
            }
            RaisePropertyChanged(nameof(IsPlayerOutOfMoves));

            return true;
        }

        public bool TryMoveSelectedUnit(Tile destination)
        {
            if (SelectedUnit == null) return false;

            IsProcessing = true;

            Unit s = SelectedUnit;
            bool success = TryMoveUnit(SelectedUnit, destination);
            if (success)
            {
                //World.ForceFocus(s);
            }

            UpdateActions();
            IsProcessing = false;

            return success;
        }

        public CombatReport DoCombat(Unit attacker, Unit defender, Tile defenderTile)
        {
            if (attacker.RemainingMoves == 0) return null;

            if (attacker.Owner == defender.Owner) return null;

            Tile attackerTile = World.GetTileForUnit(attacker);
            int distance = World.GetHexDistance(attackerTile.X, attackerTile.Y, defenderTile.X, defenderTile.Y);

            if (attacker.AttackMethod != AttackMethod.Melee && distance > attacker.AttackRange)
            {
                return null;
            }
            else if (attacker.AttackMethod == AttackMethod.Melee && distance > 1)
            {
                return null;
            }

            CombatReport report = new CombatReport(attacker, defender, defenderTile, true);

            attacker.Experience += 5;
            defender.Experience += 5;

            if (attacker.Owner == PlayerCivilization)
            {
                if (attacker.AttackMethod == AttackMethod.Melee)
                {
                    StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.CombatReport, Message = $"Your {attacker.Type} lost {report.AttackerHitPointLoss.ToString("N0")} hp while attacking; enemy {defender.Name} lost {report.DefenderHitPointLoss.ToString("N0")} hp." });
                }
                else
                {
                    StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.CombatReport, Message = $"Your {attacker.Type} attacked {defender.Name} for {report.DefenderHitPointLoss.ToString("N0")} damage" });
                }
            }
            else if (defender.Owner == PlayerCivilization)
            {
                StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.CombatReport, Message = $"Your {defender.Type} lost {report.DefenderHitPointLoss.ToString("N0")} hp while defending; enemy {attacker.Name} lost {report.AttackerHitPointLoss.ToString("N0")} hp." });
            }

            defender.HitPoints -= report.DefenderHitPointLoss;
            attacker.HitPoints -= report.AttackerHitPointLoss;

            if (attacker.HasSpecialAction(UnitSpecialAction.CanMoveAfterAttacking) || attacker.HasPromotion(PromotionType.Blitz))
            {
                attacker.RemainingMoves--;
            }
            else
            {
                attacker.RemainingMoves = 0;
            }

            defender.WasAttackedThisTurn = true;

            if (attacker.HitPoints <= 0)
            {
                defender.Kills++;

                if (attacker.Owner == PlayerCivilization)
                {
                    string messageString = string.Empty;
                    if (defender.Owner == BarbarianCivilization && defender.Type == UnitType.BarbarianLeader)
                    {
                        messageString = $"Your {attacker.Type} was defeated by {defender.Name}!";
                    }
                    else
                    {
                        messageString = $"Your {attacker.Type} was defeated by a {defender.Owner.NameSingular} {defender.Name}!";
                    }

                    StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.UnitLost, Message = messageString });
                }
                else if (defender.Owner == PlayerCivilization)
                {
                    StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.UnitVictorious, Message = $"Your {defender.Type} killed a {attacker.Owner.NameSingular} {attacker.Name} while defending!" });
                }
                
                RemoveUnit(attacker);
            }
            else
            {
                if (attacker == SelectedUnit)
                {
                    World.ForceDefocus();
                    World.ForceFocus(attacker);
                }
            }

            if (defender.HitPoints <= 0)
            {
                RemoveUnit(defender);
                if (attacker != null)
                {
                    attacker.Kills++;

                    if (attacker.Owner == PlayerCivilization)
                    {
                        StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.UnitVictorious, Message = $"Your {attacker.Type} killed a {defender.Owner.Name} {defender.Name}!" });
                    }
                    else if (defender.Owner == PlayerCivilization)
                    {
                        StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.UnitLost, Message = $"Your {defender.Type} was defeated by a {attacker.Owner.Name} {attacker.Name}!" });
                    }
                }
            }

            return report;
        }

        public CombatReport DoCityCombat(Unit attacker, City city, Tile defenderTile)
        {
            if (attacker.RemainingMoves == 0 || attacker.Owner == city.Owner) return null;

            CombatReport report = new CombatReport(attacker, city, defenderTile);

            attacker.Experience += 5;

            if (attacker.Owner == PlayerCivilization)
            {
                StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.CombatReport, Message = $"Your {attacker.Type} attacked {city.Name} for {report.DefenderHitPointLoss.ToString("N0")} damage" });
            }
            else if (city.Owner == PlayerCivilization)
            {
                StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.CombatReport, Message = $"{city.Name} lost {report.DefenderHitPointLoss.ToString("N0")} hp from {attacker.Type} attack!" });
            }

            city.HitPoints -= report.DefenderHitPointLoss;

            if (city.HitPoints < 0) city.HitPoints = 0; // don't let HP drop below zero

            attacker.HitPoints -= report.AttackerHitPointLoss;
            attacker.RemainingMoves = 0;

            if (attacker.HitPoints <= 0)
            {
                if (attacker.Owner == PlayerCivilization)
                {
                    StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.UnitLost, Message = $"Your {attacker.Type} was defeated while attacking {city.Name}!" });
                }
                else if (city.Owner == PlayerCivilization)
                {
                    StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.UnitVictorious, Message = $"{city.Name} killed a {attacker.Owner.Name} {attacker.Type} while defending!" });
                }
                
                RemoveUnit(attacker);
            }
            else
            {
                if (attacker == SelectedUnit)
                {
                    World.ForceDefocus();
                    World.ForceFocus(attacker);
                }
            }

            if (city.HitPoints <= 0)
            {
                if (attacker != null && attacker.AttackMethod == AttackMethod.Melee) // disallow ranged/bombard units to cap
                {
                    if (attacker.Owner == PlayerCivilization)
                    {
                        StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.UnitVictorious, Message = $"Your {attacker.Type} captured {city.Name}!" });
                    }
                    else if (city.Owner == PlayerCivilization)
                    {
                        StatusMessages.Add(new StatusMessage() { Type = StatusMessageType.UnitLost, Message = $"{city.Name} was captured!" });
                    }

                    city.Occupy();
                    Civilization loser = city.Owner;
                    city.Owner = attacker.Owner;
                    loser.RemoveCity(city);
                    Civilization winner = city.Owner;
                    winner.AddCity(city);

                    if (_netAdapter != null && winner != BarbarianCivilization)
                    {
                        // if the player captures a city, make it an objective!
                        _netAdapter.AddLongTermObjective(defenderTile.X, defenderTile.Y);
                    }
                    else if (_netAdapter != null && winner == BarbarianCivilization)
                    {
                        // get rid of the city as an objective if the Barbarians captured it
                        _netAdapter.RemoveLongTermObjective(defenderTile.X, defenderTile.Y);
                    }

                    PlayerCivilization.CitiesView.Refresh();

                    World.ForceRefresh();
                }
            }

            return report;
        }
    }
}