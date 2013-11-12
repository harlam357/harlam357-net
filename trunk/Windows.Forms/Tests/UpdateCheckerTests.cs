
using System;
using System.IO;

using NUnit.Framework;

namespace harlam357.Windows.Forms.Tests
{
   [TestFixture]
   public class UpdateCheckerTests
   {
      [Test]
      public void UpdateChecker_CheckForUpdate_Test()
      {
         var checker = new UpdateChecker();
         ApplicationUpdate update = checker.CheckForUpdate("uniqueId", Path.Combine(Environment.CurrentDirectory, "ApplicationUpdate.xml"));
         
         Assert.AreEqual("0.4.10.156", update.Version);
         Assert.AreEqual(new DateTime(2010, 3, 18), update.UpdateDate);
         Assert.AreEqual(2, update.UpdateFiles.Count);
         Assert.AreEqual("Windows Installer", update.UpdateFiles[0].Description);
         Assert.AreEqual("http://hfm-net.googlecode.com/files/HFM Release 0.4.10.156.msi", update.UpdateFiles[0].HttpAddress);
         Assert.AreEqual(1569792, update.UpdateFiles[0].Size);
         Assert.AreEqual("30847cc654974d969f8f5a3f2423040d", update.UpdateFiles[0].MD5);
         Assert.AreEqual("b38dd073546647dcf37d89d62cb63823934c8fce", update.UpdateFiles[0].SHA1);
         Assert.AreEqual(0, update.UpdateFiles[0].UpdateType);
         Assert.AreEqual("Zip Archive", update.UpdateFiles[1].Description);
         Assert.AreEqual("http://hfm-net.googlecode.com/files/HFM Release 0.4.10.156.zip", update.UpdateFiles[1].HttpAddress);
         Assert.AreEqual(584621, update.UpdateFiles[1].Size);
         Assert.AreEqual("d1ea4151b2165c6a7ef3ec519b479464", update.UpdateFiles[1].MD5);
         Assert.AreEqual("4103946141f90105510827a0baf13cb38cb00256", update.UpdateFiles[1].SHA1);
         Assert.AreEqual(1, update.UpdateFiles[1].UpdateType);
      }
   }
}
