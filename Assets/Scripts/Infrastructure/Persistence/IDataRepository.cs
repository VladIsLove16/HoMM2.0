using System;

namespace Adventure.Infrastructure.Persistence
{
    public interface IDataRepository<T> where T : class
    {
        T Load();
        void Save(T data);
        void Delete();
    }
}
