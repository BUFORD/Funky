﻿using System;
using FunkyTrinity.Enums;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.ability.Abilities.Wizard
{
	public class DiamondSkin : Ability, IAbility
	{
		public DiamondSkin() : base()
		{
		}



		public override void Initialize()
		{
			ExecutionType = PowerExecutionTypes.Buff;
			WaitVars = new WaitLoops(0, 1, true);
			Cost = 10;
			Counter = 1;
			Range = 0;
			UseFlagsType=AbilityUseFlags.Anywhere;
			Priority = AbilityPriority.High;
			PreCastConditions = (CastingConditionTypes.CheckCanCast |
			                     CastingConditionTypes.CheckExisitingBuff);

			Fcriteria = new Func<bool>(() =>
			{
				return (Bot.Combat.iElitesWithinRange[(int) RangeIntervals.Range_25] > 0 ||
				        Bot.Combat.iAnythingWithinRange[(int) RangeIntervals.Range_25] > 0 ||
				        Bot.Character.dCurrentHealthPct <= 0.90 || Bot.Character.bIsIncapacitated || Bot.Character.bIsRooted ||
				        (Bot.Target.CurrentTarget.RadiusDistance <= 40f));
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
			get { return SNOPower.Wizard_DiamondSkin; }
		}
	}
}
