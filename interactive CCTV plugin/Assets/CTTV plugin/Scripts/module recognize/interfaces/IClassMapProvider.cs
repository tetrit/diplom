namespace Surveillance.Recognize
{
    public interface IClassMapProvider
    {
        bool IsLoaded { get; }
        string GetClassName(int classId);
    }
}