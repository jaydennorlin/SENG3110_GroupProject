using AssignmentTracker;
using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using static Godot.Time;
using System.Linq;

public partial class SelectTermUI : Control
{
    [Export] Button createButton;
    [Export] Button OpenButton;
	// Called when the node enters the scene tree for the first time.
	[Export] VBoxContainer termListBox;
	[Export] FileDialog openFileDialog;

	//List to store File location with term info.
	List<TermLocationData> termLocationList;
	
	/// <summary>
	/// UpdateI Informations
	/// </summary>
	void updateRecentInfo()
	{
        foreach (var termInfo in termLocationList)
		{

			//Delete from List if it does not exist
			if (File.Exists(termInfo.filelocation))
			{
				termLocationList.Remove(termInfo);
			}
		}


		//Do sorting Here
		sortTermInfo();

    }


    /// <summary>
    /// Sort termLocationList 
    /// </summary>
    void sortTermInfo()
	{
        int countList = termLocationList.Count;
		int newI;//updated location of selected node
		int j;//current node to compare


        for (int i = 1; i <= countList; i++)//do it for size of array -1;
        {
			
			newI = i;//node i change the location, so set it as different variable.
            j = i - 1;
            while (termLocationList[newI].lastModifiedDate > termLocationList[j].lastModifiedDate && j>= 0)//do loop while current compareson location j become 0 or   
			{
				//swap them
                TermLocationData temp = termLocationList[j];
                termLocationList[j] = termLocationList[newI];
                termLocationList[newI] = temp;

                newI = j;//input new location of node i.
				j--;
            }
        }


        return;
	}


	void saveRecentInfo() 
	{
		
	}


	/// <summary>
	/// Update List Box
	/// </summary>
	void termListBoxClear()
	{
        //clear VBoxCOntainer
        //Chiled may not be removed. I need to some felp for it.
        foreach (var childrenNodes in termListBox.GetChildren())
        {
            termListBox.RemoveChild(childrenNodes);
        }

        foreach (var termInfo in termLocationList)
        {

            Button button = new Button();
            button.Text = termInfo.termName + "/n" + termInfo.filelocation;
            //I could not find the way to add child
            termListBox.AddChild(button);
        }

    }

	/// <summary>
	/// When ever the application close, they need to 
	/// </summary>
	void closingApplication()
	{

	}

	
	public override void _Ready()
	{
        
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}


/// <summary>
/// Class to save the file
/// </summary>
public class TermLocationData {
	public DateTime lastModifiedDate;
	public string termName;
	public string filelocation;
}
