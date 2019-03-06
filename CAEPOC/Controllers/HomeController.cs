using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CAEPOC.Models;
using EdiFabric.Framework.Readers;
using EdiFabric.Core.Model.Edi;
using System.IO;
using Edi.Templates.Hipaa5010;
using Microsoft.AspNetCore.Hosting;
using System.Text;

namespace CAEPOC.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        public HomeController(IHostingEnvironment environment)
        {
            _hostingEnvironment = environment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {

            ViewData["Message"] = "Your message";
           // hipaaTransactions.First().


            return View();
        }
        public IActionResult POC()
        {
            StringBuilder errorMessage = new StringBuilder();
            List<IEdiItem> hipaaItems;
            List<string> consolidatedErrors = new List<string>();
            //var hipaaStream = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, @"Files.Demo\Hipaa005010ClaimPayment.txt"));

         var hipaaStream = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, @"Files.Demo\ClaimPaymentWithError.txt"));
         //   var hipaaStream = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, @"Files.Demo\sampleEDIFile.txt"));


            using (var ediReader = new X12Reader(hipaaStream, "Edi.Templates.Hipaa5010"))
                hipaaItems = ediReader.ReadToEnd().ToList();

            var hipaaTransactions = hipaaItems.OfType<TS837P>();

            foreach (var transaction in hipaaTransactions)
            {
                if (transaction.HasErrors)
                {
                    //  partially parsed
                    var errors = transaction.ErrorContext.Flatten();
                    consolidatedErrors.AddRange(errors.ToList());
                    //errors.ToList().ForEach(x => errorMessage.AppendLine(x.ToString()));
                }
            }
            ViewData["Message"] = consolidatedErrors;
            return View();
        }
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
