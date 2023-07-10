using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Residence_Web_Scraper.HandlerClasses
{
    public static class StatusHandler
    {
        //get status
        public static string Status { get; set; } = "Idle";

        //reff to main window
        private static MainWindow? AppMainWindow;
        public static void InitializeComponent(MainWindow appMainWindow)
        {
            //asign the main window
            AppMainWindow = appMainWindow;
        }
        //updates the app status
        public static void UpdateAppStatus(string status)
        {
            if (AppMainWindow == null)
            {
                return;
            }
            Status = status;
            //gets the status text from main window and asign the status text
            AppMainWindow.StatusTEXT.Text = Status;
        }
        //directluy update the status bar in the main window
        public static void UpdateStatusBar(int percent)
        {
            if (AppMainWindow == null)
            {
                return;
            }
            AppMainWindow.StatusProgressBar.Value = percent;
        }
        //pass a list of tasks and update the status bar based on the finalized tasks
        public static async Task<List<T>> UpdateStatusBasedOnTaskList<T>(List<Task<T>> InputTasks) //replace return with array
        {
            //list of tasks
            List<T> ResultTasks = new();

            //while the input tasks is not empty
            while (InputTasks.Count > 0)
            {
                //wait for any task to finush
                Task<T> completedTask = await Task.WhenAny(InputTasks);
                //add to finished task
                ResultTasks.Add(completedTask.Result);
                //removed for unfinished tasks
                InputTasks.Remove(completedTask);
                //create percentage based on finished tasks
                int progressint = (ResultTasks.Count * 100) / (InputTasks.Count + ResultTasks.Count);
                //updates statusbar and log it in console
                UpdateStatusBar(progressint);
                Console.WriteLine($"Progress: {progressint}%");
            }
            //return the list of completed tasks
            return ResultTasks;
        }
    }
}
