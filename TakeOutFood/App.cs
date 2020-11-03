namespace TakeOutFood
{
    using System;
    using System.Collections.Generic;
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
            var promoRules = salesPromotionRepository.FindAll();
            var allItemsDict = BuildAllItemsDict(allItems);
            var itemsDict = BuildItemsDict(inputs);

            double totalPrice = 0;
            foreach (var id in itemsDict.Keys)
            {
                output += $"{allItemsDict[id].Name} x {itemsDict[id]} = {allItemsDict[id].Price * itemsDict[id]} yuan\n";
                totalPrice += allItemsDict[id].Price * itemsDict[id];
            }

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

        public SalesPromotion FindMostPromotedRule(List<SalesPromotion> promos, Dictionary<string, Item> AllItemsDict, Dictionary<string, int> itemsDict)
        {
            double totalPromoPrice = 0;
            SalesPromotion promoRule = null;

            foreach (var promo in promos)
            {
                var boughtPromoItems = promo.RelatedItems.Where(id => itemsDict.ContainsKey(id));
                if (!boughtPromoItems.Any())
                    continue;

                var discount = ReadDiscountFromPromoType(promo.Type);
                var promoPrice = boughtPromoItems.Select(id => AllItemsDict[id].Price * itemsDict[id] * (1 - discount)).Sum();
                if (promoPrice >= totalPromoPrice)
                {
                    totalPromoPrice = promoPrice;
                    promoRule = promo;
                }
            }

            return promoRule;
        }

        private double ReadDiscountFromPromoType(string type)
        {
            return double.Parse(type.Substring(0, 2)) * 0.01;
        }

        public Dictionary<string, Item> BuildAllItemsDict(List<Item> items)
        {
            var dict = new Dictionary<string, Item>();

            foreach (var item in items)
            {
                dict.Add(item.Id, item);
            }
            
            return dict;
        }

        public Dictionary<string, int> BuildItemsDict(List<string> inputs)
        {
            var all = new Dictionary<string, int>();

            foreach (var input in inputs)
            {
                var id = input.Split(' ')[0];
                var count = int.Parse(input.Split(' ')[2]);
                all.Add(id, count);
            }

            return all;
        }
    }
}
