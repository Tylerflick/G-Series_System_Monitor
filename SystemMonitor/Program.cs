using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading;
using System.Drawing;


namespace SystemMonitor
{
    class SystemMonitor
    {
        static void Main(string[] args)
        {
            string appName = "System Monitor";

            //Data class to get system info
            Model model = new Model(); 

            //Handles LCD drawing
            View view = new View(model, appName); 
            view.Type = 1; //Set default view to first button
            //Releases the graphics object on close
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => OnProcessExit(sender, e, view);

            //Timer to poll system data every second
            //If decreasing this time keep in mind its effect on system resources
            TimerCallback mTcb = new TimerCallback(model.queueData);
            Timer modelTimer = new Timer(mTcb, null, 0, 1000);

            ////New timer thread to poll whether an LCD button has been clicked
            ButtonHandler btnhd = new ButtonHandler(view);
            TimerCallback tcb = new TimerCallback(btnhd.buttonListener);
            Timer btnTimer = new Timer(tcb, null, 0, 100);

            //Draw the screen once a second
            while (true)
            {
                view.drawScreen();
                Thread.Sleep(1000);
            }
        }

        static void OnProcessExit(object sender, EventArgs e, View view)
        {
            //Release graphics resources
            view.emptyGraphics();
        }
    }
}
