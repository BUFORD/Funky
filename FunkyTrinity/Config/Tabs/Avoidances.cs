﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FunkyTrinity.Avoidance;
using FunkyTrinity.Cache;
using FunkyTrinity.Enums;
using FunkyTrinity.Settings;

namespace FunkyTrinity
{
	 internal partial class FunkyWindow : Window
	 {
		  #region EventHandling
		  private void AvoidanceLoadSettingsButtonClicked(object sender, EventArgs e)
		  {

				System.Windows.Forms.OpenFileDialog OFD=new System.Windows.Forms.OpenFileDialog
				{
					 InitialDirectory=Path.Combine(Funky.FolderPaths.SettingsDefaultPath, "Specific"),
					 RestoreDirectory=false,
					 Filter="xml files (*.xml)|*.xml|All files (*.*)|*.*",
					 Title="Avoidance Template",
				};
				System.Windows.Forms.DialogResult OFD_Result=OFD.ShowDialog();

				if (OFD_Result==System.Windows.Forms.DialogResult.OK)
				{
					 try
					 {
						  //;
						  SettingAvoidance newSettings=SettingAvoidance.DeserializeFromXML(OFD.FileName);
						  Bot.SettingsFunky.Avoidance=newSettings;
						  Funky.funkyConfigWindow.Close();
					 } catch
					 {

					 }
				}
		  }
		  private void AvoidanceRadiusSliderValueChanged(object sender, EventArgs e)
		  {
				Slider slider_sender=(Slider)sender;
				string[] slider_info=slider_sender.Name.Split("_".ToCharArray());
				int tb_index=Convert.ToInt16(slider_info[2]);
				float currentValue=(int)slider_sender.Value;

				TBavoidanceRadius[tb_index].Text=currentValue.ToString();
				Bot.SettingsFunky.Avoidance.Avoidances[tb_index].Radius=currentValue;
				//AvoidanceType avoidancetype=(AvoidanceType)Enum.Parse(typeof(AvoidanceType), slider_info[0]);
				//Bot.SettingsFunky.Avoidance.AvoidanceRadiusValues[avoidancetype]=currentValue;
				//Bot.SettingsFunky.Avoidance.RecreateAvoidances();
		  }

		  private void AvoidanceHealthSliderValueChanged(object sender, EventArgs e)
		  {
				Slider slider_sender=(Slider)sender;
				string[] slider_info=slider_sender.Name.Split("_".ToCharArray());
				double currentValue=Convert.ToDouble(slider_sender.Value.ToString("F2", CultureInfo.InvariantCulture));
				int tb_index=Convert.ToInt16(slider_info[2]);

				TBavoidanceHealth[tb_index].Text=currentValue.ToString();
				Bot.SettingsFunky.Avoidance.Avoidances[tb_index].Health=currentValue;
				//AvoidanceType avoidancetype=(AvoidanceType)Enum.Parse(typeof(AvoidanceType), slider_info[0]);
				//Bot.SettingsFunky.Avoidance.AvoidanceHealthValues[avoidancetype]=currentValue;
				//Bot.SettingsFunky.Avoidance.RecreateAvoidances();
		  }

		  #endregion

		  private TextBox[] TBavoidanceRadius;
		  private TextBox[] TBavoidanceHealth;

		  internal void InitAvoidanceControls()
		  {
				TabItem AvoidanceTabItem=new TabItem
				{
					 Header="Avoidances",
				};
				AvoidanceTabItem.Header="Avoidances";
				CombatTabControl.Items.Add(AvoidanceTabItem);
				StackPanel LBcharacterAvoidance=new StackPanel
				{
					 Orientation= Orientation.Vertical,
				};
				#region Avoidances

				StackPanel AvoidanceOptionsStackPanel=new StackPanel
				{
					 //Orientation= System.Windows.Controls.Orientation.Vertical,
					 //HorizontalAlignment= System.Windows.HorizontalAlignment.Stretch,
					 Margin=new Thickness(Margin.Left, Margin.Top, Margin.Right, Margin.Bottom+5),
					 Background=System.Windows.Media.Brushes.DimGray,
				};

				TextBlock Avoidance_Text_Header=new TextBlock
				{
					 Text="Avoidances",
					 FontSize=12,
					 Background=System.Windows.Media.Brushes.MediumSeaGreen,
					 TextAlignment=TextAlignment.Center,
					 HorizontalAlignment=System.Windows.HorizontalAlignment.Stretch,

				};

				#region AvoidanceCheckboxes

				StackPanel AvoidanceCheckBoxesPanel=new StackPanel
				{
					 Orientation=Orientation.Vertical,
				};

				CheckBox CBAttemptAvoidanceMovements=new CheckBox
				{
					 Content="Enable Avoidance",
					 IsChecked=Bot.SettingsFunky.Avoidance.AttemptAvoidanceMovements,

				};
				CBAttemptAvoidanceMovements.Checked+=AvoidanceAttemptMovementChecked;
				CBAttemptAvoidanceMovements.Unchecked+=AvoidanceAttemptMovementChecked;

				CheckBox CBAdvancedProjectileTesting=new CheckBox
				{
					 Content="Use Advanced Avoidance Projectile Test",
					 IsChecked=Bot.SettingsFunky.Avoidance.UseAdvancedProjectileTesting,
				};
				CBAdvancedProjectileTesting.Checked+=UseAdvancedProjectileTestingChecked;
				CBAdvancedProjectileTesting.Unchecked+=UseAdvancedProjectileTestingChecked;
				AvoidanceCheckBoxesPanel.Children.Add(CBAttemptAvoidanceMovements);
				AvoidanceCheckBoxesPanel.Children.Add(CBAdvancedProjectileTesting);
				#endregion;





				AvoidanceOptionsStackPanel.Children.Add(Avoidance_Text_Header);
				AvoidanceOptionsStackPanel.Children.Add(AvoidanceCheckBoxesPanel);
				LBcharacterAvoidance.Children.Add(AvoidanceOptionsStackPanel);
				#endregion



				Grid AvoidanceLayoutGrid=new Grid
				{
					 ShowGridLines=false,
					 //VerticalAlignment=System.Windows.VerticalAlignment.Stretch,
					 //HorizontalAlignment=System.Windows.HorizontalAlignment.Stretch,
					 FlowDirection=System.Windows.FlowDirection.LeftToRight,
					 Focusable=false,
				};

				ColumnDefinition colDef1=new ColumnDefinition();
				ColumnDefinition colDef2=new ColumnDefinition();
				ColumnDefinition colDef3=new ColumnDefinition();
				AvoidanceLayoutGrid.ColumnDefinitions.Add(colDef1);
				AvoidanceLayoutGrid.ColumnDefinitions.Add(colDef2);
				AvoidanceLayoutGrid.ColumnDefinitions.Add(colDef3);
				RowDefinition rowDef1=new RowDefinition();
				AvoidanceLayoutGrid.RowDefinitions.Add(rowDef1);

				TextBlock ColumnHeader1=new TextBlock
				{
					 Text="Type",
					 FontSize=12,
					 TextAlignment=System.Windows.TextAlignment.Center,
					 Background=System.Windows.Media.Brushes.DarkTurquoise,
					 Foreground=System.Windows.Media.Brushes.GhostWhite,
				};
				TextBlock ColumnHeader2=new TextBlock
				{
					 Text="Radius",
					 FontSize=12,
					 TextAlignment=System.Windows.TextAlignment.Center,
					 Background=System.Windows.Media.Brushes.DarkGoldenrod,
					 Foreground=System.Windows.Media.Brushes.GhostWhite,
				};
				TextBlock ColumnHeader3=new TextBlock
				{
					 Text="Health",
					 FontSize=12,
					 TextAlignment=System.Windows.TextAlignment.Center,
					 Background=System.Windows.Media.Brushes.DarkRed,
					 Foreground=System.Windows.Media.Brushes.GhostWhite,
				};
				Grid.SetColumn(ColumnHeader1, 0);
				Grid.SetColumn(ColumnHeader2, 1);
				Grid.SetColumn(ColumnHeader3, 2);
				Grid.SetRow(ColumnHeader1, 0);
				Grid.SetRow(ColumnHeader2, 0);
				Grid.SetRow(ColumnHeader3, 0);
				AvoidanceLayoutGrid.Children.Add(ColumnHeader1);
				AvoidanceLayoutGrid.Children.Add(ColumnHeader2);
				AvoidanceLayoutGrid.Children.Add(ColumnHeader3);

				//Dictionary<AvoidanceType, double> currentDictionaryAvoidance=Bot.SettingsFunky.Avoidance.AvoidanceHealthValues;
				AvoidanceValue[] avoidanceValues=Bot.SettingsFunky.Avoidance.Avoidances.ToArray();
				TBavoidanceHealth=new TextBox[avoidanceValues.Length-1];
				TBavoidanceRadius=new TextBox[avoidanceValues.Length-1];
				int alternatingColor=0;

				for (int i=0; i<avoidanceValues.Length-1; i++)
				{
					 if (alternatingColor>1) alternatingColor=0;

					 string avoidanceString=avoidanceValues[i].Type.ToString();

					 double defaultRadius=avoidanceValues[i].Radius;
					 //Bot.SettingsFunky.Avoidance.AvoidanceRadiusValues.TryGetValue(avoidanceTypes[i], out defaultRadius);

					 Slider avoidanceRadius=new Slider
					 {
						  Width=125,
						  Name=avoidanceString+"_radius_"+i.ToString(),
						  Maximum=30,
						  Minimum=0,
						  TickFrequency=5,
						  LargeChange=5,
						  SmallChange=1,
						  Value=defaultRadius,
						  HorizontalAlignment=System.Windows.HorizontalAlignment.Stretch,
						  VerticalAlignment=System.Windows.VerticalAlignment.Center,
						  //Padding=new Thickness(2),
						  Margin=new Thickness(5),
					 };
					 avoidanceRadius.ValueChanged+=AvoidanceRadiusSliderValueChanged;
					 TBavoidanceRadius[i]=new TextBox
					 {
						  Text=defaultRadius.ToString(),
						  IsReadOnly=true,
						  VerticalAlignment=System.Windows.VerticalAlignment.Top,
						  HorizontalAlignment=System.Windows.HorizontalAlignment.Right,
					 };

					 double defaultHealth=avoidanceValues[i].Health;
					 //Bot.SettingsFunky.Avoidance.AvoidanceHealthValues.TryGetValue(avoidanceTypes[i], out defaultHealth);
					 Slider avoidanceHealth=new Slider
					 {
						  Name=avoidanceString+"_health_"+i.ToString(),
						  Width=125,
						  Maximum=1,
						  Minimum=0,
						  TickFrequency=0.10,
						  LargeChange=0.10,
						  SmallChange=0.05,
						  Value=defaultHealth,
						  HorizontalAlignment=System.Windows.HorizontalAlignment.Stretch,
						  VerticalAlignment=System.Windows.VerticalAlignment.Center,
						  Margin=new Thickness(5),
					 };
					 avoidanceHealth.ValueChanged+=AvoidanceHealthSliderValueChanged;
					 TBavoidanceHealth[i]=new TextBox
					 {
						  Text=defaultHealth.ToString("F2", CultureInfo.InvariantCulture),
						  IsReadOnly=true,
						  VerticalAlignment=System.Windows.VerticalAlignment.Top,
						  HorizontalAlignment=System.Windows.HorizontalAlignment.Right,
					 };

					 RowDefinition newRow=new RowDefinition();
					 AvoidanceLayoutGrid.RowDefinitions.Add(newRow);


					 TextBlock txt1=new TextBlock
					 {
						  Text=avoidanceString,
						  FontSize=12,
						  VerticalAlignment=System.Windows.VerticalAlignment.Stretch,
						  HorizontalAlignment=System.Windows.HorizontalAlignment.Stretch,
						  Background=alternatingColor==0?System.Windows.Media.Brushes.DarkSeaGreen:Background=System.Windows.Media.Brushes.SlateGray,
						  Foreground=System.Windows.Media.Brushes.GhostWhite,
						  FontStretch=FontStretches.Medium,
						  TextAlignment= TextAlignment.Center,
					 };

					 StackPanel avoidRadiusStackPanel=new StackPanel
					 {
						  Width=175,
						  Height=25,
						  Orientation=Orientation.Horizontal,
						  Background=alternatingColor==0?System.Windows.Media.Brushes.DarkSeaGreen:Background=System.Windows.Media.Brushes.SlateGray,

					 };
					 avoidRadiusStackPanel.Children.Add(avoidanceRadius);
					 avoidRadiusStackPanel.Children.Add(TBavoidanceRadius[i]);

					 StackPanel avoidHealthStackPanel=new StackPanel
					 {
						  Width=175,
						  Height=25,
						  Orientation=Orientation.Horizontal,
						  Background=alternatingColor==0?System.Windows.Media.Brushes.DarkSeaGreen:Background=System.Windows.Media.Brushes.SlateGray,
					 };
					 avoidHealthStackPanel.Children.Add(avoidanceHealth);
					 avoidHealthStackPanel.Children.Add(TBavoidanceHealth[i]);

					 Grid.SetColumn(txt1, 0);
					 Grid.SetColumn(avoidRadiusStackPanel, 1);
					 Grid.SetColumn(avoidHealthStackPanel, 2);

					 int currentIndex=AvoidanceLayoutGrid.RowDefinitions.Count-1;
					 Grid.SetRow(avoidRadiusStackPanel, currentIndex);
					 Grid.SetRow(avoidHealthStackPanel, currentIndex);
					 Grid.SetRow(txt1, currentIndex);

					 AvoidanceLayoutGrid.Children.Add(txt1);
					 AvoidanceLayoutGrid.Children.Add(avoidRadiusStackPanel);
					 AvoidanceLayoutGrid.Children.Add(avoidHealthStackPanel);
					 alternatingColor++;
				}
				ScrollViewer AvoidanceGridScrollViewer=new ScrollViewer
				{
					 VerticalScrollBarVisibility= ScrollBarVisibility.Auto,
				};


				LBcharacterAvoidance.Children.Add(AvoidanceLayoutGrid);

				Button BtnAvoidanceLoadTemplate=new Button
				{
					 Content="Load Setup",
					 Background=System.Windows.Media.Brushes.OrangeRed,
					 Foreground=System.Windows.Media.Brushes.GhostWhite,
					 FontStyle=FontStyles.Italic,
					 FontSize=12,

					 HorizontalAlignment=System.Windows.HorizontalAlignment.Left,
					 VerticalAlignment=System.Windows.VerticalAlignment.Top,
					 Width=75,
					 Height=30,

					 Margin=new Thickness(Margin.Left, Margin.Top+5, Margin.Right, Margin.Bottom+5),
				};
				BtnAvoidanceLoadTemplate.Click+=AvoidanceLoadSettingsButtonClicked;
				LBcharacterAvoidance.Children.Add(BtnAvoidanceLoadTemplate);

				AvoidanceGridScrollViewer.Content=LBcharacterAvoidance;
				AvoidanceTabItem.Content=AvoidanceGridScrollViewer;
		  }
	 }
}