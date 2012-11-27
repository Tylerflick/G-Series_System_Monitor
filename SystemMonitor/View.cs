using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;


namespace SystemMonitor
{
    class View
    {
        Model data;
        private int device, type;
        private Point[] points;
        //Instantiate bitmap, graphics object, & various properties used for drawing on the canvas
        private Bitmap LCD = new Bitmap(160, 43);
        private Graphics gfx;
        private Font lcdFont = new Font("Calibri", 9, FontStyle.Regular, GraphicsUnit.Point);
        private Font lcdFontSmall = new Font("Segoe UI", 7, FontStyle.Regular, GraphicsUnit.Point);
        private Font lcdFontAlt = new Font("Calibri", 9, FontStyle.Regular, GraphicsUnit.Point);
        private Pen lcdPen = new Pen(Color.Black, 1);
        private SolidBrush lcdBrush = new SolidBrush(Color.Black);
        private SolidBrush activeBrush = new SolidBrush(Color.White);

        public View(Model model, string appName)
        {
            //Assign a reference to the model class. This is is where the system data will be retrieved from
            data = model;
            //Attemp to initialise an instance of the Logitech LCD library
            uint initialiseResult = DMcLgLCD.LcdInit();
            //If the initialization fails it will return an unsigned int other than the constant ERROR_SUCCUSS
            if (initialiseResult != DMcLgLCD.ERROR_SUCCESS) Console.WriteLine("LCD Failed to Initialise");
            //Connect to the Logitech device with LCD with LcdConnect. It's return value will be used to open a connection with LcdOpenByType
            device = DMcLgLCD.LcdOpenByType(DMcLgLCD.LcdConnect(appName, 0, 1), DMcLgLCD.LGLCD_DEVICE_BW);
            //Set this app to as the current LCD app
            DMcLgLCD.LcdSetAsLCDForegroundApp(device, DMcLgLCD.LGLCD_FORE_YES);
            //Initialize the Graphics object
            gfx = Graphics.FromImage(LCD);
        }

		//Getter for peripheral assinged system index
        public int returnDeviceId
        {
            get
            {
                return device;
            }
        }
		
		//Getter and setter for currently selected system monitor
		//Ex: CPU usage, memory, disk access, etc
        public int Type
        {
            get
            {
                return type;
            }
            set
            {
                if(value <= 4 || value >= 1)
                    type = value;
            }
        }
		
		//Main method to draw the graph on the LCD screen
        public void drawScreen()
        {
			//Determin system metric
            switch (this.type)
            {
                case 1:
                    points = data.cpuLoadGraph;
                    break;
                case 2:
                    points = data.memoryGraph;
                    break;
                case 3:
                    points = data.diskGraph;
                    break;
                case 4:
                    points = data.tempGraph;
                    break;
                default:
                    points = data.cpuLoadGraph;
                    type = 1;
                    break;
            }
                                                                                        //<---Issues here with callbacks
            //Clear the screen
            gfx.Clear(Color.White);
            //Draw borders for graph
            gfx.DrawLine(lcdPen, 17, 32, 17, 0);
            gfx.DrawLine(lcdPen, 17, 32, 150, 32);
            
            //Draw legend for graph
            gfx.DrawString("100", lcdFontSmall, lcdBrush, 0, 0);
            gfx.DrawString("0", lcdFontSmall, lcdBrush, 10, 22);
            
            //This is need becuase if the array of points only contains one point it will cause an error in the method
            if (points.Count() > 1)
            {
				//Draw the graph
                gfx.DrawLines(lcdPen, points);
				//Draw the current value of the meter in the top right corner
                drawCurrentMeter();
            }
			//
            drawLabels();

            //Update the bitmap on the screen w/ new graphix
            DMcLgLCD.LcdUpdateBitmap(device, LCD.GetHbitmap(), DMcLgLCD.LGLCD_DEVICE_BW);

            
        }

        private void drawLabels()
        {
            //Draw the button labels, w/ active highlighted based on "1" index
            switch (this.type)
            {
                case 1:
                    gfx.FillRectangle(lcdBrush, 6, 33, 22, 10);
                    gfx.DrawString("CPU", lcdFont, activeBrush, 5, 31);
                    gfx.DrawString("Mem", lcdFont, lcdBrush, 42, 31);
                    gfx.DrawString("Disk", lcdFont, lcdBrush, 90, 31);
                    gfx.DrawString("Temp", lcdFont, lcdBrush, 125, 31);
                    break;
                case 2:
                    gfx.FillRectangle(lcdBrush, 43, 33, 29, 10);
                    gfx.DrawString("CPU", lcdFont, lcdBrush, 5, 31);
                    gfx.DrawString("Mem", lcdFont, activeBrush, 42, 31);
                    gfx.DrawString("Disk", lcdFont, lcdBrush, 90, 31);
                    gfx.DrawString("Temp", lcdFont, lcdBrush, 125, 31);
                    break;
                case 3:
                    gfx.FillRectangle(lcdBrush, 90, 33, 26, 10);
                    gfx.DrawString("CPU", lcdFont, lcdBrush, 5, 31);
                    gfx.DrawString("Mem", lcdFont, lcdBrush, 42, 31);
                    gfx.DrawString("Disk", lcdFont, activeBrush, 90, 31);
                    gfx.DrawString("Temp", lcdFont, lcdBrush, 125, 31);
                    break;
                case 4:
                    gfx.FillRectangle(lcdBrush, 125, 33, 33, 10);
                    gfx.DrawString("CPU", lcdFont, lcdBrush, 5, 31);
                    gfx.DrawString("Mem", lcdFont, lcdBrush, 42, 31);
                    gfx.DrawString("Disk", lcdFont, lcdBrush, 90, 31);
                    gfx.DrawString("Temp", lcdFont, activeBrush, 125, 31);
                    break;
                default:
                    gfx.FillRectangle(lcdBrush, 6, 33, 22, 10);
                    gfx.DrawString("CPU", lcdFont, activeBrush, 5, 31);
                    gfx.DrawString("Mem", lcdFont, lcdBrush, 42, 31);
                    gfx.DrawString("Disk", lcdFont, lcdBrush, 90, 31);
                    gfx.DrawString("Temp", lcdFont, lcdBrush, 125, 31);
                    break;
            }
        }

        private void drawCurrentMeter()
        {
            //Draw the button labels, w/ active highlighted based on "1" index
            switch (this.type)
            {
                case 1:
                    gfx.DrawString(data.currentMetric(1).ToString() + "%", lcdFontAlt, lcdBrush, 136, 0);
                    break;
                case 2:
                    gfx.DrawString(data.currentMetric(2).ToString() + "%", lcdFontAlt, lcdBrush, 136, 0);
                    break;
                case 3:
                    gfx.DrawString((this.points[1].Y * 3.3).ToString(), lcdFontAlt, lcdBrush, 0, 0);
                    break;
                case 4:
                    gfx.DrawString(data.currentMetric(4).ToString() + "°C", lcdFontAlt, lcdBrush, 132, 0);
                    break;
                default:
                    gfx.DrawString((this.points[1].Y * 3.3).ToString(), lcdFontAlt, lcdBrush, 0, 0);
                    break;
            }
        }
		
		//Release graphics resources
        public void emptyGraphics()
        {
            gfx.Dispose();
        }

    }
}
