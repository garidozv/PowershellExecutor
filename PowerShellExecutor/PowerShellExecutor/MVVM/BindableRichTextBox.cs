using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PowerShellExecutor.MVVM;

/// <summary>
/// A RichTextBox implementation that supports data binding for the Document property
/// </summary>
public class BindableRichTextBox : RichTextBox
{
    /// <summary>
    /// Identifies the Document dependency property, which enables the binding
    /// </summary>
    public static readonly DependencyProperty DocumentProperty = 
        DependencyProperty.Register("Document", typeof(FlowDocument), 
            typeof(BindableRichTextBox), new FrameworkPropertyMetadata
                (null, new PropertyChangedCallback(OnDocumentChanged)));

    /// <summary>
    /// Gets or sets the FlowDocument associated with this RichTextBox
    /// </summary>
    public new FlowDocument Document
    {
        get => (FlowDocument)GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    /// <summary>
    /// Called when the Document property changes.
    /// Updates the internal RichTextBox.Document to reflect the new value
    /// </summary>
    /// <param name="obj">The dependency object on which the property changed</param>
    /// <param name="args">Details about the property change</param>
    public static void OnDocumentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        RichTextBox rtb = (RichTextBox)obj;
        rtb.Document = (FlowDocument)args.NewValue;
    }
}