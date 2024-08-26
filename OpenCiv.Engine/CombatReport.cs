using System;
using System.Collections.Generic;

namespace OpenCiv.Engine
{
    public sealed class CombatReport {
        public Unit Attacker {get; private set;}
        public Unit Defender {get;private set;}
        public City City { get; private set; }
        public Tile Tile {get; private set;}
        public bool IncludeRandomization { get; private set; }

        public double AttackerHitPointLoss {get;private set;}
        public double DefenderHitPointLoss {get;private set;}

        public Dictionary<string, double> DefenseModifiers { get; private set; } = new Dictionary<string, double>();
        public Dictionary<string, double> AttackModifiers { get; private set; } = new Dictionary<string, double>();

        public CombatReport(Unit attacker, Unit defender, Tile tile, bool includeRandomization = true)
        {
            Attacker = attacker;
            Defender = defender;
            Tile = tile;
            IncludeRandomization = includeRandomization;

            ComputeCombat(attacker.AttackMethod);
        }

        public CombatReport(Unit attacker, City city, Tile tile, bool includeRandomization = true)
        {
            Attacker = attacker;
            City = city;
            Tile = tile;
            IncludeRandomization = includeRandomization;

            ComputeCombat(attacker.AttackMethod);
        }

        private void ComputeCityBonuses(City city, ref double defenseModifier, ref double attackModifier, Dictionary<string, double> modifiers)
        {
            if (city == null) return;

            if (city.HasWalls)
            {
                defenseModifier += Rules.CITY_WALLS_COMBAT_MOD;
                modifiers.Add("Defender: City walls", Rules.CITY_WALLS_COMBAT_MOD);
            }
            if (city.Buildings.Contains(BuildingType.Castle))
            {
                defenseModifier += Rules.CASTLE_COMBAT_MOD;
                modifiers.Add("Defender: Has castle", Rules.CASTLE_COMBAT_MOD);
            }

            if (Attacker.Bonuses.Contains(CombatBonusType.CityGood))
            {
                modifiers.Add("Attacker: Designed for city strikes", Rules.CITY_GOOD_MOD);
                attackModifier = attackModifier + Rules.CITY_GOOD_MOD;
            }

            if (Attacker.Bonuses.Contains(CombatBonusType.CityPoor))
            {
                modifiers.Add("Attacker: Very bad against cities", Rules.CITY_BAD_MOD);
                attackModifier = attackModifier - Rules.CITY_BAD_MOD;
            }

            if (City == null) // this is a unit defending a city, not a city defending itself...
            {
                defenseModifier += Rules.FORTRESS_COMBAT_MOD;
                modifiers.Add("Defender: City defense combat bonus", Rules.FORTRESS_COMBAT_MOD);
            }
        }

        private void ComputeCombat(AttackMethod attackMethod)
        {
            Dictionary<string, double> modifiers = new Dictionary<string, double>();

            double defenseModifier = 25.0;
            double attackModifier = Attacker.CombatPower;

            if (Attacker.AttackMethod == AttackMethod.PrecisionStrike || Attacker.AttackMethod == AttackMethod.Ranged)
            {
                attackModifier = Attacker.RangedPower; // attacker used ranged power when attacking, but combat power on defense
            }
            else if (Attacker.AttackMethod == AttackMethod.IndiscriminateBombardment)
            {
                if (Defender == null && City != null)
                {
                    attackModifier = Attacker.RangedPower * 1.5; // we have a indiscriminate bombard unit attacking a city, give the appropriate bonus
                }
                else if (Defender != null)
                {
                    attackModifier = Attacker.RangedPower * 0.5; // we have a indiscriminate bombard unit attacking another unit, apply penalty
                }
            }

            if (Defender != null)
            {
                defenseModifier = Defender.CombatPower;

                if (Defender.AttackMethod == AttackMethod.None)
                {
                    attackModifier += 100;
                    defenseModifier = 0;
                }

                if (Tile.HasCity)
                {
                    ComputeCityBonuses(Tile.City, ref defenseModifier, ref attackModifier, modifiers);
                }
            }
            if (City != null)
            {
                defenseModifier = City.CombatPower;

                ComputeCityBonuses(City, ref defenseModifier, ref attackModifier, modifiers);
            }

            double baseHitPointLoss = 25.0f;

            if (IncludeRandomization)
            {
                var r = new System.Random(System.DateTime.Now.Millisecond);
                double randomAttackModifier = (r.NextDouble() - 0.5) * 6;

                System.Threading.Thread.Sleep(new System.Random().Next(1, 7));

                r = new System.Random(System.DateTime.Now.Millisecond);
                double randomDefenseModifier = (r.NextDouble() - 0.5) * 6;

                modifiers.Add("Attacker: Randomness", randomAttackModifier);
                attackModifier = attackModifier + randomAttackModifier;

                modifiers.Add("Defender: Randomness", randomDefenseModifier);
                defenseModifier = defenseModifier + randomDefenseModifier;
            }

            // bonus when under attack from ranged units
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Ranged && Defender.Promotions.Contains(PromotionType.Cover1)) defenseModifier += 4;
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Ranged && Defender.Promotions.Contains(PromotionType.Cover2)) defenseModifier += 4;
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Ranged && Defender.Promotions.Contains(PromotionType.Cover3)) defenseModifier += 4;

            // bonus when there's an adjacent unit
            if (Defender != null && Defender.Promotions.Contains(PromotionType.Discipline) && 
                (
                Tile.TileN.HasUnit && Tile.TileN.CurrentUnit.Owner == Defender.Owner ||
                Tile.TileNE.HasUnit && Tile.TileNE.CurrentUnit.Owner == Defender.Owner ||
                Tile.TileSE.HasUnit && Tile.TileSE.CurrentUnit.Owner == Defender.Owner ||
                Tile.TileS.HasUnit && Tile.TileS.CurrentUnit.Owner == Defender.Owner ||
                Tile.TileSW.HasUnit && Tile.TileSW.CurrentUnit.Owner == Defender.Owner ||
                Tile.TileNW.HasUnit && Tile.TileNW.CurrentUnit.Owner == Defender.Owner
                )) defenseModifier += 4;

            // bonus when there's an adjacent unit
            if (Attacker.Promotions.Contains(PromotionType.Discipline) &&
                (
                Tile.TileN.HasUnit && Tile.TileN.CurrentUnit.Owner == Attacker.Owner ||
                Tile.TileNE.HasUnit && Tile.TileNE.CurrentUnit.Owner == Attacker.Owner ||
                Tile.TileSE.HasUnit && Tile.TileSE.CurrentUnit.Owner == Attacker.Owner ||
                Tile.TileS.HasUnit && Tile.TileS.CurrentUnit.Owner == Attacker.Owner ||
                Tile.TileSW.HasUnit && Tile.TileSW.CurrentUnit.Owner == Attacker.Owner ||
                Tile.TileNW.HasUnit && Tile.TileNW.CurrentUnit.Owner == Attacker.Owner
                )) attackModifier += 3;

            // bonus for rough terrain
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Melee && Defender.Promotions.Contains(PromotionType.Drill1) 
                && (Tile.Terrain == TerrainType.Hills || Tile.Terrain == TerrainType.Forest || Tile.Terrain == TerrainType.ForestHills || Tile.Terrain == TerrainType.Jungle)) defenseModifier += 3;
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Melee && Defender.Promotions.Contains(PromotionType.Drill2)
                && (Tile.Terrain == TerrainType.Hills || Tile.Terrain == TerrainType.Forest || Tile.Terrain == TerrainType.ForestHills || Tile.Terrain == TerrainType.Jungle)) defenseModifier += 3;
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Melee && Defender.Promotions.Contains(PromotionType.Drill3)
                && (Tile.Terrain == TerrainType.Hills || Tile.Terrain == TerrainType.Forest || Tile.Terrain == TerrainType.ForestHills || Tile.Terrain == TerrainType.Jungle)) defenseModifier += 3;

            if (Defender != null && Attacker.AttackMethod == AttackMethod.Melee && Attacker.Promotions.Contains(PromotionType.Drill1)
                && (Tile.Terrain == TerrainType.Hills || Tile.Terrain == TerrainType.Forest || Tile.Terrain == TerrainType.ForestHills || Tile.Terrain == TerrainType.Jungle)) attackModifier += 3;
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Melee && Attacker.Promotions.Contains(PromotionType.Drill2)
                && (Tile.Terrain == TerrainType.Hills || Tile.Terrain == TerrainType.Forest || Tile.Terrain == TerrainType.ForestHills || Tile.Terrain == TerrainType.Jungle)) attackModifier += 3;
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Melee && Attacker.Promotions.Contains(PromotionType.Drill3)
                && (Tile.Terrain == TerrainType.Hills || Tile.Terrain == TerrainType.Forest || Tile.Terrain == TerrainType.ForestHills || Tile.Terrain == TerrainType.Jungle)) attackModifier += 3;

            // bonus for OPEN terrain
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Melee && Defender.Promotions.Contains(PromotionType.Shock1)
                && (Tile.Terrain == TerrainType.Grassland || Tile.Terrain == TerrainType.Plains || Tile.Terrain == TerrainType.Tundra || Tile.Terrain == TerrainType.Desert)) defenseModifier += 3;
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Melee && Defender.Promotions.Contains(PromotionType.Shock1)
                && (Tile.Terrain == TerrainType.Grassland || Tile.Terrain == TerrainType.Plains || Tile.Terrain == TerrainType.Tundra || Tile.Terrain == TerrainType.Desert)) defenseModifier += 3;
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Melee && Defender.Promotions.Contains(PromotionType.Shock1)
                && (Tile.Terrain == TerrainType.Grassland || Tile.Terrain == TerrainType.Plains || Tile.Terrain == TerrainType.Tundra || Tile.Terrain == TerrainType.Desert)) defenseModifier += 3;

            if (Defender != null && Attacker.AttackMethod == AttackMethod.Melee && Attacker.Promotions.Contains(PromotionType.Shock1)
                && (Tile.Terrain == TerrainType.Grassland || Tile.Terrain == TerrainType.Plains || Tile.Terrain == TerrainType.Tundra || Tile.Terrain == TerrainType.Desert)) attackModifier += 3;
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Melee && Attacker.Promotions.Contains(PromotionType.Shock1)
                && (Tile.Terrain == TerrainType.Grassland || Tile.Terrain == TerrainType.Plains || Tile.Terrain == TerrainType.Tundra || Tile.Terrain == TerrainType.Desert)) attackModifier += 3;
            if (Defender != null && Attacker.AttackMethod == AttackMethod.Melee && Attacker.Promotions.Contains(PromotionType.Shock1)
                && (Tile.Terrain == TerrainType.Grassland || Tile.Terrain == TerrainType.Plains || Tile.Terrain == TerrainType.Tundra || Tile.Terrain == TerrainType.Desert)) attackModifier += 3;

            if (Attacker.Promotions.Contains(PromotionType.Siege) && City != null) attackModifier += 6;

            if (Attacker.Promotions.Contains(PromotionType.Volley) && (City != null || (Defender != null && Defender.Status == UnitStatus.Fortified))) attackModifier += 5;

            if (Attacker.Promotions.Contains(PromotionType.Charge) && (Defender != null && Defender.HitPoints < 100.0)) attackModifier += 5;

            #region Tile Modifiers
            // is defender on favorable terrain?
            TerrainType terrain = Tile.Terrain;

            if (terrain == TerrainType.Swamp) {
                defenseModifier += Rules.SWAMP_TERRAIN_COMBAT_MOD;
                modifiers.Add("Defender: Favorable terrain", Rules.SWAMP_TERRAIN_COMBAT_MOD);
            }
            else if (terrain == TerrainType.Forest || terrain == TerrainType.Hills || terrain == TerrainType.Jungle) {
                defenseModifier += Rules.WOODLAND_TERRAIN_COMBAT_MOD;
                modifiers.Add("Defender: Favorable terrain", Rules.WOODLAND_TERRAIN_COMBAT_MOD);
            }
            else if (terrain == TerrainType.ForestHills) {
                defenseModifier += Rules.WOODLANDHILL_TERRAIN_COMBAT_MOD;
                modifiers.Add("Defender: Favorable terrain", Rules.WOODLANDHILL_TERRAIN_COMBAT_MOD);
            }

            // is defender in a fortress?
            ImprovementType improvement = Tile.Improvement;

            if (improvement == ImprovementType.Fortress) {
                modifiers.Add("Defender: Fortress", Rules.FORTRESS_COMBAT_MOD);
                defenseModifier += Rules.FORTRESS_COMBAT_MOD;
            }

            // apply a penalty to attacker for attacking with a weakened unit
            if (Attacker.HitPoints < 100.0f) {
                modifiers.Add("Defender: Fighting a weakened unit", Rules.WOUNDED_UNIT_COMBAT_MOD);
                defenseModifier = defenseModifier + Rules.WOUNDED_UNIT_COMBAT_MOD;
            }

            // apply a bonus to attacker when attacking a weakened unit
            if (Defender != null && Defender.HitPoints < 100.0f) {
                modifiers.Add("Attacker: Fighting a weakened unit", Rules.WOUNDED_UNIT_COMBAT_MOD);
                attackModifier = attackModifier + Rules.WOUNDED_UNIT_COMBAT_MOD;
            }

            bool hasRiver = Tile.HasRiver;
            // apply bonus when defending a river
            if (hasRiver == true) {
                modifiers.Add("Defender: River crossing", Rules.RIVER_COMBAT_MOD);
                defenseModifier = defenseModifier + Rules.RIVER_COMBAT_MOD;
            }
#endregion //Tile Modifiers

#region Unit Status Modifiers
            // is defender fortified?
            if (Defender != null && Defender.Status == UnitStatus.Fortifying) {
                modifiers.Add("Defender: River crossing", Rules.FORTIFYING_COMBAT_MOD);
                defenseModifier = defenseModifier + Rules.FORTIFYING_COMBAT_MOD;
            }
            else if (Defender != null && Defender.Status == UnitStatus.Fortified) {
                modifiers.Add("Defender: River crossing", Rules.FORTIFIED_COMBAT_MOD);
                defenseModifier = defenseModifier + Rules.FORTIFIED_COMBAT_MOD;
            }
#endregion // Unit Status Modifiers

            // horse units have a constant bonus over foot units
            if (Defender != null && Attacker.Locomotion == UnitLocomotion.Horse && Defender.Locomotion == UnitLocomotion.Foot) {
                modifiers.Add("Attacker: Horse versus foot", Rules.HORSE_VS_FOOT_COMBAT_MOD);
                attackModifier = attackModifier + Rules.HORSE_VS_FOOT_COMBAT_MOD;
            }

#region Apply Bonuses
            if (Defender != null && Attacker.Bonuses.Contains(CombatBonusType.AntiHorse) && Defender.Locomotion == UnitLocomotion.Horse) {
                modifiers.Add("Attacker: Anti-horse bonus for unit type", Rules.BONUS_ANTI_HORSE_COMBAT_MOD);
                attackModifier = attackModifier + Rules.BONUS_ANTI_HORSE_COMBAT_MOD;
            }
            if (Defender != null && Defender.Bonuses.Contains(CombatBonusType.AntiHorse) && Attacker.Locomotion == UnitLocomotion.Horse) {
                modifiers.Add("Defender: Anti-horse bonus for unit type", Rules.BONUS_ANTI_HORSE_COMBAT_MOD);
                defenseModifier = defenseModifier + Rules.BONUS_ANTI_HORSE_COMBAT_MOD;
            }

            if (Attacker.Bonuses.Contains(CombatBonusType.OpenTerrain) && (Tile.Terrain == TerrainType.Desert || Tile.Terrain == TerrainType.Grassland || Tile.Terrain == TerrainType.Plains)) {
                modifiers.Add("Attacker: Open terrain bonus for unit type", Rules.BONUS_ANTI_HORSE_COMBAT_MOD);
                attackModifier = attackModifier + Rules.BONUS_ANTI_HORSE_COMBAT_MOD;
            }
            if (Defender != null && Defender.Bonuses.Contains(CombatBonusType.OpenTerrain) && (Tile.Terrain == TerrainType.Desert || Tile.Terrain == TerrainType.Grassland || Tile.Terrain == TerrainType.Plains)) {
                modifiers.Add("Defender: Open terrain bonus for unit type", Rules.BONUS_OPEN_TERRAIN_COMBAT_MOD);
                defenseModifier = defenseModifier + Rules.BONUS_OPEN_TERRAIN_COMBAT_MOD;
            }
            if (Attacker.Bonuses.Contains(CombatBonusType.Berzerker)) {
                modifiers.Add("Attacker: Berserker bonus for unit type", Rules.BONUS_BERSERKER_COMBAT_MOD);
                attackModifier = attackModifier + Rules.BONUS_BERSERKER_COMBAT_MOD;
            }

            
            #endregion // Apply Bonuses

            double combatStrengthDiff = System.Math.Abs(attackModifier - defenseModifier);

            AttackerHitPointLoss = 30.0f;
            DefenderHitPointLoss = 30.0f;

            if (attackModifier > defenseModifier)
            {
                DefenderHitPointLoss = 30.0f * System.Math.Pow(System.Math.E, (combatStrengthDiff / baseHitPointLoss));
                AttackerHitPointLoss = 9 * (1 / (DefenderHitPointLoss / 100.0f));
            }
            else if (defenseModifier > attackModifier)
            {
                AttackerHitPointLoss = 30.0f * System.Math.Pow(System.Math.E, (combatStrengthDiff / baseHitPointLoss));            
                DefenderHitPointLoss = 9 * (1 / (AttackerHitPointLoss / 100.0f));
            }

            foreach (var modifier in modifiers)
            {
                if (modifier.Key.StartsWith("Attacker")) {
                    AttackModifiers.Add(modifier.Key.Replace("Attacker: ", string.Empty), modifier.Value);
                }
                else if (modifier.Key.StartsWith("Defender")) {
                    DefenseModifiers.Add(modifier.Key.Replace("Defender: ", string.Empty), modifier.Value);
                }
            }

            if (attackMethod == AttackMethod.Ranged || attackMethod == AttackMethod.PrecisionStrike || attackMethod == AttackMethod.IndiscriminateBombardment)
            {
                AttackerHitPointLoss = 0.0; // ranged attackers don't lose HP!
            }
        }
    }
}