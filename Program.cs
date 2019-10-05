using System;
using System.Collections.Generic;
using System.Linq;
using Northwind_linq.Models;

namespace Northwind_linq
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbContext = new NorthwindContext();

            //SingleTableQueries(dbContext);

            Console.WriteLine("/*************report showing employee orders* *************/");

            var employeeOrders = dbContext.Employees
                                            .Select(x => new
                                            {
                                                Employee = x,
                                                OrdersList = x.Orders.ToList()
                                            });
            foreach (var item in employeeOrders)
            {
                Console.WriteLine($"Employee: {item.Employee.FirstName + item.Employee.LastName}");

                Console.WriteLine( $"List od Orders for {item.Employee.FirstName + item.Employee.LastName}");
                foreach (var order in item.OrdersList)
                {
                    Console.WriteLine($"OrderId: {order.OrderId}");
                }
            }

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
    }
}
