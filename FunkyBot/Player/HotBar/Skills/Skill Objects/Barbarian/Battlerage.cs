using FunkyBot.Player.HotBar.Skills.Conditions;
using Zeta.Game.Internals.Actors;

namespace FunkyBot.Player.HotBar.Skills.Barb
{
    public class Battlerage : Skill
    {
        public override SNOPower Power
        {
            get { return SNOPower.Barbarian_BattleRage; }
        }

        public override int RuneIndex { get { return Bot.Character.Class.HotBar.RuneIndexCache.ContainsKey(Power) ? Bot.Character.Class.HotBar.RuneIndexCache[Power] : -1; } }

        public override void Initialize()
        {
            Cooldown = 100000;
            ExecutionType = AbilityExecuteFlags.Buff;
            WaitVars = new WaitLoops(1, 1, true);
            Cost = 20;
            IsBuff = true;
            UseageType = AbilityUseage.Anywhere;
            Priority = AbilityPriority.Low;
            PreCast = new SkillPreCast(AbilityPreCastFlags.CheckExisitingBuff | AbilityPreCastFlags.CheckEnergy | AbilityPreCastFlags.CheckPlayerIncapacitated | AbilityPreCastFlags.CheckCanCast);
            FcriteriaBuff = () => true;
            FcriteriaBuff = () => LastUsedMilliseconds > 30000;
        }

        #region IAbility
        public override int GetHashCode()
        {
            return (int)Power;
        }
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types. 
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Skill p = (Skill)obj;
            return Power == p.Power;
        }

        #endregion
    }
}
