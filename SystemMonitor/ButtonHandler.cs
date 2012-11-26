using System;
using System.Collections.Generic;
using System.Text;

namespace SystemMonitor
{
    class ButtonHandler
    {
        private delegate void ClickHandler(int button);
        private event ClickHandler Click;
        private int lcdDevice;
        private uint lcdButtons;
        private View view;

        public ButtonHandler(View v)
        {
            lcdDevice = v.returnDeviceId;
            view = v;
        }

        public void buttonListener(Object o)
        {
            lcdButtons = DMcLgLCD.LcdReadSoftButtons(lcdDevice);

            if ((lcdButtons & DMcLgLCD.LGLCD_BUTTON_1) == DMcLgLCD.LGLCD_BUTTON_1) 
                buttonClicked(1);
            else if ((lcdButtons & DMcLgLCD.LGLCD_BUTTON_2) == DMcLgLCD.LGLCD_BUTTON_2)
                buttonClicked(2);
            else if ((lcdButtons & DMcLgLCD.LGLCD_BUTTON_3) == DMcLgLCD.LGLCD_BUTTON_3)
                buttonClicked(3);
            else if ((lcdButtons & DMcLgLCD.LGLCD_BUTTON_4) == DMcLgLCD.LGLCD_BUTTON_4)
                buttonClicked(4);
            else return;
        }

        public void Subscribe(ButtonHandler btnHand)
        {
            btnHand.Click += new ClickHandler(buttonClicked);
        }

        private void buttonClicked(int button)
        {
            view.Type = button;
            view.drawScreen();
        }
    }
}
