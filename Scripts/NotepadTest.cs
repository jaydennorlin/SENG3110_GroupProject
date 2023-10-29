using Godot;
using System;
using System.IO;

// Most Godot scripts inherit from "Node", which is a generic object that exists within the game.
// This script inherits from a "Control" node, which is the base node type used for UI.
public partial class NotepadTest : Control
{
    // The [Export] attribute tells Godot that this variable should be exposed in the Editor.
    [Export] Button openFileButton;
    [Export] Button saveFileButton;

	[Export] TextEdit textArea;

    string currentPath;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		// Connecting the methods() to the "Button Pressed" events.
		openFileButton.Pressed += OpenFileDialog;
		saveFileButton.Pressed += SaveFileDialog;

		GD.Print("Notepad is ready.");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	// We are unlikely to use this method at all, as most of what we are doing will be driven by user interaction events.
	public override void _Process(double delta)
	{

	}

	public void OpenFileDialog()
	{
        FileDialog dialog = new FileDialog();
		AddChild(dialog);

        dialog.Size = (Vector2I)(Size * 0.7f);

        dialog.Access = FileDialog.AccessEnum.Filesystem;
        dialog.FileMode = FileDialog.FileModeEnum.OpenFile;

        dialog.FileSelected += OpenFile;

        dialog.Popup();
    }

    void OpenFile(string path)
    {
        currentPath = path;

        GD.Print("Reading from " + currentPath);
        textArea.Text = File.ReadAllText(path);
    }

	public void SaveFileDialog()
	{
        FileDialog dialog = new FileDialog();
        AddChild(dialog);

        dialog.Size = (Vector2I)(Size * 0.7f);

        dialog.Access = FileDialog.AccessEnum.Filesystem;
        dialog.FileMode = FileDialog.FileModeEnum.SaveFile;
        dialog.CurrentPath = currentPath;

        dialog.FileSelected += SaveFile;

        dialog.Popup();
    }

    void SaveFile(string path)
    {
        GD.Print("Saving to " + path);
        File.WriteAllText(path, textArea.Text);
    }
}
