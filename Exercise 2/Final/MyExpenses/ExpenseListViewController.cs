using Foundation;
using System;
using UIKit;
using MyExpenses.Data;
using System.Collections.Generic;
using System.Diagnostics;

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

        // True when we have a "fake" row at the top for inserting new data.
        bool hasInsertionRow = false;

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            // Set the 1st row as our "INSERT" row.
            if (hasInsertionRow && indexPath.Row == 0)
                return UITableViewCellEditingStyle.Insert;

            return UITableViewCellEditingStyle.Delete;
        }

        public override void WillBeginEditing(UITableView tableView, NSIndexPath indexPath)
        {
            base.WillBeginEditing(tableView, indexPath);

            // Remove the "fake" row if it is present and we are performing a "swipe-to-delete" gesture
            if (hasInsertionRow) {
                hasInsertionRow = false;
                using (indexPath = NSIndexPath.FromRowSection(0, 0)) {
                    TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }
            }
        }

        public override void SetEditing(bool editing, bool animated)
        {
            base.SetEditing(editing, animated);

            // Detect editing mode and either add or remove our fake row.
            using (var indexPath = NSIndexPath.FromRowSection(0, 0)) {
                if (editing) {
                    hasInsertionRow = true;
                    TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);

                } else if (hasInsertionRow) {
                    hasInsertionRow = false;
                    TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }
            }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            // Adjust the index for our data to account for the "fake" row.
            if (hasInsertionRow)
                return expenses.Count + 1;

            return expenses.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(CellIdentifier, indexPath);

            // Adjust the index for our data to account for the "fake" row.
            int row = indexPath.Row;
            if (hasInsertionRow) {
                if (row == 0) {
                    cell.TextLabel.Text = "Add an Expense";
                    cell.DetailTextLabel.Text = string.Empty;
                    cell.TextLabel.TextColor = UIColor.Gray;
                    return cell;
                }
                // All other rows are "pushed" down by one, adjust by subtracting from
                // the row index.
                row--;
            }

            var expense = expenses[row];

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

        public async override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            int row = hasInsertionRow ? indexPath.Row - 1 : indexPath.Row;
            if (editingStyle == UITableViewCellEditingStyle.Delete) {
                var expense = expenses[row];
                expenses.Remove(expense);
                tableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                await new DataStore().Delete(expense);
            }
            // Implement the INSERT logic
            else if (editingStyle == UITableViewCellEditingStyle.Insert) {
                Debug.Assert(indexPath.Row == 0); // should always be zero
                OnAddExpense(this, EventArgs.Empty);
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
