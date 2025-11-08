
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Project1.Models;

public class ProductController : Controller
{
    public IActionResult Index()
    {
        var products = new List<Product>    //Product is imported model
        {
            new Product {ID = 1, Name= "Laptop", Price= 1000},
            new Product {ID = 2, Name= "Phone", Price= 11000}


        };
        return View(products);
    }
}