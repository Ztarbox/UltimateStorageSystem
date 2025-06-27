namespace UltimateStorageSystem.Interfaces
{
    public interface IScrollableTable
    {
        int ScrollIndex { get; set; }

        int GetItemEntriesCount();

        int GetVisibleRows();
    }
}