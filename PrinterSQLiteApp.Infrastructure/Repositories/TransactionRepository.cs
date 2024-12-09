using System.Data.SQLite;
using PrinterSQLiteApp.Domain.Entities;
using PrinterSQLiteApp.Domain.Interfaces;

namespace PrinterSQLiteApp.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly string _connectionString;

        public TransactionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Transaction> GetTransactionsByIdApi(int idApi)
        {
            var transactions = new List<Transaction>();
            
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "SELECT * FROM \"Transaction\" WHERE IdApi = @idApi", 
                    connection);
                
                command.Parameters.AddWithValue("@idApi", idApi);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            TransactionId = Convert.ToInt32(reader["TransactionId"]),
                            IdApi = Convert.ToInt32(reader["IdApi"]),
                            Document = reader["Document"].ToString(),
                            Reference = reader["Reference"].ToString(),
                            Product = reader["Product"].ToString(),
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                            RealAmount = Convert.ToDecimal(reader["RealAmount"]),
                            IncomeAmount = Convert.ToDecimal(reader["IncomeAmount"]),
                            ReturnAmount = Convert.ToDecimal(reader["ReturnAmount"]),
                            Description = reader["Description"].ToString(),
                            IdStateTransaction = Convert.ToInt32(reader["IdStateTransaction"]),
                            StateTransaction = reader["StateTransaction"].ToString(),
                            DateCreated = Convert.ToDateTime(reader["DateCreated"]),
                            DateUpdated = Convert.ToDateTime(reader["DateUpdated"])
                        });
                    }
                }
            }

            return transactions;
        }
    }
}
