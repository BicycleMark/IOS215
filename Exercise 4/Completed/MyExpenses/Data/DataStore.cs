using System;
using System.Collections.Generic;
using SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Net;

namespace MyExpenses.Data
{
    public class DataStore
    {
        const string DbFilename = "expenses.db3";
        SQLiteAsyncConnection db;

        async Task Initialize()
        {
            string filename = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "..", "Library", DbFilename);
            db = new SQLiteAsyncConnection(filename);
            await db.CreateTableAsync<Expense>();
        }

        public async Task<IList<Expense>> LoadExpenses()
        {
            if (db == null) {
                await Initialize();
            }

            var items = await db.Table<Expense>().ToListAsync();
            if (!items.Any()) {
                items.AddRange(InitExpenses());
                await db.InsertAllAsync(items);
            }

            return items;
        }

        public async Task Update(Expense expense)
        {
            if (db == null) {
                await Initialize();
            }

            if (expense.Id == 0) {
                await db.InsertAsync(expense);
            } else {
                await db.UpdateAsync(expense);
            }
        }

        public async Task Update(IEnumerable<Expense> input)
        {
            if (db == null) {
                await Initialize();
            }

            var expenses = input.ToList();
            await db.RunInTransactionAsync(conn => {
                conn.UpdateAll(expenses.Where(e => e.Id > 0));
                conn.InsertAll(expenses.Where(e => e.Id == 0));
            });
        }

        public async Task Delete(Expense expense)
        {
            if (db == null) {
                await Initialize();
            }
            await db.DeleteAsync(expense);
        }

        public async Task<IList<Expense>> Reset()
        {
            if (db == null) {
                await Initialize();
            }

            await db.DropTableAsync<Expense>();
            await db.CreateTableAsync<Expense>();

            var items = InitExpenses().ToList();
            await db.InsertAllAsync(items);

            return items;
        }

        private IEnumerable<Expense> InitExpenses()
        {
            yield return (new Expense {
                Billable = false,
                Category = "Transportation",
                Amount = 1200,
                Title = "Flight"
            });
            yield return (new Expense {
                Billable = false,
                Category = "Transportation",
                Amount = 40,
                Title = "Taxi From Airport"
            });
            yield return (new Expense {
                Billable = false,
                Category = "Transportation",
                Amount = 30,
                Title = "Taxi to Dinner"
            });
            yield return (new Expense {
                Billable = false,
                Category = "Meal",
                Amount = 22.43,
                Title = "Meal 1st night"
            });
            yield return (new Expense {
                Billable = false,
                Category = "Lodging",
                Amount = 123.00,
                Title = "Hotel 1 Night"
            });
        }

    }
}

