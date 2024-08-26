using System.Collections.Generic;
using System.Linq;

namespace OpenCiv.Engine {
    public static class Rules {

        public const double RIVER_COMBAT_MOD = 2.0f;
        public const double SWAMP_TERRAIN_COMBAT_MOD = 2.0f;
        public const double WOODLAND_TERRAIN_COMBAT_MOD = 3.0f;
        public const double WOODLANDHILL_TERRAIN_COMBAT_MOD = 4.0f;
        public const double WOUNDED_UNIT_COMBAT_MOD = 2.0f;
        public const double HORSE_VS_FOOT_COMBAT_MOD = 4.0f;
        public const double FORTIFYING_COMBAT_MOD = 1.0f;
        public const double FORTIFIED_COMBAT_MOD = 3.0f;
        public const double FORTRESS_COMBAT_MOD = 6.0f;
        public const double COMBAT_CONSTANT = 30.0f;

        public const double BONUS_ANTI_HORSE_COMBAT_MOD = 3.0f;
        public const double BONUS_OPEN_TERRAIN_COMBAT_MOD = 3.0f;
        public const double BONUS_BERSERKER_COMBAT_MOD = 5.0f;

        public const double CITY_GOOD_MOD = 5.0f;
        public const double CITY_BAD_MOD = 5.0f;

        public const double CITY_WALLS_COMBAT_MOD = 5.0f;
        public const double CASTLE_COMBAT_MOD = 5.0f;

        public const double CITY_WALLS_HIT_POINT_BONUS = 100.0;
        public const double ARSENAL_HIT_POINT_BONUS = 50.0;
        public const double CASTLE_HIT_POINT_BONUS = 50.0;

        public const int LIBRARY_SCIENCE_POINTS = 2;
        public const double UNIVERSITY_SCIENCE_MOD = 0.25;

        public const int WORKSHOP_PRODUCTION_POINTS = 2;
        public const int STONEWORKS_PRODUCTION_POINTS = 1;

        public const int GRANARY_TILE_WHEAT_FOOD_POINTS = 1;
        public const int GRANARY_CITY_FOOD_POINTS = 2;

        public const int MARKET_GOLD_POINTS = 2;
        public const double MARKET_GOLD_MOD = 0.25;

        public const int FORGE_TILE_IRON_PRODUCTION_POINTS = 1;
        public const double FORGE_UNIT_PRODUCTION_MOD = 0.15;

        public const int EXP_REQUIRED_FOR_PROMOTION = 35;

        public static bool HasTechForImprovement(IList<Tech> techs, ImprovementType improvement)
        {
            switch (improvement) 
            {
                case ImprovementType.Farms:
                    if (techs.FirstOrDefault(t => t.Name == "Irrigation") != null) return true;
                    else return false;
                case ImprovementType.Mines:
                    if (techs.FirstOrDefault(t => t.Name == "Mining") != null) return true;
                    else return false;
                case ImprovementType.Fortress:
                    if (techs.FirstOrDefault(t => t.Name == "Masonry") != null) return true;
                    else return false;
            }

            return true;
        }

        public static UnitType UpgradePath(Unit unit)
        {
            UnitType type = unit.Type;

            if (type == UnitType.Settler || type == UnitType.None || type == UnitType.Builder || type == UnitType.BarbarianLeader || type == UnitType.Catapult) return type;

            Civilization civ = unit.Owner;

            if (type == UnitType.Slinger && civ.CanBuild(UnitType.Archer)) return UnitType.Archer;
            if ((type == UnitType.Slinger || type == UnitType.Archer) && civ.CanBuild(UnitType.Bowman)) return UnitType.Bowman;
            if ((type == UnitType.Slinger || type == UnitType.Archer || type == UnitType.Bowman) && civ.CanBuild(UnitType.Crossbowman)) return UnitType.Crossbowman;
            if ((type == UnitType.Slinger || type == UnitType.Archer || type == UnitType.Bowman || type == UnitType.Crossbowman) && civ.CanBuild(UnitType.Musketman)) return UnitType.Musketman;

            if (type == UnitType.Warrior && civ.CanBuild(UnitType.Swordsman)) return UnitType.Swordsman;
            if ((type == UnitType.Warrior || type == UnitType.Swordsman) && civ.CanBuild(UnitType.Legion)) return UnitType.Legion;
            if ((type == UnitType.Warrior || type == UnitType.Swordsman || type == UnitType.Legion) && civ.CanBuild(UnitType.Longswordsman)) return UnitType.Longswordsman;
            if ((type == UnitType.Warrior || type == UnitType.Swordsman || type == UnitType.Legion || type == UnitType.Longswordsman) && civ.CanBuild(UnitType.Musketman)) return UnitType.Musketman;

            if (type == UnitType.Spearman && civ.CanBuild(UnitType.Pikeman)) return UnitType.Pikeman;
            if ((type == UnitType.Spearman || type == UnitType.Pikeman) && civ.CanBuild(UnitType.Musketman)) return UnitType.Musketman;

            if (type == UnitType.Chariot && civ.CanBuild(UnitType.Horseman)) return UnitType.Horseman;
            if ((type == UnitType.Chariot || type == UnitType.Horseman) && civ.CanBuild(UnitType.Knight)) return UnitType.Knight;
            if ((type == UnitType.Chariot || type == UnitType.Swordsman || type == UnitType.Knight) && civ.CanBuild(UnitType.Crusader)) return UnitType.Crusader;

            return type;
        }

        public static IEnumerable<UnitType> ValidUnitsForProductionByTech(IList<Tech> techs)
        {
            List<UnitType> units = new List<UnitType>();

            units.Add(UnitType.Builder);
            units.Add(UnitType.Settler);
            units.Add(UnitType.Warrior);
            units.Add(UnitType.Slinger);

            if (techs == null || techs.Count == 0) return units;

            if (techs.FirstOrDefault(t => t.Name == "Archery") != null) units.Add(UnitType.Archer);
            if (techs.FirstOrDefault(t => t.Name == "Bronze Working") != null) units.Add(UnitType.Spearman);
            if (techs.FirstOrDefault(t => t.Name == "Iron Working") != null) units.Add(UnitType.Swordsman);
            if (techs.FirstOrDefault(t => t.Name == "Machinery") != null) units.Add(UnitType.Crossbowman);
            if (techs.FirstOrDefault(t => t.Name == "Horseback Riding") != null) units.Add(UnitType.Horseman);
            if (techs.FirstOrDefault(t => t.Name == "Wheel") != null) units.Add(UnitType.Chariot);
            if (techs.FirstOrDefault(t => t.Name == "Mathematics") != null) units.Add(UnitType.Catapult);
            if (techs.FirstOrDefault(t => t.Name == "Stirrups") != null) units.Add(UnitType.Knight);
            if (techs.FirstOrDefault(t => t.Name == "Steel") != null) units.Add(UnitType.Longswordsman);
            if (techs.FirstOrDefault(t => t.Name == "Tactics") != null) units.Add(UnitType.Legion);
            if (techs.FirstOrDefault(t => t.Name == "Gunpowder") != null) units.Add(UnitType.Musketman);
            if (techs.FirstOrDefault(t => t.Name == "Civil Service") != null) units.Add(UnitType.Pikeman);
            if (techs.FirstOrDefault(t => t.Name == "Chivalry") != null) units.Add(UnitType.Knight);
            if (techs.FirstOrDefault(t => t.Name == "Construction") != null) units.Add(UnitType.Bowman);

            if (techs.FirstOrDefault(t => t.Name == "Archery") != null && techs.FirstOrDefault(t => t.Name == "Horseback Riding") != null) units.Add(UnitType.HorseArcher);

            return units;
        }

        public static IEnumerable<BuildingType> ValidBuildingsForProductionByTech(IList<Tech> techs)
        {
            List<BuildingType> buildings = new List<BuildingType>();

            buildings.Add(BuildingType.Monument);

            if (techs == null || techs.Count == 0) return buildings;

            if (techs.FirstOrDefault(t => t.Name == "Pottery") != null) buildings.Add(BuildingType.Granary);
            if (techs.FirstOrDefault(t => t.Name == "Writing") != null) buildings.Add(BuildingType.Library);
            if (techs.FirstOrDefault(t => t.Name == "Education") != null) buildings.Add(BuildingType.University);
            if (techs.FirstOrDefault(t => t.Name == "Chivalry") != null) buildings.Add(BuildingType.Castle);
            if (techs.FirstOrDefault(t => t.Name == "Currency") != null) buildings.Add(BuildingType.Market);
            if (techs.FirstOrDefault(t => t.Name == "Bronze Working") != null) buildings.Add(BuildingType.Barracks);
            if (techs.FirstOrDefault(t => t.Name == "Masonry") != null) buildings.Add(BuildingType.Walls);
            if (techs.FirstOrDefault(t => t.Name == "Engineering") != null) buildings.Add(BuildingType.Aqueduct);
            if (techs.FirstOrDefault(t => t.Name == "Construction") != null) buildings.Add(BuildingType.Colosseum);
            if (techs.FirstOrDefault(t => t.Name == "Mathematics") != null) buildings.Add(BuildingType.Courthouse);
            if (techs.FirstOrDefault(t => t.Name == "Steel") != null) buildings.Add(BuildingType.Armory);
            if (techs.FirstOrDefault(t => t.Name == "Metal Casting") != null) buildings.Add(BuildingType.Forge);
            if (techs.FirstOrDefault(t => t.Name == "Metal Casting") != null) buildings.Add(BuildingType.Workshop);
            if (techs.FirstOrDefault(t => t.Name == "Metallurgy") != null) buildings.Add(BuildingType.Arsenal);
            if (techs.FirstOrDefault(t => t.Name == "Banking") != null) buildings.Add(BuildingType.Bank);
            if (techs.FirstOrDefault(t => t.Name == "Economics") != null) buildings.Add(BuildingType.Windmill);
            //if (techs.FirstOrDefault(t => t.Name == "Astronomy") != null) buildings.Add(BuildingType.Observatory);

            return buildings;
        }

        public static int GetUpgradeCostForUnit(UnitType unitType)
        {
            return GetProductionCostForUnit(unitType) * 2;
        }

        public static int GetProductionCostForUnit(UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.Settler:
                    return 25;
                case UnitType.Builder:
                    return 15;

                case UnitType.Slinger:
                    return 10;
                case UnitType.Archer:
                    return 23;
                case UnitType.Bowman:
                    return 33;
                case UnitType.Crossbowman:
                    return 49;

                case UnitType.Catapult:
                    return 35;

                case UnitType.Warrior:
                    return 18;
                case UnitType.Spearman:
                    return 24;
                case UnitType.Axeman:
                    return 9;
                case UnitType.Swordsman:
                    return 45;
                case UnitType.Legion:
                    return 65;
                case UnitType.Longswordsman:
                    return 85;
                case UnitType.Pikeman:
                    return 90;
                case UnitType.Musketman:
                    return 120;

                case UnitType.Horseman:
                    return 30;
                case UnitType.HorseArcher:
                    return 32;
                case UnitType.Knight:
                    return 100;
                case UnitType.Crusader:
                    return 120;                

                case UnitType.Chariot:
                    return 25;
                
                default:
                    throw new System.InvalidOperationException();
            }

            return 100;
        }

        public static int GetProductionCostForBuilding(BuildingType buildingType)
        {
            switch (buildingType)
            {
                case BuildingType.Aqueduct:
                    return 75;
                case BuildingType.Armory:
                    return 150;
                case BuildingType.Arsenal:
                    return 125;
                case BuildingType.Bank:
                    return 150;
                case BuildingType.Barracks:
                    return 45;
                case BuildingType.Castle:
                    return 200;
                case BuildingType.Colosseum:
                    return 200;
                case BuildingType.Courthouse:
                    return 100;
                case BuildingType.Forge:
                    return 175;
                case BuildingType.Granary:
                    return 50;
                case BuildingType.Library:
                    return 60;
                case BuildingType.Market:
                    return 50;
                case BuildingType.Monument:
                    return 10;
                case BuildingType.Shrine:
                    return 30;
                case BuildingType.Stable:
                    return 80;
                case BuildingType.StoneWorks:
                    return 120;
                case BuildingType.Temple:
                    return 30;
                case BuildingType.University:
                    return 220;
                case BuildingType.Walls:
                    return 140;
                case BuildingType.Windmill:
                    return 100;
                case BuildingType.Workshop:
                    return 145;

                default:
                    throw new System.InvalidOperationException();
            }

            return 100;
        }

        public static bool HasResourceForUnitType(Civilization civ, UnitType unitType)
        {
            int ironCount = civ.GetResourceCount(ResourceType.Iron);
            int horseCount = civ.GetResourceCount(ResourceType.Horse);

            if (ironCount <= 0)
            {
                if (unitType == UnitType.Swordsman || unitType== UnitType.Legion || unitType == UnitType.Longswordsman || unitType == UnitType.Knight)
                {
                    return false;
                }
            }

            if (horseCount <= 0)
            {
                if (unitType == UnitType.Chariot || unitType == UnitType.HorseArcher || unitType == UnitType.Horseman || unitType == UnitType.Knight)
                {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<UnitBuildViewModel> GetValidUnits(Civilization civ)
        {
            List<Tech> techs = civ.Techs.ToList();

            List<UnitBuildViewModel> unitBuildVMs = new List<UnitBuildViewModel>();
            
            unitBuildVMs.Add(new UnitBuildViewModel("[Convert Production to Gold]", 10, UnitType.None, true, true));

            UnitType type = UnitType.Settler;
            unitBuildVMs.Add(new UnitBuildViewModel("Settler", GetProductionCostForUnit(type), type, true, true));

            type = UnitType.Builder;
            unitBuildVMs.Add(new UnitBuildViewModel("Builder", GetProductionCostForUnit(type), type, true, true));

            type = UnitType.Warrior;
            unitBuildVMs.Add(new UnitBuildViewModel("Warrior", GetProductionCostForUnit(type), type, true, true));

            type = UnitType.Slinger;
            unitBuildVMs.Add(new UnitBuildViewModel("Slinger", GetProductionCostForUnit(type), type, true, true));

            if (!(techs == null || techs.Count == 0 || techs[0] == null))
            {
                var validTechs = ValidUnitsForProductionByTech(techs);

                type = UnitType.Archer;
                bool isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Archer", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Bowman;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Bowman", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Spearman;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Spearman", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Swordsman;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Swordsman", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Legion;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Legion", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Longswordsman;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Longswordsman", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Pikeman;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Pikeman", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.HorseArcher;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("HorseArcher", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Horseman;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Horseman", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Knight;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Knight", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Musketman;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Musketman", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Crusader;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Crusader", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Crossbowman;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Crowssbowman", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Chariot;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Chariot", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));

                type = UnitType.Catapult;
                isResearched = validTechs.Contains(type);
                unitBuildVMs.Add(new UnitBuildViewModel("Catapult", GetProductionCostForUnit(type), type, HasResourceForUnitType(civ, type), isResearched));
            }

            var warrior = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Warrior);
            var slinger = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Slinger);


            var swordsman = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Swordsman);
            if (swordsman != null && swordsman.IsAvailable && swordsman.IsResearched) warrior.IsObsolete = true; else if (warrior != null) warrior.IsObsolete = false;

            var legion = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Legion);
            if (legion != null && legion.IsAvailable && legion.IsResearched) swordsman.IsObsolete = true; else if (swordsman != null) swordsman.IsObsolete = false;

            var longswordsman = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Longswordsman);
            if (longswordsman != null && longswordsman.IsAvailable && longswordsman.IsResearched) legion.IsObsolete = true; else if (legion != null) legion.IsObsolete = false;

            var musketman = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Musketman);
            if (musketman != null && musketman.IsAvailable && musketman.IsResearched) longswordsman.IsObsolete = true; else if (longswordsman != null) longswordsman.IsObsolete = false;



            var archer = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Archer);
            if (archer != null && archer.IsAvailable && archer.IsResearched) slinger.IsObsolete = true; else if (slinger != null) slinger.IsObsolete = false;

            var bowman = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Bowman);
            if (bowman != null && bowman.IsAvailable && bowman.IsResearched) archer.IsObsolete = true; else if (archer != null) archer.IsObsolete = false;

            var crossbowman = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Crossbowman);
            if (crossbowman != null && crossbowman.IsAvailable && crossbowman.IsResearched) bowman.IsObsolete = true; else if (bowman != null) bowman.IsObsolete = false;


            var chariot = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Chariot);
            var horseman = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Horseman);

            if (horseman != null && chariot != null && horseman.IsAvailable && horseman.IsResearched) chariot.IsObsolete = true; else if (chariot != null) chariot.IsObsolete = false;

            var knight = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Knight);
            if (knight != null && horseman != null && knight.IsAvailable && knight.IsResearched) horseman.IsObsolete = true; else if (horseman != null) horseman.IsObsolete = false;


            var spearman = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Spearman);

            var pikeman = unitBuildVMs.FirstOrDefault(vm => vm.Type == UnitType.Pikeman);
            if (pikeman != null && spearman != null && pikeman.IsAvailable && pikeman.IsResearched) spearman.IsObsolete = true; else if (spearman != null) spearman.IsObsolete = false;

            return unitBuildVMs.AsEnumerable();
        }

        public static IEnumerable<BuildingBuildViewModel> GetValidBuildings(Civilization civ)
        {
            List<Tech> techs = civ.Techs.ToList();

            List<BuildingBuildViewModel> vms = new List<BuildingBuildViewModel>();

            vms.Add(new BuildingBuildViewModel("[Convert Production to Science]", 10, BuildingType.None, true, true));

            BuildingType type = BuildingType.Monument;
            vms.Add(new BuildingBuildViewModel("Monument", GetProductionCostForBuilding(type), type, true, true));

            type = BuildingType.Shrine;
            vms.Add(new BuildingBuildViewModel("Shrine", GetProductionCostForBuilding(type), type, true, true));

            type = BuildingType.Temple;
            vms.Add(new BuildingBuildViewModel("Temple", GetProductionCostForBuilding(type), type, true, true));

            if (!(techs == null || techs.Count == 0 || techs[0] == null))
            {
                var validTypes = ValidBuildingsForProductionByTech(techs);

                type = BuildingType.Aqueduct;
                bool isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Aqueduct", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Armory;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Armory", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Arsenal;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Arsenal", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Bank;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Bank", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Barracks;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Barracks", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Castle;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Castle", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Colosseum;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Colosseum", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Courthouse;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Courthouse", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Forge;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Forge", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Granary;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Granary", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Library;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Library", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Market;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Market", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Monument;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Monument", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Shrine;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Shrine", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Stable;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Stable", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.StoneWorks;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("StoneWorks", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Temple;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Temple", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.University;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("University", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Walls;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Walls", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Windmill;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Windmill", GetProductionCostForBuilding(type), type, true, isResearched));

                type = BuildingType.Workshop;
                isResearched = validTypes.Contains(type);
                vms.Add(new BuildingBuildViewModel("Workshop", GetProductionCostForBuilding(type), type, true, isResearched));
            }
            
            return vms.AsEnumerable();
        }

        public static int ComputeOneMoveRoadBonus(Unit unit, Tile start, Tile destination)
        {
            if (unit.Locomotion == UnitLocomotion.Foot || unit.Locomotion == UnitLocomotion.Horse || unit.Locomotion == UnitLocomotion.Motor)
            {
                if (unit.RoadMoves > 0 && start.HasRoad && destination.HasRoad)
                {
                    return 1;
                }
            }

            return 0;
        }

        public static int ComputeMovementCost(Unit unit, Tile destination)
        {
            switch(destination.Terrain)
            {
                case TerrainType.Hills:
                case TerrainType.Forest:
                case TerrainType.ForestHills:
                case TerrainType.Swamp:
                case TerrainType.Jungle:
                case TerrainType.Arctic:
                    return 2;
                case TerrainType.Mountains:
                    return 4;
            }

            return 1;
        }

        public static bool IsImprovementValidForTerrain(ImprovementType improvement, TerrainType terrain)
        {
            if (improvement == ImprovementType.None) return true;

            if (improvement == ImprovementType.Fortress && !(terrain == TerrainType.Coast || terrain == TerrainType.Ocean || terrain == TerrainType.Mountains)) return true;

            if (improvement == ImprovementType.Farms && (terrain == TerrainType.Grassland || terrain == TerrainType.Plains || terrain == TerrainType.Hills)) {
                return true;
            }

            if (improvement == ImprovementType.Mines && (terrain == TerrainType.Hills)) {
                return true;
            }

            return false;
        }

        public static bool IsValidTerrainForMove(Unit unit, Tile tile)
        {
            if (tile.Terrain == TerrainType.Mountains) return false;
            if (unit.Locomotion == UnitLocomotion.Sea && (tile.Terrain != TerrainType.Coast || tile.Terrain != TerrainType.Ocean)) return false;
            if ((unit.Locomotion == UnitLocomotion.Foot || unit.Locomotion == UnitLocomotion.Horse || unit.Locomotion == UnitLocomotion.Motor) && (tile.Terrain == TerrainType.Coast || tile.Terrain == TerrainType.Ocean)) return false;

            return true;
        }

        public static int GetMaintenanceCost(BuildingType building)
        {
            switch (building)
            {
                case BuildingType.Aqueduct:
                    return 1;
                case BuildingType.Armory:
                    return 2;
                case BuildingType.Arsenal:
                    return 2;
                case BuildingType.Bank:
                    return 0;
                case BuildingType.Barracks:
                    return 1;
                case BuildingType.Castle:
                    return 3;
                case BuildingType.Colosseum:
                    return 3;
                case BuildingType.Courthouse:
                    return 1;
                case BuildingType.Forge:
                    return 1;
                case BuildingType.Granary:
                    return 0;
                case BuildingType.Library:
                    return 1;
                case BuildingType.Market:
                    return 0;
                case BuildingType.Monument:
                    return 0;
                case BuildingType.Shrine:
                    return 1;
                case BuildingType.Stable:
                    return 2;
                case BuildingType.StoneWorks:
                    return 1;
                case BuildingType.Temple:
                    return 1;
                case BuildingType.University:
                    return 4;
                case BuildingType.Walls:
                    return 1;
                case BuildingType.Windmill:
                    return 1;
                case BuildingType.Workshop:
                    return 1;
            }

            return 0;
        }

        public static int GetRoadBuildTime(Tile tile)
        {
            int turns = 1;

            if (tile.Terrain == TerrainType.Forest || tile.Terrain == TerrainType.ForestHills || tile.Terrain == TerrainType.Swamp || tile.Terrain == TerrainType.Arctic)
            {
                turns += 2;
            }
            else if (tile.Terrain == TerrainType.Hills || tile.Terrain == TerrainType.Tundra)
            {
                turns += 1;
            }

            return turns;
        }

        public static int GetImprovementBuildTime(Tile tile, ImprovementType improvementType)
        {
            int turns = 3;

            if (tile.Terrain == TerrainType.Hills || tile.Terrain == TerrainType.ForestHills || tile.Terrain == TerrainType.Swamp || tile.Terrain == TerrainType.Arctic)
            {
                turns += 3;
            }
            else if (tile.Terrain == TerrainType.Forest || tile.Terrain == TerrainType.Tundra)
            {
                turns += 2;
            }
            else if (tile.Terrain == TerrainType.Desert)
            {
                turns += 1;
            }

            if (improvementType == ImprovementType.Fortress)
            {
                turns += 2;
            }

            if (improvementType == ImprovementType.Farms)
            {
                turns -= 1;
            }

            return turns;
        }

        public static bool IsResourceValidForTerrain(ResourceType resource, TerrainType terrain)
        {
            if (resource == ResourceType.None) return true;

            if (resource == ResourceType.Fish && (terrain == TerrainType.Coast || terrain == TerrainType.Ocean)) {
                return true;
            }

            if ( (resource == ResourceType.Gold || resource == ResourceType.Silver || resource == ResourceType.Iron) && (terrain == TerrainType.Hills) ) {
                return true;
            }

            if (resource == ResourceType.Wheat && (terrain == TerrainType.Plains || terrain == TerrainType.Grassland)) return true;

            if (resource == ResourceType.Horse && (terrain == TerrainType.Plains || terrain == TerrainType.Grassland)) return true;

            if (resource == ResourceType.Buffalo && (terrain == TerrainType.Plains || terrain == TerrainType.Grassland)) return true;

            if (resource == ResourceType.Citrus && (terrain == TerrainType.Plains || terrain == TerrainType.Grassland)) return true;

            return false;
        }

        public static int ComputeFoodForTile(Tile tile)
        {
            if (tile.HasCity)
            {
                return 1;
            }

            int sum = 0;

            switch (tile.Terrain)
            {
                case TerrainType.Grassland:
                    sum += 2;
                    break;
                case TerrainType.Plains:
                    sum += 1;
                    break;
                case TerrainType.Hills:
                    sum += 1;
                    break;
                case TerrainType.Forest:
                    sum += 1;
                    break;
                default:
                    sum += 0;
                    break;
            }

            switch (tile.Improvement)
            {
                case ImprovementType.Farms:
                    if (tile.Terrain == TerrainType.Grassland)
                    {
                        sum += 2;
                    }
                    else
                    {
                        sum += 1;
                    }
                    break;
                case ImprovementType.Mines:
                    sum -= 1;
                    break;
            }

            if (sum <= 0) sum = 0;

            return sum;
        }

        public static int ComputeScienceForTile(Tile tile)
        {
            if (tile.HasCity)
            {
                return 1;
            }

            int sum = 0;

            switch (tile.Terrain)
            {
                case TerrainType.Grassland:
                    sum += 0;
                    break;
                case TerrainType.Plains:
                    sum += 1;
                    break;
                case TerrainType.Hills:
                    sum += 1;
                    break;
                case TerrainType.ForestHills:
                    sum += 1;
                    break;
                case TerrainType.Forest:
                    sum += 1;
                    break;
                default:
                    sum += 0;
                    break;
            }

            if (tile.HasRiver)
            {
                sum += 1;
            }

            return sum;
        }

        public static int ComputeGoldForTile(Tile tile)
        {
            if (tile.HasCity)
            {
                return 1;
            }

            int sum = 0;

            switch (tile.Terrain)
            {
                case TerrainType.Grassland:
                    sum += 1;
                    break;
                case TerrainType.Plains:
                    sum += 2;
                    break;
                case TerrainType.Hills:
                    sum += 2;
                    break;
                case TerrainType.ForestHills:
                    sum += 2;
                    break;
                default:
                    sum += 0;
                    break;
            }

            switch (tile.Improvement)
            {
                case ImprovementType.Mines:
                    sum += 1;
                    break;
            }

            switch(tile.Resource)
            {
                case ResourceType.Gold:
                    sum += 4;

                    if (tile.Improvement == ImprovementType.Mines) sum += 2;

                    break;
                case ResourceType.Silver:
                    sum += 2;

                    if (tile.Improvement == ImprovementType.Mines) sum += 2;

                    break;
                case ResourceType.Fish:
                    sum += 1;
                    break;
                case ResourceType.Iron:
                    sum += 1;
                    break;
            }

            if (sum <= 0) sum = 0;

            return sum;
        }

        public static int ComputeProductionForTile(Tile tile)
        {
            int sum = 0;

            if (tile.HasCity)
            {
                City c = tile.City;

                if (c.Buildings.Contains(BuildingType.Forge))
                {
                    sum += 1;
                }
                if (c.Buildings.Contains(BuildingType.Workshop))
                {
                    sum += 2;
                }
                if (c.Buildings.Contains(BuildingType.Windmill))
                {
                    sum += 1;
                }

                return 1;
            }

            switch (tile.Terrain)
            {
                case TerrainType.Plains:
                case TerrainType.Tundra:
                    sum += 1;
                    break;
                case TerrainType.Hills:
                    sum += 2;
                    break;
                case TerrainType.Forest:
                    sum += 2;
                    break;
                case TerrainType.ForestHills:
                    sum += 3;
                    break;
                default:
                    sum += 0;
                    break;
            }

            if (tile.HasRiver)
            {
                sum += 1;
            }

            switch (tile.Improvement)
            {
                case ImprovementType.Mines:
                    if (tile.Terrain == TerrainType.Hills) sum += 2;
                    else sum += 1;
                    break;
            }

            if (sum <= 0) sum = 0;

            return sum;
        }
    }
}