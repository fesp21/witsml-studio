﻿using System;
using System.IO;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Energistics.Common;
using Microsoft.Win32;
using PDS.Framework;
using PDS.Witsml.Studio.Runtime;

namespace PDS.Witsml.Studio.Plugins.DataReplay.ViewModels.Simulation
{
    public class SimulationViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private static readonly string PluginVersion = typeof(SimulationViewModel).GetAssemblyVersion();

        public SimulationViewModel(IRuntimeService runtime)
        {
            Runtime = runtime;
            Model = new Models.Simulation()
            {
                Version = PluginVersion
            };
        }

        public IRuntimeService Runtime { get; private set; }

        private Models.Simulation _model;

        public Models.Simulation Model
        {
            get { return _model; }
            set
            {
                if (!ReferenceEquals(_model, value))
                {
                    _model = value;
                    NotifyOfPropertyChange(() => Model);
                }
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            ActivateItem(new GeneralViewModel());
            Items.Add(new ChannelsViewModel(Runtime));
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                foreach (var child in Items.ToArray())
                {
                    this.CloseItem(child);
                }
            }

            base.OnDeactivate(close);
        }

        public void Save()
        {
            var dialog = new SaveFileDialog()
            {
                Title = "Save Simulation Configuration Settings...",
                Filter = "JSON Files|*.json;*.js|All Files|*.*",
                DefaultExt = ".json",
                AddExtension = true,
                FileName = DisplayName
            };

            if (dialog.ShowDialog(Application.Current.MainWindow).GetValueOrDefault())
            {
                try
                {
                    Model.Name = DisplayName;
                    var json = EtpExtensions.Serialize(null, Model, true);
                    File.WriteAllText(dialog.FileName, json);
                }
                catch (Exception ex)
                {
                    Runtime.ShowError("Error saving configuration settings.", ex);
                }
            }
        }
    }
}
