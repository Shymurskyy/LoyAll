using CommunityToolkit.Maui.Views;
using LoyAll.Model;
using LoyAll.Services;
using LoyAll.Views;
using LZStringCSharp;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace LoyAll
{
    public partial class MainPage : ContentPage
    {
        private const int SearchDelayMs = 300;
        private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";

        private CancellationTokenSource _animationCts;
        private CancellationTokenSource _searchCancellationTokenSource;
        private string _currentSearchText = string.Empty;
        private DateTime _lastSearchTime = DateTime.MinValue;
        private bool _isDisappearing;
        private readonly SemaphoreSlim _cardOpeningSemaphore = new SemaphoreSlim(1, 1);

        public ObservableCollection<Card> Cards { get; } = new();
        public ObservableCollection<Card> FilteredCards { get; } = new();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            InitializeServices();
        }

        private void InitializeServices()
        {
            Android.App.Activity? activity = Platform.CurrentActivity;
            GpdrService.InitializeUmpSdk(activity);
            AdService.LoadRewardedAd(RewardedAdUnitId);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _isDisappearing = false;

            _animationCts?.Cancel();
            _animationCts = new CancellationTokenSource();

            if (!Cards.Any())
            {
                await LoadCardsAsync();
            }

            _ = AnimateFloatingButtonAsync(_animationCts.Token);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isDisappearing = true;
            _animationCts?.Cancel();
        }

        public async Task LoadCardsAsync()
        {
            try
            {
                IsBusy = true;
                List<Card> cards = await Task.Run(() => CardStorageService.GetCards()).ConfigureAwait(false);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Cards.Clear();
                    foreach (Card? card in cards)
                    {
                        Cards.Add(card);
                    }
                    FilterCards(SearchBar.Text);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading cards: {ex}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await DisplayAlert("Błąd", "Nie udało się załadować kart", "OK"));
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() => IsBusy = false);
            }
        }

        public void AddCard(Card newCard)
        {
            Cards.Insert(0, newCard);
            FilterCards(SearchBar.Text);
        }

        private async Task DeleteCardAsync(Card cardToRemove)
        {
            bool confirm = await DisplayAlert(
                "Usuń kartę",
                $"Czy na pewno chcesz usunąć kartę {cardToRemove.StoreName}?",
                "Tak", "Nie");

            if (confirm)
            {
                Cards.Remove(cardToRemove);
                CardStorageService.DeleteCard(cardToRemove);
                FilterCards(SearchBar.Text);
            }
        }

        private void FilterCards(string searchText)
        {
            _ = FilterCardsAsync(searchText, CancellationToken.None);
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearchText = e.NewTextValue;
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();

            _lastSearchTime = DateTime.Now;
            DateTime currentSearchTime = _lastSearchTime;

            try
            {
                await Task.Delay(SearchDelayMs, _searchCancellationTokenSource.Token);

                if (currentSearchTime == _lastSearchTime)
                {
                    await FilterCardsAsync(_currentSearchText, _searchCancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Search was canceled - ignore
            }
        }

        private async Task FilterCardsAsync(string searchText, CancellationToken token)
        {
            try
            {
                ObservableCollection<Card> filtered = string.IsNullOrWhiteSpace(searchText)
                    ? new ObservableCollection<Card>(Cards)
                    : new ObservableCollection<Card>(
                        await Task.Run(() =>
                            Cards.Where(c => c.StoreName?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                                .ToList(),
                            token));

                if (!token.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        FilteredCards.Clear();
                        foreach (Card card in filtered)
                        {
                            FilteredCards.Add(card);
                        }
                    });
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                // Filtering was canceled - ignore
            }
        }

        private async Task AnimateFloatingButtonAsync(CancellationToken token)
        {
            if (FloatingActionButton == null) return;

            try
            {
                await FloatingActionButton.FadeTo(1, 250).ConfigureAwait(false);

                while (!token.IsCancellationRequested && !_isDisappearing)
                {
                    await FloatingActionButton.ScaleTo(1.05, 1000, Easing.Linear).ConfigureAwait(false);
                    if (token.IsCancellationRequested) break;

                    await FloatingActionButton.ScaleTo(1.0, 1000, Easing.Linear).ConfigureAwait(false);
                    if (token.IsCancellationRequested) break;

                    await Task.Delay(500, token).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                // Animation was canceled - ignore
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await ExecuteWithCardOpeningLock(async () =>
            {
                await Navigation.PushAsync(new SettingsPage());
            });
        }

        private async void OnShareCardsClicked(object sender, EventArgs e)
        {
            await ExecuteWithCardOpeningLock(async () =>
            {
                if (!Cards.Any())
                {
                    await DisplayAlert("Brak kart", "Nie masz zapisanych kart do udostępnienia.", "OK");
                    return;
                }

                SelectCardsPopup popup = new SelectCardsPopup(Cards);
                List<Card>? selectedCards = await this.ShowPopupAsync(popup) as List<Card>;

                if (selectedCards == null || !selectedCards.Any())
                    return;

                var minimalCards = selectedCards.Select(c => new { n = c.StoreName, k = c.CardValue });
                string json = JsonSerializer.Serialize(minimalCards);
                string compressedData = LZString.CompressToEncodedURIComponent(json);

                await Navigation.PushAsync(new ShareCardPage(compressedData));
            });
        }

        private async void OnCardTapped(object sender, EventArgs e)
        {

            try
            {
                if (!await _cardOpeningSemaphore.WaitAsync(0))
                    return;

                if (sender is Frame frame && frame.BindingContext is Card selectedCard)
                {
                    var bottomSheet = new CardDetailPage(selectedCard);
                    await bottomSheet.ShowAsync();
                }
            }
            finally
            {
                if (_cardOpeningSemaphore.CurrentCount == 0)
                    _cardOpeningSemaphore.Release();
            }
        }

        private async void OnDeleteCardClicked(object sender, EventArgs e)
        {
            await ExecuteWithCardOpeningLock(async () =>
            {
                if (sender is ImageButton button && button.CommandParameter is Card cardToRemove)
                    await DeleteCardAsync(cardToRemove);
            });
        }

        private async void OnAddCardClicked(object sender, EventArgs e)
        {
            await ExecuteWithCardOpeningLock(async () =>
            {
                await Navigation.PushAsync(new AddCardPage(this));
            });
        }

        private async Task ExecuteWithCardOpeningLock(Func<Task> action)
        {
            if (!await _cardOpeningSemaphore.WaitAsync(0))
                return;

            try
            {
                await action();
            }
            finally
            {
                _cardOpeningSemaphore.Release();
            }
        }
    }
}