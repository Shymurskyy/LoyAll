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

        public static void SaveCard(Card card)
        {
            List<Card> cards = GetCards();
            cards.Add(card);

            string json = JsonConvert.SerializeObject(cards);
            File.WriteAllText(filePath, json);
        }

        public static List<Card> GetCards()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<Card>>(json) ?? new List<Card>();
            }
            return new List<Card>();
        }
    }
}
