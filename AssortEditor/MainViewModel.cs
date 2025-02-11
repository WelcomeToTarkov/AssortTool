using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using AssortEditor.Models;
using System.Text.Json;


namespace AssortEditor
{
    public class MainViewModel
    {
        public ObservableCollection<Item> LoyaltyLevel1Items { get; set; } = new();
        public ObservableCollection<Item> LoyaltyLevel2Items { get; set; } = new();
        public ObservableCollection<Item> LoyaltyLevel3Items { get; set; } = new();
        public ObservableCollection<Item> LoyaltyLevel4Items { get; set; } = new();

        public string SaveItemsAndBarters()
        {
            var loyalLevelItems = new Dictionary<string, int>();
            var barterScheme = new Dictionary<string, List<object>>();
            var allItems = new Dictionary<string, object>();
            

            // Helper to process items and populate dictionaries
            void ProcessItem(Item item, int loyaltyLevel)
            {
                try
                {
                    var baseItem = new Dictionary<string, object>
                    {
                        { "_id", item._id },
                        { "_tpl", item._tpl },
                        { "parentId", item.parentid },
                        { "slotId", item.slotid }
                    };

                    if (item.slotid == "cartridges")
                    {
                        baseItem["location"] = item.Location ?? 0;
                        
                        var updObject = new Dictionary<string, object>();
                        updObject["StackObjectsCount"] = item.upd.stackobjectscount;
                        baseItem["upd"] = updObject;
                    }

                    if (item.parentid == "hideout")
                    {
                        loyalLevelItems[item._id] = loyaltyLevel;


                        barterScheme[item._id] = new List<object>
                        {
                            item.Barters.Select(barter =>
                            {
                                var barterObject = new Dictionary<string, object>
                                {
                                    { "count", barter.Count },
                                    { "_tpl", barter._tpl }
                                };

                                // Add Level only if it exists
                                if (barter.Level.HasValue)
                                {
                                    barterObject["level"] = barter.Level.Value;
                                }

                                // Add Side only if it exists
                                if (!string.IsNullOrEmpty(barter.Side))
                                {
                                    barterObject["side"] = barter.Side;
                                }

                                return barterObject;
                            }).ToList()
                        };


                        var updObject = new Dictionary<string, object>();

                        // Add each 'upd' property with logging
                        if (item.upd.unlimitedcount != null)
                        {
                            updObject["UnlimitedCount"] = item.upd.unlimitedcount;
                        }
                        if (item.upd.stackobjectscount != null)
                        {
                            updObject["StackObjectsCount"] = item.upd.stackobjectscount;
                        }
                        if (item.upd.buyrestrictioncurrent != null)
                        {
                            updObject["BuyRestrictionCurrent"] = item.upd.buyrestrictioncurrent;
                        }
                        if (item.upd.buyrestrictionmax != null)
                        {
                            updObject["BuyRestrictionMax"] = item.upd.buyrestrictionmax;
                        }

                        if (item.upd.repairable != null)
                        {
                            var repairableObject = new Dictionary<string, object>();
                            if (item.upd.repairable.durability.HasValue)
                            {
                                repairableObject["Durability"] = item.upd.repairable.durability;
                            }

                            if (item.upd.repairable.maxdurability.HasValue)
                            {
                                repairableObject["MaxDurability"] = item.upd.repairable.maxdurability;
                            }

                            if (repairableObject.Count > 0)
                            {
                                updObject["Repairable"] = repairableObject;
                            }

                            if (item.upd.foldable != null && item.upd.foldable.foldable)
                            {
                                var foldableObject = new Dictionary<string, object>
                                {
                                    { "Folded", item.upd.foldable.folded }
                                };

                                if (foldableObject["Folded"] != null)
                                {
                                    updObject["Foldable"] = foldableObject;
                                }
                            }

                            if (!string.IsNullOrEmpty(item.upd.firemode?.firemode))
                            {
                                var firemodeObject = new Dictionary<string, object>
                                {
                                    { "FireMode", item.upd.firemode.firemode }
                                };
                                updObject["FireMode"] = firemodeObject;
                            }


                        }
                        
                        baseItem["upd"] = updObject;
                    }

                    allItems[item._id] = baseItem;

                    if (item.Children.Count > 0)
                    {
                        foreach (var child in item.Children)
                        {
                            ProcessItem(child, loyaltyLevel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing item {item._id}: {ex.Message}");
                }
            }

            // Process all items as usual
            try
            {
                foreach (var item in LoyaltyLevel1Items) ProcessItem(item, 1);
                foreach (var item in LoyaltyLevel2Items) ProcessItem(item, 2);
                foreach (var item in LoyaltyLevel3Items) ProcessItem(item, 3);
                foreach (var item in LoyaltyLevel4Items) ProcessItem(item, 4);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing loyalty level items: {ex.Message}");
            }
            
            
            Console.WriteLine($"On Save: Total Items: {allItems.Count}, Loyalty Levels: {loyalLevelItems.Count}, Barter Schemes: {barterScheme.Count}");


            var jsonObject = new
            {
                items = allItems.Values,
                barter_scheme = barterScheme,
                loyal_level_items = loyalLevelItems
            };

            try
            {
                return JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error serializing JSON: {ex.Message}");
                return null;
            }
        }

        public void LoadItemsAndBarters(string json)
        {
            try
            {
                var assortData = Helpers.DeserializeJson(json);

                var loyaltyLevelItems = assortData["loyal_level_items"] as JObject;
                var allItems = assortData["items"] as JArray;
                var barterScheme = assortData["barter_scheme"] as JObject;

                Console.WriteLine($"On Load: Total Items: {allItems.Count}, Loyalty Levels: {loyaltyLevelItems.Count}, Barter Schemes: {barterScheme.Count}");

                var idToParentId = new Dictionary<string, string>();
                foreach (var item in allItems)
                {
                    string _id = item["_id"].ToString();
                    string parentId = item["parentId"].ToString();
                    idToParentId[_id] = parentId;
                }

                List<Item> topLevelItems = new List<Item>();
                List<Item> childItems = new List<Item>();
                List<Item> allProcessedItems = new List<Item>();

                foreach (var item in allItems)
                {
                    if (item["parentId"].ToString() == "hideout")
                    {
                        Item newItem = Helpers.CreateItem(item);
                        topLevelItems.Add(newItem);
                        allProcessedItems.Add(newItem);
                        string loyaltyLevelKey = item["_id"].ToString();

                        // Add barter scheme
                        if (barterScheme.ContainsKey(loyaltyLevelKey))
                        {
                            var barters = barterScheme[loyaltyLevelKey] as JArray;
                            if (barters != null)
                            {
                                foreach (var barter in barters)
                                {
                                    foreach (var trade in barter)
                                    {
                                        newItem.Barters.Add(Helpers.CreateBarter(trade));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Item newItem = Helpers.CreateItem(item);
                        childItems.Add(newItem);
                        allProcessedItems.Add(newItem);
                    }
                }

                foreach (var item in topLevelItems)
                {
                    Helpers.AddChildrenRecursively(item, allProcessedItems, idToParentId);
                }

                int itemsAddedToUI = 0;

                foreach (var item in topLevelItems)
                {
                    int? loyaltyLevel = loyaltyLevelItems[item._id]?.Value<int?>();

                    // Debugging log for missing loyalty level
                    if (loyaltyLevel == null)
                    {
                        Console.WriteLine($"Warning: Item {item._id} has no loyalty level.");
                    }
                    else
                    {
                        if (AddItemToLoyaltyLevel(item, loyaltyLevel))
                        {
                            itemsAddedToUI++;
                        }
                        else
                        {
                            Console.WriteLine($"Failed to add item {item._id} to loyalty level {loyaltyLevel}");
                        }
                    }
                }

                // Log the final count and any issues
                Console.WriteLine($"Outro: Total Items Added to Loyalty Levels: {itemsAddedToUI}, Top-Level Items: {topLevelItems.Count}, Total Items Processed: {allItems.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading items and barters: {ex.Message}");
            }
        }

        private bool AddItemToLoyaltyLevel(Item item, int? loyaltyLevel)
        {
            if (loyaltyLevel == null || loyaltyLevel < 1 || loyaltyLevel > 4)
            {
                // Log the item and set it to level 1 by default
                Console.WriteLine($"Warning: Invalid or missing loyalty level for item {item._id}. Assigning to level 1.");
                loyaltyLevel = 1;
            }

            switch (loyaltyLevel)
            {
                case 1:
                    LoyaltyLevel1Items.Add(item);
                    return true;
                case 2:
                    LoyaltyLevel2Items.Add(item);
                    return true;
                case 3:
                    LoyaltyLevel3Items.Add(item);
                    return true;
                case 4:
                    LoyaltyLevel4Items.Add(item);
                    return true;
                default:
                    // This should never happen with the above check
                    Console.WriteLine($"Unexpected case: {item._id} with loyalty level {loyaltyLevel}");
                    return false;
            }
        }

        public void AddItem(Item item, string loyaltyLevel)
        {
            switch (loyaltyLevel)
            {
                case "Loyalty Level 1":
                    LoyaltyLevel1Items.Add(item);
                    break;
                case "Loyalty Level 2":
                    LoyaltyLevel2Items.Add(item);
                    break;
                case "Loyalty Level 3":
                    LoyaltyLevel3Items.Add(item);
                    break;
                case "Loyalty Level 4":
                    LoyaltyLevel4Items.Add(item);
                    break;
                default:
                    throw new ArgumentException("Invalid loyalty level");
            }
        }

    }

}
