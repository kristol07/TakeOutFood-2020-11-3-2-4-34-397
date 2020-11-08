namespace TakeOutFood
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    public class App
    {
        private IItemRepository itemRepository;
        private ISalesPromotionRepository salesPromotionRepository;

        public App(IItemRepository itemRepository, ISalesPromotionRepository salesPromotionRepository)
        {
            this.itemRepository = itemRepository;
            this.salesPromotionRepository = salesPromotionRepository;
        }

        public string BestCharge(List<string> inputs)
        {
            //var output = new StringBuilder();
            var output = "";
            output += "============= Order details =============\n";

            var allItems = itemRepository.FindAll();
            var allItemsDict = BuildItemFactoryDict(allItems);
            var promoRules = salesPromotionRepository.FindAll();
            var itemsDict = BuildItemsDict(inputs);

            double totalPrice = CalculateTotalPrice(allItemsDict, itemsDict, ref output);

            double totalPromoPrice = 0;
            var promo = FindMostPromotedRule(promoRules, allItemsDict, itemsDict);
            if (promo != null)
            {
                output += "-----------------------------------\n";
                output += "Promotion used:\n";
                var boughtPromoItems = promo.RelatedItems.Where(id => itemsDict.ContainsKey(id));
                var promoInfo = $"{promo.DisplayName} ({boughtPromoItems.Select(id => allItemsDict[id].Name).Aggregate((a, b) => a + ", " + b)})";
                var discount = ReadDiscountFromPromoType(promo.Type);
                var promoPrice = boughtPromoItems.Select(id => allItemsDict[id].Price * itemsDict[id] * (1 - discount)).Sum();
                totalPromoPrice += promoPrice;
                output += $"{promoInfo}, saving {promoPrice} yuan\n";
            }

            output += "-----------------------------------\n";
            output += $"Total：{totalPrice - totalPromoPrice} yuan\n";
            output += "===================================";

            return output.ToString();
        }

        public double CalculateTotalPrice(Dictionary<string, Item> allItemsDict, Dictionary<string, int> itemsDict, ref string output)
        {
            double totalPrice = 0;
            var temp = "";

            itemsDict.Keys.ToList().ForEach((id) =>
            {
                temp += $"{allItemsDict[id].Name} x {itemsDict[id]} = {allItemsDict[id].Price * itemsDict[id]} yuan\n";
                totalPrice += allItemsDict[id].Price * itemsDict[id];
            });

            output += temp;

            return totalPrice;
        }

        public SalesPromotion FindMostPromotedRule(List<SalesPromotion> promos, Dictionary<string, Item> allItemsDict, Dictionary<string, int> itemsDict)
        {
            var (maxPromosPrice, mostPromoRule) = promos.Select((promo) =>
            {
                var boughtPromoItems = promo.RelatedItems.Where(id => itemsDict.ContainsKey(id));
                if (!boughtPromoItems.Any())
                    return (price: 0, rule: null);

                var discount = ReadDiscountFromPromoType(promo.Type);
                var promoPrice = boughtPromoItems.Select(id => allItemsDict[id].Price * itemsDict[id] * (1 - discount)).Sum();
                return (price: promoPrice, rule: promo);
            }).OrderByDescending(i => i.price).First();

            return mostPromoRule;
        }

        private double ReadDiscountFromPromoType(string type)
        {
            return double.Parse(type.Substring(0, 2)) * 0.01;
        }

        public Dictionary<string, Item> BuildItemFactoryDict(List<Item> items)
        {
            var dict = new Dictionary<string, Item>();

            items.ForEach((item) => dict.Add(item.Id, item));

            return dict;
        }

        public Dictionary<string, int> BuildItemsDict(List<string> inputs)
        {
            var all = new Dictionary<string, int>();

            inputs.ForEach((input) =>
            {
                var id = input.Split(' ')[0];
                var count = int.Parse(input.Split(' ')[2]);
                all.Add(id, count);
            });

            return all;
        }
    }
}
