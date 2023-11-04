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

	[Export] Tree coursesTree;
    TreeItem treeRoot;

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

	List<TabItem> openTabs = new List<TabItem>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        #region Generation of Testing Data. 
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
        #endregion
        //Load selected term here

        //Formatting makes it larger, bold, and centered.
        termNameLabel.Text = "[font_size=20][b][center]" + activeTerm.termName;

        for (int i = 0; i < activeTerm.courses.Count; i++)
		{
			AddCourseToUI(activeTerm.courses[i]);
        }

		coursesTree.CellSelected += AddTabFromTree;

        tabBar.TabChanged += UpdateMainDisplay;
		tabBar.TabClosePressed += TabRemoved;

        UpdateOpenTabs();
    }

    #region Initialization
    public void AddCourseToUI(Course course)
    {
        if (treeRoot == null)
        {
            treeRoot = coursesTree.CreateItem();
        }

        TreeItem courseTreeItem = coursesTree.CreateItem(treeRoot);
        courseTreeItem.SetText(0, course.courseName);

        for (int i = 0; i < course.tasks.Count; i++)
        {
            TreeItem item = coursesTree.CreateItem(courseTreeItem);
            item.SetText(0, course.tasks[i].taskName);
        }
    }
    #endregion

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		//Save once every few frames?
	}

    //Consider using MouseButtonInde to bring up a context menu that lets you create a new task/course?
    private void AddTabFromTree()
    {
        TreeItem selecteditem = coursesTree.GetSelected();

        if (selecteditem != null)
        {
            TabItem createdItem;

            //Courses are childed to the root of the tree, so the selected TreeItem must be a course.
            if (selecteditem.GetParent() == treeRoot)
            {
                createdItem = new TabItem(false, null, activeTerm.courses[selecteditem.GetIndex()]);
            }
            else
            {
                createdItem = new TabItem(true, activeTerm.courses[selecteditem.GetParent().GetIndex()].tasks[selecteditem.GetIndex()], null);
            }

            int alreadyExists = openTabs.IndexOf(createdItem);

            if (alreadyExists == -1)
            {
                openTabs.Add(createdItem);
                UpdateOpenTabs();
                tabBar.CurrentTab = openTabs.Count - 1;
            }
            else
            {
                UpdateOpenTabs();
                tabBar.CurrentTab = alreadyExists;
            }
        }

        UpdateMainDisplay(tabBar.CurrentTab);
    }

    private void TabRemoved(long tab)
    {
        openTabs.RemoveAt((int)tab);

        UpdateOpenTabs();
    }

    public void UpdateOpenTabs()
	{
        tabBar.ClearTabs();

		if(openTabs.Count != 0)
		{
            for (int i = 0; i < openTabs.Count; i++)
            {
                if (openTabs[i].isTask)
                {
                    tabBar.AddTab(openTabs[i].task.taskName);
                }
                else
                {
                    tabBar.AddTab(openTabs[i].course.courseName);
                }
            }
        }
    }

	//No idea why, but Godot always seems to use the largest datatype available, even when it's totally unreasonable to.
	//Like in this case, `long tab` refers to the index of the active tab being selected.
	//Who is ever going to need 9,223,372,036,854,775,807 tabs?
	private void UpdateMainDisplay(long selectedTab)
    {
		if (openTabs[(int)selectedTab].isTask)
		{
			courseParentNode.Visible = false;
            taskParentNode.Visible = true;

            UpdateTaskDisplay(openTabs[(int)selectedTab].task);
		}
		else
		{
            courseParentNode.Visible = true;
            taskParentNode.Visible = false;

            UpdateCourseDisplay(openTabs[(int)selectedTab].course);
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
