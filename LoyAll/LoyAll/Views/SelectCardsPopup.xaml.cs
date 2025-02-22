using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using LoyAll.Model;
using LoyAll.Helper;

namespace LoyAll.Views
{
    public partial class SelectCardsPopup : Popup
    {
        public ObservableCollection<CardSelection> SelectableCards { get; set; }

        private bool _allSelected = false; 

        public SelectCardsPopup(IEnumerable<Card> cards)
        {
            InitializeComponent();

            SelectableCards = new ObservableCollection<CardSelection>(
                cards.Select(c => new CardSelection { Card = c, IsSelected = false })
            );

            BindingContext = this; 
        }

        private void OnSelectAllClicked(object sender, EventArgs e)
        {
            _allSelected = !_allSelected;

            foreach (var card in SelectableCards)
            {
                card.IsSelected = _allSelected;
            }
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

            Close(selectedCards); 
        }
    }
}
