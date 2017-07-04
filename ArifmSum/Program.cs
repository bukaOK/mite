using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArifmSum
{
    class Program
    {
        static void Main(string[] args)
        {
            var n = 30;
            var income = 1500;
            var sum = 1500 * 0.7;
            var incomes = new double[n];
            incomes[0] = 0;
            incomes[n - 1] = sum * 2 / n - incomes[0];
            var d = (incomes[n - 1] - incomes[0]) / (n - 1);
            Console.WriteLine("D: " + d);
            for(var i = 1; i < incomes.Length - 2; i++)
            {
                incomes[i] = incomes[0] + (i - 1) * d;
                Console.WriteLine("Id: {0}, Sum: {1}", i, incomes[i]);
            }
            Console.WriteLine("Id: {0}, Sum: {1}", n - 1, incomes[n - 1]);
            Console.ReadKey();
        }
    }
}
