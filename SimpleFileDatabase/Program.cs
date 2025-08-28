using System;
using System.Collections.Generic;

namespace SimpleFileDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new Database("data.json");

            Console.WriteLine("== Simpele File Database ==");

            while (true)
            {
                Console.WriteLine("\nKies een optie:");
                Console.WriteLine("1. Voeg persoon toe");
                Console.WriteLine("2. Toon alle personen");
                Console.WriteLine("3. Stop");

                var keuze = Console.ReadLine();

                if (keuze == "1")
                {
                    Console.Write("Naam: ");
                    string naam = Console.ReadLine() ?? "";

                    Console.Write("Leeftijd: ");
                    int leeftijd = int.Parse(Console.ReadLine() ?? "0");

                    db.AddPerson(new Person { Name = naam, Age = leeftijd });
                    Console.WriteLine("✅ Persoon opgeslagen!");
                }
                else if (keuze == "2")
                {
                    var personen = db.GetAllPersons();
                    Console.WriteLine("\n--- Personen in database ---");
                    foreach (var p in personen)
                    {
                        Console.WriteLine($"{p.Name}, {p.Age} jaar");
                    }
                }
                else if (keuze == "3")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("❌ Ongeldige keuze.");
                }
            }
        }
    }
}
