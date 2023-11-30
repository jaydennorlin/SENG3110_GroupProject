using AssignmentTracker;
using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using static Godot.Time;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.ComponentModel.Design;
using System.Text.Json;

public partial class SelectTermUI : Control
{
    [Export] Button AddExistingTermButton;
    [Export] Button CreateNewTermButton;
    [Export] Tree termTree;
    TreeItem termTreeRoot;

    List<string> termPathList = new List<string>();
    ConfigFile config;

    public override void _Ready()
	{
        config = new ConfigFile();

        Error error = config.Load("user://termLocationList.cfg");

        if (error == Error.Ok)
        {
            UpdateTermPathListFromConfig();
        }

        UpdateTermTree();

        termTree.CellSelected += TreeOperation;

        CreateNewTermButton.Pressed += CreateNewTermDialog;
        AddExistingTermButton.Pressed += AddExistingTermDialog;
    }

    public void AddExistingTermDialog()
    {
        FileDialog dialog = new FileDialog();
        AddChild(dialog);

        dialog.Size = (Vector2I)(Size * 0.7f);

        dialog.Access = FileDialog.AccessEnum.Filesystem;
        dialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        dialog.AddFilter("*.term");

        dialog.FileSelected += AddNewTerm;//pass file path

        dialog.Popup();
    }

    public void CreateNewTermDialog()
    {
        FileDialog dialog = new FileDialog();
        AddChild(dialog);

        dialog.Size = (Vector2I)(Size * 0.7f);

        dialog.Access = FileDialog.AccessEnum.Filesystem;
        dialog.FileMode = FileDialog.FileModeEnum.SaveFile;
        dialog.AddFilter("*.term");

        dialog.FileSelected += CreateNewTerm;

        dialog.Popup();
    }

    void CreateNewTerm(string path)
    {
        Term newTerm = new Term();
        newTerm.termName = "Unnamed New Term";
        newTerm.startDate = DateTime.Today.AddDays(-1);
        newTerm.endDate = DateTime.Today.AddMonths(4);
        newTerm.directory = path;

        newTerm.SaveTerm();

        //Open TermUI
        AddNewTerm(path);
    }

    void OpenTerm(Term term)
    {
        Node termUI = ResourceLoader.Load<PackedScene>("res://TermUI.tscn").Instantiate();

        GetTree().Root.AddChild(termUI);
        ((Tester)termUI.GetChild(0)).Setup(term);

        CallDeferred("CloseThisScene");
    }

    void CloseThisScene()
    {
        GetTree().Root.RemoveChild(GetTree().Root.GetChild(0));
    }

    void AddNewTerm(string path)
    {
        if (!termPathList.Contains(path))
        {
            termPathList.Add(path);

            UpdateConfigFromTermPathList();

            UpdateTermTree();
        }
    }

    void UpdateTermPathListFromConfig()
    {
        termPathList.Clear();

        foreach (String section in config.GetSections())
        {
            string toAdd = (string)config.GetValue(section, "List");

            if (!termPathList.Contains(toAdd))
            {
                termPathList.Add(toAdd);
            }
        }
    }

    void UpdateConfigFromTermPathList()
    {
        config.Clear();

        for (int i = 0; i < termPathList.Count; i++)
        {
            config.SetValue(i.ToString(), "List", termPathList[i]);
        }

        config.Save("user://termLocationList.cfg");
    }

    void UpdateTermTree()
    {
        GD.Print("Clear Tree");

        //Necessary due to wretched engine fuckery. Possibly an issue elsewhere as well.
        CallDeferred("BuildTermTree");
    }

    void BuildTermTree()
    {
        GD.Print("Build Tree");
        termTree.Clear();
        termTreeRoot = termTree.CreateItem();

        for (int i = 0; i < termPathList.Count; i++)
        {
            TreeItem termTreeItem = termTree.CreateItem(termTreeRoot);
            GD.Print("Null Item " + (termTreeItem == null) + " at " + termPathList[i]);

            if (File.Exists(termPathList[i]))
            {
                termTreeItem.SetText(0, Path.GetFileNameWithoutExtension(termPathList[i]));

                TreeItem open = termTree.CreateItem(termTreeItem);
                open.SetText(0, "Open");

                TreeItem removeFromList = termTree.CreateItem(termTreeItem);
                removeFromList.SetText(0, "Remove From List");

                TreeItem removeFromDisk = termTree.CreateItem(termTreeItem);
                removeFromDisk.SetText(0, "Remove From Disk");

            }
            else
            {
                termTreeItem.SetText(0, "(MISSING) " + Path.GetFileNameWithoutExtension(termPathList[i]));

                TreeItem removeFromList = termTree.CreateItem(termTreeItem);
                removeFromList.SetText(0, "Remove From List");
            }
        }
    }

    void TreeOperation()
    {
        TreeItem selectedItem = termTree.GetSelected();

        if(selectedItem.GetParent() != termTreeRoot)
        {
            GD.Print("Operation on index " + selectedItem.GetParent().GetIndex() + ", " + termPathList[selectedItem.GetParent().GetIndex()]);
            // If this item is the only item childed, then it must be childed to a MISSING item.
            if (selectedItem.GetParent().GetChildCount() == 1)
            {
                termPathList.RemoveAt(selectedItem.GetParent().GetIndex());
            }
            else
            {
                if(selectedItem.GetIndex() == 0)
                {
                    //Open Item
                    OpenTerm(Term.LoadTerm(termPathList[selectedItem.GetParent().GetIndex()]));
                }
                else if (selectedItem.GetIndex() == 1)
                {
                    //Remove from List
                    termPathList.RemoveAt(selectedItem.GetParent().GetIndex());
                }
                else
                {
                    //Remove from Disk
                    //ADD CONFIRMATION DIALOGUE TO THIS
                    File.Delete(termPathList[selectedItem.GetParent().GetIndex()]);
                    termPathList.RemoveAt(selectedItem.GetParent().GetIndex());
                }
            }

            selectedItem = null;

            UpdateConfigFromTermPathList();
            UpdateTermTree();
        }
    }
}