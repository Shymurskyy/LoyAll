using System;
using System.Linq;
using Microsoft.Maui.Controls;
using LoyAll.Services;
using LoyAll.Model;
using LoyAll.Views;
using System.Collections.ObjectModel;
using System.Text.Json;
using LZStringCSharp;

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
        private async void OnDeleteCardClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Card cardToRemove)
            {
                bool confirm = await DisplayAlert("Usuń kartę", $"Czy na pewno chcesz usunąć kartę {cardToRemove.StoreName}?", "Tak", "Nie");
                if (confirm)
                {
                    Cards.Remove(cardToRemove);
                    CardStorageService.DeleteCard(cardToRemove);
                }
            }
        }
        private async void OnShareCardsClicked(object sender, EventArgs e)
        {
            if (Cards.Any())
            {
                var minimalCards = Cards.Select(c => new { n = c.StoreName, k = c.CardValue });
                string json = JsonSerializer.Serialize(minimalCards);
                string compressedData = LZString.CompressToEncodedURIComponent(json);
                await Navigation.PushAsync(new ShareCardPage(compressedData));
            }
            else
            {
                await DisplayAlert("Brak kart", "Nie masz zapisanych kart do udostępnienia.", "OK");
            }
        }

    }
}
