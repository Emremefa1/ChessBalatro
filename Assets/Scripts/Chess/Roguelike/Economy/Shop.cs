using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Chess.Roguelike.Economy
{
    using Chess.Core;
    using Chess.Roguelike.Core;
    using Chess.Roguelike.Gambits;
    using Chess.Roguelike.Scrolls;

    /// <summary>
    /// Represents an item available for purchase in the shop
    /// </summary>
    [Serializable]
    public class ShopItem
    {
        public enum ItemType
        {
            Piece,
            Gambit,
            Scroll,
            PawnPack  // 3 pawns for discounted price
        }

        public ItemType Type;
        public PieceType PieceType;      // For Piece type
        public Gambit Gambit;             // For Gambit type
        public Scroll Scroll;             // For Scroll type
        public int Price;
        public bool IsSoldOut;
        public int Quantity;              // For packs

        public string GetDisplayName()
        {
            return Type switch
            {
                ItemType.Piece => PieceType.ToString(),
                ItemType.Gambit => Gambit?.Name ?? "Unknown Gambit",
                ItemType.Scroll => Scroll?.Name ?? "Unknown Scroll",
                ItemType.PawnPack => $"Pawn x{Quantity}",
                _ => "Unknown"
            };
        }
    }

    /// <summary>
    /// Manages the shop system - buying pieces, gambits, scrolls
    /// </summary>
    public class Shop
    {
        public event Action OnShopRefreshed;
        public event Action OnItemPurchased;

        private RunState runState;
        private GambitManager gambitManager;
        private ScrollManager scrollManager;
        
        private List<ShopItem> currentItems = new();
        private int rerollCost;
        private int rerollsThisVisit = 0;

        // Shop configuration
        private int pieceSlots = 4;
        private int gambitSlots = 2;
        private int scrollSlots = 2;

        // Available gambits/scrolls (would be loaded from database)
        private List<Gambit> availableGambits = new();
        private List<Scroll> availableScrolls = new();

        public IReadOnlyList<ShopItem> CurrentItems => currentItems;
        public int RerollCost => rerollCost + rerollsThisVisit;
        public int RerollsThisVisit => rerollsThisVisit;

        public Shop(RunState state, GambitManager gambits, ScrollManager scrolls)
        {
            runState = state;
            gambitManager = gambits;
            scrollManager = scrolls;
            rerollCost = 2;
        }

        /// <summary>
        /// Set available gambits that can appear in shop
        /// </summary>
        public void SetAvailableGambits(List<Gambit> gambits)
        {
            availableGambits = gambits ?? new List<Gambit>();
        }

        /// <summary>
        /// Set available scrolls that can appear in shop
        /// </summary>
        public void SetAvailableScrolls(List<Scroll> scrolls)
        {
            availableScrolls = scrolls ?? new List<Scroll>();
        }

        /// <summary>
        /// Generate new shop inventory
        /// </summary>
        public void RefreshShop()
        {
            currentItems.Clear();
            rerollsThisVisit = 0;

            // Generate piece items
            for (int i = 0; i < pieceSlots; i++)
            {
                currentItems.Add(GeneratePieceItem());
            }

            // Generate gambit items (if player has space)
            if (runState.GambitSlotsRemaining > 0)
            {
                for (int i = 0; i < gambitSlots && availableGambits.Count > 0; i++)
                {
                    var gambitItem = GenerateGambitItem();
                    if (gambitItem != null)
                        currentItems.Add(gambitItem);
                }
            }

            // Generate scroll items (if player has space)
            if (runState.ScrollSlotsRemaining > 0)
            {
                for (int i = 0; i < scrollSlots && availableScrolls.Count > 0; i++)
                {
                    var scrollItem = GenerateScrollItem();
                    if (scrollItem != null)
                        currentItems.Add(scrollItem);
                }
            }

            Debug.Log($"[Shop] Refreshed with {currentItems.Count} items");
            OnShopRefreshed?.Invoke();
        }

        private ShopItem GeneratePieceItem()
        {
            // Weighted random piece selection
            float roll = Random.value;
            PieceType pieceType;
            int price;

            if (roll < 0.40f)
            {
                // 40% chance: Pawn pack
                return new ShopItem
                {
                    Type = ShopItem.ItemType.PawnPack,
                    PieceType = PieceType.Pawn,
                    Price = 2,
                    Quantity = 3
                };
            }
            else if (roll < 0.60f)
            {
                pieceType = PieceType.Knight;
                price = 3;
            }
            else if (roll < 0.80f)
            {
                pieceType = PieceType.Bishop;
                price = 3;
            }
            else if (roll < 0.92f)
            {
                pieceType = PieceType.Rook;
                price = 5;
            }
            else
            {
                pieceType = PieceType.Queen;
                price = 9;
            }

            // Price scaling based on cycle
            price = Mathf.Max(1, price + (runState.CurrentCycle - 1));

            return new ShopItem
            {
                Type = ShopItem.ItemType.Piece,
                PieceType = pieceType,
                Price = price,
                Quantity = 1
            };
        }

        private ShopItem GenerateGambitItem()
        {
            if (availableGambits.Count == 0)
                return null;

            var gambit = availableGambits[Random.Range(0, availableGambits.Count)];
            
            // Apply price modifiers from existing gambits
            int price = gambitManager.GetModifiedShopPrice(gambit.ShopPrice, "gambit");

            return new ShopItem
            {
                Type = ShopItem.ItemType.Gambit,
                Gambit = gambit,
                Price = price,
                Quantity = 1
            };
        }

        private ShopItem GenerateScrollItem()
        {
            if (availableScrolls.Count == 0)
                return null;

            var scroll = availableScrolls[Random.Range(0, availableScrolls.Count)];
            
            // Apply price modifiers
            int price = gambitManager.GetModifiedShopPrice(scroll.ShopPrice, "scroll");

            return new ShopItem
            {
                Type = ShopItem.ItemType.Scroll,
                Scroll = scroll,
                Price = price,
                Quantity = 1
            };
        }

        /// <summary>
        /// Attempt to purchase an item
        /// </summary>
        public bool TryPurchase(ShopItem item)
        {
            if (item.IsSoldOut)
            {
                Debug.LogWarning("[Shop] Item is sold out!");
                return false;
            }

            if (!runState.CanAfford(item.Price))
            {
                Debug.LogWarning($"[Shop] Cannot afford {item.GetDisplayName()} (${item.Price})");
                return false;
            }

            // Check if player can hold the item
            bool canHold = item.Type switch
            {
                ShopItem.ItemType.Gambit => runState.CanAddGambit(),
                ShopItem.ItemType.Scroll => runState.CanAddScroll(),
                _ => true // Pieces always allowed
            };

            if (!canHold)
            {
                Debug.LogWarning($"[Shop] No space for {item.Type}!");
                return false;
            }

            // Make purchase
            runState.TrySpendMoney(item.Price);

            switch (item.Type)
            {
                case ShopItem.ItemType.Piece:
                    runState.AddPiece(new PieceInstance(item.PieceType));
                    break;
                    
                case ShopItem.ItemType.PawnPack:
                    for (int i = 0; i < item.Quantity; i++)
                    {
                        runState.AddPiece(new PieceInstance(PieceType.Pawn));
                    }
                    break;
                    
                case ShopItem.ItemType.Gambit:
                    runState.AddGambit(item.Gambit.Id);
                    gambitManager.AddGambit(item.Gambit);
                    break;
                    
                case ShopItem.ItemType.Scroll:
                    runState.AddScroll(item.Scroll.Id);
                    scrollManager.AddScroll(item.Scroll);
                    break;
            }

            item.IsSoldOut = true;
            Debug.Log($"[Shop] Purchased {item.GetDisplayName()} for ${item.Price}");
            OnItemPurchased?.Invoke();
            return true;
        }

        /// <summary>
        /// Attempt to reroll the shop
        /// </summary>
        public bool TryReroll()
        {
            int cost = RerollCost;
            if (!runState.CanAfford(cost))
            {
                Debug.LogWarning($"[Shop] Cannot afford reroll (${cost})");
                return false;
            }

            runState.TrySpendMoney(cost);
            rerollsThisVisit++;
            RefreshShop();
            
            Debug.Log($"[Shop] Rerolled for ${cost}. Next reroll: ${RerollCost}");
            return true;
        }

        /// <summary>
        /// Get piece price based on type and cycle
        /// </summary>
        public int GetPiecePrice(PieceType type)
        {
            int basePrice = type switch
            {
                PieceType.Pawn => 1,
                PieceType.Knight => 3,
                PieceType.Bishop => 3,
                PieceType.Rook => 5,
                PieceType.Queen => 9,
                _ => 1
            };
            
            return gambitManager.GetModifiedShopPrice(basePrice, "piece");
        }
    }
}
