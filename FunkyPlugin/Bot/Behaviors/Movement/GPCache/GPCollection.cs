﻿using System;
using System.Linq;
using Zeta;
using Zeta.Common;
using System.Collections.Generic;
using System.Collections;

namespace FunkyTrinity
{
	 public partial class Funky
	 {
		  public static partial class GridPointAreaCache
		  {
				public enum QuadrantLocation
				{
					 BottomRight=6,
					 BottomLeft=5,
					 TopRight=10,
					 TopLeft=9,
				}
				///<summary>
				///Point collection that manipulates points to test specific things about the points contained using sectors or the entire area it covers.
				///</summary>
				public class PointCollection : IDictionary<GridPoint, QuadrantLocation>
				{
					 private HashSet<GridPoint> NonNavigationalPoints=new HashSet<GridPoint>();
					 //To quickly lookup points locations within the grid.
					 //public Hashtable OutCodes=new Hashtable();
					 public Dictionary<GridPoint, QuadrantLocation> Points=new Dictionary<GridPoint, QuadrantLocation>();

					 public Dictionary<QuadrantLocation, GridPoint> CornerPoints=new Dictionary<QuadrantLocation, GridPoint>()
					 {
						  {QuadrantLocation.BottomRight, null},	 {QuadrantLocation.BottomLeft, null},
						  {QuadrantLocation.TopRight, null},		 {QuadrantLocation.TopLeft, null}, 
					 };

					 private GridPoint tl, br, tr, bl;
					 public GridPoint TopLeft { get { return tl; } }
					 public GridPoint TopRight { get { return tr; } }
					 public GridPoint BottomRight { get { return br; } }
					 public GridPoint BottomLeft { get { return bl; } }

					 private GridPoint Centeroid
					 {
						  get
						  {
								GridPoint[] thesepoints=(from GridPoint points in this.Points.Keys
																 select points).ToArray();

								if (thesepoints!=null&&thesepoints.Length>0)
								{
									 GridPoint P=thesepoints[0];
									 for (int i=1; i<thesepoints.Length-1; i++)
									 {
										  P=P+thesepoints[i];
									 }
									 P=P/thesepoints.Length;
									 return P;
								}
								return centerpoint;
						  }
					 }
					 private GridPoint centerpoint_;
					 public GridPoint centerpoint
					 {
						  get
						  {
								return centerpoint_;
						  }
						  set
						  {
								centerpoint_=value;
								tl=value;//reset tl,br
								br=value;
						  }
					 }


					 #region Quadrant Bytes
					 private const int INSIDE=0; // 0000
					 private const int LEFT=1;   // 0001
					 private const int RIGHT=2;  // 0010
					 private const int BOTTOM=4; // 0100
					 private const int TOP=8;    // 1000
					 private byte ComputeOutCode(double x, double y)
					 {
						  byte code=INSIDE;
						  if (x<=centerpoint_.X)           // to the left of center
								code|=LEFT;
						  else if (x>centerpoint_.X)      // to the right of center
								code|=RIGHT;
						  if (y>=centerpoint_.Y)           // below the center
								code|=BOTTOM;
						  else if (y<centerpoint_.Y)      // above the center
								code|=TOP;
						  return code;
					 }
					 #endregion

					 public double DiagonalLength
					 {
						  get
						  {
								return (GridPoint.GetDistanceBetweenPoints(BottomRight, TopLeft));
						  }
					 }
					 public List<GridPoint> NonIgnoredPoints
					 {
						  get
						  {
								/*
								return (from GridPoint points in OutCodes.Keys
										  where points.Ignored==false
										  select points).ToList();
						  */

								return NonNavigationalPoints.ToList();
						  }
					 }

					 private System.Windows.Rect rect_;
					 public System.Windows.Rect Rect
					 {
						  get
						  {
								if (rect_==null)
									 rect_=new System.Windows.Rect(this.CornerPoints[QuadrantLocation.TopLeft], this.CornerPoints[QuadrantLocation.BottomRight]);

								return rect_;
						  }
					 }


					 public void DebugInfo()
					 {
						  Logging.WriteVerbose("TL {0} BR {1} DiagonalLength {2} TOTAL POINTS {3} / Not Ignored {4} with centeroid {5}",
								TopLeft.ToString(), BottomRight.ToString(), DiagonalLength.ToString(), Points.Keys.Count.ToString(), NonIgnoredPoints.Count.ToString(), Centeroid.ToString());

					 }

					 public GridPoint[] QuadrantPoints(QuadrantLocation SectorCode, out GridPoint EndPoint)
					 {
						  GridPoint[] containedPoints=
						  (from p in this.Points
							where p.Value==SectorCode
							select p.Key).ToArray();

						  EndPoint=CornerPoints[SectorCode];

						  if (containedPoints==null) return null;

						  GridPoint[] SortedPoints=containedPoints.OrderBy(p => !p.Ignored).ToArray();


						  if (!SortedPoints.Any(p => !p.Ignored))
								return null;
						  else
								return SortedPoints;

						  GridPoint[] NonIgnoredPoints=SortedPoints.TakeWhile(p => !p.Ignored).ToArray();

						  int IgnoredArrayIndex=NonIgnoredPoints.Length-1;

						  string debugstring="Sector Point Count "+containedPoints.Count()+" "
								+" Endpoint "+EndPoint.ToString();

						  return NonIgnoredPoints;

					 }

					 public void Update(GridPoint point, QuadrantLocation SectorCode)
					 {
						  switch (SectorCode)
						  {
								case QuadrantLocation.BottomRight:
									 if (point>br) br=point;
									 break;
								case QuadrantLocation.TopLeft:
									 if (point<tl) tl=point;
									 break;
						  }
						  bl=new GridPoint(tl.X, br.Y);
						  tr=new GridPoint(br.X, tl.Y);

						  CornerPoints[QuadrantLocation.BottomRight]=br;
						  CornerPoints[QuadrantLocation.BottomLeft]=bl;
						  CornerPoints[QuadrantLocation.TopLeft]=tl;
						  CornerPoints[QuadrantLocation.TopRight]=tr;
					 }

					 public PointCollection()
					 {

					 }
					 public PointCollection(PointCollection clone)
					 {
						  this.bl=clone.BottomLeft.Clone();
						  this.br=clone.BottomRight.Clone();
						  this.tl=clone.TopLeft.Clone();
						  this.tr=clone.TopRight.Clone();
						  this.Points=new Dictionary<GridPoint, QuadrantLocation>(clone.Points);
						  GridPoint[] NonNavPoints=new GridPoint[clone.NonNavigationalPoints.Count-1];
						  clone.NonNavigationalPoints.CopyTo(NonNavPoints);

						  foreach (var item in NonNavPoints)
						  {
								this.NonNavigationalPoints.Add(item.Clone());
						  }

						  this.NonIgnoredPoints.AddRange(clone.NonIgnoredPoints);
						  foreach (var item in clone.CornerPoints)
						  {
								this.CornerPoints.Add(item.Key, item.Value.Clone());
						  }

						  this.centerpoint_=clone.centerpoint_.Clone();
					 }


					 #region ICollection<GridPoint> Members

					 public void Add(GridPoint item)
					 {
						  QuadrantLocation code_=(QuadrantLocation)ComputeOutCode(item.X, item.Y);
						  this.Add(item, code_);

						  if (item.Ignored&&!NonNavigationalPoints.Contains(item))
								NonNavigationalPoints.Add(item);
					 }

					 public void Clear()
					 {
						  //this.OutCodes.Clear();
						  this.Points.Clear();
					 }

					 public bool Contains(GridPoint item)
					 {
						  return this.Points.Keys.Contains(item);
					 }

					 public void CopyTo(GridPoint[] array, int arrayIndex)
					 {
						  this.Points.Keys.CopyTo(array, arrayIndex);
					 }

					 public int Count
					 {
						  get { return this.Points.Count; }
					 }

					 public bool IsReadOnly
					 {
						  get { throw new NotImplementedException(); }
					 }

					 public void Remove(GridPoint item)
					 {
						  this.Points.Remove(item);
					 }

					 #endregion



					 #region IDictionary<GridPoint,QuadrantLocation> Members

					 public void Add(GridPoint key, QuadrantLocation value)
					 {
						  this.Points.Add(key, value);
						  Update(key, value);
					 }

					 public bool ContainsKey(GridPoint key)
					 {
						  return this.Points.ContainsKey(key);
					 }

					 public ICollection<GridPoint> Keys
					 {
						  get { return this.Points.Keys; }
					 }

					 bool IDictionary<GridPoint, QuadrantLocation>.Remove(GridPoint key)
					 {
						  throw new NotImplementedException();
					 }

					 public bool TryGetValue(GridPoint key, out QuadrantLocation value)
					 {
						  return this.Points.TryGetValue(key, out value);
					 }

					 public ICollection<QuadrantLocation> Values
					 {
						  get { return this.Points.Values; }
					 }

					 public QuadrantLocation this[GridPoint key]
					 {
						  get
						  {
								return this.Points[key];
						  }
						  set
						  {
								this.Points[key]=value;
						  }
					 }

					 #endregion

					 #region ICollection<KeyValuePair<GridPoint,QuadrantLocation>> Members

					 public void Add(KeyValuePair<GridPoint, QuadrantLocation> item)
					 {
						  throw new NotImplementedException();
					 }

					 public bool Contains(KeyValuePair<GridPoint, QuadrantLocation> item)
					 {
						  throw new NotImplementedException();
					 }

					 public void CopyTo(KeyValuePair<GridPoint, QuadrantLocation>[] array, int arrayIndex)
					 {
						  throw new NotImplementedException();
					 }

					 public bool Remove(KeyValuePair<GridPoint, QuadrantLocation> item)
					 {
						  throw new NotImplementedException();
					 }

					 #endregion

					 #region IEnumerable<KeyValuePair<GridPoint,QuadrantLocation>> Members

					 public IEnumerator<KeyValuePair<GridPoint, QuadrantLocation>> GetEnumerator()
					 {
						  return this.Points.GetEnumerator();
					 }

					 #endregion

					 #region IEnumerable Members

					 IEnumerator IEnumerable.GetEnumerator()
					 {
						  return this.Points.GetEnumerator();
					 }

					 #endregion
				}
		  }
	 }
}