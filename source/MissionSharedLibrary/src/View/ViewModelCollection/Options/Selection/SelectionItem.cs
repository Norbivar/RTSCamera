namespace MissionSharedLibrary.View.ViewModelCollection.Options.Selection
{
    public struct SelectionItem
    {
        public bool IsLocalizationId;
        public string Data;
        public string Variation;

        public SelectionItem(bool isLocalizationId, string data, string variation = null)
        {
            IsLocalizationId = isLocalizationId;
            Data = data;
            Variation = variation;
        }
    }
}
