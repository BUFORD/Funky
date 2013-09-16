﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.ability.Abilities.Barb
{
	public class Sprint : Ability, IAbility
	{
		public Sprint() : base()
		{
		}

		public override SNOPower Power
		{
			get { return SNOPower.Barbarian_Sprint; }
		}

		public override int RuneIndex { get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power)?Bot.Class.RuneIndexCache[this.Power]:-1; } }

		public override void Initialize()
		{
			ExecutionType = PowerExecutionTypes.Buff;
			WaitVars = new WaitLoops(1, 1, true);
			Cost = 20;
			IsASpecialMovementPower=true;
			UseFlagsType=AbilityUseFlags.Anywhere;
			Priority = AbilityPriority.Low;
			PreCastConditions = (CastingConditionTypes.CheckEnergy | CastingConditionTypes.CheckCanCast |
			                     CastingConditionTypes.CheckPlayerIncapacitated);

			IsBuff=true;
			Fbuff=new Func<bool>(() =>
				 {
					  return Bot.SettingsFunky.OutOfCombatMovement&&!Bot.Class.HasBuff(this.Power);
				 }
			);
			Fcriteria = new Func<bool>(() =>
			{
				return (!Bot.Class.HasBuff(SNOPower.Barbarian_Sprint) && Bot.SettingsFunky.OutOfCombatMovement) ||
				       (((Bot.SettingsFunky.Class.bFuryDumpWrath && Bot.Character.dCurrentEnergyPct >= 0.95 &&
									Bot.Class.HasBuff(SNOPower.Barbarian_WrathOfTheBerserker))||
				         (Bot.SettingsFunky.Class.bFuryDumpAlways && Bot.Character.dCurrentEnergyPct >= 0.95) ||
								 ((this.AbilityUseTimer()&&!Bot.Class.HasBuff(SNOPower.Barbarian_Sprint))&&
				          // Always keep up if we are whirlwinding, or if the target is a goblin
				          (Bot.Class.HotbarPowers.Contains(SNOPower.Barbarian_Whirlwind) ||
				           Bot.Target.CurrentTarget.IsTreasureGoblin))) &&
				        (!Bot.Class.HotbarPowers.Contains(SNOPower.Barbarian_BattleRage) ||
								 (Bot.Class.HotbarPowers.Contains(SNOPower.Barbarian_BattleRage)&&Bot.Class.HasBuff(SNOPower.Barbarian_BattleRage))));
			});
		}

		#region IAbility
		public override int GetHashCode()
		{
			 return (int)this.Power;
		}
		public override bool Equals(object obj)
		{
			 //Check for null and compare run-time types. 
			 if (obj==null||this.GetType()!=obj.GetType())
			 {
				  return false;
			 }
			 else
			 {
				  Ability p=(Ability)obj;
				  return this.Power==p.Power;
			 }
		}
	
	
		#endregion
	}
}
