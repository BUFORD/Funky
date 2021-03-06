﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using FunkyBot.Settings;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using ListBox = System.Windows.Controls.ListBox;
using Orientation = System.Windows.Controls.Orientation;
using TextBox = System.Windows.Controls.TextBox;

namespace FunkyBot
{
	internal partial class FunkyWindow : Window
	{
		#region EventHandling
		private void RangeLoadXMLClicked(object sender, EventArgs e)
		{
			OpenFileDialog OFD = new OpenFileDialog
			{
				InitialDirectory = Path.Combine(FolderPaths.sTrinityPluginPath, "Config", "Defaults"),
				RestoreDirectory = false,
				Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*",
				Title = "Ranges Template",
			};
			DialogResult OFD_Result = OFD.ShowDialog();

			if (OFD_Result == System.Windows.Forms.DialogResult.OK)
			{
				try
				{
					//;
					SettingRanges newSettings = SettingRanges.DeserializeFromXML(OFD.FileName);
					Bot.Settings.Ranges = newSettings;

					funkyConfigWindow.Close();
				}
				catch
				{

				}
			}


		}

		private void IgnoreCombatRangeChecked(object sender, EventArgs e)
		{
			Bot.Settings.Ranges.IgnoreCombatRange = !Bot.Settings.Ranges.IgnoreCombatRange;
		}
		private void IgnoreLootRangeChecked(object sender, EventArgs e)
		{
			Bot.Settings.Ranges.IgnoreLootRange = !Bot.Settings.Ranges.IgnoreLootRange;
		}
		private void IgnoreProfileBlacklistChecked(object sender, EventArgs e)
		{
			Bot.Settings.Ranges.IgnoreProfileBlacklists = !Bot.Settings.Ranges.IgnoreProfileBlacklists;
		}
		private void EliteRangeSliderChanged(object sender, EventArgs e)
		{
			Slider slider_sender = (Slider)sender;
			int Value = (int)slider_sender.Value;
			Bot.Settings.Ranges.EliteCombatRange = Value;
			TBEliteRange.Text = Value.ToString();
		}
		private void GoldRangeSliderChanged(object sender, EventArgs e)
		{
			Slider slider_sender = (Slider)sender;
			int Value = (int)slider_sender.Value;
			Bot.Settings.Ranges.GoldRange = Value;
			TBGoldRange.Text = Value.ToString();
		}
		private void GlobeRangeSliderChanged(object sender, EventArgs e)
		{
			Slider slider_sender = (Slider)sender;
			int Value = (int)slider_sender.Value;
			Bot.Settings.Ranges.GlobeRange = Value;
			TBGlobeRange.Text = Value.ToString();
		}
		private void ItemRangeSliderChanged(object sender, EventArgs e)
		{
			Slider slider_sender = (Slider)sender;
			int Value = (int)slider_sender.Value;
			Bot.Settings.Ranges.ItemRange = Value;
			TBItemRange.Text = Value.ToString();
		}
		private void ShrineRangeSliderChanged(object sender, EventArgs e)
		{
			Slider slider_sender = (Slider)sender;
			int Value = (int)slider_sender.Value;
			Bot.Settings.Ranges.ShrineRange = Value;
			TBShrineRange.Text = Value.ToString();
		}

		private void ContainerRangeSliderChanged(object sender, EventArgs e)
		{
			Slider slider_sender = (Slider)sender;
			int Value = (int)slider_sender.Value;
			Bot.Settings.Ranges.ContainerOpenRange = Value;
			TBContainerRange.Text = Value.ToString();
		}
		private void NonEliteRangeSliderChanged(object sender, EventArgs e)
		{
			Slider slider_sender = (Slider)sender;
			int Value = (int)slider_sender.Value;
			Bot.Settings.Ranges.NonEliteCombatRange = Value;
			TBNonEliteRange.Text = Value.ToString();
		}
		private void TreasureGoblinRangeSliderChanged(object sender, EventArgs e)
		{
			Slider slider_sender = (Slider)sender;
			int Value = (int)slider_sender.Value;
			Bot.Settings.Ranges.TreasureGoblinRange = Value;
			TBGoblinRange.Text = Value.ToString();
		}
		private void DestructibleSliderChanged(object sender, EventArgs e)
		{
			Slider slider_sender = (Slider)sender;
			int Value = (int)slider_sender.Value;
			Bot.Settings.Ranges.DestructibleRange = Value;
			TBDestructibleRange.Text = Value.ToString();
		}
		#endregion


		private TextBox TBContainerRange, TBNonEliteRange, TBDestructibleRange,
							  TBGlobeRange, TBGoblinRange, TBItemRange,
							  TBShrineRange, TBEliteRange, TBGoldRange;


		internal void InitTargetRangeControls()
		{

			#region Targeting_Ranges
			TabItem RangeTabItem = new TabItem();
			RangeTabItem.Header = "Range";
			tcTargeting.Items.Add(RangeTabItem);
			ListBox lbTargetRange = new ListBox();

			StackPanel ProfileRelatedSettings = new StackPanel();

			TextBlock Profile_Values_Text = new TextBlock
			{
				Text = "Profile Related Values",
				FontSize = 13,
				Background = Brushes.DarkSeaGreen,
				TextAlignment = TextAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};
			ProfileRelatedSettings.Children.Add(Profile_Values_Text);

			StackPanel spIgnoreProfileValues = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};
			CheckBox cbIgnoreCombatRange = new CheckBox
			{
				Content = "Ignore Combat Range (Set by Profile)",
				// Width = 300,
				HorizontalContentAlignment = HorizontalAlignment.Left,
				Height = 30,
				IsChecked = (Bot.Settings.Ranges.IgnoreCombatRange)
			};
			cbIgnoreCombatRange.Checked += IgnoreCombatRangeChecked;
			cbIgnoreCombatRange.Unchecked += IgnoreCombatRangeChecked;
			spIgnoreProfileValues.Children.Add(cbIgnoreCombatRange);
			CheckBox cbIgnoreLootRange = new CheckBox
			{
				Content = "Ignore Loot Range (Set by Profile)",
				// Width = 300,
				Height = 30,
				HorizontalContentAlignment = HorizontalAlignment.Right,
				IsChecked = (Bot.Settings.Ranges.IgnoreLootRange)
			};
			cbIgnoreLootRange.Checked += IgnoreLootRangeChecked;
			cbIgnoreLootRange.Unchecked += IgnoreLootRangeChecked;
			spIgnoreProfileValues.Children.Add(cbIgnoreLootRange);
			ProfileRelatedSettings.Children.Add(spIgnoreProfileValues);
			CheckBox cbIgnoreProfileBlacklist = new CheckBox
			{
				Content = "Ignore Profile Blacklisted IDs",
				// Width = 300,
				Height = 30,
				HorizontalContentAlignment = HorizontalAlignment.Right,
				IsChecked = (Bot.Settings.Ranges.IgnoreProfileBlacklists)
			};
			cbIgnoreProfileBlacklist.Checked += IgnoreProfileBlacklistChecked;
			cbIgnoreProfileBlacklist.Unchecked += IgnoreProfileBlacklistChecked;
			ProfileRelatedSettings.Children.Add(cbIgnoreProfileBlacklist);
			lbTargetRange.Items.Add(ProfileRelatedSettings);


			TextBlock Target_Range_Text = new TextBlock
			{
				Text = "Targeting Extended Range Values",
				FontSize = 13,
				Background = Brushes.DarkSeaGreen,
				TextAlignment = TextAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};
			lbTargetRange.Items.Add(Target_Range_Text);

			#region EliteRange
			lbTargetRange.Items.Add("Elite Combat Range");
			Slider sliderEliteRange = new Slider
			{
				Width = 100,
				Maximum = 150,
				Minimum = 0,
				TickFrequency = 5,
				LargeChange = 5,
				SmallChange = 1,
				Value = Bot.Settings.Ranges.EliteCombatRange,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			sliderEliteRange.ValueChanged += EliteRangeSliderChanged;
			TBEliteRange = new TextBox
			{
				Text = Bot.Settings.Ranges.EliteCombatRange.ToString(),
				IsReadOnly = true,
			};
			StackPanel EliteStackPanel = new StackPanel
			{
				Width = 600,
				Height = 20,
				Orientation = Orientation.Horizontal,
			};
			EliteStackPanel.Children.Add(sliderEliteRange);
			EliteStackPanel.Children.Add(TBEliteRange);
			lbTargetRange.Items.Add(EliteStackPanel);
			#endregion

			#region NonEliteRange
			lbTargetRange.Items.Add("Non-Elite Combat Range");
			Slider sliderNonEliteRange = new Slider
			{
				Width = 100,
				Maximum = 150,
				Minimum = 0,
				TickFrequency = 5,
				LargeChange = 5,
				SmallChange = 1,
				Value = Bot.Settings.Ranges.NonEliteCombatRange,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			sliderNonEliteRange.ValueChanged += NonEliteRangeSliderChanged;
			TBNonEliteRange = new TextBox
			{
				Text = Bot.Settings.Ranges.NonEliteCombatRange.ToString(),
				IsReadOnly = true,
			};
			StackPanel NonEliteStackPanel = new StackPanel
			{
				Width = 600,
				Height = 20,
				Orientation = Orientation.Horizontal,
			};
			NonEliteStackPanel.Children.Add(sliderNonEliteRange);
			NonEliteStackPanel.Children.Add(TBNonEliteRange);
			lbTargetRange.Items.Add(NonEliteStackPanel);
			#endregion

			#region ShrineRange
			lbTargetRange.Items.Add("Shrine Range");
			Slider sliderShrineRange = new Slider
			{
				Width = 100,
				Maximum = 75,
				Minimum = 0,
				TickFrequency = 5,
				LargeChange = 5,
				SmallChange = 1,
				Value = Bot.Settings.Ranges.ShrineRange,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			sliderShrineRange.ValueChanged += ShrineRangeSliderChanged;
			TBShrineRange = new TextBox
			{
				Text = Bot.Settings.Ranges.ShrineRange.ToString(),
				IsReadOnly = true,
			};
			StackPanel ShrineStackPanel = new StackPanel
			{
				Width = 600,
				Height = 20,
				Orientation = Orientation.Horizontal,
			};
			ShrineStackPanel.Children.Add(sliderShrineRange);
			ShrineStackPanel.Children.Add(TBShrineRange);
			lbTargetRange.Items.Add(ShrineStackPanel);
			#endregion

			#region ContainerRange
			lbTargetRange.Items.Add("Container Range");
			Slider sliderContainerRange = new Slider
			{
				Width = 100,
				Maximum = 75,
				Minimum = 0,
				TickFrequency = 5,
				LargeChange = 5,
				SmallChange = 1,
				Value = Bot.Settings.Ranges.ContainerOpenRange,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			sliderContainerRange.ValueChanged += ContainerRangeSliderChanged;
			TBContainerRange = new TextBox
			{
				Text = Bot.Settings.Ranges.ContainerOpenRange.ToString(),
				IsReadOnly = true,
			};
			StackPanel ContainerStackPanel = new StackPanel
			{
				Width = 600,
				Height = 20,
				Orientation = Orientation.Horizontal,
			};
			ContainerStackPanel.Children.Add(sliderContainerRange);
			ContainerStackPanel.Children.Add(TBContainerRange);
			lbTargetRange.Items.Add(ContainerStackPanel);
			#endregion

			#region DestructibleRange
			lbTargetRange.Items.Add("Destuctible Range");
			Slider sliderDestructibleRange = new Slider
			{
				Width = 100,
				Maximum = 75,
				Minimum = 0,
				TickFrequency = 5,
				LargeChange = 5,
				SmallChange = 1,
				Value = Bot.Settings.Ranges.DestructibleRange,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			sliderDestructibleRange.ValueChanged += DestructibleSliderChanged;
			TBDestructibleRange = new TextBox
			{
				Text = Bot.Settings.Ranges.DestructibleRange.ToString(),
				IsReadOnly = true,
			};
			StackPanel DestructibleStackPanel = new StackPanel
			{
				Width = 600,
				Height = 20,
				Orientation = Orientation.Horizontal,
			};
			DestructibleStackPanel.Children.Add(sliderDestructibleRange);
			DestructibleStackPanel.Children.Add(TBDestructibleRange);
			lbTargetRange.Items.Add(DestructibleStackPanel);
			#endregion

			#region GoldRange
			lbTargetRange.Items.Add("Gold Range");
			Slider sliderGoldRange = new Slider
			{
				Width = 100,
				Maximum = 150,
				Minimum = 0,
				TickFrequency = 5,
				LargeChange = 5,
				SmallChange = 1,
				Value = Bot.Settings.Ranges.GoldRange,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			sliderGoldRange.ValueChanged += GoldRangeSliderChanged;
			TBGoldRange = new TextBox
			{
				Text = Bot.Settings.Ranges.GoldRange.ToString(),
				IsReadOnly = true,
			};
			StackPanel GoldRangeStackPanel = new StackPanel
			{
				Width = 600,
				Height = 20,
				Orientation = Orientation.Horizontal,
			};
			GoldRangeStackPanel.Children.Add(sliderGoldRange);
			GoldRangeStackPanel.Children.Add(TBGoldRange);
			lbTargetRange.Items.Add(GoldRangeStackPanel);
			#endregion

			#region GlobeRange
			lbTargetRange.Items.Add("Globe Range");
			Slider sliderGlobeRange = new Slider
			{
				Width = 100,
				Maximum = 75,
				Minimum = 0,
				TickFrequency = 5,
				LargeChange = 5,
				SmallChange = 1,
				Value = Bot.Settings.Ranges.GlobeRange,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			sliderGlobeRange.ValueChanged += GlobeRangeSliderChanged;
			TBGlobeRange = new TextBox
			{
				Text = Bot.Settings.Ranges.GlobeRange.ToString(),
				IsReadOnly = true,
			};
			StackPanel GlobeRangeStackPanel = new StackPanel
			{
				Width = 600,
				Height = 20,
				Orientation = Orientation.Horizontal,
			};
			GlobeRangeStackPanel.Children.Add(sliderGlobeRange);
			GlobeRangeStackPanel.Children.Add(TBGlobeRange);
			lbTargetRange.Items.Add(GlobeRangeStackPanel);
			#endregion

			#region ItemRange
			lbTargetRange.Items.Add("Item Loot Range");
			Slider sliderItemRange = new Slider
			{
				Width = 100,
				Maximum = 150,
				Minimum = 0,
				TickFrequency = 5,
				LargeChange = 5,
				SmallChange = 1,
				Value = Bot.Settings.Ranges.ItemRange,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			sliderItemRange.ValueChanged += ItemRangeSliderChanged;
			TBItemRange = new TextBox
			{
				Text = Bot.Settings.Ranges.ItemRange.ToString(),
				IsReadOnly = true,
			};
			StackPanel ItemRangeStackPanel = new StackPanel
			{
				Width = 600,
				Height = 20,
				Orientation = Orientation.Horizontal,
			};
			ItemRangeStackPanel.Children.Add(sliderItemRange);
			ItemRangeStackPanel.Children.Add(TBItemRange);
			lbTargetRange.Items.Add(ItemRangeStackPanel);
			#endregion

			#region GoblinRange
			lbTargetRange.Items.Add("Treasure Goblin Range");
			Slider sliderGoblinRange = new Slider
			{
				Width = 100,
				Maximum = 150,
				Minimum = 0,
				TickFrequency = 5,
				LargeChange = 5,
				SmallChange = 1,
				Value = Bot.Settings.Ranges.TreasureGoblinRange,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			sliderGoblinRange.ValueChanged += TreasureGoblinRangeSliderChanged;
			TBGoblinRange = new TextBox
			{
				Text = Bot.Settings.Ranges.TreasureGoblinRange.ToString(),
				IsReadOnly = true,
			};
			StackPanel GoblinRangeStackPanel = new StackPanel
			{
				Width = 600,
				Height = 20,
				Orientation = Orientation.Horizontal,
			};
			GoblinRangeStackPanel.Children.Add(sliderGoblinRange);
			GoblinRangeStackPanel.Children.Add(TBGoblinRange);
			lbTargetRange.Items.Add(GoblinRangeStackPanel);
			#endregion

			Button BtnRangeTemplate = new Button
			{
				Content = "Load Setup",
				Background = Brushes.OrangeRed,
				Foreground = Brushes.GhostWhite,
				FontStyle = FontStyles.Italic,
				FontSize = 12,

				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Width = 75,
				Height = 30,

				Margin = new Thickness(Margin.Left, Margin.Top + 5, Margin.Right, Margin.Bottom + 5),
			};
			BtnRangeTemplate.Click += RangeLoadXMLClicked;
			lbTargetRange.Items.Add(BtnRangeTemplate);

			RangeTabItem.Content = lbTargetRange;
			#endregion

		}
	}
}
