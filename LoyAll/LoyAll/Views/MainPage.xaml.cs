using System;
using System.Linq;
using Microsoft.Maui.Controls;
using LoyAll.Services;
using LoyAll.Model;
using LoyAll.Views;
using System.Collections.ObjectModel;

namespace LoyAll
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Card> Cards { get; set; }
        public MainPage()
        {
            InitializeComponent();
            LoadCards();
            BindingContext = this;
        }

        public void LoadCards()
        {
            var cards = CardStorageService.GetCards();
            Cards = new ObservableCollection<Card>(cards); 
        }

        public void AddCard(Card newCard)
        {
            Cards.Insert(0, newCard); 
        }

        private async void OnAddCardClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddCardPage(this)); 
        }

        private async void OnCardTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is Card selectedCard)
            {
                await Navigation.PushAsync(new CardDetailPage(card: selectedCard));
            }
        }
    }
}
