﻿using System;
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
using EdiFabric.Framework.Writers;
using CAE.Helpers.X12;
using EdiFabric.Templates.Hipaa5010;

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

            //  var hipaaStream = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, @"Files.Demo\ClaimPaymentWithError.txt"));
            var hipaaStream = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, @"Files.Demo\sampleEDIFile.txt"));


            using (var ediReader = new X12Reader(hipaaStream, "Edi.Templates.Hipaa5010"))
                hipaaItems = ediReader.ReadToEnd().ToList();

            var hipaaTransactions = hipaaItems.OfType<Edi.Templates.Hipaa5010.TS837P>();

            foreach (var transaction in hipaaTransactions)
            {
                if (transaction.HasErrors)
                {
                    //  partially parsed
                    var errors = transaction.ErrorContext.Flatten();
                    consolidatedErrors.AddRange(errors.ToList());
                    //errors.ToList().ForEach(x => errorMessage.AppendLine(x.ToString()));
                }
                else
                {//check and generate 277
                    var r = Get277(transaction);

                }
            }
            ViewData["Message"] = consolidatedErrors;
            return View();
        }

        static TS277 FetchData277(Edi.Templates.Hipaa5010.TS837P data = null)
        {
            TS277 ts277Data = new TS277();
            ts277Data.Loop2000A = new List<Loop_2000A_277>();
            var loop2000A = new Loop_2000A_277();
            loop2000A.Loop2100A = new Loop_2100A_277();
            loop2000A.Loop2100A.NM1_PayerName = new NM1_CorrectedPriorityPayerName();
            //  End 2000A Loop
            ts277Data.Loop2000A.Add(loop2000A);

            ts277Data.Loop2000A[0].Loop2100A.NM1_PayerName.ResponseContactLastorOrganizationName_03 = data.Loop2000A[0].Loop2000B[0].AllNM1.Loop2010BB.NM1_PayerName.ResponseContactLastorOrganizationName_03;
            ts277Data.Loop2000A[0].Loop2100A.NM1_PayerName.ResponseContactIdentifier_09 = data.Loop2000A[0].Loop2000B[0].AllNM1.Loop2010BB.NM1_PayerName.ResponseContactIdentifier_09;


            loop2000A.Loop2000B = new List<Loop_2000B_277>();
            var loop2000B = new Loop_2000B_277();
            loop2000B.Loop2100B = new Loop_2100B_277();
            loop2000B.Loop2100B.NM1_InformationReceiverName = new NM1_InformationReceiverName_2();
            //  End 2000B Loop
            loop2000A.Loop2000B.Add(loop2000B);

            ts277Data.Loop2000A[0].Loop2000B[0].Loop2100B.NM1_InformationReceiverName.ResponseContactLastorOrganizationName_03 = data.Loop2000A[0].AllNM1.Loop2010AA.NM1_BillingProviderName.ResponseContactLastorOrganizationName_03;
            ts277Data.Loop2000A[0].Loop2000B[0].Loop2100B.NM1_InformationReceiverName.ResponseContactIdentifier_09 = data.Loop2000A[0].AllNM1.Loop2010AA.NM1_BillingProviderName.ResponseContactIdentifier_09;

            loop2000B.Loop2000C = new List<Loop_2000C_277>();

            //  Begin 2000C Loop 1
            var loop2000C1 = new Loop_2000C_277();
            loop2000C1.Loop2100C = new List<Loop_2100C_277>();

            //  Begin 2100C Loop
            var loop2100C = new Loop_2100C_277();
            loop2000C1.Loop2100C.Add(loop2100C);

            //  Repeating 2000D Loops
            loop2000C1.Loop2000D = new List<Loop_2000D_277>();
            //  Begin 2000D Loop 1
            var loop2000D1 = new Loop_2000D_277();
            //  Begin 2100D Loop
            loop2000D1.Loop2100D = new Loop_2100D_277();
            loop2000D1.Loop2100D.NM1_SubscriberName = new NM1_InsuredName();

            loop2000C1.Loop2000D.Add(loop2000D1);

            loop2000B.Loop2000C.Add(loop2000C1);


            ts277Data.Loop2000A[0].Loop2000B[0].Loop2000C[0].Loop2000D[0].Loop2100D.NM1_SubscriberName.ResponseContactLastorOrganizationName_03 = data.Loop2000A[0].Loop2000B[0].AllNM1.Loop2010BA.NM1_SubscriberName.ResponseContactLastorOrganizationName_03;//"SMITH";
            ts277Data.Loop2000A[0].Loop2000B[0].Loop2000C[0].Loop2000D[0].Loop2100D.NM1_SubscriberName.ResponseContactFirstName_04 = data.Loop2000A[0].Loop2000B[0].AllNM1.Loop2010BA.NM1_SubscriberName.ResponseContactFirstName_04;
            ts277Data.Loop2000A[0].Loop2000B[0].Loop2000C[0].Loop2000D[0].Loop2100D.NM1_SubscriberName.IdentificationCodeQualifier_08 = data.Loop2000A[0].Loop2000B[0].AllNM1.Loop2010BA.NM1_SubscriberName.IdentificationCodeQualifier_08;
            ts277Data.Loop2000A[0].Loop2000B[0].Loop2000C[0].Loop2000D[0].Loop2100D.NM1_SubscriberName.ResponseContactIdentifier_09 = data.Loop2000A[0].Loop2000B[0].AllNM1.Loop2010BA.NM1_SubscriberName.ResponseContactIdentifier_09;

            return ts277Data;
        }

        static string Get277(Edi.Templates.Hipaa5010.TS837P ts837Data=null)
        {
            TS277 input277Data = new TS277();
            input277Data = FetchData277(ts837Data);

            var transaction = HipaaTransactionBuilders.Build277ResponseTransmission("1", input277Data);
            using (var stream = new MemoryStream())
            {
                using (var writer = new X12Writer(stream))
                {
                    writer.Write(SegmentBuilders.BuildIsa("1"));
                    writer.Write(SegmentBuilders.BuildGs("1"));
                    writer.Write(transaction);
                }

                var ediString = stream.LoadToString();
                return ediString;
            }

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