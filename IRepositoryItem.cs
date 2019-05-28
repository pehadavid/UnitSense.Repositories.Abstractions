namespace UnitSense.Repositories.Abstractions
{
    /// <summary>
    /// Define an item that can be updatable with EF Core. 
    /// </summary>

    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TSecondaryKey"></typeparam>
    public interface IRepositoryItem
    {
        void UpdateFrom(object source);
    }
  
}
