using PrinterSQLiteApp.Domain.Entities;

namespace PrinterSQLiteApp.Domain.Interfaces
{
    public interface ITransactionRepository
    {
        List<Transaction> GetTransactionsByIdApi(int idApi);
    }
}
