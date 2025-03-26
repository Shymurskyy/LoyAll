using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using LoyAll.Model;
using LoyAll.Helper;
using Plugin.MauiMTAdmob;

namespace LoyAll.Views
{
    public partial class SelectCardsPopup : Popup
    {
        public ObservableCollection<CardSelection> SelectableCards { get; set; }

        public SelectCardsPopup(IEnumerable<Card> cards)
        {
            InitializeComponent();

            SelectableCards = new ObservableCollection<CardSelection>(
                cards.Select(c => new CardSelection { Card = c, IsSelected = false })
            );

            BindingContext = this;
        }
        private void OnRowTapped(object sender, EventArgs e)
        {
            if (sender is StackLayout stackLayout && stackLayout.BindingContext is CardSelection item)
            {
                item.IsSelected = !item.IsSelected;

                UpdateSelectAllCheckbox();
            }
        }

        private void UpdateSelectAllCheckbox()
        {
            if (SelectableCards.All(c => c.IsSelected))
                SelectAllCheckBox.IsChecked = true;
            else if (SelectableCards.All(c => !c.IsSelected))
                SelectAllCheckBox.IsChecked = false;
        }

        
        private void OnSelectAllCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            foreach (var item in SelectableCards)
            {
                item.IsSelected = e.Value; 
            }
        }

        private void OnSelectAllLabelTapped(object sender, EventArgs e)
        {
            SelectAllCheckBox.IsChecked = !SelectAllCheckBox.IsChecked;
        }
        private void OnCancelClicked(object sender, EventArgs e)
        {
            Close(null);
        }
        
        private void OnShareClicked(object sender, EventArgs e)
        {
            var selectedCards = SelectableCards
                .Where(c => c.IsSelected)
                .Select(c => c.Card)
                .ToList();

            if (AdService.IsRewardedAdLoaded())
                AdService.ShowRewardedAd();

            Close(selectedCards);
        }
    }
}