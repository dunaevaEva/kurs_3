using System;
using System.Collections.Generic;
using System.Linq;

namespace AvApp1.Services
{
    public class StatisticsService
    {
        public double Mean(IEnumerable<double> data)
        {
            if (data == null || !data.Any()) return 0;
            return data.Average();
        }

        public double Median(IEnumerable<double> data)
        {
            if (data == null || !data.Any()) return 0;

            var sorted = data.OrderBy(x => x).ToList();
            int n = sorted.Count;
            
            if (n % 2 == 0)
            {
                return (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
            }
            else
            {
                return sorted[n / 2];
            }
        }

        public double StdDev(IEnumerable<double> data)
        {
            if (data == null) return 0;
            
            var list = data.ToArray();
            if (list.Length < 2) return 0;

            double mean = list.Average();
            
            double sumOfSquares = 0;
            foreach (var x in list)
            {
                double diff = x - mean;
                sumOfSquares += diff * diff;
            }

            return Math.Sqrt(sumOfSquares / list.Length);
        }
    }
}
