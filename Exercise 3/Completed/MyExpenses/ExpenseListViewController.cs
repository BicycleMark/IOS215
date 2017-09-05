using Foundation;
using System;
using UIKit;
using MyExpenses.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        UITableViewRowAction[ ] editActions;

        public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
        {
            if (editActions == null) {
                editActions = new[] {
                    UITableViewRowAction.Create(UITableViewRowActionStyle.Normal, "Billable", OnFlipBillable),
                    UITableViewRowAction.Create(UITableViewRowActionStyle.Normal, "Not Billable", OnFlipBillable),
                    UITableViewRowAction.Create(UITableViewRowActionStyle.Destructive,"Delete", OnDelete),
                };
                editActions[0].BackgroundColor = UIColor.Blue;
            }

            Expense expense = expenses[indexPath.Row];

            var rowActions = new UITableViewRowAction[2];
            rowActions[0] = (expense.Billable)
                ? editActions[1] : editActions[0];
            rowActions[1] = editActions[2];
            return rowActions;
        }

        async void OnFlipBillable(UITableViewRowAction rowAction, NSIndexPath indexPath)
        {
            Expense expense = expenses[indexPath.Row];
            expense.Billable = !expense.Billable;
            TableView.ReloadRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
            await new DataStore().Update(expense);
        }

        async void OnDelete(UITableViewRowAction rowAction, NSIndexPath indexPath)
        {
            await DeleteExpense(indexPath.Row, indexPath);
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

        Expense newExpense;

        void OnAddExpense(object sender, EventArgs e)
        {
            // Create a new expense.
            newExpense = new Expense();
            PerformSegue("showDetail", this);
        }

        async Task DeleteExpense(int row, NSIndexPath indexPath)
        {
            var expense = expenses[row];
            expenses.Remove(expense);
            TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
            await new DataStore().Delete(expense);
        }

        public async override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            if (editingStyle == UITableViewCellEditingStyle.Delete) {
                await DeleteExpense(indexPath.Row, indexPath);
            }
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
