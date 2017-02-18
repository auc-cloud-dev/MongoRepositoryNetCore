using MongoDB.Driver;
using MongoRepository.NetCore;
using MongoRepository.NetCoreTests.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Xunit;

namespace MongoRepository.NetCoreTests
{
    public class RepoTests : IDisposable
    {
        public RepoTests()
        {
            this.DropDB();
        }

        public void Dispose()
        {
            this.DropDB();
        }

        private void DropDB()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();
            var url = new MongoUrl(configuration["MongoServerSettings:connectionString"]);
            // var url = new MongoUrl(ConfigurationManager.ConnectionStrings["MongoServerSettings"].ConnectionString);
            var client = new MongoClient(url);
            client.DropDatabase(url.DatabaseName);
        }

        [Fact]
        public void AddAndUpdateTest()
        {
            IRepository<Customer> _customerRepo = new MongoRepository<Customer>();
            IRepositoryManager<Customer> _customerMan = new MongoRepositoryManager<Customer>();

            Assert.False(_customerMan.Exists);

            var customer = new Customer();
            customer.FirstName = "Bob";
            customer.LastName = "Dillon";
            customer.Phone = "0900999899";
            customer.Email = "Bob.dil@snailmail.com";
            customer.HomeAddress = new Address
            {
                Address1 = "North kingdom 15 west",
                Address2 = "1 north way",
                PostCode = "40990",
                City = "George Town",
                Country = "Alaska"
            };

            _customerRepo.Add(customer);

            Assert.True(_customerMan.Exists);

            Assert.NotNull(customer.Id);

            // fetch it back 
            var alreadyAddedCustomer = _customerRepo.Where(c => c.FirstName == "Bob").Single();

            Assert.NotNull(alreadyAddedCustomer);
            Assert.Equal(customer.FirstName, alreadyAddedCustomer.FirstName);
            Assert.Equal(customer.HomeAddress.Address1, alreadyAddedCustomer.HomeAddress.Address1);

            alreadyAddedCustomer.Phone = "10110111";
            alreadyAddedCustomer.Email = "dil.bob@fastmail.org";

            _customerRepo.Update(alreadyAddedCustomer);

            // fetch by id now 
            var updatedCustomer = _customerRepo.GetById(customer.Id);

            Assert.NotNull(updatedCustomer);
            Assert.Equal(alreadyAddedCustomer.Phone, updatedCustomer.Phone);
            Assert.Equal(alreadyAddedCustomer.Email, updatedCustomer.Email);

            Assert.True(_customerRepo.Exists(c => c.HomeAddress.Country == "Alaska"));
        }

        [Fact]
        public void ComplexEntityTest()
        {
            IRepository<Customer> _customerRepo = new MongoRepository<Customer>();
            IRepository<Product> _productRepo = new MongoRepository<Product>();

            var customer = new Customer();
            customer.FirstName = "Erik";
            customer.LastName = "Swaun";
            customer.Phone = "123 99 8767";
            customer.Email = "erick@mail.com";
            customer.HomeAddress = new Address
            {
                Address1 = "Main bulevard",
                Address2 = "1 west way",
                PostCode = "89560",
                City = "Tempare",
                Country = "Arizona"
            };

            var order = new Order();
            order.PurchaseDate = DateTime.Now.AddDays(-2);
            var orderItems = new List<OrderItem>();

            var shampoo = _productRepo.Add(new Product() { Name = "Palmolive Shampoo", Price = 5 });
            var paste = _productRepo.Add(new Product() { Name = "Mcleans Paste", Price = 4 });


            var item1 = new OrderItem { Product = shampoo, Quantity = 1 };
            var item2 = new OrderItem { Product = paste, Quantity = 2 };

            orderItems.Add(item1);
            orderItems.Add(item2);

            order.Items = orderItems;

            customer.Orders = new List<Order>
            {
                order
            };

            _customerRepo.Add(customer);

            Assert.NotNull(customer.Id);
            Assert.NotNull(customer.Orders[0].Items[0].Product.Id);

            // get the orders  
            var theOrders = _customerRepo.Where(c => c.Id == customer.Id).Select(c => c.Orders).ToList();
            var theOrderItems = theOrders[0].Select(o => o.Items);

            Assert.NotNull(theOrders);
            Assert.NotNull(theOrderItems);
        }


        [Fact]
        public void BatchTest()
        {
            IRepository<Customer> _customerRepo = new MongoRepository<Customer>();

            var custlist = new List<Customer>(new Customer[] {
                new Customer() { FirstName = "Customer A" },
                new Customer() { FirstName = "Client B" },
                new Customer() { FirstName = "Customer C" },
                new Customer() { FirstName = "Client D" },
                new Customer() { FirstName = "Customer E" },
                new Customer() { FirstName = "Client F" },
                new Customer() { FirstName = "Customer G" },
            });

            //Insert batch
            _customerRepo.Add(custlist);

            var count = _customerRepo.Count();
            Assert.Equal(7, count);
            foreach (Customer c in custlist)
                Assert.NotEqual(new string('0', 24), c.Id);

            //Update batch
            foreach (Customer c in custlist)
                c.LastName = c.FirstName;
            _customerRepo.Update(custlist);

            foreach (Customer c in _customerRepo)
                Assert.Equal(c.FirstName, c.LastName);

            //Delete by criteria
            _customerRepo.Delete(f => f.FirstName.StartsWith("Client"));

            count = _customerRepo.Count();
            Assert.Equal(4, count);

            //Delete specific object
            _customerRepo.Delete(custlist[0]);

            //Test AsQueryable
            var selectedcustomers = from cust in _customerRepo
                                    where cust.LastName.EndsWith("C") || cust.LastName.EndsWith("G")
                                    select cust;

            Assert.Equal(2, selectedcustomers.ToList().Count);

            count = _customerRepo.Count();
            Assert.Equal(3, count);

            //Drop entire repo
            new MongoRepositoryManager<Customer>().Drop();

            count = _customerRepo.Count();
            Assert.Equal(0, count);
        }

        [Fact]
        public void CollectionNamesTest()
        {
            var a = new MongoRepository<Animal>();
            var am = new MongoRepositoryManager<Animal>();
            var va = new Dog();
            Assert.False(am.Exists);
            a.Update(va);
            Assert.True(am.Exists);
            Assert.IsType(typeof(Dog), a.GetById(va.Id));
            Assert.Equal(am.Name, "AnimalsTest");
            Assert.Equal(a.CollectionName, "AnimalsTest");

            var cl = new MongoRepository<CatLike>();
            var clm = new MongoRepositoryManager<CatLike>();
            var vcl = new Lion();
            Assert.False(clm.Exists);
            cl.Update(vcl);
            Assert.True(clm.Exists);
            Assert.IsType(typeof(Lion), cl.GetById(vcl.Id));
            Assert.Equal(clm.Name, "Catlikes");
            Assert.Equal(cl.CollectionName, "Catlikes");

            var b = new MongoRepository<Bird>();
            var bm = new MongoRepositoryManager<Bird>();
            var vb = new Bird();
            Assert.False(bm.Exists);
            b.Update(vb);
            Assert.True(bm.Exists);
            Assert.IsType(typeof(Bird), b.GetById(vb.Id));
            Assert.Equal(bm.Name, "Birds");
            Assert.Equal(b.CollectionName, "Birds");

            var l = new MongoRepository<Lion>();
            var lm = new MongoRepositoryManager<Lion>();
            var vl = new Lion();
            //Assert.False(lm.Exists);   //Should already exist (created by cl)
            l.Update(vl);
            Assert.True(lm.Exists);
            Assert.IsType(typeof(Lion), l.GetById(vl.Id));
            Assert.Equal(lm.Name, "Catlikes");
            Assert.Equal(l.CollectionName, "Catlikes");

            var d = new MongoRepository<Dog>();
            var dm = new MongoRepositoryManager<Dog>();
            var vd = new Dog();
            //Assert.False(dm.Exists);
            d.Update(vd);
            Assert.True(dm.Exists);
            Assert.IsType(typeof(Dog), d.GetById(vd.Id));
            Assert.Equal(dm.Name, "AnimalsTest");
            Assert.Equal(d.CollectionName, "AnimalsTest");

            var m = new MongoRepository<Bird>();
            var mm = new MongoRepositoryManager<Bird>();
            var vm = new Macaw();
            //Assert.False(mm.Exists);
            m.Update(vm);
            Assert.True(mm.Exists);
            Assert.IsType(typeof(Macaw), m.GetById(vm.Id));
            Assert.Equal(mm.Name, "Birds");
            Assert.Equal(m.CollectionName, "Birds");

            var w = new MongoRepository<Whale>();
            var wm = new MongoRepositoryManager<Whale>();
            var vw = new Whale();
            Assert.False(wm.Exists);
            w.Update(vw);
            Assert.True(wm.Exists);
            Assert.IsType(typeof(Whale), w.GetById(vw.Id));
            Assert.Equal(wm.Name, "Whale");
            Assert.Equal(w.CollectionName, "Whale");
        }

        [Fact]
        public void CustomIDTest()
        {
            var x = new MongoRepository<CustomIDEntity>();
            var xm = new MongoRepositoryManager<CustomIDEntity>();

            x.Add(new CustomIDEntity() { Id = "aaa" });

            Assert.True(xm.Exists);
            Assert.IsType(typeof(CustomIDEntity), x.GetById("aaa"));

            Assert.Equal("aaa", x.GetById("aaa").Id);

            x.Delete("aaa");
            Assert.Equal(0, x.Count());

            var y = new MongoRepository<CustomIDEntityCustomCollection>();
            var ym = new MongoRepositoryManager<CustomIDEntityCustomCollection>();

            y.Add(new CustomIDEntityCustomCollection() { Id = "xyz" });

            Assert.True(ym.Exists);
            Assert.Equal(ym.Name, "MyTestCollection");
            Assert.Equal(y.CollectionName, "MyTestCollection");
            Assert.IsType(typeof(CustomIDEntityCustomCollection), y.GetById("xyz"));

            y.Delete("xyz");
            Assert.Equal(0, y.Count());
        }

        [Fact]
        public void CustomIDTypeTest()
        {
            var xint = new MongoRepository<IntCustomer, int>();
            xint.Add(new IntCustomer() { Id = 1, Name = "Test A" });
            xint.Add(new IntCustomer() { Id = 2, Name = "Test B" });

            var yint = xint.GetById(2);
            Assert.Equal(yint.Name, "Test B");

            xint.Delete(2);
            Assert.Equal(1, xint.Count());
        }

        [Fact]
        public void OverrideCollectionName()
        {
            IRepository<Customer> _customerRepo = new MongoRepository<Customer>("mongodb://localhost/MongoRepositoryTests", "TestCustomers123");
            _customerRepo.Add(new Customer() { FirstName = "Test" });
            Assert.True(_customerRepo.Single().FirstName.Equals("Test"));
            Assert.Equal("TestCustomers123", _customerRepo.Collection.CollectionNamespace.CollectionName);
            Assert.Equal("TestCustomers123", ((MongoRepository<Customer>)_customerRepo).CollectionName);

            IRepositoryManager<Customer> _curstomerRepoManager = new MongoRepositoryManager<Customer>("mongodb://localhost/MongoRepositoryTests", "TestCustomers123");
            Assert.Equal("TestCustomers123", _curstomerRepoManager.Name);
        }

        #region Reproduce issue: https://mongorepository.codeplex.com/discussions/433878
        public abstract class BaseItem : IEntity
        {
            public string Id { get; set; }
        }

        public abstract class BaseA : BaseItem
        { }

        public class SpecialA : BaseA
        { }

        [Fact]
        public void Discussion433878()
        {
            var specialRepository = new MongoRepository<SpecialA>();
        }
        #endregion

        #region Reproduce issue: https://mongorepository.codeplex.com/discussions/572382
        public abstract class ClassA : Entity
        {
            public string Prop1 { get; set; }
        }

        public class ClassB : ClassA
        {
            public string Prop2 { get; set; }
        }

        public class ClassC : ClassA
        {
            public string Prop3 { get; set; }
        }

        [Fact]
        public void Discussion572382()
        {
            var repo = new MongoRepository<ClassA>() { 
                new ClassB() { Prop1 = "A", Prop2 = "B" } ,
                new ClassC() { Prop1 = "A", Prop3 = "C" }
            };

            Assert.Equal(2, repo.Count());

            Assert.Equal(2, repo.OfType<ClassA>().Count());
            Assert.Equal(1, repo.OfType<ClassB>().Count());
            Assert.Equal(1, repo.OfType<ClassC>().Count());
        }
        #endregion

    }
}
