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
        //Queues are used instead of Lists as the data structure denotes intent of use
        //These queues hold the most current values of their respective system data
        private Queue<int> cpuLoadQ = new Queue<int>();
        private Queue<int> cpuTempQ = new Queue<int>();
        private Queue<int> memoryLoadQ = new Queue<int>();
        private Queue<int> pingQ = new Queue<int>();
        private Queue<int> cpuSpeedQ = new Queue<int>();
        //These integers hold the current values of their respective system data
        //If the data in the queues wasn't manipulated for the graph, the first data member could be used instead
        private int currentLoad = 0;
        private int currentTemp = 0;
        private int currentMem = 0;
        private int currentPing = 0;
        private int currentSpeed = 0;
        private Ping ping = new Ping();
        //Define the size of the graph
        private int xmax = 160;
        private int ymax = 31;
        private int xmin = 10;
        private int i = 0;
        private int qDepth;
        private Point[] points;
        private int[] intPoints;


        public void queueData(Object o)
        {
            this.cpuLoad();
            this.memoryLoad();
            this.cpuTemp();
            this.diskSpeed();
            //this.cpuSpeed();
            //this.Downspeed();
        }

        public int Xmax
        {
            get
            {
                return xmax;
            }
        }

        public int Ymax
        {
            get
            {
                return ymax;
            }
        }

        public Point[] cpuLoadGraph
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
        public Point[] tempGraph
        {
            get
            {
                return createGraph(cpuTempQ);
            }
        }
        public Point[] netGraph
        {
            get
            {
                return createGraph(pingQ);
            }
        }

        public int currentMetric(int type)
        {
            switch (type)
            {
                case 1:
                    return currentLoad;
                    break;
                case 2:
                    return currentMem;
                    break;
                case 3:
                    return currentPing;
                    break;
                case 4:
                    return currentTemp;
                    break;
                default:
                    return currentLoad;
                    break;
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
                //Points original values are divided by 3.3 to fit on a graph that is roughly 30 pixels in height
                points[i] = new Point(xmax - (i + xmin), Convert.ToInt32(ymax - intPoints[i] / 3.3));
                i++;

            }
            //return  array of points
            return points;

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
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    currentLoad = (Convert.ToInt32(queryObj["LoadPercentage"]));
                    cpuLoadQ.Enqueue(currentLoad);
                }
                if (cpuLoadQ.Count > 133) cpuLoadQ.Dequeue();
                searcher.Dispose();

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
                    searcher.Dispose();
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
                    
                    currentMem = Convert.ToInt32(((double)(MemoryTotal - Convert.ToInt32(queryObj["FreePhysicalMemory"])) / MemoryTotal) * 100);
                    memoryLoadQ.Enqueue(currentMem);
                }
                if (this.memoryLoadQ.Count > 133) this.memoryLoadQ.Dequeue();
                searcher.Dispose();
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

                        Double t = Convert.ToDouble(queryObj["CurrentTemperature"].ToString());
                    currentTemp += Convert.ToInt32(((t - 2732) / 10.0)); //Convert to Celcius
                }
                //Calculate the average of the two temp probes
                currentTemp /= 2;
                cpuTempQ.Enqueue(currentTemp);
                if (cpuTempQ.Count > 133) cpuTempQ.Dequeue();
                searcher.Dispose();
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        private void cpuSpeed()
        {
            try
            {
                ManagementObject searcher = new ManagementObject("Win32_Processor.DeviceID='CPU0'");
                currentSpeed = Convert.ToInt32((searcher["CurrentClockSpeed"]));
                cpuSpeedQ.Enqueue(currentSpeed);
                Console.WriteLine("Speed MHZ:" + currentSpeed.ToString());
                if (cpuSpeedQ.Count > 133) cpuSpeedQ.Dequeue();
                searcher.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void diskSpeed()
        {
            try
            {
                ManagementScope scope = new ManagementScope("\\\\.\\ROOT\\cimv2");

                //create object query
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_PerfFormattedData_PerfDisk_PhysicalDisk");

                //create object searcher
                ManagementObjectSearcher searcher =
                                        new ManagementObjectSearcher(scope, query);

                //get collection of WMI objects
                ManagementObjectCollection queryCollection = searcher.Get();

                //enumerate the collection.
                int diskNum = 0;
                foreach (ManagementObject m in queryCollection)
                {
                    // access properties of the WMI object
                    if(diskNum == 1) Console.WriteLine("Name : {0} | DiskBytesPerSec : {1}", m["Name"], m["DiskBytesPerSec"]);
                    diskNum++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}

                
