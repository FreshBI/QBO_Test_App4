using System.Collections.Generic;

namespace QuickBooksIntegration.Tests
{
  interface ISecureStorage
  {
    void Store<T>(string key, T itemToStore);
    T Retrieve<T>(string key);
  }

  class InMemoryStorage : ISecureStorage
  {
    readonly Dictionary<string, object> store = new Dictionary<string, object>();

    public T Retrieve<T>(string key)
    {
      return (T)store[key];
    }

    public void Store<T>(string key, T itemToStore)
    {
      store[key] = itemToStore;
    }
  }
}