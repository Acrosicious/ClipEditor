using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace VideoClipEditor
{
    public class MoveToPoint_AndDrag_Slider : Slider
    {
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //OnPreviewMouseLeftButtonDown(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
                OnThumbDragStarted(new System.Windows.Controls.Primitives.DragStartedEventArgs(0, 0));
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //OnPreviewMouseLeftButtonDown(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
                OnThumbDragStarted(new System.Windows.Controls.Primitives.DragStartedEventArgs(0, 0));
                //Console.WriteLine("Enter method");
                
            }
        }
    }
}
