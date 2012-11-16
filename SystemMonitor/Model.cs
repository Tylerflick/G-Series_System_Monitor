using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Management;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace SystemMonitor
{
    class Model
    {
        private Queue<int> cpuLoadQ = new Queue<int>();
        private Queue<int> memoryLoadQ = new Queue<int>();
        private Queue<int> pingQ = new Queue<int>();
        private ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
        private Ping ping = new Ping();
        //Define the size of the graph
        int xmax = 160;
        int ymax = 31;
        int xmin = 10;
        int i = 0;
        int qDepth;
        Point[] points;
        int[] intPoints;


        public Point[] cpuGraph
        {
            get
            {
                return createGraph(cpuLoadQ);
            }
        }
        public Point[] memoryGraph
        {
            get
            {
                return createGraph(memoryLoadQ);
            }
        }
        public Point[] netGraph
        {
            get
            {
                return createGraph(pingQ);
            }
        }
        public Point[] diskGraph
        {
            get
            {
                return createGraph(cpuLoadQ);
            }
        }

        private Point[] createGraph(Queue<int> data)
        {
            //retreive size of queue
            qDepth = data.Count();

            //Create array of points and copy queue contents
            points = new Point[qDepth];
            intPoints = data.Reverse().ToArray();

            i = 0;
            while (i < qDepth)
            {
                points[i] = new Point(xmax - (i + xmin), Convert.ToInt32(ymax - intPoints[i] / 3.3));
                i++;

            }
            //return  array of points
            return points;

        }


        public void queueData(Object o)
        {
            this.cpuLoad();
            this.memoryLoad();
            this.cpuTemp();
            //this.Downspeed();
        }

        /// <summary>
        /// Fetches the current load of a cpu core
        /// </summary>
        /// <param name="coreNum">Integer returned from returnNumCores</param>
        /// <returns>String in the form of a percentage of core load</returns>
        /// <example>
        /// CPUMonitor mon = new CPUMonitor();
        /// foreach(var i in mon.returnNumCores())
        /// {
        ///     Console.Log("Core number "+i+" load: "+coreLoad(i));
        /// }
        /// </example>
        /// 
        private void cpuLoad()
        {
            try
            {
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    cpuLoadQ.Enqueue(Convert.ToInt32(queryObj["LoadPercentage"]));
                }
                if (cpuLoadQ.Count > 133) cpuLoadQ.Dequeue();
                //Console.WriteLine(cpuLoadQ.Count);
            }
            catch (ManagementException e)
            {
                Console.WriteLine("Unable to enqueue CPU load data");
            }
        }

        private int MemoryTotal
        {
            get
            {
                try
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");
                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        return Convert.ToInt32(queryObj["TotalVisibleMemorySize"]);
                        
                    }
                }
                catch (ManagementException e)
                {
                    Console.Write("An error occurred while querying for WMI data: " + e.Message);
                    return 0;
                }
                return 0;
            }
        }

        private void memoryLoad()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    memoryLoadQ.Enqueue(Convert.ToInt32(((double)(MemoryTotal - Convert.ToInt32(queryObj["FreePhysicalMemory"])) / MemoryTotal) * 100));
                    if (this.memoryLoadQ.Count > 133) this.memoryLoadQ.Dequeue();
                    //Console.WriteLine(this.memoryLoadQ.Count);
                   
                }
            }
            catch (ManagementException e)
            {
                Console.Write("An error occurred while querying for WMI data: " + e.Message);
            }
        }

        private void downSpeed()
        {
            try
            {
                PingReply pingStatus = ping.Send(IPAddress.Parse("http://www.google.com"));
                pingQ.Enqueue(Convert.ToInt32(pingStatus.RoundtripTime));
                if (this.pingQ.Count > 133) this.pingQ.Dequeue();
            }
            catch (ManagementException e)
            {
                Console.Write("An error occurred while pinging: " + e.Message);
            }
        }

        private void cpuTemp()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                foreach (ManagementObject queryObj in searcher.Get())
                {

                    Double temp = Convert.ToDouble(queryObj["CurrentTemperature"].ToString());
                    Console.WriteLine(temp = (temp - 2732) / 10.0); //Convert Celcius
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}
