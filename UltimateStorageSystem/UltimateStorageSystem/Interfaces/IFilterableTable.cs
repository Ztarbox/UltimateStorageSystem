namespace UltimateStorageSystem.Interfaces
{
    public interface IFilterableTable
    {
        int ScrollIndex { get; set; }

        string SortedColumn { get; }

        bool isAscending { get; }

        void FilterItems(string searchText);

        void SortItemsBy(string sortBy, bool ascending);
    }
}