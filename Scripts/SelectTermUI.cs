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

// Jayden:
// I have made several changes to this, but in the process it's become quite messy and bloated.
// Should be cleaned up/refactored at some point to be more logical and concise.
public partial class SelectTermUI : Control
{
    [Export] Button createButton;
    [Export] Button openButton;
	// Called when the node enters the scene tree for the first time.
	[Export] Tree termListTree;
    TreeItem termListRoot;


	//List to store File location with term info.
    List<string> termLocationList = new List<string>();
    List<TermDetails> termDetailList = new List<TermDetails>();

    //Used to save the TermLocationList in a persistent directory.
    ConfigFile termLocationConfigFile = new ConfigFile();

    ///Open new File Group
    /// 
    /// 
    /// <summary>
    /// Open File dialog, 
    /// Credit to Jayden here. Keishi Modified and implemented.
    /// Credit to Keishi here. Jayden Further Modified and implemented.
    /// </summary>
    public void OpenExistingTermDialog()
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

    void AddNewTerm(string path)
    {
        AddNewTerm(path, true);
    }

    void AddNewTerm(string path, bool updateTermListBox, bool addToConfig = true)
    {
        if (!termLocationList.Contains(path))
        {
            termLocationList.Add(path);

            TermDetails newTermData = new TermDetails();
            newTermData.termName = Path.GetFileName(path);//get fine name
            newTermData.termFilePath = path;//get file's abusolute path
            newTermData.lastModifiedDate = File.GetLastWriteTime(path);//get last modifiedd date
            //GD.Print("Load: " + newTermData.termName + " " + newTermData.lastModifiedDate);//Debug service

            termDetailList.Add(newTermData);//add it to list

            if (addToConfig)
            {
                termLocationConfigFile.SetValue(termDetailList.Count.ToString(), "List", path);
                termLocationConfigFile.Save("user://termLocationList.cfg");
            }
        }

        if (updateTermListBox)
        {
            TermListBoxUpdater();
        }
    }

    /// <summary>
    /// Update List Box or the vBOX having terms.
    /// Created by Keishi, modified by Jayden
    /// </summary>
    /// 
    void TermListBoxUpdater()
	{
        SortTermInfo();

        UpdateTermListBox();

        /*
        termLocationList.Clear();//Clear the List
        GD.Print("Folder path is " + path);
        string[] files = Directory.GetFiles(@path, "*.json");
        //Limitation, if there is any json file not relating in here, it could ccause problem.

        if (files != null)//Do it if file are here, may be not nesessary. I dobt this is not working properly, but system does not have problem. 
        {

            foreach (string file in files)
            {
                TermLocationData newTermData = new TermLocationData();
                newTermData.termName = Path.GetFileName(file);//get fine name
                newTermData.termFilePath = file;//get file's abusolute path
                newTermData.lastModifiedDate = File.GetLastWriteTime(file);//get last modifiedd date
                GD.Print("Load: " + newTermData.termName + " " + newTermData.lastModifiedDate);//Debug service
                termLocationList.Add(newTermData);//add it to list
                
            }

            sortTermInfo();//sort the List.

            termListBoxClear();//clear the List Box.

            //And add things to list box
            foreach (var termInfo in termLocationList)
            {
                GD.Print("ADD Buttons: " + termInfo.termName + " " + termInfo.lastModifiedDate);//debug service
                Button button = new Button();//create new button
                button.Text = termInfo.termName + "\n" + termInfo.lastModifiedDate;
                //I could not find the way to add child
                termListBox.AddChild(button);
            }
        }
        else
        {

            //May be you can add pop up of "There is no json file!" or something
            GD.Print("No json file");
        }*/

    }

    void UpdateTermListBox()
    {
        termListTree.Clear();
        termListRoot = null;

        termListRoot = termListTree.CreateItem();

        foreach (TermDetails termDetails in termDetailList)
        {
            TreeItem termListItem = termListTree.CreateItem(termListRoot);
            termListItem.SetText(0, termDetails.termName);
        }
    }


    /// <summary>
    /// Sort termLocationList 
    /// </summary>
    void SortTermInfo()
    {
        int countList = termDetailList.Count;
        //GD.Print(countList);
        int newI;//updated location of selected node
        int j;//current node to compare

        if(countList > 2)
        {
            for (int i = 1; i < countList; i++)//check from 2nd one from array.
            {

                newI = i;//node i change the location, so set it as different variable.
                j = i - 1;
                //GD.Print("i "+ i + " is " + termDetailList[newI].lastModifiedDate);
                //GD.Print("j is" + termDetailList[j].lastModifiedDate);


                //GD.Print("Load: " + termLocationList[newI].lastModifiedDate);//debug
                while (j >= 0 && termDetailList[newI].lastModifiedDate > termDetailList[j].lastModifiedDate)//do loop while current compareson location j become 0 or   
                {
                    //swap them
                    //GD.Print("Change happen with " + termDetailList[newI].termName + newI + " and" + termDetailList[j].termName + j);
                    TermDetails temp = new TermDetails();
                    temp = termDetailList[j];
                    termDetailList[j] = termDetailList[newI];
                    termDetailList[newI] = temp;
                    
                    newI = j;//input new location of node i.
                    j--;
                }
            }
        }
        
        return;
    }

    ///Create New Folder Group
    /// 
    /// <summary>
    /// 
    /// </summary>
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
        OpenTermUI(newTerm);
    }

    void OpenTermUI()
    {
        Term toOpen = Term.LoadTerm(termDetailList[termListTree.GetSelected().GetIndex()].termFilePath);
        OpenTermUI(toOpen);
    }

    void OpenTermUI(Term term)
    {
        Node termUI = ResourceLoader.Load<PackedScene>("res://TermUI.tscn").Instantiate();

        //GD.Print((termUI.GetChild(0).GetScript()).GetType().ToString());

        GetTree().Root.AddChild(termUI);
        GetTree().Root.RemoveChild(GetTree().Root.GetChild(0));
        //this.Free();
    }

    ///Main Group//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This would be only done onece
    /// </summary>
    public override void _Ready()
	{
        //Can't be bothered to find a
        Error error = termLocationConfigFile.Load("user://termLocationList.cfg");

        if(error == Error.Ok)
        {
            foreach (String path in termLocationConfigFile.GetSections())
            {
                // Fetch the data for each section.
                //ADD CATCH IF FILE HAS BEEN DELETED
                AddNewTerm((String)termLocationConfigFile.GetValue(path, "List"), false, false);
            }
        }

        UpdateTermListBox();
        openButton.Pressed += OpenExistingTermDialog;
        createButton.Pressed += CreateNewTermDialog;
        termListTree.CellSelected += OpenTermUI;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}


///class here
/// <summary>
/// Class to save the file.
/// </summary>
public class TermDetails {
	public DateTime lastModifiedDate;
	public string termName;
    public string termFilePath;
    //public Node nodePath;//Maybe need.
}
