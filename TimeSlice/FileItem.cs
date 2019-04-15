using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace TimeSlice
{
    internal class FileItem
    {
        public static double Discrete { get; private set; } = 1;
        public static int MaxAmp { get; private set; } = 2508;

        private readonly FileInfo _path;

        public string Path => _path.Name;
        public int Length => Bscan.Count;
        public double MaxTime => GetMaxTime();

        [Browsable(false)]
        public List<List<int>> Bscan { get; } = new List<List<int>>();


        public FileItem(string path)
        {
            _path = new FileInfo(path);
            ReadFile();
        }


        private void ReadFile()
        {
            if (!File.Exists(_path.FullName))
                throw new FileNotFoundException("Файл не найден", Path);

            using (var fs = new FileStream(_path.FullName, FileMode.Open))
            using (var sr = new StreamReader(fs))
            {
                sr.ReadLine();

                while (!sr.EndOfStream)
                {
                    var str = sr.ReadLine();
                    var vals = str.Split(';');
                    var ascan = Convert.ToInt32(vals[0]);

                    var amp = Convert.ToInt32(vals[2]);

                    if (Bscan.Count == ascan)
                        Bscan.Add(new List<int>());
                    Bscan[ascan].Add(amp);
                }
            }
            
        }

        private double GetMaxTime()
        {
            var max = 0.0;
            foreach (var ascan in Bscan)
                if (ascan.Count * Discrete > max)
                    max = ascan.Count * Discrete;
            return max;
        }
    }
}
