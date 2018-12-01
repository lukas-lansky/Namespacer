using Microsoft.VisualStudio.TestTools.UnitTesting;
using Namespacer.Configuration;
using Namespacer.Engine;

namespace Namespacer.Test.Engine
{
    [TestClass]
    class EvaluationEngineTest
    {
        [TestMethod]
        public void EmptyConfigAllowsEverything()
        {
            var config = ConfigFile.LoadFromString("").Value;

            Assert.IsTrue(EvaluationEngine.IsOk("Abc", "Cde", config));
            Assert.IsTrue(EvaluationEngine.IsOk("Abc", "Abc", config));
            Assert.IsTrue(EvaluationEngine.IsOk("Abc.Cde", "Abc", config));
            Assert.IsTrue(EvaluationEngine.IsOk("Abc", "Abc.Cde", config));
        }

        [TestMethod]
        public void DisallowingSystemConsoleWorks()
        {
            var config = ConfigFile.LoadFromString(@"* => System.Console:
    * -!> *").Value;

            Assert.IsTrue(EvaluationEngine.IsOk("Abc", "System.String", config));
            Assert.IsFalse(EvaluationEngine.IsOk("Abc", "System.Console", config));
            Assert.IsFalse(EvaluationEngine.IsOk("System.Console", "System.Console", config));
        }

        [TestMethod]
        public void BlogpostExample()
        {
            var config = ConfigFile.LoadFromString(@"Product => Product:
    Product.Controllers->Product.Services
    Product.Services->Product.Repositories
    Product.Services->Product.Proxy
    * -!> *").Value;

            Assert.IsTrue(EvaluationEngine.IsOk("Product.Controllers.OrderController", "Product.Services.OrderService", config));
            Assert.IsTrue(EvaluationEngine.IsOk("Product.Services.OrderService", "Product.Repositories.OrderRepository", config));
            Assert.IsTrue(EvaluationEngine.IsOk("Product.Services.OrderService", "Product.Proxy.OrderProxy", config));

            Assert.IsFalse(EvaluationEngine.IsOk("Product.Services.OrderService", "Product.Services.OrderService", config));
            Assert.IsFalse(EvaluationEngine.IsOk("Product.Controllers.OrderController", "Product.Controllers.OrderController", config));
            Assert.IsFalse(EvaluationEngine.IsOk("Product.Repositories.OrderRepository", "Product.Repositories.OrderRepository", config));
        }
    }
}
