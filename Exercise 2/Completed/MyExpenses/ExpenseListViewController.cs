using Foundation;
using System;
using UIKit;
using MyExpenses.Data;
using System.Collections.Generic;

namespace MyExpenses
{
	partial class ExpenseListViewController : UITableViewController
	{
        const string CellIdentifier = "ExpenseCell";
        List<Expense> expenses;

		public ExpenseListViewController (IntPtr handle) : base (handle)
		{
		}

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.NavigationItem.RightBarButtonItem = this.EditButtonItem;

            UIBarButtonItem addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add, OnAddExpense);
            this.NavigationItem.LeftBarButtonItem = addButton;

            expenses = new List<Expense>();

            DataStore db = new DataStore();
            expenses.AddRange(await db.LoadExpenses());
            TableView.ReloadData();
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return UITableViewCellEditingStyle.Delete;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return expenses.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(CellIdentifier, indexPath);

            var expense = expenses[indexPath.Row];
            cell.TextLabel.Text = expense.Title;
            cell.DetailTextLabel.Text = expense.Amount.ToString("C");

             if (expense.Billable) {
                cell.TextLabel.TextColor = UIColor.Blue;
            }
            else {
                cell.TextLabel.TextColor = UIColor.Black;
            }

            return cell;
        }

        public async override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            if (editingStyle == UITableViewCellEditingStyle.Delete) {
                var expense = expenses[indexPath.Row];
                expenses.Remove(expense);
                tableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                await new DataStore().Delete(expense);
            }
        }

        Expense newExpense;
        void OnAddExpense(object sender, EventArgs e)
        {
            // Create a new expense.
            newExpense = new Expense();
            PerformSegue("showDetail", this);
        }

        public override void ViewWillAppear(bool animated)
        {
            // If we are adding an expense, and it was saved to the database
            // (e.g. the Id > 0) then add it to our collection and reload the
            // TableView with the new data.
            if (newExpense != null) {
                if (newExpense.Id != 0) {
                    expenses.Add(newExpense);
                    TableView.ReloadData();
                }
                newExpense = null;
            }
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            if (segue.Identifier == "showDetail") {
                var detailViewController = segue.DestinationViewController as ExpenseDetailViewController;
                if (detailViewController != null) {
                    // if we are adding expense.
                    var selectedExpense = newExpense;
                    if (selectedExpense == null)
                        selectedExpense = expenses[TableView.IndexPathForSelectedRow.Row];
                    detailViewController.SelectedExpense = selectedExpense;
                }
            }
        }
	}
}
