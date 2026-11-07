using Avalonia;
using Avalonia.Controls;

namespace ProcessExplorer.Views.Controls;

/// <summary>Отображает подпись и многострочное значение в поле только для чтения.</summary>
public partial class InfoField : UserControl
{
    /// <summary>Регистрирует стилизуемое свойство подписи поля.</summary>
    public static readonly StyledProperty<string?> LabelTextProperty =
        AvaloniaProperty.Register<InfoField, string?>(nameof(LabelText));

    /// <summary>Регистрирует стилизуемое свойство отображаемого значения.</summary>
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<InfoField, string?>(nameof(Value));

    /// <summary>Текст подписи, связанной с полем значения.</summary>
    public string? LabelText
    {
        get => GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    /// <summary>Текст отображаемого значения.</summary>
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Загружает разметку элемента отображения сведений.</summary>
    public InfoField()
    {
        InitializeComponent();
    }
}