using AssignmentTracker;
using Godot;
using System;

public partial class TaskSortDisplay : Control
{
	[Export] RichTextLabel taskNameLabel;
	[Export] RichTextLabel taskValueLabel;
    [Export] RichTextLabel taskDueDateLabel;

	public PrimaryTask task;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void UpdateDisplay()
	{
		taskNameLabel.Text = task.taskName;
        taskValueLabel.Text = "Value: " + task.value;
		//IMPROVE LATER
        taskDueDateLabel.Text = task.dueDate.ToString();
    }
}
