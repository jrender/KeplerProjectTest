using Relativity.Kepler.Services;

namespace KeplerProjectTemplate1.Interfaces.LegilityTest
{
    /// <summary>
    /// LegilityTest Module Interface.
    /// </summary>
    [ServiceModule("LegilityTest Module")]
    [RoutePrefix("LegilityTest")]
    //[RoutePrefix("LegilityTest", VersioningStrategy.Namespace)]
    public interface ILegilityTestModule
    {
    }
}