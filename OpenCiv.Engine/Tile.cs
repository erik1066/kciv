using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace OpenCiv.Engine
{
    [DebuggerDisplay("X = {Coordinates.X}, Y = {Coordinates.Y}, Terrain = {Terrain}")]
    public class Tile : ObservableObject
    {
        private TerrainType _terrain = TerrainType.Grassland;
        private ResourceType _resource = ResourceType.None;
        private ImprovementType _improvement = ImprovementType.None;
        private Coords _coords = null;
        private bool _hasRoad = false;
        private bool _isInRange = false; // for ranged attacks
        private bool _isLandUnitPassable = true;

        public Tile TileNW { get; set; }
        public Tile TileN { get; set; }
        public Tile TileNE { get; set; }
        public Tile TileSE { get; set; }
        public Tile TileS { get; set; }
        public Tile TileSW { get; set; }
        public bool IsLandUnitPassable => _isLandUnitPassable;
        public IEnumerable<Tile> BorderingTiles
        {
            get
            {
                List<Tile> tiles = new List<Tile>(6);
                tiles.Add(TileN);
                tiles.Add(TileNE);
                tiles.Add(TileSE);
                tiles.Add(TileS);
                tiles.Add(TileSW);
                tiles.Add(TileNW);
                return tiles.AsEnumerable();
            }
        }

        public bool HasRoadNW { get { return TileNW == null ? false : TileNW.HasRoad; } }
        public bool HasRoadN { get { return TileN == null ? false : TileN.HasRoad; } }
        public bool HasRoadNE { get { return TileNE == null ? false : TileNE.HasRoad; } }
        public bool HasRoadSE { get { return TileSE == null ? false : TileSE.HasRoad; } }
        public bool HasRoadS { get { return TileS == null ? false : TileS.HasRoad; } }
        public bool HasRoadSW { get { return TileSW == null ? false : TileSW.HasRoad; } }

        public string RoadConnections
        {
            get
            {
                StringBuilder roads = new StringBuilder(18);

                if (HasRoad) roads.Append("t");
                else roads.Append("f");

                if (HasRoadN) roads.Append("_n");
                if (HasRoadNE) roads.Append("_ne");
                if (HasRoadSE) roads.Append("_se");
                if (HasRoadS) roads.Append("_s");
                if (HasRoadSW) roads.Append("_sw");                                
                if (HasRoadNW) roads.Append("_nw");

                return roads.ToString();
            }
        }
        
        public TerrainType Terrain
        {
            get { return _terrain; }
            set { _terrain = value; RaisePropertyChanged(nameof(Terrain)); }
        }
        public ResourceType Resource
        {
            get { return _resource; }
            set { _resource = value; RaisePropertyChanged(nameof(Resource)); }
        }
        public ImprovementType Improvement
        {
            get { return _improvement; }
            set { _improvement = value; RaisePropertyChanged(nameof(Improvement)); }
        }
        public bool IsInRange
        {
            get { return _isInRange; }
            set { _isInRange = value; RaisePropertyChanged(nameof(IsInRange)); }
        }
        private ObservableCollection<Unit> Units { get; set; } = new ObservableCollection<Unit>();
        private ObservableCollection<City> Cities { get; set; } = new ObservableCollection<City>();

        public ICollectionView UnitsView { get; set; }
        public ICollectionView CitiesView { get; set; }
        public ICollectionViewLiveShaping UnitsViewLive { get; set; }

        public bool HasUnit
        {
            get { return Units.Count > 0; }
        }

        public bool HasPlayerUnit
        {
            get { return Units.Where(u => u.Owner.IsPlayer).Count() > 0; }
        }

        public bool HasCity
        {
            get { return Cities.Count > 0; }
        }

        public Unit CurrentUnit
        {
            get
            {
                return UnitsView.CurrentItem as Unit;
            }
        }

        public City City
        {
            get
            {
                return Cities.FirstOrDefault();
            }
        }
        
        public Coords Coordinates
        {
            get { return _coords; }
            set
            {
                _coords = value;
                RaisePropertyChanged(nameof(Coordinates));
                RaisePropertyChanged(nameof(X));
                RaisePropertyChanged(nameof(Y));
            }
        }

        public int X
        {
            get { return _coords.X; }
        }

        public int Y
        {
            get { return _coords.Y; }
        }

        public bool HasRiver {get;set;} = false;
        public bool HasRoad
        {
            get
            {
                return _hasRoad;
            }
            set
            {
                _hasRoad = value;
                RaisePropertyChanged(nameof(HasRoad));
                RaisePropertyChanged(nameof(RoadConnections));

                if (TileNW != null) TileNW.RefreshRoadConnections();
                if (TileN != null) TileN.RefreshRoadConnections();
                if (TileNE != null) TileNE.RefreshRoadConnections();
                if (TileSE != null) TileSE.RefreshRoadConnections();
                if (TileS != null) TileS.RefreshRoadConnections();
                if (TileSW != null) TileSW.RefreshRoadConnections();

                RaisePropertyChanged(nameof(RoadConnections));
            }
        }
        //public bool HasMountain { get; set; } = false;
        //public bool HasHills { get; set; } = false;
        //public bool HasForest { get; set; } = false;
        //public bool HasSwamp { get; set; } = false;

        public int Food
        {
            get
            {
                return Rules.ComputeFoodForTile(this);
            }
        }

        public int Production
        {
            get
            {
                return Rules.ComputeProductionForTile(this);
            }
        }

        public int Gold
        {
            get
            {
                return Rules.ComputeGoldForTile(this);
            }
        }

        public int Science
        {
            get
            {
                return Rules.ComputeScienceForTile(this);
            }
        }

        public void RefreshRoadConnections()
        {
            RaisePropertyChanged(nameof(RoadConnections));
            RaisePropertyChanged(nameof(HasRoad));
        }

        public Tile(TerrainType terrain, ResourceType resource, Coords coordinates) 
        {
            if (coordinates.X <    0) throw new ArgumentException(nameof(coordinates));
            if (coordinates.X >= 256) throw new ArgumentException(nameof(coordinates));
            if (coordinates.Y <    0) throw new ArgumentException(nameof(coordinates));
            if (coordinates.Y >= 256) throw new ArgumentException(nameof(coordinates));
            if (Rules.IsResourceValidForTerrain(resource, terrain) == false) throw new ArgumentException(nameof(resource));

            Terrain = terrain;
            Resource = resource;
            Coordinates = coordinates;

            RaisePropertyChanged(nameof(X));
            RaisePropertyChanged(nameof(Y));

            UnitsView = CollectionViewSource.GetDefaultView(Units);
            RaisePropertyChanged(nameof(CurrentUnit));

            UnitsViewLive = UnitsView as ICollectionViewLiveShaping;
            if (UnitsViewLive.CanChangeLiveFiltering)
            {
                UnitsViewLive.LiveFilteringProperties.Add("CurrentlyBuilding");
                UnitsViewLive.LiveFilteringProperties.Add("CombatPower");
                UnitsViewLive.LiveFilteringProperties.Add("HitPoints");
                UnitsViewLive.LiveFilteringProperties.Add("Status");
                UnitsViewLive.LiveFilteringProperties.Add("Kills");
                UnitsViewLive.LiveFilteringProperties.Add("RemainingMoves");
                UnitsViewLive.LiveFilteringProperties.Add("Name");
                UnitsViewLive.IsLiveFiltering = true;
            }

            CitiesView = CollectionViewSource.GetDefaultView(Cities);
            RaisePropertyChanged(nameof(City));

            if (_terrain == TerrainType.Coast || _terrain == TerrainType.Ocean || _terrain == TerrainType.Mountains) _isLandUnitPassable = false;
            else _isLandUnitPassable = true;
        }

        public void AddUnit(Unit unit)
        {
            if (Units.Count != 0) throw new InvalidOperationException();

            Units.Add(unit);
            UnitsView.MoveCurrentTo(unit);
            UnitsView.Refresh();
            UnitsView.MoveCurrentTo(unit);
            UnitsView.Refresh();
            RaisePropertyChanged(nameof(CurrentUnit));
            RaisePropertyChanged(nameof(HasUnit));
            RaisePropertyChanged(nameof(HasPlayerUnit));
        }

        public void RemoveUnit()
        {
            Units.Clear();
            RaisePropertyChanged(nameof(CurrentUnit));
            RaisePropertyChanged(nameof(HasUnit));
            RaisePropertyChanged(nameof(HasPlayerUnit));
            RaisePropertyChanged(nameof(UnitsView));
            UnitsView.MoveCurrentTo(null);
            UnitsView.Refresh();
            UnitsView.MoveCurrentTo(null);
            UnitsView.Refresh();
        }

        public void AddCity(City city)
        {
            if (Cities.Count != 0) throw new InvalidOperationException();

            Cities.Add(city);
            CitiesView.MoveCurrentTo(city);
            RaisePropertyChanged(nameof(City));
        }

        public void RemoveCity()
        {
            Cities.Clear();
            CitiesView.MoveCurrentTo(null);
            RaisePropertyChanged(nameof(City));
        }

        public void Recompute()
        {
            RaisePropertyChanged(nameof(Food));
            RaisePropertyChanged(nameof(Gold));
            RaisePropertyChanged(nameof(Production));
            RaisePropertyChanged(nameof(Science));

            RaisePropertyChanged(nameof(HasPlayerUnit));
        }
    }
}
