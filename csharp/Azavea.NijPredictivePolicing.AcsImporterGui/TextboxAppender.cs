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
            this._control.AppendText(loggingEvent.RenderedMessage + Environment.NewLine);
        }
    }
}
