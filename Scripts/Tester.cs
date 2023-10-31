using AssignmentTracker;
using Godot;
using System;
using System.Collections.Generic;
using System.Data;

public partial class Tester : Node
{
	[ExportGroup("UI")]
	[Export] TabBar tabBar;

	[ExportGroup("Term Fields")]
	[Export] RichTextLabel termNameLabel;

	[ExportGroup("Course Fields")]
	[Export] Control courseParentNode;
    [Export] RichTextLabel courseNameLabel;
    [Export] TextEdit courseInfoArea;

    [ExportGroup("Task Fields")]
    [Export] Control taskParentNode;
	[Export] RichTextLabel taskInfoLabel;
	[Export] TextEdit taskDescriptionArea;

	Term activeTerm;

	//Tabs can either be a Course or a Task.
	struct TabItem
	{
		public bool isTask;

        public PrimaryTask task;
        public Course course;

		public TabItem(bool isTask, PrimaryTask task, Course course)
		{
			this.isTask = isTask;
			this.task = task;
			this.course = course;
		}
	}

	TabItem[] openTabs;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		activeTerm = new Term();
		activeTerm.termName = "Example Term 20XX";

		Course exampleCourse = new Course("EXCO 1100", "Example data wabababababababababababababababa");
		exampleCourse.tasks.Add(new PrimaryTask("Example Task", exampleCourse));
		exampleCourse.tasks.Add(new PrimaryTask("Example Task 2", exampleCourse));
		exampleCourse.tasks.Add(new PrimaryTask("Example Task 3", exampleCourse));

        activeTerm.courses.Add(exampleCourse);

        exampleCourse = new Course("EXCO 2200", "Example data wibibibibibibibibibibibibibibibi");
        exampleCourse.tasks.Add(new PrimaryTask("Example Task 7", exampleCourse));
        exampleCourse.tasks.Add(new PrimaryTask("Example Task 700", exampleCourse));
        exampleCourse.tasks.Add(new PrimaryTask("Example Mission", exampleCourse));

        activeTerm.courses.Add(exampleCourse);

		UpdateTermDisplay();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public void UpdateTermDisplay()
	{
		//Formatting makes it larger, bold, and centered.
		termNameLabel.Text = "[font_size=20][b][center]" + activeTerm.termName;

		List<TabItem> allTabs = new List<TabItem>();

		for (int i = 0; i < activeTerm.courses.Count; i++)
		{
			//Generate the folder structure for each course.

			//Temporary solution: Open all tabs.
			allTabs.Add(new TabItem(false, null, activeTerm.courses[i]));

			for (int j = 0; j < activeTerm.courses[i].tasks.Count; j++)
			{
                allTabs.Add(new TabItem(true, activeTerm.courses[i].tasks[j], null));
            }
        }

		openTabs = allTabs.ToArray();

		tabBar.ClearTabs();

		for (int i = 0; i < allTabs.Count; i++)
		{
			if (allTabs[i].isTask)
			{
				tabBar.AddTab(allTabs[i].task.taskName);
            }
			else
			{
                tabBar.AddTab(allTabs[i].course.courseName);
            }
        }

        tabBar.TabChanged += UpdateMainDisplay;

		UpdateMainDisplay(0);
    }

	//No idea why, but Godot always seems to use the largest datatype available, even when it's totally unreasonable to.
	//Like in this case, `long tab` refers to the index of the active tab being selected.
	//Who is ever going to need 9,223,372,036,854,775,807 tabs?
	private void UpdateMainDisplay(long selectedTab)
    {
		if (openTabs[selectedTab].isTask)
		{
			courseParentNode.Visible = false;
            taskParentNode.Visible = true;

            UpdateTaskDisplay(openTabs[selectedTab].task);
		}
		else
		{
            courseParentNode.Visible = true;
            taskParentNode.Visible = false;

            UpdateCourseDisplay(openTabs[selectedTab].course);
        }
    }

	public void UpdateCourseDisplay(Course course)
	{
		courseNameLabel.Text = course.courseName;

		//Rework this later to use the entire array. Or change it to just be a single string.
		courseInfoArea.Text = course.additonalData[0];
	}


	public void UpdateTaskDisplay(PrimaryTask task)
	{
		//Don't forget to add Due Date to this line.
		taskInfoLabel.Text = "[b]" + task.taskName + "[\\b]\n" + task.course.courseName + "\n";

		taskDescriptionArea.Text = task.taskDescription;
	}
}
