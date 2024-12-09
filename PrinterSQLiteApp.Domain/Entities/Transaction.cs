namespace PrinterSQLiteApp.Domain.Entities
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int IdApi { get; set; }
        public string? Document { get; set; }
        public string? Reference { get; set; }
        public string? Product { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RealAmount { get; set; }
        public decimal IncomeAmount { get; set; }
        public decimal ReturnAmount { get; set; }
        public string? Description { get; set; }
        public int IdStateTransaction { get; set; }
        public string? StateTransaction { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
