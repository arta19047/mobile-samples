using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using Tasky.AL;
using Tasky.BL;

namespace Tasky.Screens.iPhone.Home {
	public class controller_iPhone : DialogViewController {
		List<Task> tasks;
		
		public controller_iPhone () : base (UITableViewStyle.Plain, null)
		{
			Initialize ();
		}
		
		protected void Initialize()
		{
			NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Add), false);
			NavigationItem.RightBarButtonItem.Clicked += (sender, e) => { ShowTaskDetails(new Task()); };
		}
		

		// MonoTouch.Dialog individual TaskDetails view (uses /AL/TaskDialog.cs wrapper class)
		BindingContext context;
		TaskDialog taskDialog;
		Task currentTask;
		DialogViewController detailsScreen;
		protected void ShowTaskDetails(Task task)
		{
			currentTask = task;
			taskDialog = new TaskDialog (task);
			context = new BindingContext (this, taskDialog, "Task Details");
			detailsScreen = new DialogViewController (context.Root, true);
			ActivateController(detailsScreen);
		}
		public void SaveTask()
		{
			context.Fetch (); // re-populates with updated values
			currentTask.Name = taskDialog.Name;
			currentTask.Notes = taskDialog.Notes;
			BL.Managers.TaskManager.SaveTask(currentTask);
			NavigationController.PopViewControllerAnimated (true);
			context.Dispose (); // per documentation
		}
		public void DeleteTask ()
		{
			if (currentTask.ID >= 0)
				BL.Managers.TaskManager.DeleteTask (currentTask.ID);
			NavigationController.PopViewControllerAnimated (true);
		}



		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			
			// reload/refresh
			PopulateTable();			
		}
		
		protected void PopulateTable()
		{
			tasks = BL.Managers.TaskManager.GetTasks().ToList ();
			Root = new RootElement("Tasky") {
				new Section() {
					from t in tasks
					select (Element) new StringElement((t.Name==""?"<new task>":t.Name), t.Notes)
				}
			}; 
		}
		public override void Selected (MonoTouch.Foundation.NSIndexPath indexPath)
		{
			var task = tasks[indexPath.Row];
			ShowTaskDetails(task);
		}
		public override Source CreateSizingSource (bool unevenRows)
		{
			return new EditingSource (this);
		}
		public void DeleteTaskRow(int rowId)
		{
			BL.Managers.TaskManager.DeleteTask(tasks[rowId].ID);
		}
	}
}