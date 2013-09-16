﻿using System;
using FunkyTrinity.Enums;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.ability.Abilities.DemonHunter
{
	public class Sentry : Ability, IAbility
	{
		public Sentry() : base()
		{
		}



		public override void Initialize()
		{
			ExecutionType = PowerExecutionTypes.Self;
			WaitVars = new WaitLoops(0, 0, true);
			Cost = 30;
			UseFlagsType=AbilityUseFlags.Anywhere;
			Priority = AbilityPriority.High;
			PreCastConditions = (CastingConditionTypes.CheckEnergy | CastingConditionTypes.CheckRecastTimer |
			                     CastingConditionTypes.CheckPlayerIncapacitated);

			Fcriteria = new Func<bool>(() =>
			{
				return Bot.Combat.powerLastSnoPowerUsed != SNOPower.DemonHunter_Sentry &&
				       (Bot.Combat.FleeingLastTarget || DateTime.Now.Subtract(Bot.Combat.LastFleeAction).TotalMilliseconds < 1000) ||
				       (Bot.Combat.iElitesWithinRange[(int) RangeIntervals.Range_40] >= 1 ||
				        Bot.Combat.iAnythingWithinRange[(int) RangeIntervals.Range_40] >= 2);
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
			get { return SNOPower.DemonHunter_Sentry; }
		}
	}
}
