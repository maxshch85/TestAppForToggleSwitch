using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MahApps.Metro.ValueBoxes;
using WpfApp4.Automation.Peers;
//using MahApps.Metro.ValueBoxes;
namespace WpfApp4
{
    /// <summary>
    /// A control that allows the user to toggle between two states: One represents true; The other represents false.
    /// </summary>
    
    [System.Diagnostics.CodeAnalysis.SuppressMessage("WpfAnalyzers.TemplatePart", "WPF0132:Use PART prefix.", Justification = "<Pending>")]
    [ContentProperty(nameof(Content))]
    [TemplatePart(Name = nameof(HeaderContentPresenter), Type = typeof(ContentPresenter))]
    [TemplatePart(Name = nameof(ContentPresenter), Type = typeof(ContentPresenter))]
    [TemplatePart(Name = nameof(OffContentPresenter), Type = typeof(ContentPresenter))]
    [TemplatePart(Name = nameof(OnContentPresenter), Type = typeof(ContentPresenter))]
    [TemplatePart(Name = nameof(SwitchKnobBounds), Type = typeof(FrameworkElement))]
    [TemplatePart(Name = nameof(SwitchKnob), Type = typeof(FrameworkElement))]
    [TemplatePart(Name = nameof(KnobTranslateTransform), Type = typeof(TranslateTransform))]
    [TemplatePart(Name = nameof(SwitchThumb), Type = typeof(Thumb))]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateMouseOver)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StatePressed)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateDisabled)]
    [TemplateVisualState(GroupName = ContentStatesGroup, Name = OffContentState)]
    [TemplateVisualState(GroupName = ContentStatesGroup, Name = OnContentState)]
    [TemplateVisualState(GroupName = ToggleStatesGroup, Name = DraggingState)]
    [TemplateVisualState(GroupName = ToggleStatesGroup, Name = OffState)]
    [TemplateVisualState(GroupName = ToggleStatesGroup, Name = OnState)]
    public class ToggleSwitchEx : HeaderedContentControl, ICommandSource
    {
        private const string ContentStatesGroup = "ContentStates";
        private const string OffContentState = "OffContent";
        private const string OnContentState = "OnContent";
        private const string ToggleStatesGroup = "ToggleStates";
        private const string DraggingState = "Dragging";
        private const string OffState = "Off";
        private const string OnState = "On";

        private double onTranslation;
        private double startTranslation;
        private bool wasDragged;

        private ContentPresenter HeaderContentPresenter { get; set; }

        private ContentPresenter ContentPresenter { get; set; }

        private ContentPresenter OffContentPresenter { get; set; }

        private ContentPresenter OnContentPresenter { get; set; }

        private FrameworkElement SwitchKnobBounds { get; set; }

        private FrameworkElement SwitchKnob { get; set; }

        private TranslateTransform KnobTranslateTransform { get; set; }

        private Thumb SwitchThumb { get; set; }

        /// <summary>Identifies the <see cref="ContentDirection"/> dependency property.</summary>
        public static readonly DependencyProperty ContentDirectionProperty
            = DependencyProperty.Register(nameof(ContentDirection),
                                          typeof(FlowDirection),
                                          typeof(ToggleSwitchEx),
                                          new PropertyMetadata(FlowDirection.LeftToRight));

        /// <summary>
        /// Gets or sets the flow direction of the switch and content.
        /// </summary>
        /// <remarks>
        /// LeftToRight means content left and button right and RightToLeft vise versa.
        /// </remarks>
        [Bindable(true)]
        [Category("MahApps.Metro")]
        public FlowDirection ContentDirection
        {
            get => (FlowDirection)this.GetValue(ContentDirectionProperty);
            set => this.SetValue(ContentDirectionProperty, value);
        }

        /// <summary>Identifies the <see cref="ContentPadding"/> dependency property.</summary>
        public static readonly DependencyProperty ContentPaddingProperty
            = DependencyProperty.Register(nameof(ContentPadding),
                                          typeof(Thickness),
                                          typeof(ToggleSwitchEx),
                                          new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Gets or sets the padding of the inner content.
        /// </summary>
        [Bindable(true)]
        [Category("MahApps.Metro")]
        public Thickness ContentPadding
        {
            get => (Thickness)this.GetValue(ContentPaddingProperty);
            set => this.SetValue(ContentPaddingProperty, value);
        }

        /// <summary>Identifies the <see cref="IsOn"/> dependency property.</summary>
        public static readonly DependencyProperty IsOnProperty
            = DependencyProperty.Register(nameof(IsOn),
                                          typeof(bool),
                                          typeof(ToggleSwitchEx),
                                          new FrameworkPropertyMetadata(
                                              BooleanBoxes.FalseBox,
                                              FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                                              OnIsOnChanged));

        private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleSwitchEx toggleSwitch && e.NewValue != e.OldValue && e.NewValue is bool newValue && e.OldValue is bool oldValue)
            {
                // doing soft casting here because the peer can be that of RadioButton and it is not derived from
                // ToggleButtonAutomationPeer - specifically to avoid implementing TogglePattern
                if (UIElementAutomationPeer.FromElement(toggleSwitch) is ToggleSwitchAutomationPeer peer)
                {
                    peer.RaiseToggleStatePropertyChangedEvent(oldValue, newValue);
                }

                toggleSwitch.OnToggled();
                toggleSwitch.UpdateVisualStates(true);
            }
        }

        /// <summary>
        /// Gets or sets a value that declares whether the state of the ToggleSwitch is "On".
        /// </summary>
        [Category("MahApps.Metro")]
        public bool IsOn
        {
            get => (bool)this.GetValue(IsOnProperty);
            set => this.SetValue(IsOnProperty, BooleanBoxes.Box(value));
        }

        /// <summary>Identifies the <see cref="OnContent"/> dependency property.</summary>
        public static readonly DependencyProperty OnContentProperty
            = DependencyProperty.Register(nameof(OnContent),
                                          typeof(object),
                                          typeof(ToggleSwitchEx),
                                          new FrameworkPropertyMetadata("On", OnOnContentChanged));

        private static void OnOnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToggleSwitchEx)d).OnOnContentChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnOnContentChanged(object oldContent, object newContent)
        {
        }

        /// <summary>
        /// Provides the object content that should be displayed using the OnContentTemplate when this ToggleSwitch has state of "On".
        /// </summary>
        [Category("MahApps.Metro")]
        public object OnContent
        {
            get => this.GetValue(OnContentProperty);
            set => this.SetValue(OnContentProperty, value);
        }

        /// <summary>Identifies the <see cref="OnContentTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty OnContentTemplateProperty
            = DependencyProperty.Register(nameof(OnContentTemplate),
                                          typeof(DataTemplate),
                                          typeof(ToggleSwitchEx));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the control's content while in "On" state.
        /// </summary>
        public DataTemplate OnContentTemplate
        {
            get => (DataTemplate)this.GetValue(OnContentTemplateProperty);
            set => this.SetValue(OnContentTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="OnContentTemplateSelector"/> dependency property.</summary>
        public static readonly DependencyProperty OnContentTemplateSelectorProperty
            = DependencyProperty.Register(nameof(OnContentTemplateSelector),
                                          typeof(DataTemplateSelector),
                                          typeof(ToggleSwitchEx),
                                          new FrameworkPropertyMetadata((DataTemplateSelector)null));

        /// <summary>
        /// Gets or sets a template selector for OnContent property that enables an application writer to provide custom template-selection logic .
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="OnContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        [Category("MahApps.Metro")]
        public DataTemplateSelector OnContentTemplateSelector
        {
            get => (DataTemplateSelector)this.GetValue(OnContentTemplateSelectorProperty);
            set => this.SetValue(OnContentTemplateSelectorProperty, value);
        }

        /// <summary>Identifies the <see cref="OnContentStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty OnContentStringFormatProperty
            = DependencyProperty.Register(nameof(OnContentStringFormat),
                                          typeof(string),
                                          typeof(ToggleSwitchEx),
                                          new FrameworkPropertyMetadata((string)null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the OnContent property if it is displayed as a string.
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="OnContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        [Category("MahApps.Metro")]
        public string OnContentStringFormat
        {
            get => (string)this.GetValue(OnContentStringFormatProperty);
            set => this.SetValue(OnContentStringFormatProperty, value);
        }

        /// <summary>Identifies the <see cref="OffContent"/> dependency property.</summary>
        public static readonly DependencyProperty OffContentProperty
            = DependencyProperty.Register(nameof(OffContent),
                                          typeof(object),
                                          typeof(ToggleSwitchEx),
                                          new FrameworkPropertyMetadata("Off", OnOffContentChanged));

        private static void OnOffContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToggleSwitchEx)d).OnOffContentChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnOffContentChanged(object oldContent, object newContent)
        {
        }

        /// <summary>
        /// Provides the object content that should be displayed using the OffContentTemplate when this ToggleSwitch has state of "Off".
        /// </summary>
        [Category("MahApps.Metro")]
        public object OffContent
        {
            get => this.GetValue(OffContentProperty);
            set => this.SetValue(OffContentProperty, value);
        }

        /// <summary>Identifies the <see cref="OffContentTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty OffContentTemplateProperty
            = DependencyProperty.Register(nameof(OffContentTemplate),
                                          typeof(DataTemplate),
                                          typeof(ToggleSwitchEx));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the control's content while in "Off" state.
        /// </summary>
        [Category("MahApps.Metro")]
        public DataTemplate OffContentTemplate
        {
            get => (DataTemplate)this.GetValue(OffContentTemplateProperty);
            set => this.SetValue(OffContentTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="OffContentTemplateSelector"/> dependency property.</summary>
        public static readonly DependencyProperty OffContentTemplateSelectorProperty
            = DependencyProperty.Register(nameof(OffContentTemplateSelector),
                                          typeof(DataTemplateSelector),
                                          typeof(ToggleSwitchEx),
                                          new FrameworkPropertyMetadata((DataTemplateSelector)null));

        /// <summary>
        /// Gets or sets a template selector for OffContent property that enables an application writer to provide custom template-selection logic .
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="OffContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        [Category("MahApps.Metro")]
        public DataTemplateSelector OffContentTemplateSelector
        {
            get => (DataTemplateSelector)this.GetValue(OffContentTemplateSelectorProperty);
            set => this.SetValue(OffContentTemplateSelectorProperty, value);
        }

        /// <summary>Identifies the <see cref="OffContentStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty OffContentStringFormatProperty
            = DependencyProperty.Register(nameof(OffContentStringFormat),
                                          typeof(string),
                                          typeof(ToggleSwitchEx),
                                          new FrameworkPropertyMetadata((string)null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the OffContent property if it is displayed as a string.
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="OffContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        [Category("MahApps.Metro")]
        public string OffContentStringFormat
        {
            get => (string)this.GetValue(OffContentStringFormatProperty);
            set => this.SetValue(OffContentStringFormatProperty, value);
        }

        private static readonly DependencyPropertyKey IsPressedPropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(IsPressed),
                                                  typeof(bool),
                                                  typeof(ToggleSwitchEx),
                                                  new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

        /// <summary>Identifies the <see cref="IsPressed"/> dependency property.</summary>
        public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;

        [Browsable(false)]
        [ReadOnly(true)]
        [Category("MahApps.Metro")]
        public bool IsPressed
        {
            get => (bool)this.GetValue(IsPressedProperty);
            protected set => this.SetValue(IsPressedPropertyKey, BooleanBoxes.Box(value));
        }

        /// <summary>Identifies the <see cref="Command"/> dependency property.</summary>
        public static readonly DependencyProperty CommandProperty
            = DependencyProperty.Register(nameof(Command),
                                          typeof(ICommand),
                                          typeof(ToggleSwitchEx),
                                          new PropertyMetadata(null, OnCommandChanged));

        /// <summary>
        /// Gets or sets a command which will be executed when the <see cref="IsOnProperty"/> changes.
        /// </summary>
        [Category("MahApps.Metro")]
        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        /// <summary>Identifies the <see cref="OnCommand"/> dependency property.</summary>
        public static readonly DependencyProperty OnCommandProperty
            = DependencyProperty.Register(nameof(OnCommand),
                                          typeof(ICommand),
                                          typeof(ToggleSwitchEx),
                                          new PropertyMetadata(null, OnCommandChanged));

        /// <summary>
        /// Gets or sets a command which will be executed when the <see cref="IsOnProperty"/> changes.
        /// </summary>
        [Category("MahApps.Metro")]
        public ICommand OnCommand
        {
            get => (ICommand)this.GetValue(OnCommandProperty);
            set => this.SetValue(OnCommandProperty, value);
        }

        /// <summary>Identifies the <see cref="OffCommand"/> dependency property.</summary>
        public static readonly DependencyProperty OffCommandProperty
            = DependencyProperty.Register(nameof(OffCommand),
                                          typeof(ICommand),
                                          typeof(ToggleSwitchEx),
                                          new PropertyMetadata(null, OnCommandChanged));

        /// <summary>
        /// Gets or sets a command which will be executed when the <see cref="IsOnProperty"/> changes.
        /// </summary>
        [Category("MahApps.Metro")]
        public ICommand OffCommand
        {
            get => (ICommand)this.GetValue(OffCommandProperty);
            set => this.SetValue(OffCommandProperty, value);
        }

        /// <summary>Identifies the <see cref="CommandParameter"/> dependency property.</summary>
        public static readonly DependencyProperty CommandParameterProperty
            = DependencyProperty.Register(nameof(CommandParameter),
                                          typeof(object),
                                          typeof(ToggleSwitchEx),
                                          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the command parameter which will be passed by the Command.
        /// </summary>
        public object CommandParameter
        {
            get => this.GetValue(CommandParameterProperty);
            set => this.SetValue(CommandParameterProperty, value);
        }

        /// <summary>Identifies the <see cref="CommandTarget"/> dependency property.</summary>
        public static readonly DependencyProperty CommandTargetProperty
            = DependencyProperty.Register(nameof(CommandTarget),
                                          typeof(IInputElement),
                                          typeof(ToggleSwitchEx),
                                          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the element on which to raise the specified Command.
        /// </summary>
        /// <returns>
        /// Element on which to raise the Command.
        /// </returns>
        public IInputElement CommandTarget
        {
            get => (IInputElement)this.GetValue(CommandTargetProperty);
            set => this.SetValue(CommandTargetProperty, value);
        }

        /// <summary>
        /// Occurs when "On"/"Off" state changes for this ToggleSwitch.
        /// </summary>
        public event RoutedEventHandler Toggled;

        /// <summary>This method is invoked when the <see cref="IsOnProperty"/> changes.</summary>
        protected virtual void OnToggled()
        {
            this.Toggled?.Invoke(this, new RoutedEventArgs());
        }

        static ToggleSwitchEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitchEx), new FrameworkPropertyMetadata(typeof(ToggleSwitchEx)));
            EventManager.RegisterClassHandler(typeof(ToggleSwitchEx), MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
        }

        public ToggleSwitchEx()
        {
            this.IsEnabledChanged += this.OnIsEnabledChanged;
        }

        public override void OnApplyTemplate()
        {
            if (this.SwitchKnobBounds != null && this.SwitchKnob != null && this.KnobTranslateTransform != null && this.SwitchThumb != null)
            {
                this.SwitchThumb.DragStarted -= this.OnSwitchThumbDragStarted;
                this.SwitchThumb.DragDelta -= this.OnSwitchThumbDragDelta;
                this.SwitchThumb.DragCompleted -= this.OnSwitchThumbDragCompleted;
            }

            base.OnApplyTemplate();

            this.HeaderContentPresenter = this.GetTemplateChild(nameof(this.HeaderContentPresenter)) as ContentPresenter;
            this.ContentPresenter = this.GetTemplateChild(nameof(this.ContentPresenter)) as ContentPresenter;
            this.OffContentPresenter = this.GetTemplateChild(nameof(this.OffContentPresenter)) as ContentPresenter;
            this.OnContentPresenter = this.GetTemplateChild(nameof(this.OnContentPresenter)) as ContentPresenter;
            this.SwitchKnobBounds = this.GetTemplateChild(nameof(this.SwitchKnobBounds)) as FrameworkElement;
            this.SwitchKnob = this.GetTemplateChild(nameof(this.SwitchKnob)) as FrameworkElement;
            this.KnobTranslateTransform = this.GetTemplateChild(nameof(this.KnobTranslateTransform)) as TranslateTransform;
            this.SwitchThumb = this.GetTemplateChild(nameof(this.SwitchThumb)) as Thumb;

            if (this.SwitchKnobBounds != null && this.SwitchKnob != null && this.KnobTranslateTransform != null && this.SwitchThumb != null)
            {
                this.SwitchThumb.DragStarted += this.OnSwitchThumbDragStarted;
                this.SwitchThumb.DragDelta += this.OnSwitchThumbDragDelta;
                this.SwitchThumb.DragCompleted += this.OnSwitchThumbDragCompleted;
            }

            this.UpdateHeaderContentPresenterVisibility();
            this.UpdateContentPresenterVisibility();

            this.UpdateVisualStates(false);
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.UpdateVisualStates(true);
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ToggleSwitchEx toggle && !toggle.IsKeyboardFocusWithin)
            {
                e.Handled = toggle.Focus() || e.Handled;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                this.Toggle();
            }

            base.OnKeyUp(e);
        }

        protected override void OnHeaderChanged(object oldHeader, object newHeader)
        {
            base.OnHeaderChanged(oldHeader, newHeader);

            this.UpdateHeaderContentPresenterVisibility();
        }

        private void UpdateHeaderContentPresenterVisibility()
        {
            if (this.HeaderContentPresenter == null)
            {
                return;
            }

            bool showHeader = (this.Header is string s && !string.IsNullOrEmpty(s)) || this.Header != null;
            this.HeaderContentPresenter.Visibility = showHeader ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            this.UpdateContentPresenterVisibility();
        }

        private void UpdateContentPresenterVisibility()
        {
            if (this.ContentPresenter == null)
            {
                return;
            }

            bool showContent = (this.Content is string s && !string.IsNullOrEmpty(s)) || this.Content != null;
            this.ContentPresenter.Visibility = showContent ? Visibility.Visible : Visibility.Collapsed;
            this.OffContentPresenter.Visibility = !showContent ? Visibility.Visible : Visibility.Collapsed;
            this.OnContentPresenter.Visibility = !showContent ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsMouseOverProperty)
            {
                this.UpdateVisualStates(true);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (this.SwitchKnobBounds != null && this.SwitchKnob != null)
            {
                this.onTranslation = this.SwitchKnobBounds.ActualWidth - this.SwitchKnob.ActualWidth - this.SwitchKnob.Margin.Left - this.SwitchKnob.Margin.Right;
            }
        }

        private void OnSwitchThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            e.Handled = true;
            this.IsPressed = true;
            this.wasDragged = false;
            this.startTranslation = this.KnobTranslateTransform.X;
            this.UpdateVisualStates(true);
            this.KnobTranslateTransform.X = this.startTranslation;
        }

        private void OnSwitchThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            e.Handled = true;
            if (e.HorizontalChange != 0)
            {
                this.wasDragged = Math.Abs(e.HorizontalChange) >= SystemParameters.MinimumHorizontalDragDistance;
                double dragTranslation = this.startTranslation + e.HorizontalChange;
                this.KnobTranslateTransform.X = Math.Max(0, Math.Min(this.onTranslation, dragTranslation));
            }
        }

        private void OnSwitchThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            e.Handled = true;
            this.IsPressed = false;
            if (this.wasDragged)
            {
                if (!this.IsOn && this.KnobTranslateTransform.X + this.SwitchKnob.ActualWidth / 2 >= this.SwitchKnobBounds.ActualWidth / 2)
                {
                    this.Toggle();
                }
                else if (this.IsOn && this.KnobTranslateTransform.X + this.SwitchKnob.ActualWidth / 2 <= this.SwitchKnobBounds.ActualWidth / 2)
                {
                    this.Toggle();
                }
                else
                {
                    this.UpdateVisualStates(true);
                }
            }
            else
            {
                this.Toggle();
            }

            this.wasDragged = false;
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            string stateName;

            if (!this.IsEnabled)
            {
                stateName = VisualStates.StateDisabled;
            }
            else if (this.IsPressed)
            {
                stateName = VisualStates.StatePressed;
            }
            else if (this.IsMouseOver)
            {
                stateName = VisualStates.StateMouseOver;
            }
            else
            {
                stateName = VisualStates.StateNormal;
            }

            VisualStateManager.GoToState(this, stateName, useTransitions);

            if (this.SwitchThumb != null && this.SwitchThumb.IsDragging)
            {
                stateName = DraggingState;
            }
            else
            {
                stateName = this.IsOn ? OnState : OffState;
            }

            VisualStateManager.GoToState(this, stateName, useTransitions);

            VisualStateManager.GoToState(this, this.IsOn ? OnContentState : OffContentState, useTransitions);
        }

        private void Toggle()
        {
            var newValue = !this.IsOn;
            this.SetCurrentValue(IsOnProperty, BooleanBoxes.Box(newValue));

            ExecuteCommandSource(this);
            ExecuteCommandSource(this, newValue ? this.OnCommand : this.OffCommand);
        }
        [SecurityCritical]
        [SecuritySafeCritical]
        internal static void ExecuteCommandSource(ICommandSource commandSource, ICommand theCommand)
        {
            CriticalExecuteCommandSource(commandSource, theCommand);
        }

        [SecurityCritical]
        internal static void CriticalExecuteCommandSource(ICommandSource commandSource, ICommand theCommand)
        {
            var command = theCommand;
            if (command == null)
            {
                return;
            }

            var commandParameter = commandSource.CommandParameter ?? commandSource;
            if (command is RoutedCommand routedCommand)
            {
                var target = commandSource.CommandTarget ?? commandSource as IInputElement;
                if (routedCommand.CanExecute(commandParameter, target))
                {
                    routedCommand.Execute(commandParameter, target);
                }
            }
            else
            {
                if (command.CanExecute(commandParameter))
                {
                    command.Execute(commandParameter);
                }
            }
        }
        [SecurityCritical]
        [SecuritySafeCritical]
        internal static void ExecuteCommandSource(ICommandSource commandSource)
        {
            CriticalExecuteCommandSource(commandSource);
        }

        [SecurityCritical]
        internal static void CriticalExecuteCommandSource(ICommandSource commandSource)
        {
            var command = commandSource.Command;
            if (command == null)
            {
                return;
            }

            var commandParameter = commandSource.CommandParameter ?? commandSource;
            if (command is RoutedCommand routedCommand)
            {
                var target = commandSource.CommandTarget ?? commandSource as IInputElement;
                if (routedCommand.CanExecute(commandParameter, target))
                {
                    routedCommand.Execute(commandParameter, target);
                }
            }
            else
            {
                if (command.CanExecute(commandParameter))
                {
                    command.Execute(commandParameter);
                }
            }
        }
        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToggleSwitchEx)d).OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        private void OnCommandChanged(ICommand oldCommand, ICommand newCommand)
        {
            if (oldCommand != null)
            {
                this.UnhookCommand(oldCommand);
            }

            if (newCommand != null)
            {
                this.HookCommand(newCommand);
            }
        }

        private void UnhookCommand(ICommand command)
        {
            CanExecuteChangedEventManager.RemoveHandler(command, this.OnCanExecuteChanged);
            this.UpdateCanExecute();
        }

        private void HookCommand(ICommand command)
        {
            CanExecuteChangedEventManager.AddHandler(command, this.OnCanExecuteChanged);
            this.UpdateCanExecute();
        }

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            this.UpdateCanExecute();
        }

        private void UpdateCanExecute()
        {
            this.CanExecute = this.Command == null || CanExecuteCommandSource(this);
        }

        /// <inheritdoc />
        protected override bool IsEnabledCore => base.IsEnabledCore && this.CanExecute;

        private bool canExecute = true;

        private bool CanExecute
        {
            get => this.canExecute;
            set
            {
                if (value == this.canExecute)
                {
                    return;
                }

                this.canExecute = value;
                this.CoerceValue(IsEnabledProperty);
            }
        }
        internal static bool CanExecuteCommandSource(ICommandSource commandSource)
        {
            var command = commandSource.Command;
            if (command == null)
            {
                return false;
            }

            var commandParameter = commandSource.CommandParameter ?? commandSource;
            if (command is RoutedCommand routedCommand)
            {
                var target = commandSource.CommandTarget ?? commandSource as IInputElement;
                return routedCommand.CanExecute(commandParameter, target);
            }

            return command.CanExecute(commandParameter);
        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ToggleSwitchAutomationPeer(this);
        }

        internal void AutomationPeerToggle()
        {
            this.Toggle();
        }
    }



    internal static class VisualStates
    {
        public const string GroupCommon = "CommonStates";
        public const string StateNormal = "Normal";
        public const string StateReadOnly = "ReadOnly";
        public const string StateMouseOver = "MouseOver";
        public const string StatePressed = "Pressed";
        public const string StateDisabled = "Disabled";
        public const string GroupFocus = "FocusStates";
        public const string StateUnfocused = "Unfocused";
        public const string StateFocused = "Focused";
        public const string GroupSelection = "SelectionStates";
        public const string StateSelected = "Selected";
        public const string StateUnselected = "Unselected";
        public const string StateSelectedInactive = "SelectedInactive";
        public const string GroupExpansion = "ExpansionStates";
        public const string StateExpanded = "Expanded";
        public const string StateCollapsed = "Collapsed";
        public const string GroupPopup = "PopupStates";
        public const string StatePopupOpened = "PopupOpened";
        public const string StatePopupClosed = "PopupClosed";
        public const string GroupValidation = "ValidationStates";
        public const string StateValid = "Valid";
        public const string StateInvalidFocused = "InvalidFocused";
        public const string StateInvalidUnfocused = "InvalidUnfocused";
        public const string GroupExpandDirection = "ExpandDirectionStates";
        public const string StateExpandDown = "ExpandDown";
        public const string StateExpandUp = "ExpandUp";
        public const string StateExpandLeft = "ExpandLeft";
        public const string StateExpandRight = "ExpandRight";
        public const string GroupHasItems = "HasItemsStates";
        public const string StateHasItems = "HasItems";
        public const string StateNoItems = "NoItems";
        public const string GroupIncrease = "IncreaseStates";
        public const string StateIncreaseEnabled = "IncreaseEnabled";
        public const string StateIncreaseDisabled = "IncreaseDisabled";
        public const string GroupDecrease = "DecreaseStates";
        public const string StateDecreaseEnabled = "DecreaseEnabled";
        public const string StateDecreaseDisabled = "DecreaseDisabled";
        public const string GroupInteractionMode = "InteractionModeStates";
        public const string StateEdit = "Edit";
        public const string StateDisplay = "Display";
        public const string GroupLocked = "LockedStates";
        public const string StateLocked = "Locked";
        public const string StateUnlocked = "Unlocked";
        public const string StateActive = "Active";
        public const string StateInactive = "Inactive";
        public const string GroupActive = "ActiveStates";
        public const string StateUnwatermarked = "Unwatermarked";
        public const string StateWatermarked = "Watermarked";
        public const string GroupWatermark = "WatermarkStates";
        public const string StateCalendarButtonUnfocused = "CalendarButtonUnfocused";
        public const string StateCalendarButtonFocused = "CalendarButtonFocused";
        public const string GroupCalendarButtonFocus = "CalendarButtonFocusStates";
        public const string StateBusy = "Busy";
        public const string StateIdle = "Idle";
        public const string GroupBusyStatus = "BusyStatusStates";
        public const string StateVisible = "Visible";
        public const string StateHidden = "Hidden";
        public const string GroupVisibility = "VisibilityStates";

        public static void GoToState(Control control, bool useTransitions, params string[] stateNames)
        {
            Debug.Assert(control != null, "control should not be null!");
            Debug.Assert(stateNames != null, "stateNames should not be null!");
            Debug.Assert(stateNames.Length > 0, "stateNames should not be empty!");

            foreach (string name in stateNames)
            {
                if (VisualStateManager.GoToState(control, name, useTransitions))
                {
                    break;
                }
            }
        }

        public static FrameworkElement GetImplementationRoot(DependencyObject dependencyObject)
        {
            Debug.Assert(dependencyObject != null, "DependencyObject should not be null.");
            return VisualTreeHelper.GetChildrenCount(dependencyObject) == 1
                ? VisualTreeHelper.GetChild(dependencyObject, 0) as FrameworkElement
                : null;
        }

        public static VisualStateGroup TryGetVisualStateGroup(DependencyObject dependencyObject, string groupName)
        {
            FrameworkElement root = GetImplementationRoot(dependencyObject);
            return root == null
                ? null
                : VisualStateManager.GetVisualStateGroups(root)?.OfType<VisualStateGroup>().FirstOrDefault(group => string.CompareOrdinal(groupName, group.Name) == 0);
        }
    }
}