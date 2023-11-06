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

        //Test
        List<TaskBase> alltasks = new List<TaskBase>();

        for (int i = 0; i < activeTerm.courses.Count; i++)
        {
            for (int j = 0; j < activeTerm.courses[i].tasks.Count; j++)
            {
                alltasks.Add(activeTerm.courses[i].tasks[j]);
                //Psuedorandom date value.
                activeTerm.courses[i].tasks[j].dueDate = DateTime.Today.AddDays(Mathf.Sin((float)i * activeTerm.courses.Count + j) * 10.0f);
            }
        }

        List<SortExample.TaskPlaceholder> sortedTasks = SortExample.SortTasks(alltasks.ToArray());

        for (int i = 0; i < sortedTasks.Count; i++)
        {
            int j = sortedTasks[i].originalIndex;
            GD.Print(((PrimaryTask)alltasks[j]).taskName + " date " + ((PrimaryTask)alltasks[j]).dueDate.ToString());
        }
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

public class SortExample
{
    //Our application will be sorting a list of Tasks based on a variety of factors.
    //As such, this struct is used as a placeholder.
    //The "value" variable will be set based on what factor we want to sort by (due date, assignment weight, expected assignment length, etc).
    public struct TaskPlaceholder
    {
        public int originalIndex;
        public double value;
    }

    public static List<TaskPlaceholder> SortTasks(TaskBase[] unsortedTasks)
    {
        List<TaskPlaceholder> unsortedList = new List<TaskPlaceholder>(unsortedTasks.Length);

        for (int i = 0; i < unsortedTasks.Length; i++)
        {
            TaskPlaceholder t = new TaskPlaceholder();

            t.originalIndex = i;

            //Here the "value" would be set based on the factor we wanted to sort by. 
            //Setting it to the Due Date for the demonstration.
            t.value = unsortedTasks[i].dueDate.ToOADate();

            unsortedList.Add(t);
        }

        //Heapify
        for (int i = Mathf.CeilToInt(unsortedList.Count / 2); i >= 0; i--)
        {
            MaxHeapNode(i, unsortedList);
        }

        List<TaskPlaceholder> sortedList = new List<TaskPlaceholder>();

        //"Deheapify"
        //Linearly arrange all elements of the heap.
        while(unsortedList.Count != 0)
        {
            sortedList.Add(unsortedList[0]);

            Swap(0, unsortedList.Count - 1, unsortedList);
            unsortedList.RemoveAt(unsortedList.Count - 1);

            MaxHeapNode(0, unsortedList);
        }

        return sortedList;
    }

    //Referred to as "Heapify" in some places.
    static void MaxHeapNode(int node, List<TaskPlaceholder> heap)
    {
        int largestNode = -1;
        //Check Left Child.
        if ((node * 2) + 1 < heap.Count && heap[(node * 2) + 1].value > heap[node].value)
        {
            largestNode = (node * 2) + 1;
        }
        else
        {
            largestNode = node;
        }
        //Check Right Node.
        if ((node * 2) + 2 < heap.Count && heap[(node * 2) + 2].value > heap[largestNode].value)
        {
            largestNode = (node * 2) + 2;
        }
        //Swap current node with largest node, whether that is the left or right one.
        if (largestNode != node)
        {
            Swap(node, largestNode, heap);
            //If the node is swapped, echo the process down the tree.
            MaxHeapNode(largestNode, heap);
        }
    }

    static void Swap(int a, int b, List<TaskPlaceholder> heap)
    {
        TaskPlaceholder hold = heap[a];
        heap[a] = heap[b];
        heap[b] = hold;
    }
}