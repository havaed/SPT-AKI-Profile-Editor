﻿using SPT_AKI_Profile_Editor.Core.HelperClasses;
using SPT_AKI_Profile_Editor.Core.ServerClasses;
using SPT_AKI_Profile_Editor.Helpers;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace SPT_AKI_Profile_Editor.Core.ProfileClasses
{
    public class CharacterTraderStandingExtended : BindableEntity
    {
        private long? salesSumStart;
        private float? staindingStart;
        private bool? unlockedStart;
        private int? levelStart;

        public CharacterTraderStandingExtended(CharacterTraderStanding standing, string id, TraderBase traderBase, float ragfairRating)
        {
            TraderStanding = standing;
            Standing = id == AppData.AppSettings.RagfairTraderId ? ragfairRating : standing.Standing;
            Id = id;
            TraderBase = traderBase;
            LoadBitmapImage();
        }

        public string Id { get; }

        public CharacterTraderStanding TraderStanding { get; }
        public TraderBase TraderBase { get; }
        public BitmapImage BitmapImage { get; set; }

        public string LocalizedName => AppData.ServerDatabase.LocalesGlobal.ContainsKey(Id.Nickname()) ? AppData.ServerDatabase.LocalesGlobal[Id.Nickname()] : Id;

        public int LoyaltyLevel
        {
            get => TraderStanding.LoyaltyLevel;
            set
            {
                if (levelStart == null)
                    levelStart = TraderStanding.LoyaltyLevel;
                value = Math.Min(Math.Max(value, 1), MaxLevel);
                TraderStanding.LoyaltyLevel = value;
                OnPropertyChanged("LoyaltyLevel");
                SetSalesSum(value);
                SetStanding(value);
                SetUnlocked(value);
            }
        }

        public float Standing
        {
            get => TraderStanding.Standing;
            set
            {
                TraderStanding.Standing = value;
                OnPropertyChanged("Standing");
                SetLevel();
            }
        }

        public long SalesSum
        {
            get => TraderStanding.SalesSum;
            set
            {
                TraderStanding.SalesSum = value;
                OnPropertyChanged("SalesSum");
                SetLevel();
            }
        }

        public int MaxLevel => TraderBase?.LoyaltyLevels.Count ?? 0;

        public bool HasLevelIssue(int? level)
        {
            var currentLevelIndex = Math.Min(Math.Max(0, LoyaltyLevel - 1), MaxLevel - 1);
            return TraderBase?.LoyaltyLevels[currentLevelIndex].MinLevel > level;
        }

        private void SetSalesSum(int level)
        {
            if (salesSumStart == null)
                salesSumStart = TraderStanding.SalesSum;
            var minSalesSum = TraderBase?.LoyaltyLevels[level - 1].MinSalesSum ?? TraderStanding.SalesSum;
            TraderStanding.SalesSum = level >= levelStart ? Math.Max(minSalesSum, salesSumStart.Value) : Math.Min(minSalesSum, salesSumStart.Value);
            OnPropertyChanged("SalesSum");
        }

        private void SetStanding(int level)
        {
            if (staindingStart == null)
                staindingStart = TraderStanding.Standing;
            var minStanding = TraderBase?.LoyaltyLevels[level - 1].MinStanding ?? TraderStanding.Standing;
            TraderStanding.Standing = level >= levelStart ? Math.Max(minStanding, staindingStart.Value) : Math.Min(minStanding, staindingStart.Value);
            OnPropertyChanged("Standing");
        }

        private void SetUnlocked(int level)
        {
            if (unlockedStart == null)
                unlockedStart = TraderStanding.Unlocked;
            TraderStanding.Unlocked = level > 1 || unlockedStart.Value;
        }

        private void SetLevel()
        {
            if (TraderBase?.LoyaltyLevels == null)
                return;
            int newLevel = 1;
            foreach (var level in TraderBase.LoyaltyLevels)
            {
                if (Standing >= level.MinStanding && SalesSum >= level.MinSalesSum && LoyaltyLevel != newLevel)
                {
                    TraderStanding.LoyaltyLevel = newLevel;
                    OnPropertyChanged("LoyaltyLevel");
                }
                newLevel++;
            }
        }

        private void LoadBitmapImage()
        {
            var imageUrl = TraderBase?.ImageUrl ?? "unknown.png";

            var imagePath = Path.Combine(AppData.AppSettings.ServerPath,
                AppData.AppSettings.DirsList[SPTServerDir.traderImages],
                Path.GetFileNameWithoutExtension(imageUrl) + ".png");

            if (File.Exists(imagePath))
            {
                try
                {
                    BitmapImage = new BitmapImage(new Uri(imagePath));
                }
                catch { }
            }
        }
    }
}