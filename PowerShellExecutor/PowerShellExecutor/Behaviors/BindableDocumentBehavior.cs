using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Xaml.Behaviors;

namespace PowerShellExecutor.Behaviors;


/// <summary>
/// A behavior for a WPF <see cref="RichTextBox"/> control to enable a one-way binding for the document property
/// </summary>
public class BindableDocumentBehavior : Behavior<RichTextBox>
{
    /// <summary>
    /// Identifies the Document dependency property for use in enabling one-way data binding
    /// of the document in a <see cref="RichTextBox"/> through the <see cref="BindableDocumentBehavior"/>
    /// </summary>
    public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(
        "Document",
        typeof(FlowDocument),
        typeof(BindableDocumentBehavior),
        new FrameworkPropertyMetadata(null, OnDocumentChanged)
    );
    
    /// <summary>
    /// Gets or sets the document within an associated <see cref="RichTextBox"/> when used with the
    /// <see cref="BindableDocumentBehavior"/>
    /// </summary>
    public FlowDocument Document
    {
        get => (FlowDocument)GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }
    
    /// <summary>
    /// Handles changes to the <see cref="DocumentProperty"/> dependency property by updating
    /// the document in the associated <see cref="RichTextBox"/> control when the bound property changes
    /// </summary>
    /// <param name="d">
    /// The dependency object on which the property change occurred.
    /// Expected to be an instance of <see cref="BindableDocumentBehavior"/>
    /// </param>
    /// <param name="e">Provides data for the property changed event</param>
    private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (BindableDocumentBehavior)d;
        
        if (behavior.AssociatedObject != null)
        {
            var newValue = (FlowDocument)e.NewValue;
            behavior.AssociatedObject.Document = newValue;
        }
    }
}