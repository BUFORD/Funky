﻿using System;
using FunkyTrinity.Cache;
using Zeta.CommonBot;
using Zeta.Internals.SNO;

namespace FunkyTrinity.ability
{
	 ///<summary>
	 ///Creates Funcs from a created Ability and is to be used in testing of usability.
	 ///</summary>
	public class AbilityUsablityTests
	{

		public AbilityUsablityTests(Ability ability)
		{
			 this.LastConditionPassed=ConditionCriteraTypes.None;
			 this.Fcriteria=ability.Fcriteria;
			 this.Fprecast=ability.Fprecast;
			this.CreatePreCastConditions(ref Fprecast, ability);
			this.CreateTargetConditions(ref FSingleTargetUnitCriteria, ability);
			this.CreateUnitsInRangeConditions(ref FUnitsInRangeConditions, ability);
			this.CreateElitesInRangeConditions(ref FElitesInRangeConditions, ability);
			this.CreateClusterConditions(ref FClusterConditions, ability);
		}

		 public AbilityUsablityTests()
		 {
				this.LastConditionPassed=ConditionCriteraTypes.None;
				Fprecast=null;
				Fcriteria=new Func<bool>(() => { return true; });
				FClusterConditions=null;
				FUnitsInRangeConditions=null;
				FElitesInRangeConditions=null;
			 FSingleTargetUnitCriteria = null;
		 }

		 private Func<bool> Fprecast;
		 private Func<bool> Fcriteria;
		 private Func<bool> FClusterConditions;
		 private Func<bool> FUnitsInRangeConditions;
		 private Func<bool> FElitesInRangeConditions;
		 private Func<bool> FSingleTargetUnitCriteria;


		 #region Function Creation Methods
		 private void CreateClusterConditions(ref Func<bool> FClusterConditions, Ability ability)
		 {
				FClusterConditions=null;
				if (ability.ClusterConditions==null) return;

				FClusterConditions=new Func<bool>(() => { return Ability.CheckClusterConditions(ability.ClusterConditions); });
		 }

		private void CreatePreCastConditions(ref Func<bool> Fprecast, Ability ability)
		 {
				AbilityConditions precastconditions_=ability.PreCastConditions;
				if (precastconditions_.Equals(AbilityConditions.None))
					 return;
				else
				{
					 if (precastconditions_.HasFlag(AbilityConditions.CheckPlayerIncapacitated))
						  Fprecast+=(new Func<bool>(() => { return !Bot.Character.bIsIncapacitated; }));

					 if (precastconditions_.HasFlag(AbilityConditions.CheckPlayerRooted))
						  Fprecast+=(new Func<bool>(() => { return !Bot.Character.bIsRooted; }));

					 if (precastconditions_.HasFlag(AbilityConditions.CheckExisitingBuff))
						  Fprecast+=(new Func<bool>(() => { return !Bot.Class.HasBuff(ability.Power); }));

					 if (precastconditions_.HasFlag(AbilityConditions.CheckPetCount))
						  Fprecast+=(new Func<bool>(() => { return Bot.Class.MainPetCount<ability.Counter; }));

					 if (precastconditions_.HasFlag(AbilityConditions.CheckRecastTimer))
						  Fprecast+=(new Func<bool>(() => { return ability.LastUsedMilliseconds>ability.Cooldown; }));

					 if (precastconditions_.HasFlag(AbilityConditions.CheckCanCast))
					 {
						  Fprecast+=(new Func<bool>(() =>
						  {
								bool cancast=PowerManager.CanCast(ability.Power, out ability.CanCastFlags);

								//Special Ability -- Trigger Waiting For Special When Not Enough Resource to Cast.
								if (ability.IsSpecialAbility)
								{
									 if (!cancast&&ability.CanCastFlags.HasFlag(PowerManager.CanCastFlags.PowerNotEnoughResource))
										  Bot.Class.bWaitingForSpecial=true;
									 else
										  Bot.Class.bWaitingForSpecial=false;
								}

								return cancast;
						  }));
					 }

					 if (precastconditions_.HasFlag(AbilityConditions.CheckEnergy))
					 {
						  if (!ability.SecondaryEnergy)
								Fprecast+=(new Func<bool>(() =>
								{
									 bool energyCheck=Bot.Character.dCurrentEnergy>=ability.Cost;
									 if (ability.IsSpecialAbility) //we trigger waiting for special here.
										  Bot.Class.bWaitingForSpecial=!energyCheck;
									 return energyCheck;
								}));
						  else
								Fprecast+=(new Func<bool>(() =>
								{
									 bool energyCheck=Bot.Character.dDiscipline>=ability.Cost;
									 if (ability.IsSpecialAbility) //we trigger waiting for special here.
										  Bot.Class.bWaitingForSpecial=!energyCheck;
									 return energyCheck;
								}));
					 }
				}

		 }

		private void CreateTargetConditions(ref Func<bool> FSingleTargetUnitCriteria, Ability ability)
		 {

				FSingleTargetUnitCriteria=null;

			   //No Conditions Set by default.. (?? May have to verify ability execution can be Target)
			   //-- Ranged Abilities that do not set any single target conditions will never be checked for LOS.
				if (ability.TargetUnitConditionFlags==null)
				{
					 //No Default Conditions Set.. however if ability uses target as a execution type then we implement the LOS conditions.
					 if (ability.ExecutionType.HasFlag(AbilityUseType.Target|AbilityUseType.ClusterTarget|AbilityUseType.ClusterTargetNearest))
						  FSingleTargetUnitCriteria+=new Func<bool>(() => { return true; });
					 else
						  return;
				}
				else
					 CreateTargetFlagConditions(ref FSingleTargetUnitCriteria, ability.TargetUnitConditionFlags);	//Create conditions using TargetUnitCondition object

			
				

				//Ranged Abilities should check LOS!
				if (ability.IsRanged)
				{
					 FSingleTargetUnitCriteria+=new Func<bool>(() =>
					 {
						  if (!Bot.Target.CurrentUnitTarget.IgnoresLOSCheck)
						  {
								ability.LOSInfo LOSINFO=Bot.Target.CurrentTarget.LineOfSight;
								if (!Bot.Character.bIsIncapacitated&&(LOSINFO.LastLOSCheckMS>2000||(ability.IsProjectile&&!LOSINFO.ObjectIntersection.HasValue)||!LOSINFO.NavCellProjectile.HasValue))
								{
									 if (!LOSINFO.LOSTest(Bot.Character.Position, true, ability.IsProjectile, NavCellFlags.AllowProjectile))
									 {
										  //Raycast failed.. reset LOS Check -- for valid checking.
										  if (!LOSINFO.RayCast.Value) Bot.Target.CurrentTarget.RequiresLOSCheck=true;
										  return false;
									 }
								}
								else if ((ability.IsProjectile&&LOSINFO.ObjectIntersection.Value)||!LOSINFO.NavCellProjectile.Value)
								{
									 return false;
								}
						  }
						  return true;
					 });
				}
				else if(ability.Range>0)
				{//Melee
					 FSingleTargetUnitCriteria+=new Func<bool>(() =>
					 {
						  if (!Bot.Target.CurrentUnitTarget.IgnoresLOSCheck)
						  {
								//Check if within interaction range..
								if (Bot.Target.CurrentTarget.RadiusDistance>ability.Range)
								{
									 //Verify LOS walk
									 ability.LOSInfo LOSINFO=Bot.Target.CurrentTarget.LineOfSight;
									 if (!Bot.Character.bIsIncapacitated&&(LOSINFO.LastLOSCheckMS>2000||!LOSINFO.NavCellWalk.HasValue))
									 {
										  if (!LOSINFO.LOSTest(Bot.Character.Position, true, false, NavCellFlags.AllowWalk))
										  {
												//Raycast failed.. reset LOS Check -- for valid checking.
												if (!LOSINFO.RayCast.Value) 
													 Bot.Target.CurrentTarget.RequiresLOSCheck=true;
												if (!LOSINFO.NavCellWalk.Value)
													 return false;
										  }
									 }
									 else if (LOSINFO.NavCellWalk.HasValue&&!LOSINFO.NavCellWalk.Value)
									 {
										  return false;
									 }
								}
						  }
						  return true;
					 });
				}
			  
		 }
		
		 //We take the enums given by UnitTargetConditions (ability property) and add any new conditions to the func for testing
		private void CreateTargetFlagConditions(ref Func<bool> FSingleTargetUnitCriteria, UnitTargetConditions TargetUnitConditionFlags_)
		 {
			 //Distance
			  if (TargetUnitConditionFlags_.Distance>-1)
					FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentTarget.CentreDistance<=TargetUnitConditionFlags_.Distance; });
			 //Health
			 if (TargetUnitConditionFlags_.HealthPercent>0d)
					FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.CurrentHealthPct.Value<=TargetUnitConditionFlags_.HealthPercent; });


			  //TRUE CONDITIONS
			  if (TargetUnitConditionFlags_.TrueConditionFlags.Equals(TargetProperties.None))
					FSingleTargetUnitCriteria+=new Func<bool>(() => { return true; });
			  else
			  {
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.Boss))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentTarget.IsBoss; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.Burrowing))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentTarget.IsBurrowableUnit; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.FullHealth))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.CurrentHealthPct.Value==1d; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.IsSpecial))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentTarget.ObjectIsSpecial; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.Weak))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.UnitMaxHitPointAverageWeight<0; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.MissileDampening))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.MonsterMissileDampening; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.RareElite))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.IsEliteRareUnique; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.MissileReflecting))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentTarget.IsMissileReflecting; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.Shielding))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.MonsterShielding; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.Stealthable))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentTarget.IsStealthableUnit; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.SucideBomber))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentTarget.IsSucideBomber; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.TreasureGoblin))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentTarget.IsTreasureGoblin; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.Unique))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.MonsterUnique; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.Ranged))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.Monstersize.Value==MonsterSize.Ranged; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.TargetableAndAttackable))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.IsTargetableAndAttackable; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.Fast))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.IsFast; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.TrueConditionFlags,TargetProperties.DOTDPS))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.HasDOTdps.HasValue&&Bot.Target.CurrentUnitTarget.HasDOTdps.Value; });
			  }

			  //FALSE CONDITIONS
			  if (TargetUnitConditionFlags_.FalseConditionFlags.Equals(TargetProperties.None))
					FSingleTargetUnitCriteria+=new Func<bool>(() => { return true; });
			  else
			  {
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.Boss))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentTarget.IsBoss; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.Burrowing))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentTarget.IsBurrowableUnit; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.FullHealth))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.CurrentHealthPct.Value!=1d; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.IsSpecial))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentTarget.ObjectIsSpecial; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.Weak))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.UnitMaxHitPointAverageWeight>0; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.MissileDampening))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentUnitTarget.MonsterMissileDampening; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.RareElite))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentUnitTarget.IsEliteRareUnique; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.MissileReflecting))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentTarget.IsMissileReflecting; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.Shielding))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentUnitTarget.MonsterShielding; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.Stealthable))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentTarget.IsStealthableUnit; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.SucideBomber))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentTarget.IsSucideBomber; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.TreasureGoblin))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentTarget.IsTreasureGoblin; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.Unique))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentUnitTarget.MonsterUnique; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.Ranged))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return Bot.Target.CurrentUnitTarget.Monstersize.Value!=MonsterSize.Ranged; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.TargetableAndAttackable))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentUnitTarget.IsTargetableAndAttackable; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.Fast))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentUnitTarget.IsFast; });
					if (CheckTargetPropertyFlag(TargetUnitConditionFlags_.FalseConditionFlags,TargetProperties.DOTDPS))
						 FSingleTargetUnitCriteria+=new Func<bool>(() => { return !Bot.Target.CurrentUnitTarget.HasDOTdps.HasValue||!Bot.Target.CurrentUnitTarget.HasDOTdps.Value; });
			  }
		 }

		 //Quick research showed Enum.HasFlag is slower compared to the below method.
		 internal static bool CheckTargetPropertyFlag(TargetProperties property, TargetProperties flag)
		 {
			  return (property&flag)!=0;
		 }

		 internal static TargetProperties EvaluateUnitProperties(CacheUnit unit)
		 {
			  TargetProperties properties=TargetProperties.None;

			  if (unit.IsBoss)
					properties|=TargetProperties.Boss;

			  if (unit.IsBurrowableUnit)
					properties|=TargetProperties.Burrowing;

			  if (unit.MonsterMissileDampening)
					properties|=TargetProperties.MissileDampening;

			  if (unit.IsMissileReflecting)
					properties|=TargetProperties.MissileReflecting;

			  if (unit.MonsterShielding)
					properties|=TargetProperties.Shielding;

			  if (unit.IsStealthableUnit)
					properties|=TargetProperties.Stealthable;

			  if (unit.IsSucideBomber)
					properties|=TargetProperties.SucideBomber;

			  if (unit.IsTreasureGoblin)
					properties|=TargetProperties.TreasureGoblin;

			  if (unit.IsFast)
					properties|=TargetProperties.Fast;



			  if (unit.IsEliteRareUnique)
					properties|=TargetProperties.RareElite;

			  if (unit.MonsterUnique)
					properties|=TargetProperties.Unique;

			  if (unit.ObjectIsSpecial)
					properties|=TargetProperties.IsSpecial;

			  if (unit.CurrentHealthPct.HasValue&&unit.CurrentHealthPct.Value==1d)
					properties|=TargetProperties.FullHealth;

			  if (unit.UnitMaxHitPointAverageWeight<0)
					properties|=TargetProperties.Weak;


			  if (unit.Monstersize.HasValue&&unit.Monstersize.Value==MonsterSize.Ranged)
					properties|=TargetProperties.Ranged;


			  if (unit.IsTargetableAndAttackable)
					properties|=TargetProperties.TargetableAndAttackable;


			  if (unit.HasDOTdps.HasValue&&unit.HasDOTdps.Value)
					properties|=TargetProperties.DOTDPS;


			  return properties;
		 }

		private void CreateUnitsInRangeConditions(ref Func<bool> FUnitRange, Ability ability)
		 {
				FUnitRange=null;
				if (ability.UnitsWithinRangeConditions!=null)
					 FUnitRange+=new Func<bool>(() => { return Bot.Combat.iAnythingWithinRange[(int)ability.UnitsWithinRangeConditions.Item1]>=ability.UnitsWithinRangeConditions.Item2; });
		 }

		private void CreateElitesInRangeConditions(ref Func<bool> FUnitRange, Ability ability)
		 {
				FUnitRange=null;
				if (ability.ElitesWithinRangeConditions!=null)
					 FUnitRange+=new Func<bool>(() => { return Bot.Combat.iElitesWithinRange[(int)ability.ElitesWithinRangeConditions.Item1]>=ability.ElitesWithinRangeConditions.Item2; });
		 }
		 #endregion

		///<summary>
		///Tracks last successful condition if any.
		///</summary>
		public ConditionCriteraTypes LastConditionPassed
		{
			 get { return lastConditionPassed; }
			 set { lastConditionPassed=value; }
		}
		private ConditionCriteraTypes lastConditionPassed=ConditionCriteraTypes.None;

		///<summary>
		///Check Ability is valid to use.
		///</summary>
		public bool CheckPreCastConditionMethod()
		  {
				foreach (Func<bool> item in Fprecast.GetInvocationList())
				{
					 if (!item()) return false;
				}

				//Reset Last Condition
				LastConditionPassed=ConditionCriteraTypes.None;
				return true;
		  }
		///<summary>
		///Check Combat
		///</summary>
		public bool CheckCombatConditionMethod(ConditionCriteraTypes conditions=ConditionCriteraTypes.All)
		{
			 //Order in which tests are conducted..

			 //Units in Range (Not Cluster)
			 //Clusters
			 //Single Target

			 //If all are null or any of them are successful, then we test Custom Conditions
			 //Custom Condition


			 bool TestCustomConditions=false;
			 bool FailedCondition=false;

			 if (conditions.HasFlag(ConditionCriteraTypes.ElitesInRange)&&FElitesInRangeConditions!=null)
			 {
					foreach (Func<bool> item in this.FElitesInRangeConditions.GetInvocationList())
					{
						 if (!item())
						 {
								FailedCondition=true;
								break;
						 }
					}
					if (!FailedCondition)
					{
						 TestCustomConditions=true;
						 LastConditionPassed=ConditionCriteraTypes.ElitesInRange;
					}
			 }
			 if ((!TestCustomConditions||FailedCondition)&&conditions.HasFlag(ConditionCriteraTypes.UnitsInRange)&&FUnitsInRangeConditions!=null)
			 {
					FailedCondition=false;
					foreach (Func<bool> item in this.FUnitsInRangeConditions.GetInvocationList())
					{
						 if (!item())
						 {
								FailedCondition=true;
								break;
						 }
					}
					if (!FailedCondition)
					{
						 LastConditionPassed=ConditionCriteraTypes.UnitsInRange;
						 TestCustomConditions=true;
					}
			 }
			 if ((!TestCustomConditions||FailedCondition)&&conditions.HasFlag(ConditionCriteraTypes.Cluster)&&FClusterConditions!=null)
			 {
					FailedCondition=false;

					if (!this.FClusterConditions.Invoke())
					{
						 FailedCondition=true;
					}

					if (!FailedCondition)
					{
						 LastConditionPassed=ConditionCriteraTypes.Cluster;
						 TestCustomConditions=true;
					}
			 }
			 if ((!TestCustomConditions||FailedCondition)&&conditions.HasFlag(ConditionCriteraTypes.SingleTarget)&&FSingleTargetUnitCriteria!=null)
			 {
					FailedCondition=false;
					foreach (Func<bool> item in this.FSingleTargetUnitCriteria.GetInvocationList())
					{
						 if (!item())
						 {
								FailedCondition=true;
								break;
						 }
					}
					if (!FailedCondition)
					{
						 LastConditionPassed=ConditionCriteraTypes.SingleTarget;
						 TestCustomConditions=true;
					}
			 }

			 //If TestCustomCondtion failed, and FailedCondition is true.. then we tested a combat condition.
			 //If FailedCondition is false, then we never tested a condition.
			 if (!TestCustomConditions&&FailedCondition) return false;


			 foreach (Func<bool> item in this.Fcriteria.GetInvocationList())
					if (!item()) return false;


			 return true;
		}
	}
}
