using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net.Appender;

namespace Azavea.NijPredictivePolicing.AcsAlchemistGui
{
    public class TextboxAppender : AppenderSkeleton
    {
        protected TextBox _control;

        public TextboxAppender(TextBox control)
        {
            this._control = control;
        }

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            if (!this._control.InvokeRequired)
            {
                this._control.AppendText(loggingEvent.RenderedMessage + Environment.NewLine);
            }
            else
            {
                this._control.Invoke((MethodInvoker)delegate
                {
                    this._control.AppendText(loggingEvent.RenderedMessage + Environment.NewLine);
                });
            }
        }


    }
}
