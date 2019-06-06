using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WaterTotalizer
{
    class Program
    {
        /// <summary>
        /// Read all the text lines from a file passed as parameter
        /// </summary>
        /// <param name="fileLines"></param>
        /// <returns></returns>
        private static IEnumerable<string> _GetLinesFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = "samples.csv";
            }

            // check if the file exists
            var file = new FileInfo(filePath);

            if (!file.Exists)
                throw new Exception("Check if the fucking file existes!!!");

            return File.ReadAllLines(filePath);
        }

        /// <summary>
        ///  Receives the lines readed from the csv file and transform in a list of samples
        /// </summary>
        /// <param name="fileLines"></param>
        /// <returns></returns>
        private static IEnumerable<FlowRateSample> _GetSamplesFromCsvFile(IEnumerable<string> fileLines)
        {
            foreach (var line in fileLines)
            {
                var splitedLine = line.Split(";");
                int curTimeStamp = 0;
                decimal curFlowRate = 0;

                if (splitedLine.Length != 2)
                    throw new ApplicationException("The is in a wrong format");

                if (!int.TryParse(splitedLine.FirstOrDefault(), out curTimeStamp))
                    throw new ApplicationException("The time stamp need to be a fucking integer number!!!");

                if (!decimal.TryParse(splitedLine.LastOrDefault(), out curFlowRate))
                    throw new ApplicationException("The flowrate need to be a fucking decimal number!!!");

                yield return new FlowRateSample() { Minute = curTimeStamp, ActualFlowRateInLitersByMin = curFlowRate };
            }
        }

        /// <summary>
        /// Helpper method to calculate the value of the delta x of the defined integrate
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <returns></returns>
        private static decimal _CalculateDeltaX(decimal minX, decimal maxX, int intervalCounts)
        {
            if (intervalCounts <= 0)
                throw new DivideByZeroException("Are you fucking stupid? the numeber of intervals cannot be 0 or less");

            return (maxX - minX) / intervalCounts;
        }

        /// <summary>
        /// calculate the ammout of water based on samples collected from a flow metter device
        /// </summary>
        /// <param name="readedSamples"></param>
        /// <returns></returns>
        private static decimal _CalculateAmountOfWater(List<FlowRateSample> readedSamples)
        {
            if (readedSamples.Count() <= 1)
                throw new ApplicationException("The file is too small!!!");

            // order the collection and the get the max and min of the interval
            readedSamples = readedSamples.OrderBy(a => a.Minute).ToList();
            var minValueForXAxys = readedSamples.FirstOrDefault().Minute;
            var maxValueForXAxys = readedSamples.LastOrDefault().Minute;

            //calculate the value of n
            var intervalCounts = readedSamples.Count() - 1;

            //calculate the delta X value
            var deltaX = _CalculateDeltaX(minValueForXAxys, maxValueForXAxys, intervalCounts);

            // now we sumarize all this fuccking sheet
            decimal totalizer = 0;

            for (var i = 0; i < readedSamples.Count(); i++)
            {
                // for the first one and the last one we don`t to multiply by two the function result
                if (i == 0 || i == readedSamples.Count() - 1)
                {
                    totalizer += readedSamples[i].ActualFlowRateInLitersByMin;
                }
                else
                {
                    totalizer += (2 * readedSamples[i].ActualFlowRateInLitersByMin);
                }
            }

            return deltaX * totalizer;
        }

        /// <summary>
        /// Main function of the program
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                // get the csv file path
                Console.WriteLine("Input the csv file path or leave empty to use the defualt!");
                var csvFilePath = Console.ReadLine();
                var readedLines = _GetLinesFromFile(csvFilePath);

                // get samples from the lines
                var readedSamples = _GetSamplesFromCsvFile(readedLines);

                // calculate the total amount of water
                var totalOfWater = _CalculateAmountOfWater(readedSamples.ToList());
                Console.WriteLine("Total of Water is {0:n3} liters", totalOfWater);
                Console.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    class FlowRateSample
    {
        public int Minute { get; set; }
        public decimal ActualFlowRateInLitersByMin { get; set; }
    }
}
