using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sukul.Media.Backup.FileSystem;
using Sukul.Media.Backup.Shared;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace media_backup.tests
{
    [TestClass]
    public abstract class SourceFileSpecification
    {
        protected IMediaDiscovery _discovery;
        protected IMediaDestination _destination;
        protected Coordinator<IMediaDiscovery, IMediaDestination> _coordinator;

        public SourceFileSpecification()
        {
        }

        public abstract void Given();

        public abstract void When();

        [TestInitialize]
        public void Run()
        {
            this.Given();
            this.When();
        }

    }
}
