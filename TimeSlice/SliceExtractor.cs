using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace TimeSlice
{
    internal class SliceExtractor
    {

        public void WriteFile(string filename, List<FileItem> bscans, double time)
        {
            var timeSlice = ExtractTimeSlice(bscans, time);
            SaveFile(filename, timeSlice);
        }

        private void SaveFile(string filename, List<List<int>> timeSlice)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Time slice");

            var k = 1;
            for (int i = 0; i < timeSlice.Count; i++)
            for (int j = 0; j < timeSlice[i].Count; j++)
            {
                worksheet.Cell(k, 1).Value = i;
                worksheet.Cell(k, 2).Value = j;
                worksheet.Cell(k, 3).Value = timeSlice[i][j].ToString();
                ++k;
            }

            workbook.SaveAs(filename);

        }

        private List<List<int>> ExtractTimeSlice(List<FileItem> bscans, double time)
        {
            if (bscans == null)
                throw new ArgumentNullException(nameof(bscans));
            if (bscans.All(x => x.MaxTime < time) || time < 0)
                throw new ArgumentException(nameof(time));

            var data = new List<List<int>>(bscans.Count());
            for (int i = 0; i < bscans.Count(); i++)
            {
                data.Add(new List<int>());
                foreach (var ascan in bscans[i].Bscan)
                {
                    var index = (int)(time * FileItem.Discrete);
                    data[i].Add(ascan.Count > index ? ascan[index] : 0);
                }
            }

            return data;
        }




    }
}
