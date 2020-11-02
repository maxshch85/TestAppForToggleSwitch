using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp4
{
    class ButtonEx :Button
    {
        static ButtonEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonEx), new FrameworkPropertyMetadata(typeof(ButtonEx)));
        }
        public override void OnApplyTemplate()
        {
           

            base.OnApplyTemplate();

        }
    }
}
