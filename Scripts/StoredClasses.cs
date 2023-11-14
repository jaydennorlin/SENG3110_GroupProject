using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AssignmentTracker {
    public class Term {
        public string termName;
        //Add start/end date.
        public DateTime startDate;
        public DateTime endDate;

        public string directory;

        //Investigate other means of storing this.
        public List<Course> courses = new List<Course>();

        public void SaveTerm()
        {
            string jsonString = JsonSerializer.Serialize(this);

            File.WriteAllText(directory, jsonString);
        }

        public static Term LoadTerm(string directory)
        {
            Term toReturn = JsonSerializer.Deserialize<Term>(directory);

            if (toReturn == null)
            {
                //Throw some error or something.
            }

            return toReturn;
        }
    }

    public class Course {
        //Consider adding an additional variable, "Course Code" or something like that.
        //So we can refer to the couse either by it's fill name (ex "Algorithms and Data Structures") or by its Code (ex "SENG 4530)
        public string courseName;
        //Consider making this some sort of two-part string.
        //First part is the type of data being stored (Professor Name, Phone Number, etc), second part is the actual data.
        public string[] additonalData;


        public List<PrimaryTask> tasks = new List<PrimaryTask>();

        public Course(string courseName, string addData)
        {
            this.courseName = courseName;
            this.additonalData = new string[] { addData };
        }
    }

    public class TaskBase {
        public string taskDescription;

        //Task Type could be Quiz, Midterm, Aiisgments, Project or Others
        //You may want to implement this by 
        public string taskType;

        //Add due date.
        public DateTime dueDate;

        //Two way connection, so a task alone knows what course its part of.
        public Course course;

        public TaskBase(string desc, string taskType)
        {
            this.taskDescription = desc;
            this.taskType = taskType;
        }

        public TaskBase()
        {

        }
    }

    public class PrimaryTask : TaskBase {
        public string taskName;

        public List<TaskBase> subtasks = new List<TaskBase>();

        public PrimaryTask(string taskName, Course course)
        {
            this.taskName = taskName;
            this.course = course;
        }
    }

    public class Subtask : TaskBase {
        //Have primary task to look up. Do we need this one?
        public PrimaryTask primaryTask;
    }
}