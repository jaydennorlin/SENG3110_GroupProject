using AssignmentTracker;
using Godot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static Godot.Time;

public partial class Tester : Node
{
    [ExportGroup("UI")]
    [Export] TabBar primaryTabBar;

    [ExportGroup("Term Fields")]
    [Export] RichTextLabel termNameLabel;
    [Export] Button createCourseButton;
    [Export] Tree coursesTree;
    TreeItem coursesTreeRoot;

    [ExportGroup("Course Fields")]
    [Export] Control courseParentNode;
    [Export] LabelEdit courseNameLabelEdit;
    [Export] TextEdit courseInfoArea;
    [Export] Button createTaskButton;
    [Export] Button deleteCourseButton;

    [ExportGroup("Task Fields")]
    [Export] Control taskParentNode;
    [Export] LabelEdit taskTitleLabelEdit;
    [Export] RichTextLabel taskInfoLabel;
    [Export] OptionButton taskHourOption;
    [Export] OptionButton taskMinuteOption;
    [Export] OptionButton taskMonthOption;
    [Export] OptionButton taskDayOption;
    [Export] OptionButton taskYearOption;
    [Export] TextEdit taskDescriptionArea;
    [Export] Button createSubtaskButton;
    [Export] Tree subtaskTree;
    TreeItem subtaskTreeRoot;
    [Export] Button deleteTaskButton;

    [ExportGroup("Secondary Display")]
    [Export] TabBar secondaryTabBar;
    [Export] Control calendarNode;
    [Export] Control upcomingAssignmentsNode;
    [Export] Control subtaskEditNode;

    [ExportSubgroup("Subtask Editting Menu")]
    [Export] TextEdit subtaskTypeEdit;
    [Export] TextEdit subtaskDescriptionEdit;


    Term activeTerm;
    int edittingTask = -1;

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

        //Change this to Subtask
        exampleCourse.tasks[0].subtasks.Add(new TaskBase("How many eggs isd?", "Quiz"));
        exampleCourse.tasks[0].subtasks.Add(new TaskBase("KIll the birds", "Assignment"));

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

        createCourseButton.Pressed += CreateNewCourse;

        for (int i = 0; i < activeTerm.courses.Count; i++)
        {
            AddCourseToTree(activeTerm.courses[i]);
        }

        //Primary/Generic Events
        coursesTree.CellSelected += OpenTab;

        primaryTabBar.TabChanged += UpdatePrimaryDisplay;
        primaryTabBar.TabClosePressed += TabRemoved;
        primaryTabBar.ActiveTabRearranged += TabRearranged;

        //Course Events
        courseNameLabelEdit.EditComplete += Course_UpdateTitle;
        courseInfoArea.TextChanged += Course_UpdateDescription;
        createTaskButton.Pressed += CreateNewTask;
        deleteCourseButton.Pressed += Course_DeleteConfirmPopup;

        //Task Events
        taskTitleLabelEdit.EditComplete += Task_UpdateTitle;
        taskDescriptionArea.TextChanged += Task_UpdateDescription;
        createSubtaskButton.Pressed += CreateSubtask;
        subtaskTree.CellSelected += SubtaskEdit_Update;
        deleteTaskButton.Pressed += Task_DeleteConfirmPopup;

        //Secondary Display Events
        secondaryTabBar.TabChanged += UpdateSecondaryDisplay;

        //Subtask Edit
        subtaskTypeEdit.TextChanged += PrimaryDisplay_UpdateType;
        taskHourOption.ItemSelected += PrimaryDisplay_UpdateDueDate_Hour;
        taskMinuteOption.ItemSelected += PrimaryDisplay_UpdateDueDate_Minute;
        taskMonthOption.ItemSelected += PrimaryDisplay_UpdateDueDate_Month;
        taskDayOption.ItemSelected += PrimaryDisplay_UpdateDueDate_Day;
        taskYearOption.ItemSelected += PrimaryDisplay_UpdateDueDate_Year;
        subtaskDescriptionEdit.TextChanged += PrimaryDisplay_UpdateDescription;

        //Final Initialization.
        UpdateSecondaryDisplay(0);
        UpdateOpenTabs();

        //Test
        /*List<TaskBase> alltasks = new List<TaskBase>();

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
        }*/
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        //Save once every few frames?
    }

    public void UpdateTree()
    {
        //Lazy, but effective.
        coursesTree.Clear();
        //Disposed by the above method, but not immediately null.
        coursesTreeRoot = null;

        for (int i = 0; i < activeTerm.courses.Count; i++)
        {
            AddCourseToTree(activeTerm.courses[i]);
        }

        /*for (int i = 0; i < activeTerm.courses.Count; i++)
        {
            coursesTreeRoot.GetChild(i).SetText(0, activeTerm.courses[i].courseName);
            for (int j = 0; j < activeTerm.courses[i].tasks.Count; j++)
            {
                coursesTreeRoot.GetChild(i).GetChild(j).SetText(0, activeTerm.courses[i].tasks[j].taskName);
            }
        }

        if(activeTerm.courses.Count < coursesTreeRoot.GetChildCount())
        {
            for (int i = activeTerm.courses.Count - 1; i < coursesTreeRoot.GetChildCount(); i++)
            {
                TreeItem toRemove = coursesTreeRoot.GetChild(i);
                coursesTreeRoot.RemoveChild(toRemove);
                //Removing a TreeItem from it's parent does not automatically remove it from memory, so Dispose() must manually be called.
                toRemove.Dispose();
            }
        }*/
    }

    public void AddCourseToTree(Course course, bool select = false)
    {
        if (coursesTreeRoot == null)
        {
            coursesTreeRoot = coursesTree.CreateItem();
        }

        TreeItem courseTreeItem = coursesTree.CreateItem(coursesTreeRoot);
        courseTreeItem.SetText(0, course.courseName);

        for (int i = 0; i < course.tasks.Count; i++)
        {
            TreeItem item = coursesTree.CreateItem(courseTreeItem);
            item.SetText(0, course.tasks[i].taskName);
        }

        if (select)
        {
            coursesTree.SetSelected(courseTreeItem, 0);
        }
    }


    //Consider using MouseButtonIndex to bring up a context menu that lets you create a new task/course?
    private void OpenTab()
    {
        TreeItem selecteditem = coursesTree.GetSelected();

        if (selecteditem != null)
        {
            //Courses are childed to the root of the tree, so the selected TreeItem must be a course.
            if (selecteditem.GetParent() == coursesTreeRoot)
            {
                OpenCourseTab(activeTerm.courses[selecteditem.GetIndex()]);
            }
            else
            {
                OpenTaskTab(activeTerm.courses[selecteditem.GetParent().GetIndex()].tasks[selecteditem.GetIndex()]);
            }
        }
    }

    void OpenTaskTab(PrimaryTask task)
    {
        TabItem createdItem = new TabItem(true, task, null);

        int alreadyExists = openTabs.IndexOf(createdItem);

        if (alreadyExists == -1)
        {
            openTabs.Add(createdItem);
            UpdateOpenTabs();
            primaryTabBar.CurrentTab = openTabs.Count - 1;
        }
        else
        {
            primaryTabBar.CurrentTab = alreadyExists;
        }
    }

    void OpenCourseTab(Course course)
    {
        TabItem createdItem = new TabItem(false, null, course);

        int alreadyExists = openTabs.IndexOf(createdItem);

        if (alreadyExists == -1)
        {
            openTabs.Add(createdItem);
            UpdateOpenTabs();
            primaryTabBar.CurrentTab = openTabs.Count - 1;
        }
        else
        {
            primaryTabBar.CurrentTab = alreadyExists;
        }
    }

    private void TabRemoved(long tab)
    {
        openTabs.RemoveAt((int)tab);

        UpdateOpenTabs();
    }

    //NOTE: Rearranging tabs does not work at all. Disabled.
    //There is a GetPreviousTab() method, allegedly. Look into that.
    void TabRearranged(long idxTo)
    {
        /*GD.Print("Tab moved from " + lastTab + " to " + idxTo);
        TabItem rearrangedTab = openTabs[lastTab];

        openTabs.RemoveAt(lastTab);

        openTabs.Insert((int)idxTo, rearrangedTab);

        for (int i = 0; i < openTabs.Count; i++)
        {
            if (openTabs[i].isTask)
            {
                GD.Print(i + ": " + openTabs[i].task.taskName);
            }
            else
            {
                GD.Print(i + ": " + openTabs[i].course.courseName);
            }
        }*/
    }

    public void UpdateOpenTabs()
    {
        primaryTabBar.ClearTabs();

        if (openTabs.Count != 0)
        {
            for (int i = 0; i < openTabs.Count; i++)
            {
                if (openTabs[i].isTask)
                {
                    primaryTabBar.AddTab(openTabs[i].task.taskName);
                }
                else
                {
                    primaryTabBar.AddTab(openTabs[i].course.courseName);
                }
            }
        }
        else
        {
            courseParentNode.Hide();
            taskParentNode.Hide();
        }
    }

    //No idea why, but Godot always seems to use the largest datatype available, even when it's totally unreasonable to.
    //Like in this case, `long tab` refers to the index of the active tab being selected.
    //Who is ever going to need 9,223,372,036,854,775,807 tabs?
    private void UpdatePrimaryDisplay(long selectedTab)
    {
        if (selectedTab == -1)
        {
            selectedTab = primaryTabBar.CurrentTab;
        }
        else
        {
            //Really bad workaround. Improve this later.
            SubtaskEdit_Update(true);
        }

        GD.Print("Update Primary Display to tab " + selectedTab);
        if (openTabs[(int)selectedTab].isTask)
        {
            courseParentNode.Visible = false;
            taskParentNode.Visible = true;

            //Clearing undo history so you don't get weird behaviours, like undoing text from a previous tab into your current tab.
            taskDescriptionArea.ClearUndoHistory();
            Task_UpdateDisplay(openTabs[(int)selectedTab].task);
        }
        else
        {
            courseParentNode.Visible = true;
            taskParentNode.Visible = false;

            courseInfoArea.ClearUndoHistory();
            Course_UpdateDisplay(openTabs[(int)selectedTab].course);
        }

        //Cancel all current open label edits.
        taskTitleLabelEdit.CancelEdit();
        courseNameLabelEdit.CancelEdit();
    }

    #region Course Methods
    public void CreateNewCourse()
    {
        Course newCourse = new Course();

        //No other initial data needed?
        newCourse.courseName = "New Course";

        activeTerm.courses.Add(newCourse);
        AddCourseToTree(newCourse, true);
        OpenTab();
    }

    public void Course_UpdateDisplay(Course course)
    {
        courseNameLabelEdit.Text = course.courseName;

        //Rework this later to use the entire array. Or change it to just be a single string.
        courseInfoArea.Text = course.notes;

        secondaryTabBar.SetTabDisabled(2, true);
    }

    private void Course_UpdateTitle()
    {
        openTabs[primaryTabBar.CurrentTab].course.courseName = courseNameLabelEdit.Text;

        UpdateTree();
    }

    private void Course_UpdateDescription()
    {
        openTabs[primaryTabBar.CurrentTab].course.notes = courseInfoArea.Text;
    }

    private void Course_DeleteConfirmPopup()
    {
        ConfirmationDialog dialog = new ConfirmationDialog();
        AddChild(dialog);

        dialog.Confirmed += Course_Delete;

        //Consider finding some way of avoiding this. Investigate Obsidian/System Trash method, and file structure serializaliation.
        dialog.Title = openTabs[primaryTabBar.CurrentTab].course.courseName + " WILL BE PERMANENTLY DELETED.";

        dialog.Popup();
    }

    void Course_Delete()
    {
        //Validate that this results in it being properly removed.
        Course toBeRemoved = openTabs[primaryTabBar.CurrentTab].course;
        openTabs.RemoveAt(primaryTabBar.CurrentTab);

        //Remove any currently open tabs of Tasks belonging to the course being deleted.
        for (int i = 0; i < openTabs.Count; i++)
        {
            for (int j = 0; j < toBeRemoved.tasks.Count; j++)
            {
                if (openTabs[i].task == toBeRemoved.tasks[j])
                {
                    GD.Print("Removed");
                    openTabs.RemoveAt(i);
                    i--;
                    break;
                }
            }
        }

        activeTerm.courses.Remove(toBeRemoved);

        UpdateOpenTabs();
        UpdateTree();
    }
    #endregion

    #region TaskMethods
    public void CreateNewTask()
    {
        Course course = openTabs[primaryTabBar.CurrentTab].course;

        PrimaryTask newTask = new PrimaryTask();

        //No other initial data needed?
        newTask.taskName = "New Task";
        newTask.course = course;
        newTask.dueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 0).AddDays(1);

        course.tasks.Add(newTask);
        UpdateTree();
        OpenTaskTab(newTask);
    }

    public void Task_UpdateDisplay(PrimaryTask task)
	{
        taskTitleLabelEdit.Text = task.taskName;
        taskInfoLabel.Text = task.course.courseName;
        PrimaryDisplay_SetTime(task.dueDate);
        taskDescriptionArea.Text = task.taskDescription;

        GD.Print("Clear Tree");
        subtaskTree.Clear();
        subtaskTreeRoot = subtaskTree.CreateItem();

        for (int i = 0; i < task.subtasks.Count; i++)
        {
            GD.Print("Add Item " + i);

            TreeItem treeItem = subtaskTree.CreateItem(subtaskTreeRoot);

            if (task.subtasks[i] is PrimaryTask)
            {
                treeItem.SetText(0, ((PrimaryTask)task.subtasks[i]).taskName);

                //Add recursion.
            }
            else
            {
                //Add due date, or other items?
                string taskType = task.subtasks[i].taskType;
                taskType = taskType.Replace("\n", "");

                string taskDesc = task.subtasks[i].taskDescription;
                //Arbitrary limit.
                if(taskDesc.Length > 25 || taskDesc.Contains("\n"))
                {
                    int removeIndex = 22;
                    if (taskDesc.Contains("\n"))
                    {
                        removeIndex = Mathf.Min(22, taskDesc.IndexOf("\n"));
                    }

                    taskDesc = taskDesc.Remove(removeIndex) + "...";
                }

                if(taskType.Length != 0)
                {
                    treeItem.SetText(0, taskType + ": " + taskDesc);
                }
                else
                {
                    treeItem.SetText(0, taskDesc);
                }
            }
        }

        if(task.subtasks.Count != 0)
        {
            secondaryTabBar.SetTabDisabled(2, false);
        }
        else
        {
            secondaryTabBar.SetTabDisabled(2, true);
        }
    }

    private void Task_UpdateTitle()
    {
        int currentTab = primaryTabBar.CurrentTab;
        openTabs[currentTab].task.taskName = taskTitleLabelEdit.Text;

        UpdateOpenTabs();
        UpdateTree();

        //UpdateOpenTabs() causes the first tab to be selected, so we force it back to the current tab we're at.
        primaryTabBar.CurrentTab = currentTab;
    }

    private void Task_UpdateDescription()
    {
        openTabs[primaryTabBar.CurrentTab].task.taskDescription = taskDescriptionArea.Text;
    }

    private void CreateSubtask()
    {
        TaskBase newTask = new TaskBase();

        newTask.taskDescription = "New Subtask";

        int currentTab = primaryTabBar.CurrentTab;
        openTabs[currentTab].task.subtasks.Add(newTask);

        UpdateOpenTabs();
        primaryTabBar.CurrentTab = currentTab;
        GD.Print("Updating display with " + openTabs[primaryTabBar.CurrentTab].task.taskName);
        Task_UpdateDisplay(openTabs[primaryTabBar.CurrentTab].task);

        subtaskTree.SetSelected(subtaskTreeRoot.GetChild(subtaskTreeRoot.GetChildCount() - 1), 0);
    }

    private void Task_DeleteConfirmPopup()
    {
        ConfirmationDialog dialog = new ConfirmationDialog();
        AddChild(dialog);

        dialog.Confirmed += Task_Delete;

        //Consider finding some way of avoiding this. Investigate Obsidian/System Trash method, and file structure serializaliation.
        dialog.Title = openTabs[primaryTabBar.CurrentTab].task.taskName + " WILL BE PERMANENTLY DELETED.";

        dialog.Popup();
    }

    void Task_Delete()
    {
        //Validate that this results in it being properly removed.
        PrimaryTask toBeRemoved = openTabs[primaryTabBar.CurrentTab].task;
        openTabs.RemoveAt(primaryTabBar.CurrentTab);

        toBeRemoved.course.tasks.Remove(toBeRemoved);

        UpdateOpenTabs();
        UpdateTree();

        //Adjusting the current tab and forcing the SubtaskEdit tab to be redrawn, removing a subtask if it's connected to this task.
        secondaryTabBar.CurrentTab = 0;
        SubtaskEdit_Update(true);
    }

    private void PrimaryDisplay_SetTime(DateTime time)
    {
        if (time.Hour < 12)
        {
            taskHourOption.Selected = time.Hour;
        }
        else
        {
            //The 12th item is used as a seperator, so we need to add 1 to the hour index to account for this.
            taskHourOption.Selected = time.Hour + 1;
        }

        if (taskMinuteOption.ItemCount != 60)
        {
            taskMinuteOption.Clear();
            for (int i = 0; i < 60; i++)
            {
                if (i < 10)
                {
                    taskMinuteOption.AddItem("0" + i);
                }
                else
                {
                    taskMinuteOption.AddItem(i.ToString());
                }
            }
        }
        taskMinuteOption.Selected = time.Minute;

        taskMonthOption.Selected = time.Month - 1;

        PrimaryDisplay_UpdateDateOptions(time.Year, time.Month);
        taskDayOption.Selected = time.Day - 1;

        //Adjust available options to be based on current term?
        //subtaskYearOption.Selected
    }

    void PrimaryDisplay_UpdateDateOptions(int year, int month)
    {
        if (taskDayOption.ItemCount != DateTime.DaysInMonth(year, month))
        {
            GD.Print("Date update from " + taskDayOption.ItemCount + " to " + DateTime.DaysInMonth(year, month));
            int selected = taskDayOption.Selected;
            taskDayOption.Clear();

            for (int i = 1; i <= DateTime.DaysInMonth(year, month); i++)
            {
                if (i < 10)
                {
                    taskDayOption.AddItem("0" + i);
                }
                else
                {
                    taskDayOption.AddItem(i.ToString());
                }
            }

            taskDayOption.Selected = Mathf.Clamp(selected, 0, DateTime.DaysInMonth(year, month) - 1);
        }
    }

    void PrimaryDisplay_UpdateType()
    {
        if (edittingTask != -1)
        {
            openTabs[primaryTabBar.CurrentTab].task.subtasks[edittingTask].taskType = subtaskTypeEdit.Text;
        }

        UpdatePrimaryDisplay(-1);
    }

    void PrimaryDisplay_UpdateDueDate_Hour(long hour)
    {
        DateTime time = openTabs[primaryTabBar.CurrentTab].task.dueDate;

        if (hour > 12)
        {
            hour--;
        }

        time = new DateTime(time.Year, time.Month, time.Day, (int)hour, time.Minute, 0);

        openTabs[primaryTabBar.CurrentTab].task.dueDate = time;
    }

    void PrimaryDisplay_UpdateDueDate_Minute(long minute)
    {
        DateTime time = openTabs[primaryTabBar.CurrentTab].task.dueDate;

        time = new DateTime(time.Year, time.Month, time.Day, time.Hour, (int)minute, 0);

        openTabs[primaryTabBar.CurrentTab].task.dueDate = time;
    }

    void PrimaryDisplay_UpdateDueDate_Month(long month)
    {
        DateTime time = openTabs[primaryTabBar.CurrentTab].task.dueDate;

        //Months do not start indexing from 0.
        PrimaryDisplay_UpdateDateOptions(time.Year, (int)month + 1);
        time = new DateTime(time.Year, (int)month + 1, taskDayOption.Selected + 1, time.Hour, time.Minute, 0);

        openTabs[primaryTabBar.CurrentTab].task.dueDate = time;
    }

    void PrimaryDisplay_UpdateDueDate_Day(long day)
    {
        DateTime time = openTabs[primaryTabBar.CurrentTab].task.dueDate;

        time = new DateTime(time.Year, time.Month, (int)day, time.Hour, time.Minute, 0);

        openTabs[primaryTabBar.CurrentTab].task.dueDate = time;
    }

    void PrimaryDisplay_UpdateDueDate_Year(long year)
    {
        GD.PrintErr("YEAR IS NOT YET IMPLEMENTED");
    }

    void PrimaryDisplay_UpdateDescription()
    {
        if (edittingTask != -1)
        {
            openTabs[primaryTabBar.CurrentTab].task.subtasks[edittingTask].taskDescription = subtaskDescriptionEdit.Text;
        }

        UpdatePrimaryDisplay(-1);
    }
    #endregion

    #region Secondary Display
    private void UpdateSecondaryDisplay(long selectedTab)
    {
        calendarNode.Visible = false;
        upcomingAssignmentsNode.Visible = false;
        subtaskEditNode.Visible = false;

        //Add code that updates the relevant information under these nodes.
        if (selectedTab == 0)
        {
            calendarNode.Visible = true;
        }
        else if(selectedTab == 1)
        {
            upcomingAssignmentsNode.Visible = true;
        }
        else
        {
            //subtaskEditNode.Visible = true;
            SubtaskEdit_Update();
        }
    }

    private void SubtaskEdit_Update()
    {
        SubtaskEdit_Update(false);
    }

    private void SubtaskEdit_Update(bool forceClearSubtask)
    {
        TreeItem selecteditem = subtaskTree.GetSelected();

        if (selecteditem != null && openTabs[primaryTabBar.CurrentTab].isTask && !forceClearSubtask)
        {
            secondaryTabBar.CurrentTab = 2;
            edittingTask = selecteditem.GetIndex();

            TaskBase selectedTask = openTabs[primaryTabBar.CurrentTab].task.subtasks[selecteditem.GetIndex()];

            //Make this more accurate later.
            secondaryTabBar.SetTabTitle(2, selectedTask.taskDescription);

            subtaskTypeEdit.Text = selectedTask.taskType;
            subtaskDescriptionEdit.Text = selectedTask.taskDescription;

            subtaskEditNode.Visible = true;
        }
        else
        {
            GD.Print("Item null " + (selecteditem == null) + " is task " + openTabs[primaryTabBar.CurrentTab].isTask + " force " + forceClearSubtask);
            secondaryTabBar.SetTabTitle(2, "No Subtask Selected");

            subtaskEditNode.Visible = false;
        }
    }
    #endregion
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

    public static List<TaskPlaceholder> SortTasks(PrimaryTask[] unsortedTasks)
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