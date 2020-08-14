using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace media_backup.tests
{
    [TestClass]
    public class FilesAreCopiedWithoutDuplicates : SourceFileSpecification<byte[]>
    {
        public FilesAreCopiedWithoutDuplicates() 
        {}
        public override IEnumerable<byte[]> Givens()
        {
            yield return new byte[];
        }

        public override void When()
        {
            throw new NotImplementedException();
        }
    }
}
