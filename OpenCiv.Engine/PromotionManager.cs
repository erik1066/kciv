using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCiv.Engine
{
    internal sealed class PromotionManager
    {
        private List<PromotionType> _promotions = new List<PromotionType>(50);
        private readonly int _max;
        public PromotionManager()
        {
            _promotions.Add(PromotionType.Charge);
            _promotions.Add(PromotionType.Cover1);
            _promotions.Add(PromotionType.Cover2);
            _promotions.Add(PromotionType.Cover3);
            _promotions.Add(PromotionType.Discipline);
            _promotions.Add(PromotionType.Drill1);
            _promotions.Add(PromotionType.Drill2);
            _promotions.Add(PromotionType.Drill3);
            _promotions.Add(PromotionType.March);
            _promotions.Add(PromotionType.Medic);
            _promotions.Add(PromotionType.Shock1);
            _promotions.Add(PromotionType.Shock2);
            _promotions.Add(PromotionType.Shock3);
            _promotions.Add(PromotionType.Siege);
            _promotions.Add(PromotionType.Volley);
            _promotions.Add(PromotionType.Blitz);
            _promotions.Add(PromotionType.Mobility);

            _max = _promotions.Count - 1;
        }

        public bool DoesUnitNeedPromotion(Unit unit)
        {
            int promotionCount = unit.Promotions.Count;

            if (promotionCount == _max + 1) return false;

            int nextPromotionAmount = Rules.EXP_REQUIRED_FOR_PROMOTION * (promotionCount + 1);

            if (unit.Experience > nextPromotionAmount)
            {
                return true;
            }

            return false;
        }

        public void TransferPromotions(Unit source, Unit destination)
        {
            foreach(var promotion in source.Promotions)
            {
                if (IsValidPromotion(destination, promotion))
                {
                    destination.Promote(promotion);
                }
            }
        }

        public PromotionType PromoteUnit(Unit unit)
        {
            PromotionType promotedTo = PromotionType.None;

            int maxIterations = 50;
            int currentIteration = 0;

            while (currentIteration < maxIterations)
            {
                var r = new System.Random();
                int indexToTry = r.Next(0, _max);

                PromotionType type = _promotions[indexToTry];

                if (!unit.Promotions.Contains(type) && IsValidPromotion(unit, type))
                {
                    unit.Promote(type);

                    promotedTo = type;
                    break;
                }

                currentIteration++;
            }

            return promotedTo;
        }

        private bool IsValidPromotion(Unit unit, PromotionType type)
        {
            var promotions = unit.Promotions;

            if (!promotions.Contains(PromotionType.Cover1) && (type == PromotionType.Cover2 || type == PromotionType.Cover3)) return false;
            if (!promotions.Contains(PromotionType.Cover2) && type == PromotionType.Cover3) return false;

            if (!promotions.Contains(PromotionType.Drill1) && (type == PromotionType.Drill2 || type == PromotionType.Drill3)) return false;
            if (!promotions.Contains(PromotionType.Drill2) && type == PromotionType.Drill3) return false;

            if (!promotions.Contains(PromotionType.Shock1) && (type == PromotionType.Shock2 || type == PromotionType.Shock3)) return false;
            if (!promotions.Contains(PromotionType.Shock2) && type == PromotionType.Shock3) return false;

            if (unit.AttackMethod != AttackMethod.Ranged && type == PromotionType.Volley) return false;

            if (unit.Locomotion != UnitLocomotion.Horse && unit.Locomotion != UnitLocomotion.Motor && type == PromotionType.Blitz) return false;

            return true;
        }
    }
}
