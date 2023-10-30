using System;
using System.Collections.Generic;

namespace AssignmentTracker
{
    public class Term
    {
        public string termName;
        //Add start/end date.
        //Add Directory where this is stored.

        //Investigate other means of storing this.
        public List<Course> courses;

        public void SaveTerm()
        {
            //Save all data to directory.

            
        }

        public static Term LoadTerm(string directory)
        {
            //Loads a term from the directory, turns it into an object, then returns it.
            return null;
        }
    }

    public class Course
    {
        public string courseName;
        //Consider making this some sort of two-part string.
        //First part is the type of data being stored (Professor Name, Phone Number, etc), second part is the actual data.
        public string[] additonalData;

        public List<PrimaryTask> tasks;
    }

    public class TaskBase
    {
        public string taskDescription;
    }

    public class PrimaryTask : TaskBase
    {
        public string taskName;
        //Add due date.

        public List<TaskBase> subtasks;
    }

    public class Subtask : TaskBase
    {

    }
}