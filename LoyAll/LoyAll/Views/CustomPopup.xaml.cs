using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LoyAll.Helper
{
    public partial class CustomPopup : ContentView, IDisposable
    {
        public event EventHandler Canceled;
        private TapGestureRecognizer _parentTapGesture;
        private static readonly SemaphoreSlim _popupSemaphore = new SemaphoreSlim(1, 1);
        private static CustomPopup _currentlyOpenedPopup;
        private TaskCompletionSource<bool> _userResponseTask;

        public CustomPopup(bool showCancelButton = true)
        {
            InitializeComponent();
            this.IsVisible = false;
            cancelButton.IsVisible = showCancelButton;
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await CloseAsync(false);
        }

        public void SetTitle(string title)
        {
            var titleLabel = new Label
            {
                Text = title,
                Style = (Style)Resources["PopupTitleStyle"]
            };

            if (contentLayout.Children.Count > 0 && contentLayout.Children[0] is Label)
            {
                contentLayout.Children[0] = titleLabel;
            }
            else
            {
                contentLayout.Children.Insert(0, titleLabel);
            }
        }

        public void AddOption(string text, Action action, int margin = 10)
        {
            var optionButton = new Button
            {
                Text = text,
                Style = (Style)Resources["PopupButtonStyle"],
                Margin = new Thickness(0, 0, 0, margin)
            };

            optionButton.Clicked += async (s, e) =>
            {
                await CloseAsync(true);
                action?.Invoke();
            };

            contentLayout.Children.Insert(contentLayout.Children.Count - 1, optionButton);
        }  

        public void SetMessage(string message)
        {
            var messageLabel = new Label
            {
                Text = message,
                Style = (Style)Resources["PopupMessageStyle"]
            };

            if (contentLayout.Children.Count > 1 && contentLayout.Children[1] is Label)
            {
                contentLayout.Children[1] = messageLabel;
            }
            else
            {
                contentLayout.Children.Insert(1, messageLabel);
            }
        }

        public async Task<bool> ShowConfirmationAsync(ContentPage parentPage, string confirmText)
        {
            var tcs = new TaskCompletionSource<bool>();

            this.AddOption(confirmText, () => tcs.TrySetResult(true),-10);
            await this.ShowAsync(parentPage);

            bool result = await tcs.Task;
            await this.CloseAsync(result);

            return result;
        }

        public async Task ShowAsync(ContentPage parentPage)
        {
            await _popupSemaphore.WaitAsync();

            try
            {
                if (_currentlyOpenedPopup != null && _currentlyOpenedPopup != this)
                {
                    await _currentlyOpenedPopup.CloseAsync(false);
                }

                if (!(parentPage.Content is AbsoluteLayout))
                {
                    var content = parentPage.Content;
                    var absoluteLayout = new AbsoluteLayout();
                    absoluteLayout.Children.Add(content);
                    AbsoluteLayout.SetLayoutBounds(content, new Rect(0, 0, 1, 1));
                    AbsoluteLayout.SetLayoutFlags(content, AbsoluteLayoutFlags.All);
                    parentPage.Content = absoluteLayout;
                }

                var parentLayout = (AbsoluteLayout)parentPage.Content;

                if (_parentTapGesture != null)
                {
                    parentLayout.GestureRecognizers.Remove(_parentTapGesture);
                }

                _parentTapGesture = new TapGestureRecognizer();
                _parentTapGesture.Tapped += async (s, e) => await CloseAsync(false);
                parentLayout.GestureRecognizers.Add(_parentTapGesture);

                if (!parentLayout.Children.Contains(this))
                {
                    parentLayout.Children.Add(this);
                    AbsoluteLayout.SetLayoutBounds(this, new Rect(0, 0, 1, 1));
                    AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.All);
                }

                this.IsVisible = true;
                _currentlyOpenedPopup = this;
            }
            catch
            {
                _popupSemaphore.Release();
                throw;
            }
        }
        private void OnOverlayTapped(object sender, EventArgs e)
        {
          
        }

        public async Task CloseAsync(bool result)
        {
            if (this.IsVisible)
            {
                this.IsVisible = false;
                if (_currentlyOpenedPopup == this)
                {
                    _currentlyOpenedPopup = null;
                }
                Canceled?.Invoke(this, EventArgs.Empty);
                _userResponseTask?.TrySetResult(result);
            }

            if (_popupSemaphore.CurrentCount == 0)
            {
                _popupSemaphore.Release();
            }
        }

        public void ClearOptions()
        {
            while (contentLayout.Children.Count > 1)
            {
                contentLayout.Children.RemoveAt(0);
            }
        }

        public void InsertView(int index, View view)
        {
            contentLayout.Children.Insert(index, view);
        }

        public void Dispose()
        {
            _parentTapGesture = null;
            _userResponseTask = null;
        }
    }
}