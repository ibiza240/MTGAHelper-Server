using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTGAHelper.Lib.OutputLogParser.InMatchTracking;
using MTGAHelper.Lib.OutputLogParser.Models.GRE.MatchToClient;
using MTGAHelper.Lib.OutputLogParser.Models.GRE.MatchToClient.GameStateMessage;
using Newtonsoft.Json;

namespace MTGAHelper.Tool.OutputLogMessageViewer.ViewModels
{
    public class MainWindowVM : ObservableObject
    {
        public ICollection<OutputLogMessagesGroupVM> GroupedMessages { get; private set; } = Array.Empty<OutputLogMessagesGroupVM>();

        public string SelectedPart { get; set; } = "";
        public string SelectedSubPart { get; set; } = "";

        public string SimulationMatchId { get; set; } = "";
        public string SimulationGoto { get; set; } = "";
        public string SimulationLastPart { get; set; }
        public string SimulationResult { get; set; } = "test";
        public int SimulationCurrentMessageId { get; set; } = 0;
        public string SimulationOutputFile { get; set; } = "";

        public int SimulationAllCurrentGroupId { get; set; } = 0;

        private readonly InGameTracker2 simulationTracker;

        public MainWindowVM(InGameTracker2 inMatchTracker)
        {
            simulationTracker = inMatchTracker;
        }

        public void RefreshTree(ICollection<OutputLogMessagesGroupVM> groupedMessages)
        {
            GroupedMessages = groupedMessages;
            RaisePropertyChangedEvent(nameof(GroupedMessages));
        }

        internal void GoTo()
        {
            Simulate(Convert.ToInt32(SimulationGoto));
        }

        public void SelectPart(OutputLogMessagesGroupVM toSelect)
        {
            if (toSelect.Message == null)
                return;

            SelectedPart = toSelect.Message.Part ?? "";
            RaisePropertyChangedEvent(nameof(SelectedPart));

            SelectedSubPart = "";
            if (toSelect.Message.SubPart != null)
            {
                SelectedSubPart = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(toSelect.Message.SubPart), Formatting.Indented);
            }
            RaisePropertyChangedEvent(nameof(SelectedSubPart));
        }

        internal void SimulationInit()
        {
            if (SimulationMatchId == "" || SimulationMatchId == "all" || GroupedMessages.FirstOrDefault(i => i.Name.Contains(SimulationMatchId)) != null)
            {
                if (File.Exists(SimulationOutputFile))
                    File.Delete(SimulationOutputFile);

                SimulationLastPart = null;
                SimulationCurrentMessageId = 0;
                SimulationAllCurrentGroupId = 0;
                SimulationResult = "";
                simulationTracker.Reset();
                RaisePropertyChangedEvent(nameof(SimulationResult));
            }
        }

        internal void Simulate(int goTo = 0)
        {
            var continueReading = true;
            while (continueReading)
            {
                OutputLogMessagesGroupVM groupOfMessages;

                if (SimulationMatchId == "")
                    groupOfMessages = GroupedMessages.First();
                else if (SimulationMatchId == "all")
                {
                    if (SimulationCurrentMessageId >= GroupedMessages.Skip(SimulationAllCurrentGroupId).First().Children.Count)
                    {
                        SimulationCurrentMessageId = 0;
                        SimulationAllCurrentGroupId++;
                    }

                    groupOfMessages = GroupedMessages.Skip(SimulationAllCurrentGroupId).First();
                }
                else
                    groupOfMessages = GroupedMessages.FirstOrDefault(i => i.Name.Contains(SimulationMatchId));

                OutputLogMessagesGroupVM message = null;

                var newMsg = groupOfMessages.Children[SimulationCurrentMessageId].Message.Part;

                if (newMsg != SimulationLastPart)
                {
                    SimulationLastPart = newMsg;
                    using (var fs = new FileStream(SimulationOutputFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(SimulationLastPart);
                    }
                }
                continueReading = false;
                //while (SimulationCurrentMessageId < groupOfMessages.Children.Count &&
                //    (
                //        SimulationLastPart == null ||
                //        groupOfMessages.Children[SimulationCurrentMessageId].Message.Part == SimulationLastPart ||
                //        groupOfMessages.Children[SimulationCurrentMessageId].Message is IgnoredResult
                //    ))
                //{
                //    //if (SimulationCurrentMessageId == 900) Debugger.Break();

                //    message = groupOfMessages.Children[SimulationCurrentMessageId];

                //    //if (message.Message is Lib.IO.Reader.MtgaOutputLog.UnityCrossThreadLogger.DuelSceneSideboardingStopResult) Debugger.Break();
                //    //if (message.Message?.Part?.Contains("tchDoor_ConnectedToGRE_Waiting -> Playing") == true) Debugger.Break();

                //    //if (message.Message is IgnoredResult == false && message.Message is IgnoredMatchResult == false)
                //    {
                //        //if (message.Message.Part.Contains("637043644063348386")) Debugger.Break();

                //        simulationTracker.ProcessMessage(message.Message);

                //        if (message.Message is IgnoredResult == false && message.Message is IgnoredMatchResult == false)
                //            SimulationLastPart = groupOfMessages.Children[SimulationCurrentMessageId].Message.Part;
                //    }

                //    SimulationCurrentMessageId++;
                //}

                ////if (message.Message.Timestamp == 637040234351148140) System.Diagnostics.Debugger.Break();

                //SimulationResult = simulationTracker.ToString();
                //if (string.IsNullOrWhiteSpace(SimulationOutputFile) == false)
                //{
                //    using (var fs = new FileStream(SimulationOutputFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                //    using (var sw = new StreamWriter(fs))
                //    {
                //        sw.WriteLine(SimulationLastPart);
                //    }
                //}
                message = groupOfMessages.Children[SimulationCurrentMessageId];
                simulationTracker.ProcessMessage(message.Message);
                SimulationCurrentMessageId++;
                SimulationResult = simulationTracker.ToString();

                continueReading = goTo == 0 ? false :
                message?.Message is GameStateMessageResult == false || (message?.Message is GameStateMessageResult gsm && gsm.Raw.msgId < goTo); //SimulationLastPart.Contains($"\"msgId\": {goTo}") == false;
            }

            RaisePropertyChangedEvent(nameof(SimulationResult));
            RaisePropertyChangedEvent(nameof(SimulationLastPart));
        }
    }
}