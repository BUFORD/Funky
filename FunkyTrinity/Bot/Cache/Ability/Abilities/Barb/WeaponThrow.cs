﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.ability.Abilities.Barb
{
	public class WeaponThrow : Ability, IAbility
	{
		public WeaponThrow() : base()
		{
		}

		public override SNOPower Power
		{
			get { return SNOPower.Barbarian_WeaponThrow; }
		}

		public override int RuneIndex { get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power)?Bot.Class.RuneIndexCache[this.Power]:-1; } }

		protected override void Initialize()
		{
			ExecutionType = AbilityUseType.Target;
			WaitVars = new WaitLoops(0, 1, true);
			Cost = 10;
			Range = 44;
			IsRanged = true;
			UseageType=AbilityUseage.Combat;
			Priority = AbilityPriority.None;
			PreCastConditions = (AbilityConditions.CheckRecastTimer | AbilityConditions.CheckEnergy |
			                     AbilityConditions.CheckCanCast | AbilityConditions.CheckPlayerIncapacitated);
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
