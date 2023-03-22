using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using MTGAHelper.Lib.OutputLogParser;
using MTGAHelper.Lib.OutputLogParser.Models;
using MTGAHelper.Tool.OutputLogMessageViewer.ViewModels;

namespace MTGAHelper.Tool.OutputLogMessageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ReaderMtgaOutputLog readerMtgaOutputLog;
        readonly OutputLogMessagesBatcher outputLogMessagesBatcher;

        readonly SimulationWindow simulationWindow;

        readonly MainWindowVM vm;

        public MainWindow(
            ReaderMtgaOutputLog readerMtgaOutputLog,
            OutputLogMessagesBatcher outputLogMessagesBatcher,
            MainWindowVM vm,
            SimulationWindow simulationWindow)
        {
            InitializeComponent();

            this.readerMtgaOutputLog = readerMtgaOutputLog;
            this.outputLogMessagesBatcher = outputLogMessagesBatcher;

            this.simulationWindow = simulationWindow;

            this.vm = vm;
            DataContext = vm;

            this.simulationWindow.SetVM(vm);
        }

        private void MenuItemSimulation_Click(object sender, RoutedEventArgs e)
        {
            simulationWindow.Show();
        }

        private async void MenuItemOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() != true)
                return;

            await using var stream = new FileStream(dialog.FileName, FileMode.Open);
            var messages = await readerMtgaOutputLog.ProcessIntoMessagesAsync("OutputLogMessageViewer", stream);

            //var groups = new List<OutputLogMessagesGroupVM>();

            //var currentGroup = new OutputLogMessagesGroupVM("General");
            //foreach (var m in messages)
            //{
            //    if (m is StateChangedResult stateChanged && stateChanged.Raw.Contains("-> MatchCompleted"))
            //    {
            //        // Messages that signal the end of the current group

            //        currentGroup.Children.Add(new OutputLogMessagesGroupVM(m.GetType().ToString().Split(".").Last(), m));
            //        groups.Add(currentGroup);
            //        currentGroup = new OutputLogMessagesGroupVM($"General");
            //    }
            //    else
            //    {
            //        if (m is MatchCreatedResult matchCreated)
            //        {
            //            // Messages that signal the start of a new group

            //            groups.Add(currentGroup);
            //            currentGroup = new OutputLogMessagesGroupVM($"Match {matchCreated.MatchId}");
            //        }

            //        currentGroup.Children.Add(new OutputLogMessagesGroupVM(m.GetType().ToString().Split(".").Last(), m));
            //    }
            //}
            //groups.Add(currentGroup);

            var msgsInBatches = outputLogMessagesBatcher.BatchMessages(messages);
            var groups = msgsInBatches
                .Select(i => BuildVM(i))
                .ToArray();

            vm.RefreshTree(groups);
        }

        private OutputLogMessagesGroupVM BuildVM(KeyValuePair<string, IMtgaOutputLogPartResult[]> i)
        {
            var nameParent = i.Key;

            var datetimeParent = i.Value.FirstOrDefault(r => r.LogDateTime != (default(DateTime)));
            if (datetimeParent != null)
                nameParent = $"{datetimeParent.LogDateTime:HH:mm:ss} {nameParent}";

            return new OutputLogMessagesGroupVM(nameParent)
            {
                Children = i.Value.Select(x =>
                {
                    var name = x.GetType().ToString().Split(".").Last();

                    if (x.LogDateTime != default(DateTime))
                        name = $"{x.LogDateTime:HH:mm:ss} {name}";

                    return new OutputLogMessagesGroupVM(name, x);
                }).ToArray()
            };
        }

        void treeItem_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TreeViewItem item = sender as TreeViewItem;
                //you can access item properties eg item.Header etc. 
                //your logic here 

                vm.SelectPart(item.DataContext as OutputLogMessagesGroupVM);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
