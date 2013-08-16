﻿using System;
using FunkyTrinity.Enums;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.ability.Abilities.Wizard
{
	public class MirrorImage : Ability, IAbility
	{
		public MirrorImage() : base()
		{
		}



		public override void Initialize()
		{
			ExecutionType = AbilityUseType.Buff;
			WaitVars = new WaitLoops(1, 1, true);
			Cost = 10;
			Range = 48;
			UseageType=AbilityUseage.Combat;
			Priority = AbilityPriority.High;
			PreCastConditions = (AbilityConditions.CheckCanCast);

			Fcriteria = new Func<bool>(() =>
			{
				return (Bot.Character.dCurrentHealthPct <= 0.50 ||
				        Bot.Combat.iAnythingWithinRange[(int) RangeIntervals.Range_30] >= 5 || Bot.Character.bIsIncapacitated ||
				        Bot.Character.bIsRooted || Bot.Target.CurrentTarget.ObjectIsSpecial);
			});
		}

		public override void InitCriteria()
		{
			base.AbilityTestConditions = new AbilityUsablityTests(this);
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
			get { return SNOPower.Wizard_MirrorImage; }
		}
	}
}