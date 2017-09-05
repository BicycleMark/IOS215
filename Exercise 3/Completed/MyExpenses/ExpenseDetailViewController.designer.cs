// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace MyExpenses
{
	[Register ("ExpenseDetailViewController")]
	partial class ExpenseDetailViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UISwitch billableSwitch { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField categoryText { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField expenseAmount { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField expenseName { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIActivityIndicatorView isBusyIndicator { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton saveButton { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (billableSwitch != null) {
				billableSwitch.Dispose ();
				billableSwitch = null;
			}
			if (categoryText != null) {
				categoryText.Dispose ();
				categoryText = null;
			}
			if (expenseAmount != null) {
				expenseAmount.Dispose ();
				expenseAmount = null;
			}
			if (expenseName != null) {
				expenseName.Dispose ();
				expenseName = null;
			}
			if (isBusyIndicator != null) {
				isBusyIndicator.Dispose ();
				isBusyIndicator = null;
			}
			if (saveButton != null) {
				saveButton.Dispose ();
				saveButton = null;
			}
		}
	}
}
