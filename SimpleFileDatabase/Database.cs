using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SimpleFileDatabase
{
    public class Database
    {
        private readonly string _filePath;
        private List<Person> _persons;

        public Database(string filePath)
        {
            _filePath = filePath;
            _persons = Load();
        }

        public void AddPerson(Person person)
        {
            _persons.Add(person);
            Save();
        }

        public List<Person> GetAllPersons()
        {
            return new List<Person>(_persons);
        }

        private List<Person> Load()
        {
            if (!File.Exists(_filePath))
                return new List<Person>();

            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Person>>(json) ?? new List<Person>();
        }

        private void Save()
        {
            string json = JsonSerializer.Serialize(_persons, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}
