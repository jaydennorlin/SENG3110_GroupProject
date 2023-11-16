using Godot;
using System;

public partial class LabelEdit : RichTextLabel
{
	[Export] Control displayUI;
	[Export] Control editUI;
    [Export] TextEdit textEdit;
    //If possible, replace save button with unhandled Input handler that grabs the Enter key.
    [Export] Button saveButton;
    [Export] Button cancelButton;

    //I do not fully understand this Syntax yet, but it works.
    public event Action EditComplete = delegate { };

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        FocusEntered += EditStart;
        saveButton.Pressed += EditStop;
        cancelButton.Pressed += CancelEdit;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    void EditStart()
    {
        displayUI.Hide();
        editUI.Show();

        textEdit.Text = Text;
    }

    //These three are poorly thought out. Fix later.
    public void CancelEdit()
    {
        displayUI.Show();
        editUI.Hide();
    }

    void EditStop()
    {
        displayUI.Show();
        editUI.Hide();

        Text = textEdit.Text;

        EditComplete.Invoke();
    }
}
