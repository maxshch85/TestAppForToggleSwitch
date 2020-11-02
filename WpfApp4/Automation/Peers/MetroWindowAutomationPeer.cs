﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows;
using System.Windows.Automation.Peers;


namespace WpfApp4.Automation.Peers
{
    public class MetroWindowAutomationPeer : WindowAutomationPeer
    {
        public MetroWindowAutomationPeer(Window owner)
            : base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return "MetroWindow";
        }
    }
}