using System;
using Platform.Data.Core.Doublets;
using Platform.Helpers.Counters;
using Platform.Memory;

namespace Platform.Sandbox
{
    public class Zadacha
    {
        public const int StepsFor7DigitPhoneNumbers = 6;

        public void RunOne()
        {
            Console.Write("Введите начальную цифру (от 0 до 9): ");
            int start;

            if (int.TryParse(Console.ReadLine(), out start))
            {
                if (start < 0 || start > 9)
                    Console.WriteLine("Можно использовать только цифры от 0 до 9.");
                else
                {
                    var total = Calculate(start, StepsFor7DigitPhoneNumbers);
                    Console.WriteLine($"Общее количество возможных шахматных семизначных чисел начинающихся с {start}: {total}.");
                }
            }
            else
                Console.WriteLine("Распознать число в введённых данных не удалось.");

            Console.ReadKey();
        }

        public void RunAll()
        {
            for (int start = 0; start < 10; start++)
            {
                var total = Calculate(start, StepsFor7DigitPhoneNumbers);
                Console.WriteLine($"Общее количество возможных шахматных семизначных чисел начинающихся с {start}: {total}.");
            }

            Console.ReadKey();
        }

        private ulong Calculate(int start, int steps)
        {
            const int mb4 = 4 * 1024 * 1024;

            using (var memory = new HeapResizableDirectMemory(mb4))
            using (var memoryManager = new UInt64ResizableDirectMemoryLinks(memory))
            using (var links = new UInt64Links(memoryManager))
            {
                ulong[] digits = new ulong[10];

                SetPoints(links, digits);
                SetSteps(links, digits);

                ulong current = digits[start];

                var counter = new Counter();

                CalculateCore(links, current, steps, counter);

                return counter.Count;
            }
        }

        private void CalculateCore(ILinks<ulong> links, ulong current, int stepsLeft, Counter counter)
        {
            links.Each(link =>
                       {
                           if (links.IsFullPoint(link[links.Constants.IndexPart]))
                               return links.Constants.Continue;

                           if (stepsLeft == 0)
                               counter.Count++;
                           else
                           {
                               var target = link[links.Constants.TargetPart];
                               CalculateCore(links, target, stepsLeft - 1, counter);
                           }

                           return links.Constants.Continue;
                       }, links.Constants.Any, current, links.Constants.Any);
        }

        private static void SetPoints(ILinks<ulong> links, ulong[] digits)
        {
            // Создадим точки в графе для каждой цифры
            for (int i = 0; i < digits.Length; i++)
                digits[i] = links.CreatePoint();
        }

        private static void SetSteps(ILinks<ulong> links, ulong[] digits)
        {
            // 0
            links.CreateAndUpdate(digits[0], digits[4]);
            links.CreateAndUpdate(digits[0], digits[6]);

            // 1
            links.CreateAndUpdate(digits[1], digits[6]);
            links.CreateAndUpdate(digits[1], digits[8]);

            // 2
            links.CreateAndUpdate(digits[2], digits[7]);
            links.CreateAndUpdate(digits[2], digits[9]);

            // 3
            links.CreateAndUpdate(digits[3], digits[4]);
            links.CreateAndUpdate(digits[3], digits[8]);

            // 4
            links.CreateAndUpdate(digits[4], digits[0]);
            links.CreateAndUpdate(digits[4], digits[3]);
            links.CreateAndUpdate(digits[4], digits[9]);

            // 5

            // 6
            links.CreateAndUpdate(digits[6], digits[0]);
            links.CreateAndUpdate(digits[6], digits[1]);
            links.CreateAndUpdate(digits[6], digits[7]);

            // 7
            links.CreateAndUpdate(digits[7], digits[2]);
            links.CreateAndUpdate(digits[7], digits[6]);

            // 8
            links.CreateAndUpdate(digits[8], digits[1]);
            links.CreateAndUpdate(digits[8], digits[3]);

            // 9
            links.CreateAndUpdate(digits[9], digits[2]);
            links.CreateAndUpdate(digits[9], digits[4]);
        }
    }
}
