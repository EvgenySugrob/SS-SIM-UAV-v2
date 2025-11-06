namespace Vmaya.RW
{
    public interface INameOwner
    {
        void ResetNames();
        string checkNextName(string origin);
    } 

    public interface INameProvider: INameOwner
    {
        void reset();
    }

    public interface IRW
    {
        Indent getIndent();
        bool readRecord(RWEvents.dataRecord rec);
        RWEvents.dataRecord writeRecord();
        string writeData();
    }

    public interface IWriter
    {
        void Save(string dataName);
    }

    public interface IOpener
    {
        void Open(string dataName);
    }
}