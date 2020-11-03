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
            var promos = salesPromotionRepository.FindAll();
            var dictItems = BuildDict(allItems);

            var boughtDict = GetItemsFromInput(inputs);

            double totalPrice = 0;

            foreach (var id in boughtDict.Keys)
            {
                output += $"{dictItems[id].Name} x {boughtDict[id]} = {dictItems[id].Price * boughtDict[id]} yuan\n";
                totalPrice += dictItems[id].Price * boughtDict[id];
            }

            double totalPromoPrice = 0;

            var promo = FindMostPromotedRule(promos, dictItems, boughtDict);

            if (promo != null)
            {
                output += "-----------------------------------\n";
                output += "Promotion used:\n";
                var boughtPromoItems = promo.RelatedItems.Where(id => boughtDict.ContainsKey(id));
                var note = $"{promo.DisplayName} ({boughtPromoItems.Select(id => dictItems[id].Name).Aggregate((a, b) => a + ", " + b)})";
                var discount = 0.5;
                var promoPrice = boughtPromoItems.Select(id => dictItems[id].Price * boughtDict[id] * (1 - discount)).Sum();
                totalPromoPrice += promoPrice;
                output += $"{note}, saving {promoPrice} yuan\n";
            }

            output += "-----------------------------------\n";
            output += $"Total: {totalPrice - totalPromoPrice} yuan\n";
            output += "===================================";

            var result = output.ToString();

            return output.ToString();

        }

        public SalesPromotion FindMostPromotedRule(List<SalesPromotion> promos, Dictionary<string, Item> dictItems, Dictionary<string, int> boughtDict)
        {
            double totalPromoPrice = 0;
            SalesPromotion promoRule = null;
            foreach(var promo in promos)
            {
                var boughtPromoItems = promo.RelatedItems.Where(id => boughtDict.ContainsKey(id));
                if (!boughtPromoItems.Any())
                    continue;
                var discount = 0.5;
                var promoPrice = boughtPromoItems.Select(id => dictItems[id].Price * boughtDict[id] * (1 - discount)).Sum();
                if(promoPrice>=totalPromoPrice)
                {
                    totalPromoPrice = promoPrice;
                    promoRule = promo;
                }
            }

            return promoRule;

        }

        public Dictionary<string, Item> BuildDict(List<Item> items)
        {
            var dict = new Dictionary<string, Item>();
            foreach (var item in items)
            {
                dict.Add(item.Id, item);
            }
            return dict;
        }

        public Dictionary<string, int> GetItemsFromInput(List<string> inputs)
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
