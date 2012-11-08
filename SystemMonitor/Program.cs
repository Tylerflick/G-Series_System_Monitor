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
            Queue<int> cpuLoad = new Queue<int>();

            //int maxQueueSize = 130, btnNumber = 1;
            string appName = "System Monitor";

            Model model = new Model(); //Graph class for plotting information to points

            View view = new View(model, appName);
            view.Type = 1;

            TimerCallback mTcb = new TimerCallback(model.queueData);
            Timer modelTimer = new Timer(mTcb, null, 0, 1000);

            ////New timed thread to poll weather an LCD button has been clicked
            ButtonHandler btnhd = new ButtonHandler(view);

            TimerCallback tcb = new TimerCallback(btnhd.buttonListener);
            Timer btnTimer = new Timer(tcb, null, 0, 100);

            while (true)
            {
                view.drawScreen();
                Thread.Sleep(1000);
            }

            ////Subscribe(btnhd);
            ////Release graphics resources
            //view.emptyGraphics();
        }





        
    }


}
