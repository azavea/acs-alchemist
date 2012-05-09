/*
  Copyright (c) 2012 Azavea, Inc.
 
  This file is part of ACS Alchemist.

  ACS Alchemist is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  ACS Alchemist is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with ACS Alchemist.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net.Appender;

namespace Azavea.NijPredictivePolicing.AcsAlchemistGui
{
    /// <summary>
    /// Simple helper class so we can append our log to the form in a thread safe way
    /// </summary>
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
