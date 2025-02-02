using LoyAll.Model;
using LoyAll.Services;

namespace LoyAll.Views
{
    public partial class AddCardPage : ContentPage
    {
        private string selectedImagePath;
        private MainPage mainPage;

        public AddCardPage(MainPage mainPage)
        {
            InitializeComponent();
            this.mainPage = mainPage;
        }

        private async void OnPickImageClicked(object sender, EventArgs e)
        {
            FileResult? result = await MediaPicker.PickPhotoAsync();
            if (result != null)
            {
                Stream stream = await result.OpenReadAsync();
                CardImage.Source = ImageSource.FromStream(() => stream);
                selectedImagePath = result.FullPath;
            }
        }

        private async void OnSaveCardClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StoreNameEntry.Text) || CardImage.Source == null)
            {
                await DisplayAlert("B³¹d", "Podaj nazwê i wybierz obrazek!", "OK");
                return;
            }
            
            Card card = new Card
            {
                StoreName = StoreNameEntry.Text,
                ImagePath = selectedImagePath
            };

            CardStorageService.SaveCard(card);
            mainPage.AddCard(card);
            await Navigation.PopAsync();

        }
    }
}
