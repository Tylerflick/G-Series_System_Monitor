using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;


namespace SystemMonitor
{
    class View
    {
        Model data;
        private int device, type;
        Point[] points;
        //Instantiate bitmap, graphics object, & various properties used for drawing on the canvas
        private Bitmap LCD = new Bitmap(160, 43);
        private Graphics gfx;
        private Font lcdFont = new Font("Calibri", 9, FontStyle.Regular, GraphicsUnit.Point);
        private Font lcdFontSmall = new Font("Tahoma", 7, FontStyle.Regular, GraphicsUnit.Point);
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

        public int returnDeviceId
        {
            get
            {
                return device;
            }
        }

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

        public void drawScreen()
        {
            switch (this.type)
            {
                case 1:
                    points = data.cpuGraph;
                    break;
                case 2:
                    points = data.memoryGraph;
                    break;
                case 3:
                    points = data.netGraph;
                    break;
                case 4:
                    points = data.diskGraph;
                    break;
                default:
                    points = data.cpuGraph;
                    type = 1;
                    break;
            }

            //Clear the screen
            gfx.Clear(Color.White);
            
            //Draw borders for graph
            gfx.DrawLine(lcdPen, 17, 32, 17, 0);
            gfx.DrawLine(lcdPen, 17, 32, 150, 32);
            
            //Draw legend for graph
            gfx.DrawString("100", lcdFontSmall, lcdBrush, 0, 0);
            gfx.DrawString("0", lcdFontSmall, lcdBrush, 10, 24);
            
            //This is need becuase if the array of points only contains one point it will cause an error in the method
            if (points.Count() > 1) gfx.DrawLines(lcdPen, points);
            
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
                    gfx.DrawString("Net", lcdFont, lcdBrush, 90, 31);
                    gfx.DrawString("Disk", lcdFont, lcdBrush, 130, 31);
                    break;
                case 2:
                    gfx.FillRectangle(lcdBrush, 43, 33, 29, 10);
                    gfx.DrawString("CPU", lcdFont, lcdBrush, 5, 31);
                    gfx.DrawString("Mem", lcdFont, activeBrush, 42, 31);
                    gfx.DrawString("Net", lcdFont, lcdBrush, 90, 31);
                    gfx.DrawString("Disk", lcdFont, lcdBrush, 130, 31);
                    break;
                case 3:
                    gfx.FillRectangle(lcdBrush, 90, 33, 24, 10);
                    gfx.DrawString("CPU", lcdFont, lcdBrush, 5, 31);
                    gfx.DrawString("Mem", lcdFont, lcdBrush, 42, 31);
                    gfx.DrawString("Net", lcdFont, activeBrush, 90, 31);
                    gfx.DrawString("Disk", lcdFont, lcdBrush, 130, 31);
                    break;
                case 4:
                    gfx.FillRectangle(lcdBrush, 131, 33, 26, 10);
                    gfx.DrawString("CPU", lcdFont, lcdBrush, 5, 31);
                    gfx.DrawString("Mem", lcdFont, lcdBrush, 42, 31);
                    gfx.DrawString("Net", lcdFont, lcdBrush, 90, 31);
                    gfx.DrawString("Disk", lcdFont, activeBrush, 130, 31);
                    break;
            }
        }

        public void emptyGraphics()
        {
            gfx.Dispose();
        }

    }
}
