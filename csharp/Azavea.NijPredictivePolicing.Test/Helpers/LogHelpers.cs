/*
  Copyright (c) 2011 Azavea, Inc.
 
  This file is part of _SOLUTIONNAME_.

  _SOLUTIONNAME_ is free software: you can redistribute it and/or modify
  it under the terms of the GNU Lesser General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  _SOLUTIONNAME_ is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Lesser General Public License for more details.

  You should have received a copy of the GNU Lesser General Public License
  along with _SOLUTIONNAME_.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Azavea.NijPredictivePolicing.Test.Helpers
{
    public static class LogHelpers
    {
        public static ILog ResetLogger(Type declaringType)
        {
            log4net.LogManager.ResetConfiguration();
            log4net.Config.BasicConfigurator.Configure();
            return log4net.LogManager.GetLogger(declaringType);
        }

        public static ILog ResetLogger(string declaringType)
        {
            log4net.LogManager.ResetConfiguration();
            log4net.Config.BasicConfigurator.Configure();
            return log4net.LogManager.GetLogger(declaringType);
        }
    }
}
