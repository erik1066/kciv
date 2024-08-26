using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C5;

namespace OpenCiv.Engine
{
    internal class Pathfinder
    {
        private readonly World _world;

        [DebuggerDisplay("X = {Tile.Y}, Y = {Tile.X}")]
        private class TileScore : IComparable<TileScore>
        {
            public Tile Tile { get; private set; }
            public TileScore ParentTile { get; private set; }
            public bool HasParent => ParentTile != null;
            public int ParentCount => HasParent == false ? 0 : 1 + ParentTile.ParentCount;
            public bool IsLandUnitPassable => Tile == null ? false : Tile.IsLandUnitPassable;
            public bool HasUnit => Tile.HasUnit;
            public int Score { get; set; }
            public IEnumerable<TileScore> BorderingTiles
            {
                get
                {
                    List<TileScore> borderingTileScores = new List<TileScore>(6);

                    foreach (var tile in Tile.BorderingTiles)
                    {
                        TileScore tileScore = new TileScore(tile, this, ParentCount);
                        borderingTileScores.Add(tileScore);
                    }

                    return borderingTileScores.AsEnumerable();
                }
            }

            public int CompareTo(TileScore that)
            {
                return this.Score - that.Score;
            }

            public TileScore(Tile tile, TileScore parentTile, int score)
            {
                Tile = tile;
                ParentTile = parentTile;
                Score = score;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if (obj.GetType() != GetType())
                    return false;

                TileScore compare = obj as TileScore;

                if (compare == null) return false;

                if (compare.Tile == this.Tile) return true;
                return false;
            }

            public override int GetHashCode()
            {
                return Tile.GetType().GetHashCode() ^ Tile.GetHashCode();
            }
        }
        

        public Pathfinder(World world)
        {
            _world = world;
        }

        private int GetEstimatedScore(Tile start, Tile end)
        {
            return _world.GetLandDistance(start, end);
        }

        public IEnumerable<Tile> FindPath(Tile start, Tile end)
        {
            int MAX_TRAVERSED_TILES = _world.LandPassableTileCount;

            IPriorityQueue<TileScore> openNodes = new IntervalHeap<TileScore>(MAX_TRAVERSED_TILES);
            Dictionary<TileScore, IPriorityQueueHandle<TileScore>> tileHandles = new Dictionary<TileScore, IPriorityQueueHandle<TileScore>>();

            List<TileScore> closedNodes = new List<TileScore>();

            TileScore startNode = new TileScore(start, null, 0);

            IPriorityQueueHandle<TileScore> h = null;

            openNodes.Add(ref h, startNode);

            tileHandles.Add(startNode, h);

            while (openNodes.Count != 0)
            {
                TileScore current = openNodes.DeleteMin();
                closedNodes.Add(current);

                foreach(var neighbor in current.BorderingTiles.Where(t => t.IsLandUnitPassable).Where(t => t.HasUnit == false || (start.HasUnit && t.Tile.CurrentUnit.Owner != start.CurrentUnit.Owner)))
                {
                    if (closedNodes.Contains(neighbor)) continue; // contains
                    
                    int fCost = GetEstimatedScore(neighbor.Tile, end) + neighbor.ParentCount;

                    if (openNodes.Contains(neighbor))
                    {
                        double priority = -1;
                        foreach (var node in openNodes)
                        {
                            if (node.Equals(neighbor))
                            {
                                priority = node.Score;
                                break;
                            }
                        }
                        if (fCost < priority)
                        {
                            IPriorityQueueHandle<TileScore> handle = tileHandles[neighbor];
                            openNodes.Delete(handle);
                            neighbor.Score = fCost;

                            openNodes.Add(ref handle, neighbor); // update the priority of the tile
                        }
                    }
                    else
                    {
                        neighbor.Score = fCost;
                        IPriorityQueueHandle<TileScore> nh = null;

                        openNodes.Add(ref nh, neighbor);

                        tileHandles.Add(neighbor, nh);

                        if (neighbor.Tile.Equals(end))
                        {
                            // found the path
                            List<Tile> path = new List<Tile>();
                            TileScore currentNode = neighbor;
                            while (currentNode.HasParent)
                            {
                                path.Insert(0, currentNode.Tile);
                                currentNode = currentNode.ParentTile;
                            }
                            return path;
                        }
                    }
                }
            }

            //IPriorityQueue<TileScore> openNodes = new IntervalHeap<TileScore>(MAX_TRAVERSED_TILES);

            //List<TileScore> closedNodes = new List<TileScore>();

            //// Add the start node with an F cost of 0
            //openNodes.Add(new TileScore(start, 0));

            //while (openNodes.Count != 0)
            //{
            //    //The one with the least F cost
            //    //Tile current = openNodes.Dequeue();
            //    TileScore current = openNodes.DeleteMin();
            //    closedNodes.Add(current);

            //    foreach (Tile neighbor in current.BorderingTiles.Where(n => n.IsLandUnitPassable))
            //    {
            //        // if we already processed this node
            //        if (closedNodes.FirstOrDefault(n => n.Tile == neighbor) != null) continue; // contains

            //        int fCost = GetEstimatedScore(neighbor, end) + neighbor.ParentCount;

            //        if (openNodes.FirstOrDefault(n => n.Tile == neighbor) != null) // contains
            //        {
            //            double priority = -1;
            //            foreach (var node in openNodes)
            //            {
            //                if (node.Equals(neighbor))
            //                {
            //                    priority = node.Score;
            //                    break;
            //                }
            //            }
            //            if (fCost < priority)
            //            {
            //                openNodes.UpdatePriority(neighbor, fCost);
            //            }
            //        }
            //        else
            //        {
            //            openNodes.Enqueue(neighbor, fCost);
            //            if (neighbor.node.Equals(end))
            //            {
            //                // found the path
            //                List<Room> path = new List<Room>();
            //                AStarNode<Room> currentNode = neighbor;
            //                while (currentNode.parent != null)
            //                {
            //                    path.Insert(0, currentNode.node);
            //                    currentNode = currentNode.parent;
            //                }
            //                return path;
            //            }
            //        }
            //    }

            //}
            // path not found
            return null;
        }
    }
}
