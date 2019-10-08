using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Northwind_linq.Models;

namespace Northwind_linq
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbContext = new NorthwindContext();

            //SingleTableQueries(dbContext);

            // ReportEmployeeOrdersAndNameOfCompanyThatPlacedOrders(dbContext);

            // ReportEmployeeOrders(dbContext);

            //ReportCustomerProductSupplierName(dbContext);
            ReportProductsBoughtOrSoldByPeopleWhoLiveInLondon(dbContext);

        }

        private static void SingleTableQueries(NorthwindContext dbContext)
        {
            /************************************* First _Last name **************/

            var employees = dbContext.Employees.Select(x => new { x.FirstName, x.LastName });

            foreach (var empl in employees)
            {
                Console.WriteLine($"FirstName: {empl.FirstName} , LastName:{empl.LastName}");
            }


            Console.WriteLine("/************************************* First _Last name Sorted **************/");

            var employeesSorted = employees.OrderBy(x => x.LastName);

            foreach (var empl in employees)
            {
                Console.WriteLine($"FirstName: {empl.FirstName} , LastName:{empl.LastName}");
            }


            Console.WriteLine("/*************showing Northwind's orders sorted by Freight from most expensive to cheapest.* *************/");

            var orders = dbContext.Orders
                                .Where(x => x.ShippedDate != null)
                                .Select(x => new { x.OrderId, x.OrderDate, x.ShippedDate, x.CustomerId, x.Freight })
                                .OrderByDescending(x => x.Freight);

            foreach (var order in orders)
            {
                Console.WriteLine($"ID: {order.OrderId} , OrderDate:{order.OrderDate}, ShippedDate: {((DateTime)order.ShippedDate).Date}, CustID: {order.CustomerId}, Freight: {order.Freight}");
            }
        }

        private static void ReportEmployeeOrdersAndNameOfCompanyThatPlacedOrders(NorthwindContext dbContext)
        {
            Console.WriteLine("/****************************************************************/");
            Console.WriteLine("/************* report showing the Order ID, the name of the company that placed the order, and the first and last name of the associated employee. * ************/");
            Console.WriteLine("/****************************************************************/");


            /*
            var employeeOrders2 = dbContext.Employees
                                   // *******Trying to access the related entity here and doing filtering is wrong.********
                                   // *******The related entity should be accessed in the Select projection, any filtering on this related entity should be done there. *********
                                   .Where(x => x.Orders.Where(y => y.OrderDate > date)) 
                                   .Select(x => new
                                   {
                                       EmployeeName = x.FirstName + x.LastName,
                                   });
            */
            var date = new DateTime(1988, 01, 1);
            var employeeOrders2 = dbContext.Employees
                                           .Select(x => new
                                           {
                                               EmployeeName = x.FirstName + x.LastName,
                                               OrderCustomer = x.Orders // project into orders here, i.e access related entity in the Select Projection.
                                                                .Where(or => or.OrderDate > date)
                                                                .OrderBy(c => c.Customer.CompanyName)
                                                                .Select(y => new
                                                                {
                                                                    y.OrderId,
                                                                    y.Customer.CompanyName
                                                                })

                                           });





            foreach (var item in employeeOrders2)
            {
                foreach (var ordercus in item.OrderCustomer)
                {
                    Console.WriteLine($"OrderId: {ordercus.OrderId} Customer: {ordercus.CompanyName}, Employee: {item.EmployeeName}");
                }
            }

        }

        /// <summary>
        /// Get data and related data by projecting into it, using SELECT operator to access and load /filter related data
        /// </summary>
        /// <param name="dbContext"></param>
        private static void ReportEmployeeOrders(NorthwindContext dbContext)
        {
            // title and names of employees who have sold products: 'Gravad Lax', 'Mishi Kobe Niku'

            var employees = dbContext.Products
                                    .Where(x => x.ProductName == "Gravad Lax" || x.ProductName == "Mishi Kobe Niku")
                                    .Select(y => new  // project to the related entity: OrderDetails
                                    {
                                        employee = y.OrderDetails
                                                    .Select(y => new // project to the related entity: Orders
                                                    {
                                                        employeesDetails = y.Order.Employee
                                                    }),
                                        product = y.ProductName
                                    });

            var employeesProducts = employees
                                    .AsEnumerable()
                                    .GroupBy(x => x.product);


            Console.WriteLine("****************************************************************************");
            Console.WriteLine(" Title and names of employees who have sold products: 'Gravad Lax', 'Mishi Kobe Niku' ");
            Console.WriteLine("****************************************************************************");


            foreach (var employeeProductGroup in employeesProducts)
            {
                Console.WriteLine($" ********************************* Prooduct Name: {employeeProductGroup.Key}");

                foreach (var item in employeeProductGroup.SelectMany(x => x.employee))
                {

                    Console.WriteLine($"{item.employeesDetails.Title} {item.employeesDetails.FirstName} {item.employeesDetails.LastName}");
                }
            }

        }


        /// <summary>
        /// Get data and related data by using Include and ThenInclude, then later iterate over it
        /// </summary>
        /// <param name="context"></param>
        private static void ReportCustomerProductSupplierName(NorthwindContext context)
        {
            Console.WriteLine("****************************************************************************");
            Console.WriteLine("  customer name, the product name and the supplier name for customers who live" +
                              " in London and suppliers whose name is `Pavlova, Ltd.' or `Karkki Oy' ");
            Console.WriteLine("****************************************************************************");

            // load all data and all related data
            var suppliers = context.Suppliers
                              .Include(x => x.Products)
                                .ThenInclude(y => y.OrderDetails)
                                 .ThenInclude(y => y.Order)
                                    .ThenInclude(y => y.Employee)
                              .ToList();

            suppliers = suppliers.Where(x => x.CompanyName == "Pavlova, Ltd." || x.CompanyName == "Karkki Oy").ToList();


            var responseList = new List<ReportCustomerProductSupplierNameDTO>();


            foreach (var supplier in suppliers)
            {
                foreach (var product in supplier.Products)
                {
                    foreach (var orderDetails in product.OrderDetails)
                    {
                        var employee = orderDetails.Order.Employee;
                        if (employee.City == "London") { 
                            responseList.Add(new ReportCustomerProductSupplierNameDTO
                            {
                                CustomrName = $"{employee.FirstName} {employee.LastName}",
                                ProductName = product.ProductName,
                                SupplierName = supplier.CompanyName
                            });
                        }
                    }
                }
            }

            foreach (var item in responseList)
            {
                Console.WriteLine($"CustomerName: {item.CustomrName}, ProductName: {item.ProductName}, SupplierName: {item.SupplierName}");
            }

        }

        /// <summary>
        /// Get data and related data by using Include and ThenInclude, then later iterate over it
        /// </summary>
        /// <param name="context"></param>
        private static void ReportProductsBoughtOrSoldByPeopleWhoLiveInLondon (NorthwindContext context)
        {
            Console.WriteLine("****************************************************************************");
            Console.WriteLine(" name of products that were bought or sold by people who live in London");
            Console.WriteLine("****************************************************************************");


            Console.WriteLine("==================== Data with Relted Data Using Includes =========================");

            var products = context.Products
                                 .Include(x => x.OrderDetails)
                                    .ThenInclude(y => y.Order)
                                        .ThenInclude(y => y.Customer)
                                 .Include(x => x.OrderDetails)
                                        .ThenInclude(y => y.Order)
                                            .ThenInclude(y => y.Employee)
                                 .ToList();

            var productNames = new List<string> (); 

            foreach (var product in products)
            {
                foreach (var orderDetail in product.OrderDetails)
                {
                    if (orderDetail.Order.Customer.City == "London" || (orderDetail.Order.Employee.City == "London"))
                    {
                        productNames.Add(product.ProductName);
                    }
                }
            }


            Console.WriteLine("Count: {0}", productNames.Distinct().ToList().Count());

            productNames.Distinct().ToList().ForEach(x => Console.WriteLine($"{x}"));



            Console.WriteLine("****************************************************************************");
            Console.WriteLine(" name of products that were bought or sold by people who live in London");
            Console.WriteLine("****************************************************************************");


            Console.WriteLine("==================== Data with Relted Data Using SELECT Projection =========================");

            var productNames2 = context.Products
                                  .Select(x => new
                                  {
                                    validList =  x.OrderDetails.Select(y => new
                                              {
                                                  validLondonCustomer = (y.Order.Customer.City == "London"
                                                                            || y.Order.Employee.City == "London") ? y.Product.ProductName :  null,
                                              })
                                              .Where(z => z.validLondonCustomer != null ).ToList()
                                  }).ToList();

            var productNames2Flattened = productNames2.SelectMany(x => x.validList).ToList();

            Console.WriteLine("Count: {0}", productNames2Flattened.Distinct().ToList().Count());

            productNames.Distinct().ToList().ForEach(x => Console.WriteLine($"{x}"));

        }
    }

}


public class ReportCustomerProductSupplierNameDTO
{
    public string CustomrName { get; set; }
    public string ProductName { get; set; }
    public string SupplierName { get; set; }
}