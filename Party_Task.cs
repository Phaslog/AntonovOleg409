using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static List<string> Friends = new List<string>();
    static List<string> Venues = new List<string>();
    static List<Expense> Expenses = new List<Expense>();
    static string CalculationName = null;
    static string CalculationDate = null;
    static string SavePath = null;
    static bool FirstSave = true;

    static void Main()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1. Добавить друга");
            Console.WriteLine("2. Добавить заведение");
            Console.WriteLine("3. Добавить расходы");
            Console.WriteLine("4. Рассчитать долги");
            Console.WriteLine("5. Очистить расчеты");
            Console.WriteLine("6. Выйти");

            switch (Console.ReadLine())
            {
                case "1": AddFriend(); break;
                case "2": AddVenue(); break;
                case "3": AddExpense(); break;
                case "4": CalculateDebts(); break;
                case "5": ClearExpenses(); break;
                case "6": SaveAndExit(); return;
                default: Console.WriteLine("Неверный ввод!"); break;
            }
        }
    }

    static void AddFriend()
    {
        Console.Write("Введите имя друга: ");
        string name = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(name) && !Friends.Contains(name))
        {
            Friends.Add(name);
            Console.WriteLine("Друг добавлен!");
        }
        else
        {
            Console.WriteLine("Ошибка: имя уже существует или некорректно!");
        }
        Console.ReadKey();
    }

    static void AddVenue()
    {
        Console.Write("Введите название заведения: ");
        string venue = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(venue) && !Venues.Contains(venue))
        {
            Venues.Add(venue);
            Console.WriteLine("Заведение добавлено!");
        }
        else
        {
            Console.WriteLine("Ошибка: заведение уже существует или некорректно!");
        }
        Console.ReadKey();
    }

    static void AddExpense()
    {
        if (Friends.Count == 0 || Venues.Count == 0)
        {
            Console.WriteLine("Сначала добавьте друзей и заведения!");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Выберите заведение:");
        for (int i = 0; i < Venues.Count; i++)
            Console.WriteLine($"{i + 1}. {Venues[i]}");
        int venueIndex = int.Parse(Console.ReadLine()) - 1;

        Console.WriteLine("Кто был в этом заведении и сколько потратил? (введите Enter для завершения)");
        var expenses = new Dictionary<string, double>();

        while (true)
        {
            Console.Write("Имя друга: ");
            string name = Console.ReadLine();
            if (name.ToLower() == "") break;
            if (!Friends.Contains(name)) { Console.WriteLine("Ошибка: друга нет в списке!"); continue; }

            Console.Write("Сумма: ");
            if (double.TryParse(Console.ReadLine(), out double amount))
                expenses[name] = amount;
        }

        Console.Write("Кто оплатил счет? ");
        string payer = Console.ReadLine();
        if (!Friends.Contains(payer)) { Console.WriteLine("Ошибка: друга нет в списке!"); return; }

        Expenses.Add(new Expense(Venues[venueIndex], expenses, payer));
        Console.WriteLine("Расходы добавлены!");
        Console.ReadKey();
    }

    static void CalculateDebts()
    {
        if (CalculationName == null || CalculationDate == null)
        {
            Console.Write("Введите имя расчета: ");
            CalculationName = Console.ReadLine();
            Console.Write("Введите дату (ГГГГ-ММ-ДД): ");
            CalculationDate = Console.ReadLine();
        }

        SaveResults();
        Console.ReadKey();
    }

    static void ClearExpenses()
    {
        Expenses.Clear();
        CalculationName = null;
        CalculationDate = null;
        SavePath = null;
        FirstSave = true;
        Console.WriteLine("Расчеты очищены!");
        Console.ReadKey();
    }

    static void SaveResults()
    {
        if (SavePath == null)
        {
            Console.Write("Введите путь для сохранения (например, C:\\результаты.txt): ");
            SavePath = Console.ReadLine();
        }

        try
        {
            using (StreamWriter writer = new StreamWriter(SavePath, true))
            {
                if (FirstSave)
                {
                    writer.WriteLine($"Расчет: {CalculationName}");
                    writer.WriteLine($"Дата: {CalculationDate}");
                    FirstSave = false;
                }

                foreach (var expense in Expenses)
                {
                    writer.WriteLine($"Заведение: {expense.Venue}");
                    foreach (var kvp in expense.Expenses)
                        writer.WriteLine($"{kvp.Key} потратил {kvp.Value:F2}");
                    writer.WriteLine($"Оплатил: {expense.Payer}");
                    writer.WriteLine("--------------------------------------------");
                }

                writer.WriteLine("Итоговый расчет долгов:");
                Dictionary<string, double> balances = new Dictionary<string, double>();
                foreach (var expense in Expenses)
                {
                    foreach (var kvp in expense.Expenses)
                    {
                        if (!balances.ContainsKey(kvp.Key)) balances[kvp.Key] = 0;
                        if (!balances.ContainsKey(expense.Payer)) balances[expense.Payer] = 0;

                        balances[kvp.Key] -= kvp.Value;
                        balances[expense.Payer] += kvp.Value;
                    }
                }

                foreach (var kvp in balances.Where(b => b.Value < 0))
                {
                    string debtor = kvp.Key;
                    double amount = -kvp.Value;
                    string creditor = balances.First(b => b.Value > 0).Key;

                    writer.WriteLine($"{debtor} -> {amount:F2} -> {creditor}");
                }
            }
            Console.WriteLine("Результаты сохранены!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения: {ex.Message}");
        }
    }

    static void SaveAndExit()
    {
        Console.WriteLine("Выход из приложения...");
    }
}

class Expense
{
    public string Venue { get; }
    public Dictionary<string, double> Expenses { get; }
    public string Payer { get; }

    public Expense(string venue, Dictionary<string, double> expenses, string payer)
    {
        Venue = venue;
        Expenses = expenses;
        Payer = payer;
    }
}