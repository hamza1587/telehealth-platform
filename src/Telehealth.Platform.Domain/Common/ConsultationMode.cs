namespace Telehealth.Platform.Domain.Common;

/// <summary>
/// Defines the mode of consultation.
/// </summary>
public enum ConsultationMode
{
    /// <summary>
    /// Video call consultation.
    /// </summary>
    Video = 1,

    /// <summary>
    /// Phone call consultation.
    /// </summary>
    Phone = 2,

    /// <summary>
    /// Text/chat consultation.
    /// </summary>
    Text = 3
}