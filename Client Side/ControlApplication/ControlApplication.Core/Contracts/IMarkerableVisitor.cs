namespace ControlApplication.Core.Contracts
{
    public interface IMarkerableVisitor
    {
        //Visit
        void AddMarker(Detection detection);

        void AddMarker(Area area);
    }
}