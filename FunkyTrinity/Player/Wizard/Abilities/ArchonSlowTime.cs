﻿using System;

using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.Ability.Abilities.Wizard
{
	public class ArchonSlowTime : ability, IAbility
	{
		public ArchonSlowTime() : base()
		{
		}



		public override void Initialize()
		{
			ExecutionType = AbilityUseType.Target;
			WaitVars = new WaitLoops(1, 1, true);
			Range = 48;
			UseageType=AbilityUseage.Anywhere;
			Priority = AbilityPriority.Low;
			PreCastConditions = (AbilityConditions.CheckPlayerIncapacitated | AbilityConditions.CheckRecastTimer |
			                     AbilityConditions.CheckCanCast);
			UnitsWithinRangeConditions = new Tuple<RangeIntervals, int>(RangeIntervals.Range_25, 2);
			ElitesWithinRangeConditions = new Tuple<RangeIntervals, int>(RangeIntervals.Range_25, 1);
			TargetUnitConditionFlags = new UnitTargetConditions(TargetProperties.IsSpecial, 35);
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
				ability p = (ability) obj;
				return this.Power == p.Power;
			}
		}

		#endregion

		public override SNOPower Power
		{
			get { return SNOPower.Wizard_Archon_SlowTime; }
		}
	}
}
