using Designs;
using UnityEngine;

namespace Etchings
{
    public class LoneWolfEtching : MeleeEtching
    {
        private StatModifier _damageModifer;

        protected override void Start()
        {
            UpdateDamage();
            BattleEvents.Current.OnSticksUpdated += UpdateDamage;
            base.Start();
        }

        private void UpdateDamage()
        {
            if (_damageModifer is not null)
                design.Stats[StatType.Damage].RemoveModifier(_damageModifer);

            var penalty = design.GetStat(StatType.DamageFlatModifier) * (StickManager.current.stickCount - 2);

            _damageModifer = new StatModifier(penalty, StatModType.Flat);
            design.Stats[StatType.Damage].AddModifier(_damageModifer);
        }

        private void OnDestroy()
        {
            BattleEvents.Current.OnSticksUpdated -= UpdateDamage;
            design.Stats[StatType.Damage].RemoveModifier(_damageModifer);
        }
    }
}
