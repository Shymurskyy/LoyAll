﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LoyAll.Model
{
    public class Card : INotifyPropertyChanged
    {
        public string _storeName { get; set; }
        public string CardValue { get; set; }
        private bool _isFavorite;
        public string StoreName
        {
            get => _storeName;
            set
            {
                if (_storeName != value)
                {
                    _storeName = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite != value)
                {
                    _isFavorite = value;
                    OnPropertyChanged(nameof(IsFavoriteImage));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public string IsFavoriteImage => IsFavorite ? "orange_favorite_full.svg" : "favorite_empty.svg";
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool Equals(Card other)
        {
            if (other is null) return false;
            return StoreName == other.StoreName && CardValue == other.CardValue;
        }
        public string CleanCardValue
        {
            get
            {
                if (CardValue.StartsWith("Q:#")|| CardValue.StartsWith("B:#") || CardValue.StartsWith("P:#"))
                    return CardValue.Substring(3); 
                return CardValue;
            }
        }
    }
}
