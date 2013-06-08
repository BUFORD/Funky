﻿using System;
using System.Linq;
using Zeta.Common;
using Zeta.Internals.Actors;
using System.Windows;

namespace FunkyTrinity
{
	 public partial class Funky
	 {

		  internal abstract class CacheObstacle : CacheObject
		  {

				public virtual double PointRadius
				{
					 get
					 {
						  return this.Radius/2.5f;
					 }
				}
				public override float Radius
				{
					 get
					 {
						  return base.CollisionRadius.HasValue?base.CollisionRadius.Value:base.Radius;
					 }
				}
				public override float RadiusDistance
				{
					 get
					 {
						  return Math.Max(0f, base.CentreDistance-this.Radius);
					 }
				}
				public new string DebugString
				{
					 get
					 {
						  string debugstring=base.DebugString+"\r\n";
						  debugstring+="Type: "+this.Obstacletype.Value.ToString()+"\r\n";
						  return debugstring;
					 }
				}

				public int BlacklistRefreshCounter { get; set; }

				///<summary>
				///Tests if this intersects with current bot position using CacheObject
				///</summary>
				public virtual bool TestIntersection(CacheObject OBJ, Vector3 BotPosition)
				{
					 return GilesIntersectsPath(base.Position, this.Radius, BotPosition, base.BotMeleeVector);

				}
				///<summary>
				///Tests if this intersects between two vectors
				///</summary>
				public virtual bool TestIntersection(Vector3 V1, Vector3 V2)
				{
					 return GilesIntersectsPath(base.Position, this.Radius, V1, V2);
				}
				///<summary>
				///Tests if this intersects between two gridpoints
				///</summary>
				public virtual bool TestIntersection(GridPoint V1, GridPoint V2)
				{
					 return GridPoint.LineIntersectsRect(V1, V2, this.AvoidanceRect);
					// return GilesIntersectsPath(base.Position, this.Radius, V1, V2);
				}
				//Special Method used inside cache collection
				public virtual bool PointInside(GridPoint Pos)
				{
					 //return (Math.Min(rect_.TopLeft.X, rect_.BottomRight.X)>=Pos.X&&Math.Max(rect_.TopLeft.X, rect_.BottomRight.X)<=Pos.X&&
					 //Math.Min(rect_.TopLeft.Y, rect_.BottomRight.Y)>=Pos.Y&&Math.Max(rect_.TopLeft.Y, rect_.BottomRight.Y)<=Pos.Y);
					 return (GridPoint.GetDistanceBetweenPoints(base.PointPosition, Pos))<=(this.PointRadius);
				}
				public virtual bool PointInside(Vector3 V3)
				{
					 return base.Position.Distance(V3)<=this.Radius;
				}

				private System.Windows.Rect rect_;
				public virtual System.Windows.Rect AvoidanceRect
				{
					 get
					 {
						  if (!this.rect_.Contains(PointPosition))
						  {
								this.rect_=new Rect(PointPosition, new Size(0, 0));
								this.rect_.Inflate(new Size(Radius, Radius));
						  }
						  return rect_;
					 }
				}


				///<summary>
				///
				///</summary>
				public CacheObstacle(CacheObject fromObj)
					 : base(fromObj)
				{
					 if (!base.Obstacletype.HasValue)
						  base.Obstacletype=ObstacleType.None;
				}

				public override int GetHashCode()
				{
					 return base.GetHashCode();
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
						  return base.Equals((CacheObject)obj);
					 }
				}

		  }



		  ///<summary>
		  ///Used for all avoidance objects
		  ///</summary>
		  internal class CacheAvoidance : CacheObstacle
		  {
				public new string DebugString
				{
					 get
					 {
						  string debugstring=base.DebugString+"\r\n";
						  debugstring+="AvoidanceType: "+this.AvoidanceType.ToString()+"\r\n";
						  debugstring+=this.Obstacletype.Value==ObstacleType.MovingAvoidance?"Rotation "+Rotation.ToString()+" Speed "+Speed.ToString()+" ":"";
						  return debugstring;
					 }
				}
				public override double Weight
				{
					 get
					 {
						  double thisweight=0d;
						  if (this.AvoidanceType.HasFlag(Funky.AvoidanceType.ArcaneSentry|Funky.AvoidanceType.Dececrator|Funky.AvoidanceType.MoltenCore|Funky.AvoidanceType.PlagueCloud))
						  {
								thisweight+=10d;
						  }
						  if (this.AvoidanceType==Funky.AvoidanceType.ArcaneSentry&&base.CentreDistance<30f)
						  {
								thisweight+=30-base.CentreDistance;
						  }

						  return base.Weight;
					 }
					 set
					 {
						  base.Weight=value;
					 }
				}

				public override Vector3 Position
				{
					 get
					 {
						  return base.Position;
					 }
					 set
					 {
						  base.Position=value;
						  if (this.Obstacletype.Value==ObstacleType.MovingAvoidance&&ray_.Position!=null)
						  {
								//Update projecitle related
								Vector3 newDirectionV3=ray_.Direction;
								ray_=new Ray(value, ray_.Direction);
								ProjectileMaxRange=100f-(Vector3.Distance(value, projectile_startPosition));

						  }
					 }
				}

				public override double PointRadius
				{
					 get
					 {
						  return this.Radius/2.5f;
					 }
				}

				///<summary>
				///Returns avoidance type if any was set
				///</summary>
				public AvoidanceType AvoidanceType { get; set; }

				public override float Radius
				{
					 get
					 {
						  float radius=dictAvoidanceRadius[AvoidanceType];

						  //Modify radius during critical avoidance for arcane sentry.
						  if (Bot.Combat.CriticalAvoidance&&AvoidanceType==Funky.AvoidanceType.ArcaneSentry)
								radius=25f;

						  return radius;
					 }
				}

				public bool CheckUpdateForProjectile
				{
					 get
					 {
						  return Vector3.Distance(Bot.Character.Position, Position)>Bot.Character.fCharacterRadius;

					 }
				}

				private bool projectileraytest_=true;
				///<summary>
				///Check if this projectile will directly impact the bot.
				///</summary>
				public bool UpdateProjectileRayTest(Vector3 newPosition)
				{

					 //Return last value during blacklist count
					 if (BlacklistRefreshCounter>0) return projectileraytest_;

					 this.Position=newPosition;

					 if (SettingsFunky.UseAdvancedProjectileTesting)
					 {
						  //Do fancy checks for this fixed projectile.
						  if (this.Ray.Intersects(Bot.Character.CharacterSphere).HasValue)
						  {
								//We get the total Milliseconds it took from now since last refresh occured.
								double LastRefreshMS=DateTime.Now.Subtract(lastRefreshedObjects).TotalMilliseconds;
								//LastRefreshMS=MathEx.Clamp(150f, 250f, (float)LastRefreshMS);

								//Now we get the distance from us, divide it by the speed (which is also divided by 10 to normalize it) to get the total, this is than divided by our lastrefresh time, which gives us our loops before intersection.
								//Example: 35f away, 0.02f is speed, when divided is equal to 1750. Average Refresh is 150ms, so the loops would be ~11.6
								BlacklistRefreshCounter=(int)(Math.Round((Vector3.Distance(Position, Bot.Character.Position)/(Speed/10))/LastRefreshMS));
								if (BlacklistRefreshCounter<5&&BlacklistRefreshCounter>1)
								{
									 projectileraytest_=true;
									 return projectileraytest_;
								}
						  }
						  else
								projectileraytest_=false;
					 }
					 else
					 {
						  Vector3 ProjectileEndPoint=MathEx.GetPointAt(this.Position, 40f, this.Rotation);
						  projectileraytest_=MathEx.IntersectsPath(Bot.Character.Position, Bot.Character.fCharacterRadius, this.Position, ProjectileEndPoint);
					 }

					 BlacklistRefreshCounter=3;
					 return projectileraytest_;
				}

				private Vector3 DirectionVector3;
				private Vector2 directionV_;
				public Vector2 DirectionVector
				{
					 get
					 {
						  return directionV_;
					 }
					 set
					 {
						  directionV_=value;
						  DirectionVector3=value.ToVector3();
					 }
				}
				private Ray ray_;
				public Ray Ray
				{
					 get
					 {
						  return ray_;
					 }
				}
				public float Rotation { get; set; }
				public double Speed { get; set; }
				private Vector3 projectile_startPosition { get; set; }
				private float ProjectileMaxRange { get; set; }

				public bool ShouldAvoid
				{
					 get
					 {
						  return !(Bot.Class.IgnoreAvoidance(AvoidanceType));
					 }
				}

				public bool IsActiveAvoidance
				{
					 get
					 {
						  return ((SettingsFunky.AttemptAvoidanceMovements||Bot.Combat.CriticalAvoidance)
									 &&Bot.Class.AvoidancesHealth.ContainsKey(this.AvoidanceType)&&Bot.Class.AvoidancesHealth[this.AvoidanceType]>0d);
					 }
				}


				public override bool TestIntersection(CacheObject OBJ, Vector3 BotPosition)
				{
					 if (this.Obstacletype.Value==ObstacleType.MovingAvoidance)
					 {
						  return ProjectileIntersects(this, BotPosition, OBJ.Position, this.ProjectileMaxRange);
					 }

					 return GilesIntersectsPath(base.Position, this.Radius, BotPosition, OBJ.Position);
				}

				public override bool PointInside(GridPoint Pos)
				{
					 return GridPoint.GetDistanceBetweenPoints(base.PointPosition, Pos)<=(this.PointRadius);
				}
				public override bool PointInside(Vector3 V3)
				{
					 return base.Position.Distance2D(V3)<=this.Radius;
				}
				public override bool TestIntersection(Vector3 V1, Vector3 V2)
				{
					 return GilesIntersectsPath(this.Position, this.Radius, V1, V2);
				}

				public override Rect AvoidanceRect
				{
					 get
					 {
						  if (this.Obstacletype.Value==ObstacleType.MovingAvoidance)
						  {
								Vector3 thisEndVector=MathEx.GetPointAt(base.Position, this.ProjectileMaxRange, this.Rotation);
								return new Rect(base.PointPosition, (GridPoint)thisEndVector);
						  }

						  return base.AvoidanceRect;
					 }
				}

				public CacheAvoidance(CacheObject parent, AvoidanceType avoidancetype)
					 : base(parent)
				{
					 this.AvoidanceType=avoidancetype;
				}

				public CacheAvoidance(CacheObject parent, AvoidanceType type, Ray R, double speed)
					 : base(parent)
				{
					 this.AvoidanceType=type;
					 this.ray_=R;
					 this.Speed=speed;
					 this.projectile_startPosition=base.Position;
				}
		  }


		  ///<summary>
		  ///Used for all non-avoidance objects (Monsters, Gizmos, and Misc Objects)
		  ///</summary>
		  internal class CacheServerObject : CacheObstacle
		  {
				public CacheServerObject(CacheObject parent)
					 : base(parent)
				{
				}
		  }
	 }
}