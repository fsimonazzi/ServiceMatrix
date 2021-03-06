﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel.Composition;
using NServiceBusStudio.Automation.Infrastructure;
using NServiceBusStudio.Automation.Extensions;

namespace NServiceBusStudio
{
    partial class Command : IRenameRefactoring
    {
        public string Namespace
        {
            get { return this.Parent.Namespace; }
        }

        partial void Initialize()
        {
            this.AsElement().Deleting += (sender, eventargs) =>
            {
                // Find Component Links to the deleted Component
                var root = this.AsElement().Root.As<IApplication>();
                
                var commandLinks = root.Design.Services.Service.SelectMany(s => s.Components.Component.SelectMany (c => c.Publishes.CommandLinks.Where (cl => cl.CommandReference.Value == this))).ToList();
                commandLinks.ForEach(cl => cl.Delete());

                var processedCommandLinks = root.Design.Services.Service.SelectMany(s => s.Components.Component.SelectMany(c => c.Subscribes.ProcessedCommandLinks.Where(cl => cl.CommandReference.Value == this))).ToList();
                processedCommandLinks.ForEach(cl => cl.Delete());

                // Remove related components
                var result = MessageBox.Show("Do you want to delete the related Components?", "ServiceMatrix - Delete related Components", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    DeleteComponent(String.Format("{0}Sender", this.InstanceName));
                    DeleteComponent(String.Format("{0}Handler", this.InstanceName));
                }
            };
        }

        private void DeleteComponent(string componentName)
        {
            var component = this.Parent.Parent.Parent.
                                 Components.
                                 Component.
                                 FirstOrDefault(x => x.InstanceName == componentName);

            if (component != null)
                component.Delete();
        }
    }
}
