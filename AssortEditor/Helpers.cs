using Newtonsoft.Json.Linq;
using AssortEditor.Models;

public static class Helpers
{
    public static dynamic DeserializeJson(string json)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject(json);
    }

    public static void AddChildrenRecursively(Item parentItem, List<Item> allItems, Dictionary<string, string> idToParentId)
    {
        var childItems = idToParentId
            .Where(kvp => kvp.Value == parentItem._id)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var childId in childItems)
        {
            var childItem = allItems.FirstOrDefault(i => i._id == childId);
            if (childItem != null)
            {
                if (!parentItem.Children.Contains(childItem))
                {
                    parentItem.Children.Add(childItem);
                }
                AddChildrenRecursively(childItem, allItems, idToParentId);
            }
            else
            {
                Console.WriteLine($"Warning: Child item with ID {childId} not found.");
            }
        }
    }

    public static Item CreateItem(JToken itemData)
    {
        if (itemData == null)
        {
            Console.WriteLine("Error: Item data is null.");
            return null;
        }

        if (itemData.Type != JTokenType.Object)
        {
            Console.WriteLine($"Error: Expected JObject but got {itemData.Type}. Skipping item creation.");
            return null;
        }

        var baseItem = new Item
        {
            _id = itemData["_id"]?.ToString(),
            _tpl = itemData["_tpl"]?.ToString(),
            slotid = itemData["slotId"]?.ToString(),
            parentid = itemData["parentId"]?.ToString(),
        };

        if (baseItem.slotid == "cartridges")
        {
            baseItem.Location = itemData["location"]?.ToObject<int>();
            baseItem.upd.stackobjectscount = itemData["upd"]["StackObjectsCount"]?.ToObject<int>();
        }

        if (baseItem.parentid == "hideout")
        {
            var upd = new Upd
            {
                unlimitedcount = itemData["upd"]?["UnlimitedCount"]?.ToObject<bool>(),
                stackobjectscount = itemData["upd"]?["StackObjectsCount"]?.ToObject<int>(),
                buyrestrictionmax = itemData["upd"]?["BuyRestrictionMax"]?.ToObject<int?>(),
                buyrestrictioncurrent = itemData["upd"]?["BuyRestrictionCurrent"]?.ToObject<int>(),
                repairable = itemData["upd"]?["Repairable"] != null
                    ? new Repairable
                    {
                        durability = itemData["upd"]["Repairable"]["Durability"]?.ToObject<int?>(),
                        maxdurability = itemData["upd"]["Repairable"]["MaxDurability"]?.ToObject<int?>()
                    }
                    : null,
                firemode = itemData["upd"]?["FireMode"] != null
                    ? new FireMode
                    {
                        firemode = itemData["upd"]["FireMode"]["FireMode"]?.ToString()
                    }
                    : null,
                foldable = itemData["upd"]?["Foldable"] != null
                    ? new Foldable
                    {
                        folded = itemData["upd"]["Foldable"]["Folded"]?.ToObject<bool>() ?? false
                    }
                    : null
            };

            baseItem.upd = upd;
        }

        return baseItem;
    }

    public static Barter CreateBarter(JToken barterData)
    {
        var barter = barterData as JObject;
        if (barter == null || barter.Count == 0)
        {
            throw new ArgumentException("Barter data is not in the expected format.");
        }

        var newBarter = new Barter
        {
            _tpl = barter["_tpl"].ToString(),
            Count = Convert.ToInt32(barter["count"])
        };

        // Add Level if it exists and is a valid number
        if (barter.TryGetValue("level", out JToken levelToken) && levelToken.Type == JTokenType.Integer)
        {
            newBarter.Level = levelToken.ToObject<int>();
        }

        // Add Side if it exists and is a valid string
        if (barter.TryGetValue("side", out JToken sideToken) && sideToken.Type == JTokenType.String)
        {
            newBarter.Side = sideToken.ToString();
        }

        return newBarter;
    }

    public static int _objectIdCounter = new Random().Next(0, 0xFFFFFF);
    public static string GenerateMongoObjectId()
    {
        var timestamp = ((int)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds).ToString("x8");

        var random = new byte[5];
        new Random().NextBytes(random);
        var randomHex = BitConverter.ToString(random).Replace("-", "").ToLower();

        var counter = Interlocked.Increment(ref _objectIdCounter) & 0xFFFFFF;
        var counterHex = counter.ToString("x6");

        return timestamp + randomHex + counterHex;
    }
}
