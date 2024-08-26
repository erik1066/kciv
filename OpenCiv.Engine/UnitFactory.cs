using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenCiv.Engine
{
    public class UnitFactory 
    {
        private int _nextId = 0;
        public UnitFactory()
        {

        }

        public Unit ProduceUnit(UnitType unitType, Civilization owner)
        {

            Unit unit = new Unit(unitType, owner);
            unit.Status = UnitStatus.None;
            unit.Locomotion = UnitLocomotion.Foot;
            unit.UpgradesTo = UnitType.None;
            unit.CombatPower = 0;
            unit.RangedPower = 0;
            unit.MaxMoves = 2;
            unit.AttackRange = 0;
            unit.Maintenance = 0;
            unit.Id = _nextId;

            switch (unitType) {

                // basic foot melee
                case UnitType.Warrior:
                    unit.UpgradesTo = UnitType.Swordsman;
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 20;
                    unit.Maintenance = 1;
                    unit.Name = "Warrior";
                    break;
                case UnitType.Axeman:
                    unit.UpgradesTo = UnitType.Swordsman;
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 22;
                    unit.Name = "Axeman";
                    unit.Maintenance = 0;
                    unit.AddCombatBonus(CombatBonusType.Berzerker);
                    break;
                case UnitType.Swordsman:
                    unit.UpgradesTo = UnitType.Legion;
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 38;
                    unit.MaxMoves = 2;
                    unit.Maintenance = 2;
                    unit.Name = "Swordsman";
                    break;
                case UnitType.Legion:
                    unit.UpgradesTo = UnitType.Longswordsman;
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 43;
                    unit.MaxMoves = 2;
                    unit.Maintenance = 2;
                    unit.Name = "Legion";
                    break;
                case UnitType.Longswordsman:
                    unit.UpgradesTo = UnitType.Musketman;
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 50;
                    unit.MaxMoves = 2;
                    unit.Maintenance = 3;
                    unit.Name = "Longswordsman";
                    break;
                case UnitType.Musketman:
                    unit.UpgradesTo = UnitType.Rifleman;
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 65;
                    unit.MaxMoves = 2;
                    unit.Maintenance = 3;
                    unit.Name = "Musketman";
                    break;
                case UnitType.Rifleman:
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 80;
                    unit.MaxMoves = 2;
                    unit.Maintenance = 3;
                    unit.Name = "Rifleman";
                    break;

                // anti-horse units
                case UnitType.Spearman:
                    unit.UpgradesTo = UnitType.Pikeman;
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 21;
                    unit.MaxMoves = 2;
                    unit.Maintenance = 1;
                    unit.Name = "Spearman";
                    unit.AddCombatBonus(CombatBonusType.AntiHorse);
                    break;
                case UnitType.Pikeman:
                    unit.UpgradesTo = UnitType.Musketman;
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 45;
                    unit.MaxMoves = 2;
                    unit.Maintenance = 3;
                    unit.Name = "Pikeman";
                    unit.AddCombatBonus(CombatBonusType.AntiHorse);
                    break;

                // basic foot ranged
                case UnitType.Slinger:
                    unit.UpgradesTo = UnitType.Archer;
                    unit.AttackMethod = AttackMethod.Ranged;
                    unit.AttackRange = 2;
                    unit.CombatPower = 11;
                    unit.RangedPower = 15;
                    unit.AttackRange = 1;
                    unit.MaxMoves = 2;
                    unit.Maintenance = 1;
                    unit.Name = "Slinger";
                    break;
                case UnitType.Archer:
                    unit.UpgradesTo = UnitType.Bowman;
                    unit.AttackMethod = AttackMethod.Ranged;
                    unit.AttackRange = 2;
                    unit.CombatPower = 15;
                    unit.RangedPower = 22;
                    unit.Maintenance = 1;
                    unit.Name = "Archer";
                    break;
                case UnitType.Bowman:
                    unit.UpgradesTo = UnitType.Crossbowman;
                    unit.AttackMethod = AttackMethod.Ranged;
                    unit.AttackRange = 2;
                    unit.CombatPower = 20;
                    unit.RangedPower = 30;
                    unit.Maintenance = 2;
                    unit.Name = "Composite Bowman";
                    break;
                case UnitType.Crossbowman:
                    unit.UpgradesTo = UnitType.Musketman;
                    unit.AttackMethod = AttackMethod.Ranged;
                    unit.AttackRange = 2;
                    unit.CombatPower = 32;
                    unit.RangedPower = 43;
                    unit.AttackRange = 2;
                    unit.MaxMoves = 2;
                    unit.Maintenance = 3;
                    unit.Name = "Crowssbowman";
                    break;

                // special units
                case UnitType.Builder:
                    unit.AttackMethod = AttackMethod.None;
                    unit.CombatPower = 0;
                    unit.Maintenance = 0;
                    unit.Name = "Builder";
                    break;
                case UnitType.Settler:
                    unit.AttackMethod = AttackMethod.None;
                    unit.CombatPower = 0;
                    unit.MaxMoves = 2;
                    unit.Maintenance = 1;
                    unit.Name = "Settler";
                    unit.AddSpecialAction(UnitSpecialAction.BuildCity);
                    break;

                // special bombardment ranged units
                case UnitType.Catapult:                
                    unit.AttackMethod = AttackMethod.IndiscriminateBombardment;
                    unit.AttackRange = 2;
                    unit.CombatPower = 15;
                    unit.RangedPower = 35;
                    unit.Maintenance = 2;
                    unit.Name = "Catapult";
                    unit.Locomotion = UnitLocomotion.Foot;
                    unit.AddSpecialAction(UnitSpecialAction.PlagueBodyAttack);
                    unit.AddSpecialAction(UnitSpecialAction.FireAttack);
                    unit.AddCombatBonus(CombatBonusType.CityGood); 
                    break;
                
                // mounted units
                case UnitType.Chariot:
                    unit.UpgradesTo = UnitType.Horseman;
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 18;
                    unit.MaxMoves = 3;
                    unit.Name = "Chariot";
                    unit.Maintenance = 2;
                    unit.Locomotion = UnitLocomotion.Horse;
                    unit.AddSpecialAction(UnitSpecialAction.CanMoveAfterAttacking);
                    break;
                case UnitType.HorseArcher:
                    unit.AttackMethod = AttackMethod.Ranged;
                    unit.AttackRange = 2;
                    unit.CombatPower = 21;
                    unit.RangedPower = 23;
                    unit.MaxMoves = 4;
                    unit.Maintenance = 2;
                    unit.Name = "Horse archer";
                    unit.Locomotion = UnitLocomotion.Horse;
                    unit.AddSpecialAction(UnitSpecialAction.CanMoveAfterAttacking);
                    break;
                case UnitType.Horseman:
                    unit.UpgradesTo = UnitType.Knight;
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 25;
                    unit.MaxMoves = 4;
                    unit.Maintenance = 2;
                    unit.Name = "Horseman";
                    unit.Locomotion = UnitLocomotion.Horse;
                    unit.AddSpecialAction(UnitSpecialAction.CanMoveAfterAttacking);
                    unit.AddCombatBonus(CombatBonusType.CityPoor); // not good at attacking cities
                    unit.AddCombatBonus(CombatBonusType.OpenTerrain); // but are good at slaughtering things in the open field
                    break;
                case UnitType.Knight:
                    unit.UpgradesTo = UnitType.Crusader;
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 51;
                    unit.MaxMoves = 4;
                    unit.Maintenance = 4;
                    unit.Name = "Knight";
                    unit.Locomotion = UnitLocomotion.Horse;
                    unit.AddSpecialAction(UnitSpecialAction.CanMoveAfterAttacking);
                    unit.AddCombatBonus(CombatBonusType.CityPoor); // not good at attacking cities
                    unit.AddCombatBonus(CombatBonusType.OpenTerrain); // but are good at slaughtering things in the open field
                    break;
                case UnitType.Crusader:
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 55;
                    unit.MaxMoves = 4;
                    unit.Maintenance = 5;
                    unit.Name = "Crusader";
                    unit.Locomotion = UnitLocomotion.Horse;
                    unit.AddSpecialAction(UnitSpecialAction.CanMoveAfterAttacking);
                    unit.AddCombatBonus(CombatBonusType.CityPoor); // not good at attacking cities
                    unit.AddCombatBonus(CombatBonusType.OpenTerrain); // but are good at slaughtering things in the open field
                    break;

                // special
                case UnitType.BarbarianLeader:
                    unit.AttackMethod = AttackMethod.Melee;
                    unit.CombatPower = 48;
                    unit.MaxMoves = 3;
                    unit.Name = "Lartigue the Tormentor";
                    unit.Locomotion = UnitLocomotion.Foot;
                    unit.AddCombatBonus(CombatBonusType.Berzerker);
                    unit.AddSpecialAction(UnitSpecialAction.CanMoveAfterAttacking);
                    unit.Promote(PromotionType.Cover1);
                    break;

                default:
                    throw new NotImplementedException();
            }

            unit.RemainingMoves = unit.MaxMoves;

            _nextId++;

            return unit;
        }
    }
}