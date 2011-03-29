using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azavea.NijPredictivePolicing.Common.Data
{
    /// <summary>
    /// Represents a column, and contains data about where that column is located in a row and what type of data it contains.  This class contains some error checking, but it can be used in some very weird ways (for example, it doesn't care if columns overlap with each other).  In general, it's your responsibility to use it sensibly.
    /// </summary>
    public class FixedWidthField
    {
        public FixedWidthField() { }

        public FixedWidthField(int start, int end, FixedWidthTypes type, FixedWidthPositions seeker, FixedWidthTerminators terminator)
        {
            Start = start;
            End = end;
            Type = type;
            Seeker = seeker;
            Terminator = terminator;
        }

        public FixedWidthField(int start, int end, FixedWidthTypes type, FixedWidthPositions seeker, FixedWidthTerminators terminator,
            object defaultValue, bool strict) :
            this(start, end, type, seeker, terminator)
        {
            DefaultValue = defaultValue;
            Strict = strict;
        }


        public FixedWidthField(string columnName, string description, int length, int start) :
            this(start, length, FixedWidthTypes.STRING, FixedWidthPositions.FROM_START, FixedWidthTerminators.LENGTH)
        {
            this.ColumnName = columnName;
            this.Description = description;
        }

        public FixedWidthField(string columnName, string description, int length, int start, FixedWidthTypes type) :
            this(start, length, type, FixedWidthPositions.FROM_START, FixedWidthTerminators.LENGTH)
        {
            this.ColumnName = columnName;
            this.Description = description;
        }

        public FixedWidthField(string columnName, string description, int length, int start, FixedWidthTypes type, FixedWidthTerminators term) :
            this(start, length, type, FixedWidthPositions.FROM_START, term)
        {
            this.ColumnName = columnName;
            this.Description = description;
        }


        public FixedWidthField(FixedWidthField old)
        {
            Start = old.Start;
            End = old.End;
            Strict = old.Strict;
            DefaultValue = old.DefaultValue;
            Type = old.Type;
            Seeker = old.Seeker;
            Terminator = old.Terminator;
        }



        public string ColumnName;
        public string Description;


        /// <summary>
        /// Determines where the record starts
        /// </summary>
        public int Start = -1;


        /// <summary>
        /// Determines where the record ends
        /// </summary>
        public int End = -1;

        /// <summary>
        /// What type is this field?
        /// </summary>
        public FixedWidthTypes Type = FixedWidthTypes.STRING;

        /// <summary>
        /// Determines how we interpret Start
        /// </summary>
        public FixedWidthPositions Seeker = FixedWidthPositions.FROM_START;

        /// <summary>
        /// Determines how we interpret End
        /// </summary>
        public FixedWidthTerminators Terminator = FixedWidthTerminators.LENGTH;

        /// <summary>
        /// If we fail to retrieve a value for this field, what should we use instead?  Note this isn't checked against Type, so if you set this to one type and Type to something else, it's your fault when you get type errors.
        /// </summary>
        public object DefaultValue = null;

        /// <summary>
        /// If true, throws exceptions on error.  If false, silently uses DefaultValue and continues.
        /// </summary>
        public bool Strict = true;

        

        /// <summary>
        /// Gets the raw data for this field from a row and updates the current position.  Does not do any parsing.  Throws errors if Strict is false.
        /// </summary>
        /// <param name="row">The row to read from</param>
        /// <param name="currentPos">The current position in the row</param>
        /// <returns>Field data in string form</returns>
        public string GetFieldData(string row, ref int currentPos)
        {
            try
            {
                int myStart = 0;
                int myEnd = 0;
                GetFieldRange(row, currentPos, out myStart, out myEnd);
                currentPos = myEnd;

                return row.Substring(myStart, myEnd - myStart);
            }
            catch (Exception ex)
            {
                if (Strict)
                    throw ex;
                else
                    return DefaultValue.ToString();
            }
        }

        /// <summary>
        /// Gets the parsed object for this field from a row and updates the current position, as determined by Type.
        /// </summary>
        /// <param name="row">The row to read from</param>
        /// <param name="currentPos">The current position in the row</param>
        /// <returns>Field data in an appropriate object</returns>
        public object GetFieldObject(string row, ref int currentPos)
        {
            try
            {
                string data = GetFieldData(row, ref currentPos);
                object result = null;
                switch (this.Type)
                {
                    case FixedWidthTypes.NULL:
                        break;

                    case FixedWidthTypes.STRING:
                        result = data;
                        break;

                    case FixedWidthTypes.INT:
                        result = int.Parse(data);
                        break;

                    case FixedWidthTypes.LONG:
                        result = long.Parse(data);
                        break;

                    case FixedWidthTypes.FLOAT:
                        result = float.Parse(data);
                        break;

                    case FixedWidthTypes.DOUBLE:
                        result = double.Parse(data);
                        break;

                    case FixedWidthTypes.DECIMAL:
                        result = decimal.Parse(data);
                        break;

                    case FixedWidthTypes.DATETIME:
                        result = DateTime.Parse(data);
                        break;

                    default:
                        throw new NotImplementedException("Invalid value for Type");
                }

                return result;
            }
            catch (Exception ex)
            {
                if (Strict)
                    throw ex;
                else
                    return DefaultValue;
            }
        }

        private void GetFieldRange(string row, int currentPos, out int start, out int end)
        {
            if (string.IsNullOrEmpty(row))
            {
                start = end = 0;
                return;
            }

            int rowSize = row.Length;

            switch (this.Seeker)
            {
                case FixedWidthPositions.FROM_START:
                    if (Start < 0)
                        throw new ArgumentOutOfRangeException("Start", Start, "Seeker is FROM_START, but Start < 0");
                    start = Start;
                    if (start >= rowSize)
                        throw new ArgumentOutOfRangeException("Start", Start, "Seeker is FROM_START, but value of Start results in out of range index");
                    break;

                case FixedWidthPositions.FROM_END:
                    if (Start < 1)
                        throw new ArgumentOutOfRangeException("Start", Start, "Seeker is FROM_END, but Start < 1");
                    start = rowSize - Start;
                    if (start >= rowSize)
                        throw new ArgumentOutOfRangeException("Start", Start, "Seeker is FROM_END, but value of Start results in out of range index");
                    break;

                case FixedWidthPositions.FROM_CURRENT:
                    start = currentPos + Start;
                    if (start < 0 || start >= rowSize)
                        throw new ArgumentOutOfRangeException("Start", Start, "Seeker is FROM_CURRENT, but value of current position and Start results in out of range index");
                    break;

                default:
                    throw new NotImplementedException("Invalid Seeker value");
            }

            switch (this.Terminator)
            {
                case FixedWidthTerminators.NEWLINE:
                    end = rowSize;
                    break;

                case FixedWidthTerminators.LENGTH:
                    if (End < 0)
                        throw new ArgumentOutOfRangeException("End", End, "Terminator is LENGTH, but End < 0");
                    end = start + End;
                    if (end > rowSize)
                        throw new ArgumentOutOfRangeException("End", End, "Terminator is LENGTH, but value of current position results in out of range index");
                    break;

                case FixedWidthTerminators.INDEX:
                    if (End < 0 || End > rowSize)
                        throw new ArgumentOutOfRangeException("End", End, "Terminator is INDEX, but End is out of range");
                    else if (End < start)
                        throw new ArgumentOutOfRangeException("End", End, "Terminator is INDEX, but End < Start");
                    else
                        end = End;
                    break;

                default:
                    throw new NotImplementedException("Invalid Terminator Value");
            }
        }
    }


    /// <summary>
    /// Determines how the field is parsed and what type of object is returned
    /// </summary>
    public enum FixedWidthTypes
    {
        NULL = 0,
        STRING,
        INT,
        LONG,
        FLOAT,
        DOUBLE,
        DECIMAL,
        DATETIME
    }


    /// <summary>
    /// Determines where to start reading an entry, or how Start is interpreted
    /// </summary>
    public enum FixedWidthPositions
    {
        /// <summary>
        /// Start is number of characters from start of row.  Start must be positive.
        /// </summary>
        FROM_START = 0,

        /// <summary>
        /// Start is number of characters from end of row.  Start must be positive.
        /// </summary>
        FROM_END,

        /// <summary>
        /// Start is number of characters from current position.  
        /// Start may be positive or negative to denote direction.
        /// </summary>
        FROM_CURRENT
    }

    /// <summary>
    /// Determines where to stop reading an entry, or how End is interpreted
    /// </summary>
    public enum FixedWidthTerminators
    {
        /// <summary>
        /// Field ends with newline, End is ignored
        /// </summary>
        NEWLINE,

        /// <summary>
        /// Field has a static length, End denotes the size of the field
        /// </summary>
        LENGTH,

        /// <summary>
        /// End is a offset index from the start of the line corresponding to the character AFTER the last character to read for this field.  If End &lt; Start, an exception is thrown.
        /// </summary>
        INDEX
    }


}
