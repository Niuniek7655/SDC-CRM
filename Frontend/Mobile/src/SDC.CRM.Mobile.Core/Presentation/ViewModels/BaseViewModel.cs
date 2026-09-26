using CommunityToolkit.Mvvm.ComponentModel;

namespace SDC.CRM.Mobile.Presentation.ViewModels;

/// <summary>
/// Bazowy ViewModel z podstawowymi właściwościami dla wszystkich ViewModeli.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    // Partial properties (C# 14) instead of [ObservableProperty] fields: the generated code is
    // trimming/AOT friendly, including WinRT (MVVMTK0045).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    public bool IsNotBusy => !IsBusy;

    protected void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    protected void ClearError()
    {
        ErrorMessage = null;
        HasError = false;
    }
}

