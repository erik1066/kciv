using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace OpenCiv.Engine
{
    public class World : ObservableObject
    {
        public event EventHandler TileRefreshComplete;

        private Tile [,] _tiles = new Tile[64,64];

        private int _width = 64;
        private int _height = 64;

        private Dictionary<Unit, Tile> _unitDictionary = new Dictionary<Unit, Tile>();

        private Dictionary<City, Tile> _cityDictionary = new Dictionary<City, Tile>();

        public ObservableCollection<Unit> Units { get; set; } = new ObservableCollection<Unit>();
        public ObservableCollection<Tile> Tiles { get; set; } = new ObservableCollection<Tile>();

        public ObservableCollection<Tile> StaticTiles { get; set; } = new ObservableCollection<Tile>();

        public ICollectionView UnitsView { get; set; }
        public ICollectionView TilesView { get; set; }
        public ICollectionViewLiveShaping TilesViewLive { get; set; }
        public int TileCount { get { return Tiles.Count; } }
        public int LandPassableTileCount => Tiles.Where(t => t.IsLandUnitPassable).Count();

        public Unit GetSelectedUnit()
        {
            if (TilesView.CurrentItem == null) return null;
            return (TilesView.CurrentItem as Tile).CurrentUnit;
        }

        private void AssignNeighboringTiles(Tile tile)
        {
            IEnumerable<Tile> tiles = GetBorderingTiles(tile);

            int x = tile.X;
            int y = tile.Y;

            if (y % 2 == 0) // even
            {
                tile.TileNW = tiles.Where(t => t.X == x - 1).Where(t => t.Y == y - 1).FirstOrDefault();
                tile.TileN  = tiles.Where(t => t.X == x - 1).Where(t => t.Y == y).FirstOrDefault();
                tile.TileNE = tiles.Where(t => t.X == x - 1).Where(t => t.Y == y + 1).FirstOrDefault();
                tile.TileSE = tiles.Where(t => t.X == x).Where(t => t.Y == y + 1).FirstOrDefault();
                tile.TileS =  tiles.Where(t => t.X == x + 1).Where(t => t.Y == y).FirstOrDefault();
                tile.TileSW = tiles.Where(t => t.X == x).Where(t => t.Y == y - 1).FirstOrDefault();
            }
            else
            {
                tile.TileNW = tiles.Where(t => t.X == x).Where(t => t.Y == y - 1).FirstOrDefault();
                tile.TileN = tiles.Where(t => t.X == x - 1).Where(t => t.Y == y).FirstOrDefault();
                tile.TileNE = tiles.Where(t => t.X == x).Where(t => t.Y == y + 1).FirstOrDefault();
                tile.TileSE = tiles.Where(t => t.X == x + 1).Where(t => t.Y == y + 1).FirstOrDefault();
                tile.TileS = tiles.Where(t => t.X == x + 1).Where(t => t.Y == y).FirstOrDefault();
                tile.TileSW = tiles.Where(t => t.X == x + 1).Where(t => t.Y == y - 1).FirstOrDefault();
            }
        }

        public int GetHexDistance(int x1, int y1, int x2, int y2)
        {
            int y1d = (x1 << 1) | (y1 & 1);
            int y2d = (x2 << 1) | (y2 & 1);
            int dx = Math.Abs(y2 - y1);
            int dyd = Math.Abs(y2d - y1d);
            return (dx < dyd) ? (dyd - dx) / 2 + dx : dx;
        }

        public IEnumerable<Tile> GetBorderingTiles(Tile tile, Unit unit = null)
        {
            List<Tile> tiles = new List<Tile>(6);// { tile.TileN, tile.TileNE, tile.TileSE, tile.TileS, tile.TileSW, tile.TileNW };

            //if (unit != null)
            //{
            //    List<Tile> tilesToRemove = new List<Tile>();

            //    foreach (var t in tiles)
            //    {
            //        if (!Rules.IsValidTerrainForMove(unit, t))
            //        {
            //            tilesToRemove.Add(t);
            //        }
            //    }

            //    foreach (var t in tilesToRemove)
            //    {
            //        tiles.Remove(t);
            //    }
            //}

            int x = tile.Coordinates.X;
            int y = tile.Coordinates.Y;

            if (y % 2 == 0)
            {
                // get first ring
                int startX = x - 1;
                int endX = x;

                int startY = y - 1;
                int endY = y + 1;

                for (int i = startX; i <= endX; i++)
                {
                    for (int j = startY; j <= endY; j++)
                    {
                        if (i < 0 || j < 0 || j >= _width || i >= _height) continue;

                        Coords c = new Coords(i, j);
                        Tile t = GetTileAt(c);

                        if (tile != t && !tiles.Contains(t))
                        {
                            tiles.Add(t);
                        }
                    }
                }

                if (x + 1 < _height)
                {
                    Coords cBottom = new Coords(x + 1, y);
                    Tile tBottom = GetTileAt(cBottom);
                    if (!tiles.Contains(tBottom))
                    {
                        tiles.Add(tBottom);
                    }
                }
                else
                {

                }
            }
            else
            {
                // get first ring
                int startX = x;
                int endX = x + 1;

                int startY = y - 1;
                int endY = y + 1;

                for (int i = startX; i <= endX; i++)
                {
                    for (int j = startY; j <= endY; j++)
                    {
                        if (i < 0 || j < 0 || j >= _width || i >= _height) continue;

                        Coords c = new Coords(i, j);
                        Tile t = GetTileAt(c);

                        if (tile != t && !tiles.Contains(t))
                        {
                            tiles.Add(t);
                        }
                    }
                }

                if (x - 1 >= 0)
                {
                    Coords cTop = new Coords(x - 1, y);
                    Tile tTop = GetTileAt(cTop);
                    if (!tiles.Contains(tTop))
                    {
                        tiles.Add(tTop);
                    }
                }
            }

            if (unit != null)
            {
                List<Tile> tilesToRemove = new List<Tile>();

                foreach (var t in tiles)
                {
                    if (!Rules.IsValidTerrainForMove(unit, t))
                    {
                        tilesToRemove.Add(t);
                    }
                }

                foreach (var t in tilesToRemove)
                {
                    tiles.Remove(t);
                }
            }

            return tiles.AsEnumerable();
        }

        public IEnumerable<Tile> GetCityOwnedTiles(Tile cityCenter)
        {
            List<Tile> tiles = new List<Tile>();
            List<Tile> tiles2 = new List<Tile>();

            tiles.AddRange(GetBorderingTiles(cityCenter));

            foreach (var t in tiles)
            {
                IEnumerable<Tile> outerTiles = GetBorderingTiles(t);

                foreach (var t2 in outerTiles)
                {
                    if (!tiles.Contains(t2) && !tiles2.Contains(t2) && t2 != cityCenter)
                    {
                        tiles2.Add(t2);
                    }
                }
            }

            foreach (var t in tiles2)
            {
                if (!tiles.Contains(t) && cityCenter != t)
                {
                    tiles.Add(t);
                }
            }

            return tiles;
        }

        public World() 
        {
            string currentFolder = Environment.CurrentDirectory;

            string[] worldMapLines = System.IO.File.ReadAllLines(System.IO.Path.Combine(currentFolder, "terrain\\worldmap.csv"));

            string[] firstWorldMapRow = worldMapLines[0].Split(',');

            string[,] worldMap = new string[worldMapLines.Length, firstWorldMapRow.Length];

            int row = 0;
            foreach (string line in worldMapLines)
            {
                string[] tileDefinitions = line.Split(',');
                int col = 0;

                foreach (string val in tileDefinitions)
                {
                    worldMap[row, col] = val;
                    col++;
                }

                row++;
            }

            int width = firstWorldMapRow.Length;
            int height = worldMapLines.Length;

            UnitsView = CollectionViewSource.GetDefaultView(Units);
            TilesView = new CollectionViewSource { Source = Tiles }.View;

            TilesViewLive = TilesView as ICollectionViewLiveShaping;
            if (TilesViewLive.CanChangeLiveFiltering)
            {
                TilesViewLive.LiveFilteringProperties.Add("CurrentUnit");
                TilesViewLive.LiveFilteringProperties.Add("HasUnit");
                TilesViewLive.LiveFilteringProperties.Add("HasPlayerUnit");
                TilesViewLive.LiveFilteringProperties.Add("City");
                TilesViewLive.LiveFilteringProperties.Add("Coordinates");
                TilesViewLive.LiveFilteringProperties.Add("HasCity");
                TilesViewLive.LiveFilteringProperties.Add("Improvement");
                TilesViewLive.LiveFilteringProperties.Add("IsInRange");
                TilesViewLive.LiveFilteringProperties.Add("Resource");

                TilesViewLive.LiveFilteringProperties.Add("HasRiver");
                TilesViewLive.LiveFilteringProperties.Add("HasRoad");
                TilesViewLive.LiveFilteringProperties.Add("RoadConnections");

                TilesViewLive.LiveFilteringProperties.Add("Food");
                TilesViewLive.LiveFilteringProperties.Add("Gold");
                TilesViewLive.LiveFilteringProperties.Add("Production");
                TilesViewLive.LiveFilteringProperties.Add("Science");

                TilesViewLive.IsLiveFiltering = true;
            }

            //CollectionViewSource.GetDefaultView(Tiles);

            TilesView.CurrentChanged += TilesView_CurrentChanged;            
        
            _width = width;
            _height = height;
            _tiles = new Tile[_height, _width];

            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    string tileDefinition = worldMap[i, j];
                    TerrainType terrainType = GetTerrainTypeForChar(tileDefinition.Substring(0, 1));
                    ResourceType resourceType = ResourceType.None;

                    if (tileDefinition.Contains(":") && tileDefinition.Length >= 3)
                    {
                        string resourceDef = tileDefinition.Substring(2, 1);
                        resourceType = GetResourceTypeForChar(resourceDef);
                    }


                    Tile tile = new Tile(terrainType, resourceType, new Coords(i, j));
                    _tiles[i, j] = tile;
                    Tiles.Add(tile);
                    StaticTiles.Add(tile);
                    RaisePropertyChanged(nameof(Tiles));
                }
            }

            foreach(Tile t in Tiles)
            {
                AssignNeighboringTiles(t);
            }
        }

        //private void AddTileRing(Unit unit, Tile center, Tile destination, int ringNumber, Dictionary<Tile, int> dictionary)
        //{
        //    List<Tile> ring = GetBorderingTiles(center, unit).ToList();

        //    foreach (var tile in ring)
        //    {
        //        if (!Rules.IsValidTerrainForMove(unit, tile)) continue;
        //        if (!dictionary.ContainsKey(tile)) dictionary.Add(tile, ringNumber);
        //        if (dictionary.ContainsKey(destination)) break;
        //        AddTileRing(unit, tile, destination, ringNumber + 1, dictionary);
        //    }
        //}

        public int GetLandDistance(Tile start, Tile end)
        {
            return GetHexDistance(start.X, start.Y, end.X, end.Y);
        }

        public int GetValidLandDistanceForMove(Unit unit, Tile end)
        {
            //Tile start = GetTileForUnit(unit);
            ////Dictionary<Tile, int> tiles = new Dictionary<Tile, int>(2000);
            ////AddTileRing(unit, start, destination, 1, tiles);

            //List<Tile> ring = GetBorderingTiles(start, unit).ToList();

            //if (ring.Contains(destination))
            //{
            //    return 1;
            //}
            //else
            //{
            //    return -1;
            //}

            ////return -1;

            Tile unitTile = GetTileForUnit(unit);
            return GetLandDistance(unitTile, end);
        }

        public void ShowTargeting(Tile center, int distance)
        {
            // TODO: Shorten this

            if (distance < 1) throw new ArgumentOutOfRangeException(nameof(distance));

            IEnumerable<Tile> ring1 = GetBorderingTiles(center);
            List<Tile> ring2 = new List<Tile>();
            List<Tile> ring3 = new List<Tile>();
            List<Tile> ring4 = new List<Tile>();

            if (distance >= 2)
            {
                foreach (var tile in ring1)
                {
                    ring2.AddRange(GetBorderingTiles(tile));
                }
            }

            if (distance >= 3)
            {
                foreach (var tile in ring2)
                {
                    ring3.AddRange(GetBorderingTiles(tile));
                }
            }

            if (distance >= 4)
            {
                foreach (var tile in ring3)
                {
                    ring4.AddRange(GetBorderingTiles(tile));
                }
            }

            List<Tile> finalList = new List<Tile>();
            finalList.AddRange(ring1);
            finalList.AddRange(ring2);
            finalList.AddRange(ring3);
            finalList.AddRange(ring4);

            foreach (var tile in finalList)
            {
                tile.IsInRange = true;
            }
        }

        public void HideTargeting()
        {
            foreach(var t in Tiles.Where(t => t.IsInRange))
            {
                t.IsInRange = false;
            }
        }

        public void ImproveTile(Tile tileToImprove, ImprovementType improvementType)
        {
            if (Rules.IsImprovementValidForTerrain(improvementType, tileToImprove.Terrain))
            {
                tileToImprove.Improvement = improvementType;
            }
        }

        private void TilesView_CurrentChanged(object sender, EventArgs e)
        {
            TileRefreshComplete?.Invoke(this, new EventArgs());
        }

        private ResourceType GetResourceTypeForChar(string s)
        {
            switch (s)
            {
                case "w":
                    return ResourceType.Wheat;
                case "i":
                    return ResourceType.Iron;
                case "h":
                    return ResourceType.Horse;
                case "g":
                    return ResourceType.Gold;
                case "c":
                    return ResourceType.Citrus;
                default:
                    return ResourceType.None;
            }
        }

        private TerrainType GetTerrainTypeForChar(string s)
        {
            switch (s)
            {
                case "g":
                    return TerrainType.Grassland;
                case "p":
                    return TerrainType.Plains;
                case "s":
                    return TerrainType.Coast;
                case "o":
                    return TerrainType.Ocean;
                case "t":
                    return TerrainType.Tundra;
                case "a":
                    return TerrainType.Arctic;
                case "d":
                    return TerrainType.Desert;
                case "j":
                    return TerrainType.Jungle;
                case "r":
                    return TerrainType.RockDesert;
                case "m":
                    return TerrainType.Mountains;
                case "h":
                    return TerrainType.Hills;
                case "f":
                    return TerrainType.Forest;
                case "z":
                    return TerrainType.ForestHills;
                case "w":
                    return TerrainType.Swamp;
                default:
                    return TerrainType.Grassland;
            }
        }

        public Tile GetTileAt(Coords coords) => _tiles[coords.X, coords.Y];

        public Tile GetTileForUnit(Unit unit) => _unitDictionary.ContainsKey(unit) ? _unitDictionary[unit] : null;
        public Tile GetTileForCity(City city) => _cityDictionary[city];

        public Tile GetStartingLocationForPlayer(int playerId)
        {
            switch (playerId)
            {
                case 1: // Barbarians
                    return _tiles[5, 14];
                case 2: // Humans
                    return _tiles[22, 4];
                default:
                    throw new NotImplementedException();
            }
        }

        public void AddOrMoveUnitToTile(Unit unit, Tile tile, bool shiftFocus = false)
        {
            if (tile.CurrentUnit == unit) return;

            if (tile.HasUnit)
            {
                throw new InvalidOperationException();
            }

            if (_unitDictionary.ContainsKey(unit))
            {
                Tile previousTile = _unitDictionary[unit];
                _unitDictionary[unit] = tile;
                previousTile.RemoveUnit();
            }
            else
            {
                _unitDictionary.Add(unit, tile);
            }

            tile.AddUnit(unit);

            if (!Units.Contains(unit))
            {
                Units.Add(unit);
            }

            if (shiftFocus)
            {
                TilesView.MoveCurrentTo(tile);
            }
        }

        public void ForceDefocus()
        {
            TilesView.MoveCurrentTo(null);
        }

        public void ForceFocus(Tile tile)
        {
            TilesView.MoveCurrentTo(tile);
        }

        public void ForceFocus(Unit unit)
        {
            Tile tile = _unitDictionary[unit];
            TilesView.MoveCurrentTo(tile);
        }

        public void ForceRefresh()
        {
            TilesView.Refresh();
            TileRefreshComplete?.Invoke(this, new EventArgs());
        }

        public void RemoveUnit(Unit unit)
        {
            if (_unitDictionary.ContainsKey(unit))
            {
                Tile tile = _unitDictionary[unit];
                _unitDictionary[unit] = tile;
                tile.RemoveUnit();
                _unitDictionary.Remove(unit);
                Units.Remove(unit);
            }
            else
            {
                throw new InvalidOperationException("Unit already removed!");
            }
        }

        public void AddCityToTile(City city, Tile tile)
        {
            if (_cityDictionary.ContainsKey(city)) {
                throw new InvalidOperationException("City already exists on the map, wtf?");
            }
            else {
                _cityDictionary.Add(city, tile);
            }            

            if (tile.HasCity == true) throw new InvalidOperationException("Two cities cannot occupy the same tile!");
            tile.AddCity(city);
            tile.Improvement = ImprovementType.None;
        }
    }
}
