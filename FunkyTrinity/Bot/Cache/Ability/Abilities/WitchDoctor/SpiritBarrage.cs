﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace FunkyTrinity.ability.Abilities.WitchDoctor
{
	 public class SpiritBarrage : Ability, IAbility
	 {
			public SpiritBarrage()
				 : base()
			{
			}



			public override void Initialize()
		{
			ExecutionType = AbilityUseType.ClusterTargetNearest | AbilityUseType.Target;
			ClusterConditions = new ClusterConditions(5d, 20f, 1, true);
			TargetUnitConditionFlags = new UnitTargetConditions(TargetProperties.None, 25,
				falseConditionalFlags: TargetProperties.DOTDPS);
			WaitVars = new WaitLoops(1, 1, true);
			Cost = 108;
			Range = 21;
			UseageType= AbilityUseage.Combat;
			Priority = AbilityPriority.Low;
			PreCastConditions = (AbilityConditions.CheckPlayerIncapacitated | AbilityConditions.CheckCanCast |
				                     AbilityConditions.CheckEnergy);

			Fprecast=new Func<bool>(() => { return !Bot.Class.HasDebuff(SNOPower.Succubus_BloodStar); });

			Fcriteria = new Func<bool>(() =>
			{
				return !Bot.Class.bWaitingForSpecial;
			});
		}

			public override void InitCriteria()
			{
				 base.AbilityTestConditions=new AbilityUsablityTests(this);
			}

			#region IAbility

			public override int RuneIndex
			{
				 get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power)?Bot.Class.RuneIndexCache[this.Power]:-1; }
			}

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

			public override SNOPower Power
			{
				 get { return SNOPower.Witchdoctor_SpiritBarrage; }
			}
	 }
}
