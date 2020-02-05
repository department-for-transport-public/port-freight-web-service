using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PortFreight.FileProcess.GESMES
{
    public class GesmesHelpers
    {
        public string GesmesFile { get; set; }
        private string _dataItem;
        public string[] Lines;

        private string getSplit(string whichDataItem)
        {
            return this.GesmesFile.Replace(" ", "").Split(
                new string[] { whichDataItem },
                StringSplitOptions.None
            )[1].Split('\'')[0].Trim();
        }

        public string GetDataItem(string whichDataItem)
        {
            string dataRow = getSplit(whichDataItem);
            switch (whichDataItem)
            {
                case "UNH":
                case "UNOC:3":
                    _dataItem = dataRow.Split('+', '+')[1];
                    break;
                case "DSI":
                    _dataItem = dataRow.Split('+', '\'')[1];
                    break;
                case "UNB":
                    _dataItem = dataRow.Substring(dataRow.LastIndexOf('+') + 1);
                    break;
                case "ARR":
                    _dataItem = dataRow.Split("++", '\'')[1];
                    break;
                default:
                    return _dataItem;
            }
            return _dataItem;
        }

        public sbyte GetAmmendment(string whichDataItem)
        {
            string dataRow = getSplit(whichDataItem);
            return Convert.ToSByte(dataRow.Contains("142+8"));
        }

        public int GetNumberOfRecords()
        {
            Lines = Split("'", this.GesmesFile);
            Array.Resize(ref Lines, Lines.Length - 1);
            return Lines.Count(row => row.Substring(1, 3) == "ARR");
        }

        public string[] Split(string delim, string dataItem)
        {
            return Regex.Split(dataItem, delim);
        }
    }
}
