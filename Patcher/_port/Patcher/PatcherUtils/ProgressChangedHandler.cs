namespace PatcherUtils
{
    /// <summary>
    /// <see cref="WriteProgressHandler(object, int, int, int, string, string[])"/> delegate
    /// </summary>
    /// <param name="Sender">The object calling the handler</param>
    /// <param name="Progress">The current number of items processed</param>
    /// <param name="Total">The total number of items</param>
    /// <param name="Percent">The percentage of items processed</param>
    /// <param name="Message">An optional message to display above the progress bar</param>
    /// <param name="AdditionalLineItems">Additional information to display below the progress bar.</param>
    public delegate void ProgressChangedHandler(object Sender, int Progress, int Total, int Percent, string Message = "", params LineItem[] AdditionalLineItems);
}
