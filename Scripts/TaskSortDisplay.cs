using AssignmentTracker;
using Godot;
using System;

public partial class TaskSortDisplay : Button
{
	[Export] Tester mainUI;
	[Export] Button button;
    [Export] RichTextLabel taskNameLabel;
	[Export] RichTextLabel taskValueLabel;
    [Export] RichTextLabel taskDueDateLabel;

	public PrimaryTask task;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Pressed += OpenTask;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/*if (Visible)
		{
			UpdateDisplay();
        }*/
    }

	void OpenTask()
	{
		mainUI.AddTaskTab(task);
    }

	//Necessary due to duplicate not copying references to children, for some unfathomable reason.
	public void Setup()
	{
		taskNameLabel = (RichTextLabel)GetChild(0).GetChild(0);
        taskValueLabel = (RichTextLabel)GetChild(0).GetChild(1);
        taskDueDateLabel = (RichTextLabel)GetChild(0).GetChild(2);
    }

	public void UpdateDisplay()
	{
		taskNameLabel.Text = task.taskName;
        taskValueLabel.Text = "Value: " + task.value;
		//IMPROVE LATER
		taskDueDateLabel.Text = task.dueDate.Date.ToString("ddd") + " " + task.dueDate.ToShortTimeString() + ", " + task.dueDate.ToString("m") + " ";
    }
}
