using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using LoyAll.Model;
using Microsoft.Maui.Storage;

namespace LoyAll.Services
{
    public static class CardStorageService
    {
        private static string filePath = Path.Combine(FileSystem.AppDataDirectory, "cards.json");
        private static readonly object _fileLock = new object();

        public static void SaveCard(Card card)
        {
            var cards = GetCards();
            cards.Add(card);
            SaveAllCards(cards);
        }

        public static List<Card> GetCards()
        {
            if (!File.Exists(filePath))
                return new List<Card>();

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<Card>>(json) ?? new List<Card>();
        }

        public static void DeleteCard(Card card)
        {
            var cards = GetCards();
            var cardToRemove = cards.FirstOrDefault(c => c.Equals(card));

            if (cardToRemove != null)
            {
                cards.Remove(cardToRemove);
                SaveAllCards(cards);
            }
        }

        public static void DeleteAllCards() => SaveAllCards(new List<Card>());

        public static void ToggleFavorite(Card card)
        {
            lock (_fileLock)
            {
                var cards = GetCards();
                var cardToUpdate = cards.FirstOrDefault(c => c.Equals(card));

                if (cardToUpdate != null)
                {
                    cardToUpdate.IsFavorite = !cardToUpdate.IsFavorite;
                    SaveAllCards(cards);
                }
            }
        }

        private static void SaveAllCards(List<Card> cards)
        {
            var json = JsonConvert.SerializeObject(cards);
            File.WriteAllText(filePath, json);
        }
    }
}