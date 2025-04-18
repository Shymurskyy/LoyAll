using CommunityToolkit.Maui.Views;
using LoyAll.Helper;
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
                using (CustomPopup errorPopup = new CustomPopup(false))
                {
                    errorPopup.SetTitle(LanguageHelper.Instance["ErrorTitle"]);
                    errorPopup.SetMessage(LanguageHelper.Instance["LoadCardsError"]);
                    errorPopup.AddOption(LanguageHelper.Instance["OKButton"], () => { });
                    await errorPopup.ShowAsync(this);
                }
                return;
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
                List<Card> sortedCards = await Task.Run(() =>
                    GetSortedCards(Cards, searchText).ToList(), token);

                if (!token.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        FilteredCards.Clear();
                        foreach (Card? card in sortedCards)
                        {
                            FilteredCards.Add(card);
                        }
                    });
                }
            }
            catch (TaskCanceledException)
            {
                //ignore
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
                //ignore
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
                    using (CustomPopup errorPopup = new CustomPopup(false))
                    {
                        errorPopup.SetTitle(LanguageHelper.Instance["NoCardsTitle"]);
                        errorPopup.SetMessage(LanguageHelper.Instance["NoCardsMessage"]);
                        errorPopup.AddOption(LanguageHelper.Instance["OKButton"], () => { });
                        await errorPopup.ShowAsync(this);
                    }
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
                    CardDetailPage bottomSheet = new CardDetailPage(selectedCard);
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

            if (sender is ImageButton button && button.CommandParameter is Card cardToRemove)
            {
                using (CustomPopup deletePopup = new CustomPopup())
                {
                    deletePopup.SetTitle(LanguageHelper.Instance["DeleteCardTitle"]);
                    deletePopup.SetMessage(string.Format(LanguageHelper.Instance["DeleteCardMessage"], cardToRemove.StoreName));
                    bool confirm = await deletePopup.ShowConfirmationAsync(this, LanguageHelper.Instance["ConfirmButton"]);

                    if (confirm)
                    {
                        Cards.Remove(cardToRemove);
                        FilteredCards.Remove(cardToRemove);
                        CardStorageService.DeleteCard(cardToRemove);
                    }
                }
            }
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
    private void OnFavoriteClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is Card card)
        {
            if (button.IsEnabled)
            {
                button.IsEnabled = false;

                card.IsFavorite = !card.IsFavorite;

                _ = Task.Run(() =>
                {
                    CardStorageService.ToggleFavorite(card);
                    Dispatcher.Dispatch(() =>
                    {
                        ReorderList();
                        button.IsEnabled = true;
                    });
                });
            }
        }
    }
    private void ReorderList()
    {
        string currentSearch = SearchBar.Text;
        List<Card> sorted = GetSortedCards(Cards, currentSearch).ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            if (!FilteredCards[i].Equals(sorted[i]))
            {
                FilteredCards.Move(FilteredCards.IndexOf(sorted[i]), i);
            }
        }
    }
    private IEnumerable<Card> GetSortedCards(IEnumerable<Card> cards, string searchText)
    {
        IEnumerable<Card> filtered = string.IsNullOrWhiteSpace(searchText)
            ? cards
            : cards.AsParallel()
                  .Where(c => c.StoreName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false);

        return filtered.OrderByDescending(c => c.IsFavorite)
                      .ThenBy(c => c.StoreName);
    }
}
}