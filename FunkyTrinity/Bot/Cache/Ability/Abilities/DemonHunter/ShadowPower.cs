﻿using System;
using FunkyTrinity.Enums;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.ability.Abilities.DemonHunter
{
	public class ShadowPower : Ability, IAbility
	{
		public ShadowPower() : base()
		{
		}



		public override void Initialize()
		{
			ExecutionType = PowerExecutionTypes.Buff;
			WaitVars = new WaitLoops(1, 1, true);
			Cost = 14;
			SecondaryEnergy = true;
			UseFlagsType=AbilityUseFlags.Anywhere;
			Priority = AbilityPriority.High;
			PreCastConditions = (CastingConditionTypes.CheckPlayerIncapacitated | CastingConditionTypes.CheckRecastTimer |
			                     CastingConditionTypes.CheckEnergy);

			Fcriteria = new Func<bool>(() =>
			{
				return (Bot.Character.dCurrentHealthPct <= 0.99d || Bot.Character.bIsRooted ||
				        Bot.Combat.iElitesWithinRange[(int) RangeIntervals.Range_25] >= 1 ||
				        Bot.Combat.iAnythingWithinRange[(int) RangeIntervals.Range_15] >= 3);

			});
		}

		#region IAbility

		public override int RuneIndex
		{
			get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power) ? Bot.Class.RuneIndexCache[this.Power] : -1; }
		}

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
			get { return SNOPower.DemonHunter_ShadowPower; }
		}
	}
}
