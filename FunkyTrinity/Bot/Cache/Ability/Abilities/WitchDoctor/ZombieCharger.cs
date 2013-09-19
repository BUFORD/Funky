﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.ability.Abilities.WitchDoctor
{
	public class ZombieCharger : Ability, IAbility
	{
		public ZombieCharger() : base()
		{
		}



		public override int RuneIndex { get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power)?Bot.Class.RuneIndexCache[this.Power]:-1; } }

		public override void Initialize()
		{
			ExecutionType = AbilityUseType.ClusterTarget | AbilityUseType.Target;

			WaitVars = new WaitLoops(0, 1, true);
			Cost = 134;
			Range = 11;
			UseageType=AbilityUseage.Combat;
			Priority = AbilityPriority.Low;

			PreCastConditions = (AbilityConditions.CheckPlayerIncapacitated | AbilityConditions.CheckEnergy |
			                     AbilityConditions.CheckCanCast);

			Fprecast=new Func<bool>(() => { return !Bot.Class.HasDebuff(SNOPower.Succubus_BloodStar); });

			ClusterConditions = new ClusterConditions(5d, 20f, 2, true);
			TargetUnitConditionFlags = new UnitTargetConditions(TargetProperties.IsSpecial, 15);
		}

		public override void InitCriteria()
		{
			base.AbilityTestConditions = new AbilityUsablityTests(this);
		}

		#region IAbility

		public override int GetHashCode()
		{
			return (int) this.Power;
		}

		public override bool Equals(object obj)
		{
			//Check for null and compare run-time types. 
			if (obj == null || this.GetType() != obj.GetType())
			{
				return false;
			}
			else
			{
				Ability p = (Ability) obj;
				return this.Power == p.Power;
			}
		}

		#endregion

		public override SNOPower Power
		{
			get { return SNOPower.Witchdoctor_ZombieCharger; }
		}
	}
}
