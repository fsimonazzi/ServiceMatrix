﻿using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NuPattern;
using NuPattern.Runtime;
using System.Windows.Input;
using NServiceBusStudio.Automation.Dialog;
using NuPattern.Diagnostics;
using NuPattern.Presentation;
using System.Collections.ObjectModel;
using System.Windows;

namespace NServiceBusStudio.Automation.Commands
{
    /// <summary>
    /// A custom command that performs some automation.
    /// </summary>
    [DisplayName("Show an Event Picker Dialog for Subscription")]
    [Category("General")]
    [Description("Shows a dialog where the user can choose an Event and adds a Subscribe link to it.")]
    [CLSCompliant(false)]
    public class ShowEventSubscriptionPicker : NuPattern.Runtime.Command
    {
        private static readonly ITracer tracer = Tracer.Get<ShowEventSubscriptionPicker>();

        /// <summary>
        /// Gets or sets the Window Factory, used to create a Window Dialog.
        /// </summary>
        [Required]
        [Import(AllowDefault = true)]
        private IDialogWindowFactory WindowFactory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current element.
        /// </summary>
        [Required]
        [Import(AllowDefault = true)]
        public IComponent CurrentElement
        {
            get;
            set;
        }

        /// <summary>
        /// Executes this commmand.
        /// </summary>
        /// <remarks></remarks>
        public override void Execute()
        {
            // Verify all [Required] and [Import]ed properties have valid values.
            this.ValidateObject();

            var events = CurrentElement.Parent.Parent.Parent.Service.SelectMany(s => s.Contract.Events.Event);

            // Filter those events that already processed by the component
            events = events.Where(e => !CurrentElement.Subscribes.SubscribedEventLinks.Any(el => el.EventReference.Value == e));

            // Get event names
            var existingEventNames = events.Select(e => e.InstanceName);
            var picker = WindowFactory.CreateDialog<EventReadOnlyPicker>() as IEventPicker;
            picker.Elements = new ObservableCollection<string>(existingEventNames);
            picker.Title = "Subscribe to Event";
            using (new MouseCursor(Cursors.Arrow))
            {
                if (picker.ShowDialog().Value)
                {
                    foreach (var selectedElement in picker.SelectedItems)
                    {
                        var selectedEvent = events.FirstOrDefault(e => string.Equals(e.InstanceName, selectedElement, StringComparison.InvariantCultureIgnoreCase));
                        CurrentElement.Subscribes.CreateLink(selectedEvent);
                    }

                    if (CurrentElement.Subscribes.ProcessedCommandLinks.Count() + CurrentElement.Subscribes.SubscribedEventLinks.Count() > 1)
                    {
                        var result = MessageBox.Show(String.Format("Convert ‘{0}’ to saga to correlate between commands & events?", CurrentElement.CodeIdentifier), "ServiceMatrix - Saga recommendation", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            new ShowComponentSagaStarterPicker()
                            {
                                WindowFactory = this.WindowFactory,
                                CurrentElement = CurrentElement
                            }.Execute();
                        }
                    }
                }
            }

            // TODO: Implement command automation code
            //	TODO: Use tracer.Warning() to note expected and recoverable errors
            //	TODO: Use tracer.Verbose() to note internal execution logic decisions
            //	TODO: Use tracer.Info() to note key results of execution
            //	TODO: Raise exceptions for all other errors
        }
    }
}
