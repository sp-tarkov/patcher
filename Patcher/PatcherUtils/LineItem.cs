namespace PatcherUtils
{
    public class LineItem
    {
        public string ItemText;
        public string ItemValue;
        public bool HasValue => ItemValue != "";

        public LineItem(string ItemText, string ItemValue = "")
        {
            this.ItemText = ItemText;
            this.ItemValue = ItemValue;
        }
    }
}
