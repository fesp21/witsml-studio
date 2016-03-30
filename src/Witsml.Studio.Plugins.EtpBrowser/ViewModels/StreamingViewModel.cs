﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using Energistics.Common;
using Energistics.Datatypes;
using Energistics.Datatypes.ChannelData;
using Energistics.Protocol;
using Energistics.Protocol.ChannelStreaming;
using Energistics.Protocol.Core;
using PDS.Framework;
using PDS.Witsml.Studio.Plugins.EtpBrowser.Models;
using PDS.Witsml.Studio.Runtime;

namespace PDS.Witsml.Studio.Plugins.EtpBrowser.ViewModels
{
    /// <summary>
    /// Manages the behavior of the Streaming user interface elements.
    /// </summary>
    /// <seealso cref="Caliburn.Micro.Screen" />
    public class StreamingViewModel : Screen, ISessionAware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingViewModel"/> class.
        /// </summary>
        public StreamingViewModel(IRuntimeService runtime)
        {
            Runtime = runtime;
            DisplayName = string.Format("{0:D} - {0}", Protocols.ChannelStreaming);
            Channels = new List<ChannelMetadataRecord>();
            ChannelStreamingInfos = new List<ChannelStreamingInfo>();
        }

        /// <summary>
        /// Gets or Sets the Parent <see cref="T:Caliburn.Micro.IConductor" />
        /// </summary>
        public new MainViewModel Parent
        {
            get { return (MainViewModel)base.Parent; }
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        public EtpSettings Model
        {
            get { return Parent.Model; }
        }

        /// <summary>
        /// Gets the runtime service.
        /// </summary>
        /// <value>The runtime service.</value>
        public IRuntimeService Runtime { get; private set; }

        /// <summary>
        /// Gets the collectino of channel metadata.
        /// </summary>
        /// <value>The channel metadata.</value>
        public IList<ChannelMetadataRecord> Channels { get; private set; }

        /// <summary>
        /// Gets the collectino of channel streaming information.
        /// </summary>
        /// <value>The channel streaming information.</value>
        public IList<ChannelStreamingInfo> ChannelStreamingInfos { get; private set; }

        private bool _isSimpleStreamer;
        /// <summary>
        /// Gets or sets a value indicating whether the Channel Streaming Producer is a simple streamer.
        /// </summary>
        /// <value><c>true</c> if the Channel Streaming Producer is a simple streamer; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsSimpleStreamer
        {
            get { return _isSimpleStreamer; }
            set
            {
                if (_isSimpleStreamer != value)
                {
                    _isSimpleStreamer = value;
                    NotifyOfPropertyChange(() => IsSimpleStreamer);
                }
            }
        }

        private bool _canStart;
        /// <summary>
        /// Gets or sets a value indicating whether a Channel Streaming session can be started.
        /// </summary>
        /// <value><c>true</c> if a Channel Streaming session can be started; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool CanStart
        {
            get { return _canStart; }
            set
            {
                if (_canStart != value)
                {
                    _canStart = value;
                    NotifyOfPropertyChange(() => CanStart);
                }
            }
        }

        private bool _canDescribe;
        /// <summary>
        /// Gets or sets a value indicating whether a Channels session can be described.
        /// </summary>
        /// <value><c>true</c> if a Channels session can be described; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool CanDescribe
        {
            get { return _canDescribe; }
            set
            {
                if (_canDescribe != value)
                {
                    _canDescribe = value;
                    NotifyOfPropertyChange(() => CanDescribe);
                }
            }
        }

        private bool _canStartStreaming;
        /// <summary>
        /// Gets or sets a value indicating whether Channel Streaming can be started.
        /// </summary>
        /// <value><c>true</c> if Channel Streaming can be started; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool CanStartStreaming
        {
            get { return _canStartStreaming; }
            set
            {
                if (_canStartStreaming != value)
                {
                    _canStartStreaming = value;
                    NotifyOfPropertyChange(() => CanStartStreaming);
                }
            }
        }

        private bool _canStopStreaming;
        /// <summary>
        /// Gets or sets a value indicating whether Channel Streaming can be stopped.
        /// </summary>
        /// <value><c>true</c> if Channel Streaming can be stopped; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool CanStopStreaming
        {
            get { return _canStopStreaming; }
            set
            {
                if (_canStopStreaming != value)
                {
                    _canStopStreaming = value;
                    NotifyOfPropertyChange(() => CanStopStreaming);
                }
            }
        }

        private bool _canRequestRange;
        /// <summary>
        /// Gets or sets a value indicating whether Channel Range Request can be made.
        /// </summary>
        /// <value><c>true</c> if Channel Range Request can be made; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool CanRequestRange
        {
            get { return _canRequestRange; }
            set
            {
                if (_canRequestRange != value)
                {
                    _canRequestRange = value;
                    NotifyOfPropertyChange(() => CanRequestRange);
                }
            }
        }

        /// <summary>
        /// Adds the URI to the collection of URIs.
        /// </summary>
        public void AddUri()
        {
            var uri = Model.Streaming.Uri;

            if (IsSimpleStreamer || string.IsNullOrWhiteSpace(uri) || Model.Streaming.Uris.Contains(uri))
                return;

            Model.Streaming.Uris.Add(uri);
            Model.Streaming.Uri = string.Empty;
            UpdateCanDescribe();
        }

        /// <summary>
        /// Handles the KeyUp event for the ListBox control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        public void OnKeyUp(ListBox control, KeyEventArgs e)
        {
            var index = control.SelectedIndex;

            if (e.Key == Key.Delete && index > -1)
            {
                Model.Streaming.Uris.RemoveAt(index);
                UpdateCanDescribe();
            }
        }

        /// <summary>
        /// Starts a Channel Streaming session.
        /// </summary>
        public void Start()
        {
            Parent.Client.Handler<IChannelStreamingConsumer>()
                .Start(Model.Streaming.MaxDataItems, Model.Streaming.MaxMessageRate);

            CanStart = false;
            Channels.Clear();
            ChannelStreamingInfos.Clear();
            UpdateCanDescribe();
        }

        /// <summary>
        /// Requests channel metadata for the collection of URIs.
        /// </summary>
        public void Describe()
        {
            Channels.Clear();
            ChannelStreamingInfos.Clear();

            Parent.Client.Handler<IChannelStreamingConsumer>()
                .ChannelDescribe(Model.Streaming.Uris);
        }

        /// <summary>
        /// Starts the streaming of channel data.
        /// </summary>
        public void StartStreaming()
        {
            Parent.Client.Handler<IChannelStreamingConsumer>()
                .ChannelStreamingStart(ChannelStreamingInfos);

            CanDescribe = false;
            CanStartStreaming = false;
            CanStopStreaming = true;
            CanRequestRange = false;
        }

        /// <summary>
        /// Stops the streaming of channel data.
        /// </summary>
        public void StopStreaming()
        {
            var channelIds = Channels
                .Select(x => x.ChannelId)
                .ToArray();

            Parent.Client.Handler<IChannelStreamingConsumer>()
                .ChannelStreamingStop(channelIds);

            CanStartStreaming = true;
            CanStopStreaming = false;
            CanRequestRange = !IsSimpleStreamer;
            UpdateCanDescribe();
        }

        /// <summary>
        /// Requests a range of channel data.
        /// </summary>
        public void RequestRange()
        {
            var rangeInfo = new ChannelRangeInfo()
            {
                ChannelId = Channels.Select(x => x.ChannelId).ToArray(),
                StartIndex = Model.Streaming.StartIndex,
                EndIndex = Model.Streaming.EndIndex
            };

            Parent.Client.Handler<IChannelStreamingConsumer>()
                .ChannelRangeRequest(new[] { rangeInfo });

            CanDescribe = false;
            CanStartStreaming = false;
            CanStopStreaming = true;
            CanRequestRange = false;
        }

        /// <summary>
        /// Called when the <see cref="OpenSession" /> message is recieved.
        /// </summary>
        /// <param name="e">The <see cref="ProtocolEventArgs{OpenSession}" /> instance containing the event data.</param>
        public void OnSessionOpened(ProtocolEventArgs<OpenSession> e)
        {
            if (!e.Message.SupportedProtocols.Any(x => x.Protocol == (int)Protocols.ChannelStreaming))
                return;

            var protocol = e.Message.SupportedProtocols
                .FirstOrDefault(x => x.Protocol == (int)Protocols.ChannelStreaming);

            IsSimpleStreamer = protocol.ProtocolCapabilities
                .Where(x => x.Key.EqualsIgnoreCase(ChannelStreamingProducerHandler.SimpleStreamer))
                .Select(x => x.Value.Item)
                .OfType<bool>()
                .FirstOrDefault();

            var handler = Parent.Client.Handler<IChannelStreamingConsumer>();
            handler.OnChannelMetadata += OnChannelMetadata;
            handler.OnChannelData += OnChannelData;

            CanStart = true;
        }

        /// <summary>
        /// Called when the <see cref="Energistics.EtpClient" /> web socket is closed.
        /// </summary>
        public void OnSocketClosed()
        {
            var handler = Parent.Client.Handler<IChannelStreamingConsumer>();
            handler.OnChannelMetadata -= OnChannelMetadata;
            handler.OnChannelData -= OnChannelData;

            IsSimpleStreamer = false;
            CanStart = false;
            CanDescribe = false;
            CanStartStreaming = false;
            CanStopStreaming = false;
            CanRequestRange = false;
        }

        private void OnChannelMetadata(object sender, ProtocolEventArgs<ChannelMetadata> e)
        {
            if (!e.Message.Channels.Any())
                return;

            // add to channel metadata collection
            e.Message.Channels.ForEach(x =>
            {
                Channels.Add(x);
                ChannelStreamingInfos.Add(ToChannelStreamingInfo(x));
            });

            if (e.Header.MessageFlags == (int)MessageFlags.FinalPart)
            {
                CanStartStreaming = !IsSimpleStreamer;
                CanStopStreaming = IsSimpleStreamer;
                CanRequestRange = !IsSimpleStreamer;
            }
        }

        private void OnChannelData(object sender, ProtocolEventArgs<ChannelData> e)
        {
        }

        private ChannelStreamingInfo ToChannelStreamingInfo(ChannelMetadataRecord channel)
        {
            return new ChannelStreamingInfo()
            {
                ChannelId = channel.ChannelId,
                StartIndex = new StreamingStartIndex()
                {
                    Item = Model.Streaming.StartIndex
                }
            };
        }

        private void UpdateCanDescribe()
        {
            CanDescribe = !IsSimpleStreamer && Model.Streaming.Uris.Any();
        }
    }
}
