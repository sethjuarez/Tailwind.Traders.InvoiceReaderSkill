using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tailwind.Traders.InvoiceReaderSkill
{
    public static class Parser
    {
        private static Dictionary<string, string> _personMapping = new Dictionary<string, string>
        {
            {"Name", "Name"},
            {"Address", "Address"},
            {"City", "City"},
            {"State", "StateProvince"},
            {"Postal Code", "PostalCode"},
            {"Country", "Country"}
        };

        private static Dictionary<string, string> _companyMapping = new Dictionary<string, string>
        {
            {"Company", "Name"},
            {"Company Address", "Address"},
            {"Company City", "City"},
            {"Company State", "StateProvince"},
            {"Company Postal Code", "PostalCode"},
            {"Company Country", "Country"},
        };

        private static Dictionary<string, string> _invoiceMapping = new Dictionary<string, string>
        {
            {"Invoice No.", "InvoiceId"},
            {"Order Date", "OrderDate"},
            {"Subtotal", "SubTotal"},
            {"Total Discount", "TotalDiscount"},
            {"Tax Rate", "TaxRate"},
            {"Total Tax", "TotalTax"},
            {"Total", "Total"}
        };

        private static Dictionary<string, string> _lineItemMapping = new Dictionary<string, string>()
        {
            {"Itm", "ItemId"},
            {"Qty", "Quantity"},
            {"Description", "Description"},
            {"Price", "Price"},
            {"Discount", "DiscountTotal"},
            {"(Pct)", "Discount"},
            {"Tax", "Tax"},
            {"LineTotal", "LineTotal"}
        };

        public static Invoice Parse(string json)
        {
            dynamic data = JsonConvert.DeserializeObject(json);
            var keyVals = new Dictionary<string, string>();
            foreach (dynamic keyVal in data.pages[0].keyValuePairs)
            {
                string key = keyVal.key[0].text;
                string val = string.Empty;
                if (keyVal.value.Count > 0)
                     val = keyVal.value[0].text;
                    
                keyVals[key] = val;
            }

            
            var tableCols = new Dictionary<string, List<string>>();
            int totalItems = 10000;
            foreach (dynamic col in data.pages[0].tables[0].columns)
            {
                string key = col.header[0].text;
                var vals = new List<string>();
                foreach (dynamic entry in col.entries)
                {
                    string text = entry[0].text;
                    vals.Add(text);
                }
                if (vals.Count < totalItems)
                    totalItems = vals.Count;

                tableCols[key] = vals;
            }

            string keys = new string(tableCols.SelectMany(s => s.Key + "\n").ToArray());

            var invoice = Map<Invoice>(_invoiceMapping, keyVals);
            invoice.Person = Map<Person>(_personMapping, keyVals);
            invoice.Company = Map<Company>(_companyMapping, keyVals);
            invoice.LineItems = Map<LineItem>(_lineItemMapping, tableCols, totalItems).ToArray();
            return invoice;
        }

        private static T Map<T>(Dictionary<string, string> mapping, Dictionary<string, string> values)
            where T : new()
        {
            Type t = typeof(T);
            T o = new T();
            foreach (var item in mapping.Keys)
            {
                var prop = t.GetProperty(mapping[item]);
                if(prop.PropertyType == typeof(int))
                    prop.SetValue(o, int.Parse(values[item]));
                else if(prop.PropertyType == typeof(decimal))
                    prop.SetValue(o, decimal.Parse(values[item].Replace("$","").Replace("%","")));
                else if(prop.PropertyType == typeof(DateTime))
                    prop.SetValue(o, DateTime.Parse(values[item]));
                else
                    prop.SetValue(o, values[item]);
            }

            return o;
        }

        private static IEnumerable<T> Map<T>(Dictionary<string, string> mapping, Dictionary<string, List<string>> values, int count)
            where T : new()
        {
            Type t = typeof(T);
            for (int i = 0; i < count; i++)
            {
                T o = new T();
                foreach (var item in mapping.Keys)
                {
                    var prop = t.GetProperty(mapping[item]);
                    if (prop.PropertyType == typeof(int))
                        prop.SetValue(o, int.Parse(values[item][i]));
                    else if (prop.PropertyType == typeof(decimal))
                        prop.SetValue(o, decimal.Parse(values[item][i].Replace("$", "").Replace("%", "")));
                    else if (prop.PropertyType == typeof(DateTime))
                        prop.SetValue(o, DateTime.Parse(values[item][i]));
                    else
                        prop.SetValue(o, values[item][i]);
                }

                yield return o;
            }
        }
    }
}
