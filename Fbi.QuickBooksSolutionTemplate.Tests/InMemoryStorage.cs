using System.Collections.Generic;

namespace Fbi.QuickBooksSolutionTemplate
{
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