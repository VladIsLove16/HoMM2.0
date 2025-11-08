using System;
using System.IO;
using UnityEngine;

namespace Adventure.Infrastructure.Persistence
{
    public sealed class JsonFileStorage : IJsonFileStorage
    {
        private readonly string _rootDirectory;

        public JsonFileStorage()
        {
            _rootDirectory = Path.Combine(Application.persistentDataPath, "Saves");
            Directory.CreateDirectory(_rootDirectory);
        }

        public bool Exists(string fileName)
        {
            return File.Exists(ResolvePath(fileName));
        }

        public void Delete(string fileName)
        {
            var path = ResolvePath(fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public T Load<T>(string fileName) where T : class
        {
            var path = ResolvePath(fileName);
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JsonFileStorage] Failed to deserialize '{fileName}': {ex}");
                return null;
            }
        }

        public void Save<T>(string fileName, T data) where T : class
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var json = JsonUtility.ToJson(data, prettyPrint: true);
            var path = ResolvePath(fileName);
            File.WriteAllText(path, json);
        }

        private string ResolvePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name must be provided.", nameof(fileName));

            return Path.Combine(_rootDirectory, $"{fileName}.json");
        }
    }
}
