using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace PowerShellExecutor.Behaviors;

/// <summary>
/// A behavior for a WPF <see cref="TextBox"/> control to enable two-way binding for the caret index property
/// </summary>
public class BindableCaretIndexBehavior : Behavior<TextBox>
{
    /// <summary>
    /// Identifies the CaretIndex dependency property for use in enabling two-way data binding
    /// of the caret position in a <see cref="TextBox"/> through the <see cref="BindableCaretIndexBehavior"/>
    /// </summary>
    public static readonly DependencyProperty CaretIndexProperty = DependencyProperty.Register(
        "CaretIndex",
        typeof(int),
        typeof(BindableCaretIndexBehavior),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCaretIndexChanged));

    /// <summary>
    /// Gets or sets the current position of the caret within an associated <see cref="TextBox"/> when used with the
    /// <see cref="BindableCaretIndexBehavior"/>
    /// </summary>
    /// <remarks>
    /// The <see cref="CaretIndex"/> property enables two-way data binding of the caret position in a <see cref="TextBox"/> to a
    /// property in the ViewModel, allowing for dynamic updates and synchronization
    /// </remarks>
    public int CaretIndex
    {
        get => (int)GetValue(CaretIndexProperty);
        set => SetValue(CaretIndexProperty, value);
    }

    /// <summary>
    /// Called when the behavior is attached to a <see cref="TextBox"/> control.
    /// Sets up event handling
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectionChanged += OnSelectionChanged;
    }

    /// <summary>
    /// Called when the behavior is being detached from its associated object
    /// Performs cleanup operations
    /// </summary>
    protected override void OnDetaching()
    {
        AssociatedObject.SelectionChanged -= OnSelectionChanged;
        base.OnDetaching();
    }

    /// <summary>
    /// Handles changes to the <see cref="CaretIndexProperty"/> dependency property by updating
    /// the caret position in the associated <see cref="TextBox"/> control when the bound property changes
    /// </summary>
    /// <param name="d">
    /// The dependency object on which the property change occurred.
    /// Expected to be an instance of <see cref="BindableCaretIndexBehavior"/>
    /// </param>
    /// <param name="e">Provides data for the property changed event</param>
    private static void OnCaretIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (BindableCaretIndexBehavior)d;

        if (behavior.AssociatedObject != null)
        {
            var newValue = (int)e.NewValue;
            behavior.AssociatedObject.CaretIndex = newValue;
        }
    }

    /// <summary>
    /// Handles the SelectionChanged event of the associated TextBox by updating the
    /// <see cref="CaretIndex"/> property to reflect the current caret index position
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event arguments</param>
    private void OnSelectionChanged(object sender, RoutedEventArgs e)
    {
        CaretIndex = AssociatedObject.CaretIndex;
    }
}