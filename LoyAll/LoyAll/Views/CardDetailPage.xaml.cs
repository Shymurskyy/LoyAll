using LoyAll.Model;

namespace LoyAll
{
    public partial class CardDetailPage : ContentPage
    {
        public CardDetailPage(Card card)
        {
            InitializeComponent();
            StoreNameLabel.Text = card.StoreName;
            CardImage.Source = ImageSource.FromFile(card.CardValue);
        }
    }
}
