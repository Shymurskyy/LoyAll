using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyAll.Model
{
    public class Card
    {
        public string StoreName { get; set; }
        public string CardValue { get; set; }
        public string CleanCardValue
        {
            get
            {
                if (CardValue.StartsWith("Q:#")|| CardValue.StartsWith("B:#"))
                    return CardValue.Substring(3); 
                return CardValue;
            }
        }
    }
}
