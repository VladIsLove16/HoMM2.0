using System;

namespace Adventure.Infrastructure.Persistence
{
    public sealed class JsonDataRepository<T> : IDataRepository<T> where T : class
    {
        private readonly IJsonFileStorage _storage;
        private readonly string _fileName;
        private readonly Func<T> _factory;

        public JsonDataRepository(IJsonFileStorage storage, string fileName, Func<T> factory = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name must be provided.", nameof(fileName));

            _fileName = fileName;
            _factory = factory;
        }

        public T Load()
        {
            return _storage.Load<T>(_fileName) ?? _factory?.Invoke();
        }

        public void Save(T data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _storage.Save(_fileName, data);
        }

        public void Delete()
        {
            _storage.Delete(_fileName);
        }
    }
}
