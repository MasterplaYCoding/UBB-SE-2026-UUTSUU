using System.Collections.Generic;

namespace SearchAndBook.Repositories;

public interface IRepository<T>
{
    T? Get(int id);
    List<T> GetAll();
}