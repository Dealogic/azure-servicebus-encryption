namespace Dealogic.ServiceBus.Azure.Encryption.UnitTest
{
    using System.Diagnostics.Tracing;
    using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TracingTests
    {
        [TestMethod]
        public void EventSourceConsistencyTest()
        {
            var eventSource = new PrivateType("Dealogic.ServiceBus.Azure.Encryption", "Dealogic.ServiceBus.Azure.Encryption.Tracing.ServiceBusEncryptionEventSource");
            var log = (EventSource)eventSource.GetStaticProperty("Log");
            EventSourceAnalyzer.InspectAll(log);
        }
    }
}