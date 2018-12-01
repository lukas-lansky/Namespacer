using Microsoft.VisualStudio.TestTools.UnitTesting;
using Namespacer.Configuration;

namespace Namespacer.Test.Configuration
{
    [TestClass]
    class ConfigFileTest
    {
        [TestMethod]
        public void EmptyConfigIsEmpty()
        {
            var config = ConfigFile.LoadFromString("").Value;
            Assert.AreEqual(0, config.Rules.Count);
        }

        [TestMethod]
        public void DisallowingSystemConsoleWorks()
        {
            var config = ConfigFile.LoadFromString(@"* => System.Console:
    * -!> *").Value;

            Assert.AreEqual(1, config.Rules.Count);

            var rule = config.Rules[0];

            Assert.AreEqual("*", rule.NamespaceFrom);
            Assert.AreEqual("System.Console", rule.NamespaceTo);
            Assert.AreEqual(1, rule.Prescriptions.Count);
            
            var prescription = rule.Prescriptions[0];

            Assert.AreEqual(PrescriptionType.Disallow, prescription.Type);
            Assert.AreEqual("*", prescription.NamespaceFrom);
            Assert.AreEqual("*", prescription.NamespaceTo);
        }

        [TestMethod]
        public void BlogpostExample()
        {
            var config = ConfigFile.LoadFromString(@"Product => Product:
    Product.Controllers->Product.Services
    Product.Services->Product.Repositories
    Product.Services->Product.Proxy
    * -!> *").Value;

            Assert.AreEqual(1, config.Rules.Count);

            var rule = config.Rules[0];

            Assert.AreEqual(rule.NamespaceFrom, "Product");
            Assert.AreEqual(rule.NamespaceTo, "Product");
            Assert.AreEqual(rule.Prescriptions.Count, 4);

            var prescription1 = rule.Prescriptions[0];
            Assert.AreEqual(prescription1.Type, PrescriptionType.Allow);
            Assert.AreEqual(prescription1.NamespaceFrom, "Product.Controllers");
            Assert.AreEqual(prescription1.NamespaceTo, "Product.Services");

            var prescription2 = rule.Prescriptions[1];
            Assert.AreEqual(prescription2.Type, PrescriptionType.Allow);
            Assert.AreEqual(prescription2.NamespaceFrom, "Product.Services");
            Assert.AreEqual(prescription2.NamespaceTo, "Product.Repositories");

            var prescription3 = rule.Prescriptions[2];
            Assert.AreEqual(prescription3.Type, PrescriptionType.Allow);
            Assert.AreEqual(prescription3.NamespaceFrom, "Product.Services");
            Assert.AreEqual(prescription3.NamespaceTo, "Product.Proxy");

            var prescription4 = rule.Prescriptions[3];
            Assert.AreEqual(prescription4.Type, PrescriptionType.Disallow);
            Assert.AreEqual(prescription4.NamespaceFrom, "*");
            Assert.AreEqual(prescription4.NamespaceTo, "*");
        }
    }
}
