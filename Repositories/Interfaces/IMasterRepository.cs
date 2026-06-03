namespace Dia_CoreAPI.Repositories.Interfaces
{
    public interface IMasterRepository
    {
        string GetConnectionString();
        string GetConnectionString(string connectionName);
    }
}
