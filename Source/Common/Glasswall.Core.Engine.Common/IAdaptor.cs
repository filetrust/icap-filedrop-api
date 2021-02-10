namespace Glasswall.Core.Engine.Common
{
    public interface IAdaptor<in TAdaptee, out TTarget>
    {
        TTarget Adapt(TAdaptee adaptee);
    }
}
