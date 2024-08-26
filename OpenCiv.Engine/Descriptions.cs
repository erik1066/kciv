using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCiv.Engine
{
    public static class Descriptions
    {
        public static string ConvertBonusToDescription(CombatBonusType bonus)
        {
            switch (bonus)
            {
                case CombatBonusType.AntiHorse:
                    return "Bonus vs. mounted units";
                case CombatBonusType.Berzerker:
                    return "Bonus when attacking";
                case CombatBonusType.CityGood:
                    return "Bonus when attacking cities";
                case CombatBonusType.CityPoor:
                    return "Weak vs. cities";
                case CombatBonusType.None:
                    return string.Empty;
                case CombatBonusType.OpenTerrain:
                    return "Bonus when attacking in open terrain";
            }

            return string.Empty;
        }

        public static string ConvertBuildingTypeToDescription(BuildingType building)
        {
            switch (building)
            {
                case BuildingType.Aqueduct:
                    return "Allows a city to grow past size 8. Reduces the food needed to grow a city by 40%.";
                case BuildingType.Barracks:
                case BuildingType.Armory:
                    return "All units start with +15 experience points.";
                case BuildingType.Walls:
                    return $"Adds +{Rules.CITY_WALLS_COMBAT_MOD} combat strength to a city when defending. Adds {Rules.CITY_WALLS_HIT_POINT_BONUS} hit points to the city.";
                case BuildingType.Arsenal:
                    return $"Adds {Rules.ARSENAL_HIT_POINT_BONUS} hit points to the city.";
                case BuildingType.Castle:
                    return $"Adds {Rules.CASTLE_HIT_POINT_BONUS} hit points to the city.";
                case BuildingType.Library:
                    return $"Adds +{Rules.LIBRARY_SCIENCE_POINTS} science to city. Increases science output by +1 for every 2 citizens.";
                case BuildingType.University:
                    return $"Increases science output by {Rules.UNIVERSITY_SCIENCE_MOD * 100}%.";
                case BuildingType.Workshop:
                    return $"Increases production by +{Rules.WORKSHOP_PRODUCTION_POINTS}.";
                case BuildingType.StoneWorks:
                    return $"Increases production by +{Rules.STONEWORKS_PRODUCTION_POINTS}.";
                case BuildingType.Granary:
                    return $"Increases food output by +{Rules.GRANARY_CITY_FOOD_POINTS}. Each worked wheat tile generates +{Rules.GRANARY_TILE_WHEAT_FOOD_POINTS} food.";
                case BuildingType.Market:
                    return $"Increases gold by +{Rules.MARKET_GOLD_POINTS}. Increases overall gold output by {Rules.MARKET_GOLD_MOD * 100}%";
                case BuildingType.Forge:
                    return $"Each worked iron tile nets +{Rules.FORGE_TILE_IRON_PRODUCTION_POINTS} production. Increases overall land unit production by {Rules.FORGE_UNIT_PRODUCTION_MOD * 100}%";
                case BuildingType.Stable:
                    return "Horse units start with +15 experience points.";

            }

            return string.Empty;
        }

        public static string ConvertPromotionTypeToDescription(PromotionType promotion)
        {
            switch (promotion)
            {
                case PromotionType.Charge:
                    return $"+6 combat power when attacking wounded units";
                case PromotionType.Cover1:
                    return $"+3 combat power when defending against ranged attacks";
                case PromotionType.Cover2:
                    return $"+3 combat power when defending against ranged attacks";
                case PromotionType.Cover3:
                    return $"+3 combat power when defending against ranged attacks";
                case PromotionType.Discipline:
                    return $"+4 combat power when next to a friendly unit";
                case PromotionType.Drill1:
                    return $"+4 combat power when fighting in rough terrain";
                case PromotionType.Drill2:
                    return $"+4 combat power when fighting in rough terrain";
                case PromotionType.Drill3:
                    return $"+4 combat power when fighting in rough terrain";
                case PromotionType.March:
                    return $"Unit heals every turn even if performed an action or was attacked";
                case PromotionType.Medic:
                    return $"This unit and all other adjacent units heal an extra +5 per turn";
                case PromotionType.Shock1:
                    return $"+4 combat power when fighting in open terrain";
                case PromotionType.Shock2:
                    return $"+4 combat power when fighting in open terrain";
                case PromotionType.Shock3:
                    return $"+4 combat power when fighting in open terrain";
                case PromotionType.Siege:
                    return $"+6 combat power when attacking cities";
                case PromotionType.Volley:
                    return $"+6 combat power when attacking cities or fortified units";
                case PromotionType.Woodsman:
                    return $"+1 movement point through forests";
                case PromotionType.Blitz:
                    return $"+1 attack per turn";
            }

            return string.Empty;
        }

        public static IEnumerable<string> GetCityNames(Civilization civ)
        {
            if (civ.Name.Equals("Romans", StringComparison.OrdinalIgnoreCase))
            {
                return new List<string>() { "Leodis", "Valentia", "Tyrus", "Palmyra", "Pompeii", "Adana", "Lentia", "Malaca", "Pax Iulia", "Novae", "Utinum", "Osca", "Tomis", "Tingi", "Puteoli", "Narbo", "Histria", "Iberia", "Isara", "Gades", "Eryx", "Florentia", "Ebrus", "Dubris", "Drobeta", "Dertona", "Deva", "Curia", "Corcyra", "Clupea", "Carrhae", "Carales", "Bostra" };
            }
            if (civ.Name.Equals("Barbarians", StringComparison.OrdinalIgnoreCase))
            {
                return new List<string>() { "Nidaros", "Njord", "Sigurd", "Askival", "Goathland", "Scargill", "Thorgill", "Thurso", "Svanhild" };
            }

            return new List<string>();
        }
    }
}
