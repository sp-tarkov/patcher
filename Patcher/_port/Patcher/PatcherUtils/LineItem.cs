namespace PatcherUtils
{
    public class LineItem
    {
        public string ItemText;
        public int ItemValue;

        public LineItem(string ItemText, int ItemValue = 0)
        {
            this.ItemText = ItemText;
            this.ItemValue = ItemValue;
        }
    }
}
