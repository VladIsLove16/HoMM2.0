using System;

namespace Adventure.Infrastructure.Persistence
{
    public interface IJsonFileStorage
    {
        T Load<T>(string fileName) where T : class;
        void Save<T>(string fileName, T data) where T : class;
        bool Exists(string fileName);
        void Delete(string fileName);
    }
}
