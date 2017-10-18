using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskExperiments
{
    class Program
    {
        static void Main(string[] args)
        {
            Task task1 = new Task(() => {
                Console.WriteLine("Id задачи: {0}", Task.CurrentId);
            });

            Task task2 = new Task(SecondTask);
            // задача продолжения
            task1.ContinueWith(Display);

            task1.Start();
            task1.Wait();
            task2.Start();

            // ждем окончания второй задачи
//            task2.Wait();
//            Console.WriteLine("Выполняется работа метода Main");
//            for (int i = 0; i < 200; i++)
//            {
//                Console.WriteLine(i);
//            }
            Console.ReadLine();
        }

        static void SecondTask()
        {
            for (int i = 0; i < 200; i++)
            {
                Console.WriteLine($"A{i}");
            }
        }

        static void Display(Task t)
        {
            Console.WriteLine("Id задачи: {0}", Task.CurrentId);
            Console.WriteLine("Id предыдущей задачи: {0}", t.Id);
        }
    }
}