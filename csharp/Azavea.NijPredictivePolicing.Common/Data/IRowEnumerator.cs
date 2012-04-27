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

namespace Azavea.NijPredictivePolicing.Common.Data
{
    /// <summary>
    /// Just like an IEnumerator for an array of strings, except this gives us a list of column names too
    /// </summary>
    public interface IRowEnumerator : IEnumerator<List<string>>
    {
        /// <summary>
        /// Get a list of column names for this enumerator (so we know what order the values are in)
        /// </summary>
        List<string> GetColumns();
    }
}
