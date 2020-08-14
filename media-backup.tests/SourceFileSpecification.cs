using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace media_backup.tests
{
    [TestClass]
    public abstract class SourceFileSpecification<T>
    {
        public void GiveSourceFile()
        {
            this.Givens();
        }

        public abstract IEnumerable<T> Givens();

        [TestInitialize]
        public abstract void When();

    }
}
