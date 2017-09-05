using SQLite;

namespace MyExpenses.Data
{
    public class Expense
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public bool Billable { get; set; }
        public string Category { get; set; }
        public double Amount { get; set; }
    }
}

