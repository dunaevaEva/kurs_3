using System;
using System.Collections.Generic;
using System.Linq;
using AvApp1.Model;

namespace AvApp1.Services
{
    public class ChartService
    {
        public Dictionary<string, int> CountByKey(
            IEnumerable<DataRow> data,
            string key)
        {
            if (string.IsNullOrEmpty(key)) return new Dictionary<string, int>();

            return data
                .Select(r => r.Values.TryGetValue(key, out var val) ? val : "unknown")
                .GroupBy(v => v)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public IDictionary<string, int> ApplyTopN(
            IDictionary<string, int> source,
            int topN)
        {
            if (topN <= 0) return source;

            return source
                .OrderByDescending(x => x.Value)
                .Take(topN)
                .ToDictionary(x => x.Key, x => x.Value);
        }
        
        public IEnumerable<ChartItem> BuildChartItems(
            IDictionary<string, int> values,
            IDictionary<string, LabelInfo> labels)
        {
            foreach (var kv in values)
            {
                labels.TryGetValue(kv.Key, out var info);

                yield return new ChartItem
                {
                    Label = string.IsNullOrEmpty(info?.DisplayName) ? kv.Key : info.DisplayName,
                    Value = kv.Value,
                    Color = info?.Color ?? "#808080"
                };
            }
        }
    }
}
