﻿//Copyright (c) <2012> <Ling Xing>
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
//and associated documentation files (the "Software"), to deal in the Software without restriction, 
//including without limitation the rights to use, copy, modify, merge, publish, distribute, 
//sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all copies or 
//substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
//PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
//FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace NiosParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region Properties

        public String InputPath { get; set; }
        public String OutputPath { get; set; }
        public ObservableCollection<Run> Results { get; set; }

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            InputPath = string.Empty;
            OutputPath = string.Empty;
            Results = new ObservableCollection<Run>();
        }

        #endregion

        #region Methods

        private Boolean Validate()
        {
            String outputDirectory = OutputPath.Substring(0, OutputPath.LastIndexOf('\\') == -1 ? 0:OutputPath.LastIndexOf('\\'));
            return File.Exists(InputPath) && Directory.Exists(outputDirectory);
        }

        private Boolean Parse()
        {
            Results = new ObservableCollection<Run>();

            try
            {
                using (StreamReader sr = new StreamReader(InputPath))
                {
                    String line;
                    String parsedLine;
                    Run run = new Run();

                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.StartsWith("missed"))
                        {
                            parsedLine = Regex.Match(line, @"\d+").Value;
                            run.PulseMissed = Int32.Parse(parsedLine);
                        }
                        else if(line.StartsWith("period"))
                        {
                            parsedLine = Regex.Match(line, @"\d+").Value;
                            run.Period = Int32.Parse(parsedLine);
                        }
                        else if(line.StartsWith("duty cycle"))
                        {
                            parsedLine = Regex.Match(line, @"\d+").Value;
                            run.DutyCycle = Int32.Parse(parsedLine);
                        }
                        else if(line.StartsWith("latency resolution"))
                        {
                            parsedLine = Regex.Match(line, @"\d+").Value;
                            run.LatencyResolution = Int32.Parse(parsedLine);
                        }
                        else if(line.StartsWith("max latency"))
                        {
                            if (line.EndsWith("period"))
                            {
                                parsedLine = Regex.Match(line, @"\d+").Value;
                                run.LatencyByPeriod = Int32.Parse(parsedLine);
                            }
                            else
                            {
                                parsedLine = Regex.Match(line, @"\d+").Value;
                                run.LatencyByTime = Int32.Parse(parsedLine);
                            }
                        }
                        else if (line.StartsWith("task"))
                        {
                            parsedLine = Regex.Match(line, @"\d+").Value;
                            run.TaskUnitsProcessed = Int32.Parse(parsedLine);
                        }
                        else if (line.StartsWith("background"))
                        {
                            parsedLine = Regex.Match(line, @"\d+").Value;
                            run.BackgroundBatchSize = Int32.Parse(parsedLine);
                        }
                        else if (line.StartsWith("Exiting"))
                        {
                            Results.Add(run);
                            run = new Run();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            return true;
        }

        private Boolean SaveToFile()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Period(ms),Duty Cycle(%),Latency Resolution(ns),Background Size,Missed(pulses),Max Latency(1024th of a period),Max Latency(us),Task Unit Processed(units),");
            foreach (Run run in Results)
            {
                builder.AppendLine(String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", run.Period, run.DutyCycle, run.LatencyResolution, run.BackgroundBatchSize, run.PulseMissed, run.LatencyByPeriod, run.LatencyByTime, run.TaskUnitsProcessed));
            }
            System.IO.StreamWriter writer = new System.IO.StreamWriter(OutputPath);
            writer.WriteLine(builder.ToString());

            writer.Close();

            return true;
        }

        #endregion

        #region Event Handler

        private void _bnIO_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openDialog;
            SaveFileDialog saveDialog;

            if (((Button)sender).Name.Equals("_bnInput"))
            {
                openDialog = new OpenFileDialog();
                openDialog.Multiselect = false;
                openDialog.Filter = "Text Files | *.txt;*.dat|All Files |*.*";
                openDialog.DefaultExt = "txt";

                if (openDialog.ShowDialog().Value)
                {
                    InputPath = openDialog.FileName;
                    OnPropertyChanged("InputPath");
                }
            }
            else
            {
                saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV Files | *.csv";
                saveDialog.DefaultExt = "csv";
                saveDialog.AddExtension = true;

                if (saveDialog.ShowDialog().Value)
                {
                    OutputPath = saveDialog.FileName;
                    OnPropertyChanged("OutputPath");
                }
            }

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Parse_Click(object sender, RoutedEventArgs e)
        {
            if (Validate())
            {
                if (Parse())
                {
                    if (SaveToFile())
                    {
                        MessageBox.Show("Parsing Completed...");
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid Parameters");
            }
        }

        #endregion

        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

    public class Run
    {
        public int Period { get; set; }
        public int DutyCycle { get; set; }
        public int LatencyResolution { get; set; }
        public int PulseMissed { get; set; }
        public int LatencyByPeriod { get; set; }
        public int LatencyByTime { get; set; }
        public int TaskUnitsProcessed { get; set; }
        public int BackgroundBatchSize { get; set; }

        public Run()
        {
            Period = 0;
            DutyCycle = 0;
            LatencyResolution = 0;
            PulseMissed = 0;
            LatencyByPeriod = 0;
            LatencyByTime = 0;
            TaskUnitsProcessed = 0;
            BackgroundBatchSize = -1;
        }
    }

}