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

            // ReportEmployeeOrdersAndNameOfCompanyThatPlacedOrders(dbContext);

            ReportEmployeeOrders(dbContext);



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


    }
}
