namespace WebApp.Models.Repositories;
public interface IRepository<T> where T : IObject
{
    Task<IEnumerable<T>> GetAll();
    Task<T> Get(int id);
    Task<String> Add(T obj);
    Task<String> Update(T obj);
    Task<String> Delete(int id);
}