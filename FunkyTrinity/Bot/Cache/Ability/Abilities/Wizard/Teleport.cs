﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.ability.Abilities.Wizard
{
	public class Teleport : Ability, IAbility
	{
		public Teleport() : base()
		{
		}



		public override void Initialize()
		{
			ExecutionType = PowerExecutionTypes.ClusterLocation | PowerExecutionTypes.ZigZagPathing;
			WaitVars = new WaitLoops(0, 1, true);
			Cost = 15;
			Range = 35;
			UseFlagsType=AbilityUseFlags.Combat;
			//IsNavigationSpecial = true;
			Priority = AbilityPriority.High;
			PreCastConditions = (CastingConditionTypes.CheckPlayerIncapacitated | CastingConditionTypes.CheckCanCast |
			                     CastingConditionTypes.CheckEnergy);
			ClusterConditions = new ClusterConditions(5d, 48f, 2, false);
								//TestCustomCombatConditionAlways=true,
			Fcriteria = new Func<bool>(() =>
			{
				return ((Bot.SettingsFunky.Class.bTeleportFleeWhenLowHP && Bot.Character.dCurrentHealthPct < 0.5d)
				        ||
				        (Bot.SettingsFunky.Class.bTeleportIntoGrouping &&
				         Bot.Combat.Clusters(new ClusterConditions(5d, 48f, 2, false)).Count > 0 &&
				         Bot.Combat.Clusters(new ClusterConditions(5d, 48f, 2, false))[0].Midpoint.Distance(
					         Bot.Character.PointPosition) > 15f)
				        || (!Bot.SettingsFunky.Class.bTeleportFleeWhenLowHP && !Bot.SettingsFunky.Class.bTeleportIntoGrouping));
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
			get { return SNOPower.Wizard_Teleport; }
		}
	}
}
