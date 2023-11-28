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

public partial class SelectTermUI : Control
{
    [Export] Button createButton;
    [Export] Button openButton;
	// Called when the node enters the scene tree for the first time.
	[Export] VBoxContainer termListBox;


	//List to store File location with term info.
	List<TermLocationData> termLocationList = new List<TermLocationData>();


    ///Open new File Group
    /// 
    /// 
    /// <summary>
    /// Open File dialog, 
    /// Credit to Jayden here. Keishi Modified and implemented.
    /// </summary>
    public void openFolderDialog()
    {
        FileDialog dialog = new FileDialog();
        AddChild(dialog);

        dialog.Size = (Vector2I)(Size * 0.7f);

        dialog.Access = FileDialog.AccessEnum.Filesystem;
        dialog.FileMode = FileDialog.FileModeEnum.OpenDir;//open folder mode

        dialog.DirSelected += termListBoxUpdater;//pass folder path

        dialog.Popup();
    }
    /// <summary>
    /// Update List Box or the vBOX having terms.
    /// </summary>
    void termListBoxUpdater(string path)
	{
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
        }

    }


    void termListBoxClear()
    {
        //clear VBoxCOntainer
        //Chiled may not be removed from here.
        foreach (var childrenNodes in termListBox.GetChildren())
        {
            termListBox.RemoveChild(childrenNodes);
        }
    }


    /// <summary>
    /// Sort termLocationList 
    /// </summary>
    void sortTermInfo()
    {
        int countList = termLocationList.Count;
        GD.Print(countList);
        int newI;//updated location of selected node
        int j;//current node to compare

        if(countList > 2)
        {
            for (int i = 1; i < countList; i++)//check from 2nd one from array.
            {

                newI = i;//node i change the location, so set it as different variable.
                j = i - 1;
                GD.Print("i "+ i + " is " + termLocationList[newI].lastModifiedDate);
                GD.Print("j is" + termLocationList[j].lastModifiedDate);


                //GD.Print("Load: " + termLocationList[newI].lastModifiedDate);//debug
                while (j >= 0 && termLocationList[newI].lastModifiedDate > termLocationList[j].lastModifiedDate)//do loop while current compareson location j become 0 or   
                {
                    //swap them
                    GD.Print("Change happen with " + termLocationList[newI].termName + newI + " and" + termLocationList[j].termName + j);
                    TermLocationData temp = new TermLocationData();
                    temp = termLocationList[j];
                    termLocationList[j] = termLocationList[newI];
                    termLocationList[newI] = temp;
                    
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
    public void createFolderDialog()
    {
        FileDialog dialog = new FileDialog();
        AddChild(dialog);

        dialog.Size = (Vector2I)(Size * 0.7f);

        dialog.Access = FileDialog.AccessEnum.Filesystem;
        dialog.FileMode = FileDialog.FileModeEnum.OpenDir;//open folder mode


        dialog.DirSelected += createNewTerm;

        dialog.Popup();
    }

    void createNewTerm(string path)
    {
        //Create Json File


        //Open TermUI
    }



    ///Main Group//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This would be only done onece
    /// </summary>
    public override void _Ready()
	{
        termListBoxClear();
        openButton.Pressed += openFolderDialog;
        createButton.Pressed += createFolderDialog;
        GD.Print("Ready!");
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
public class TermLocationData {
	public DateTime lastModifiedDate;
	public string termName;
    public string termFilePath;
    //public Node nodePath;//Maybe need.
}
