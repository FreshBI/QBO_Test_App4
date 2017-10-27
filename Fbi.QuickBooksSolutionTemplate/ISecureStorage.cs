namespace Fbi.QuickBooksSolutionTemplate
{
  public interface ISecureStorage
  {
    void Store<T>(string key, T itemToStore);
    T Retrieve<T>(string key);
  }
}