using System;
using System.Linq;
using Microsoft.Maui.Controls;
using LoyAll.Services;
using LoyAll.Model;
using LoyAll.Views;
using System.Collections.ObjectModel;
using System.Text.Json;
using LZStringCSharp;
using CommunityToolkit.Maui.Views;
using Plugin.MauiMTAdmob;


namespace LoyAll
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Card> Cards { get; set; }
        public ObservableCollection<Card> FilteredCards { get; set; }
        public MainPage()
        {
            InitializeComponent();
            LoadCards();
            FilteredCards = new ObservableCollection<Card>(Cards);
            BindingContext = this;
            var activity = Platform.CurrentActivity;
            GpdrService.InitializeUmpSdk(activity);


            AdService.LoadRewardedAd("ca-app-pub-3940256099942544/5224354917");
        }
        
        public void LoadCards()
        {
            var cards = CardStorageService.GetCards();
            Cards = new ObservableCollection<Card>(cards);
            FilterCards("");
        }
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterCards(e.NewTextValue);
        }
        private void FilterCards(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                FilteredCards = new ObservableCollection<Card>(Cards);
            }
            else
            {
                var filtered = Cards.Where(c => c.StoreName.ToLower().Contains(searchText.ToLower())).ToList();
                FilteredCards = new ObservableCollection<Card>(filtered);
            }
            CardsCollectionView.ItemsSource = FilteredCards;
        }
        public void AddCard(Card newCard)
        {
            Cards.Insert(0, newCard);
            FilterCards(SearchBar.Text);
        }

        private async void OnAddCardClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddCardPage(this)); 
        }

        private async void OnCardTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is Card selectedCard)
            {
                var bottomSheet = new CardDetailPage(selectedCard);
                await bottomSheet.ShowAsync();
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
                    FilterCards(SearchBar.Text);
                }
            }
        }
        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }
        private async void OnShareCardsClicked(object sender, EventArgs e)
        {
            if (!Cards.Any())
            {
                await DisplayAlert("Brak kart", "Nie masz zapisanych kart do udostępnienia.", "OK");
                return;
            }

            var popup = new SelectCardsPopup(Cards);
            var selectedCards = await this.ShowPopupAsync(popup) as List<Card>;

            if (selectedCards == null || !selectedCards.Any())
                return; 

            var minimalCards = selectedCards.Select(c => new { n = c.StoreName, k = c.CardValue });
            string json = JsonSerializer.Serialize(minimalCards);
            string compressedData = LZString.CompressToEncodedURIComponent(json);

            await Navigation.PushAsync(new ShareCardPage(compressedData));
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadCards(); 
        }

    }
}
