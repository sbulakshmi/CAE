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
using EdiFabric.Framework.Writers;
using CAE.Helpers.X12;
using EdiFabric.Templates.Hipaa5010;
using CAEPOC.Interfaces;
using HL7.Dotnetcore;
//using MongoDB.Driver;
//using Microsoft.Extensions.Options;
//using MongoDB.Bson;

namespace CAEPOC.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        private readonly ICAERepository _cAERepository;

        //private readonly IMongoDatabase _db;
        //private readonly IMongoClient client;

        //public HomeController(IHostingEnvironment environment, IOptions<Settings> options)
        public HomeController(IHostingEnvironment environment, ICAERepository cAERepository)
        {
            _hostingEnvironment = environment;
            _cAERepository = cAERepository;
            //client = new MongoClient(options.Value.ConnectionString);
            //_db = client.GetDatabase(options.Value.Database);
            //var collection = _db.GetCollection<BsonDocument>("settings");
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
        public string ParseHL7()
        {
            var hl7Stream = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, @"Files.Demo\hl7Sample.txt"));
            Message message = new Message(hl7Stream.LoadToString());
            bool isParsed = false;
            try
            {
                isParsed = message.ParseMessage();
                if (isParsed)
                {
                    List<Segment> segList = message.Segments();

                    //OBX
                    List<Segment> OBXList = message.Segments("OBX");

                    foreach (Segment seg in OBXList)
                    {
                        if ((seg.Fields(2).Value == "CE" || seg.Fields(2).Value == "SN") && seg.Fields(3).IsComponentized && seg.Fields(5).IsComponentized)
                        {
                            //bool isRepeated = seg.Fields(3).HasRepetitions;
                            //code - 3 (1,3)
                            var OBXId = seg.Fields(3).Components()[0];
                            var OBXIdCode = seg.Fields(3).Components()[3];
                            //value - 5 (1,3)
                            var OBXVal = seg.Fields(5).Components()[0];
                            var OBXValCode = seg.Fields(5).Components()[3];

                        }
                    }
                }
               
            }
            catch (Exception ex)
            {
                // Handle the exception
                throw ex;
            }
            return string.Empty;
        }
        public IActionResult POC()
        {
            StringBuilder errorMessage = new StringBuilder();
            List<IEdiItem> hipaaItems;
            List<string> consolidatedErrors = new List<string>();
            //var hipaaStream = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, @"Files.Demo\Hipaa005010ClaimPayment.txt"));

            //  var hipaaStream = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, @"Files.Demo\ClaimPaymentWithError.txt"));
            //var hipaaStream = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, @"Files.Demo\sampleEDIFile.txt"));
            var hipaaStream = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, @"Files.Demo\sampleEDIFileEditedCPT.txt"));
            //  var hipaaStream = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, @"Files.Demo\837_5010X222A1.X12"));


            using (var ediReader = new X12Reader(hipaaStream, "Edi.Templates.Hipaa5010"))
                hipaaItems = ediReader.ReadToEnd().ToList();

            var hipaaTransactions = hipaaItems.OfType<Edi.Templates.Hipaa5010.TS837P>();

            foreach (var transaction in hipaaTransactions)
            {
                var js = Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
                if (transaction.HasErrors)
                {
                    //  partially parsed
                    var errors = transaction.ErrorContext.Flatten();
                    consolidatedErrors.AddRange(errors.ToList());
                    //errors.ToList().ForEach(x => errorMessage.AppendLine(x.ToString()));
                }
                else
                {//check and generate 277
                    _cAERepository.AddT837PClaim(transaction);
                    (TS277 t, long ctrlNum) = Get277(transaction);
                    _cAERepository.AddT277(t);
                    var r = Get277Edi(t);
                    Save277Edi(r, ctrlNum);
                }
            }
            ViewData["Message"] = consolidatedErrors;
            return View();
        }

        private void Save277Edi(string r, long ctrlNum)
        {
           System.IO.File.WriteAllTextAsync(Path.Combine(_hostingEnvironment.WebRootPath, $"Upload\\277\\Output\\{ctrlNum}.edi"),r);
        }

        private TS277 FetchData277(Edi.Templates.Hipaa5010.TS837P data = null)
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

            loop2000D1.Loop2200D = new List<Loop_2200D_277>();

            
            //  Begin 2200D Loop
            var loop2200D = new Loop_2200D_277();
            loop2000D1.Loop2200D.Add(loop2200D);
            loop2200D.STC_ClaimLevelStatusInformation = new List<STC_BillingProviderStatusInformation>();

            //get the list of LOINC codes and add a request for each
            foreach (string reqCode in GetRequestCodes(data.Loop2000A[0].Loop2000B[0].Loop2300[0].Loop2400[0].SV1_ProfessionalService.CompositeMedicalProcedureIdentifier_01.ProcedureCode_02).Result.Distinct())
            {
                var stc1 = new STC_BillingProviderStatusInformation();
                stc1.HealthCareClaimStatus_01 = new C043_HealthCareClaimStatus();
                loop2200D.STC_ClaimLevelStatusInformation.Add(stc1);
                //ex STC*R4:18657-7::LOI*2*3*4*5*6*7*8*9*R4:18803-7::LOI~
                stc1.HealthCareClaimStatus_01.HealthCareClaimStatusCategoryCode_01 = "R4";
                stc1.HealthCareClaimStatus_01.StatusCode_02 = reqCode;// GetCPT2Loinc(data.Loop2000A[0].Loop2000B[0].Loop2300[0].Loop2400[0].SV1_ProfessionalService.CompositeMedicalProcedureIdentifier_01.ProcedureCode_02);//"19016-5";// 18657-7";
                stc1.HealthCareClaimStatus_01.CodeListQualifierCode_04 = "LOI";

                stc1.Date_02 = String.Format("{0:yyyyMMdd}", System.DateTime.UtcNow.AddDays(30));

                stc1.HealthCareClaimStatus_10 = new C043_HealthCareClaimStatus();
                stc1.HealthCareClaimStatus_10.HealthCareClaimStatusCategoryCode_01 = "R4";
                stc1.HealthCareClaimStatus_10.StatusCode_02 = "18594-2";//"18803-7";
                stc1.HealthCareClaimStatus_10.CodeListQualifierCode_04 = "LOI";
            }
            //end of the list request

            // stc1.HealthCareClaimStatus_11 = "";
            loop2200D.AllREF = new All_REF_277();
            loop2200D.AllREF.REF_PatientControlNumber = new REF_PatientControlNumber();
            loop2200D.AllREF.REF_PatientControlNumber.MemberGrouporPolicyNumber_02 = data.Loop2000A[0].Loop2000B[0].Loop2300[0].CLM_ClaimInformation.PatientControlNumber_01;


            return ts277Data;
        }

        private  string GetCPT2Loinc(string CptCode)
        {
            return _cAERepository.GetLOINCCode4CPTCode(CptCode);//.Result.FirstOrDefault().ToString();
        }


        private Task<List<string>> GetRequestCodes(string CptCode)
        {
            return _cAERepository.GetRequestCodes(CptCode);//.Result.FirstOrDefault().ToString();
        }

        private (TS277,long) Get277(Edi.Templates.Hipaa5010.TS837P ts837Data=null)
        {
            TS277 input277Data = new TS277();
            input277Data = FetchData277(ts837Data);
            long cntlNum= _cAERepository.GetNextSequence("trnId");
            var transaction = HipaaTransactionBuilders.Build277ResponseTransmission(cntlNum.ToString(), input277Data);
            //using (var stream = new MemoryStream())
            //{
            //    using (var writer = new X12Writer(stream))
            //    {
            //        writer.Write(SegmentBuilders.BuildIsa("1"));
            //        writer.Write(SegmentBuilders.BuildGs("1"));
            //        writer.Write(transaction);
            //    }

            //    var ediString = stream.LoadToString();
            //    return ediString;
            //}
            return (transaction,cntlNum);

        }

        private string Get277Edi(TS277 ts277)
        {
            var transaction = ts277;
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
