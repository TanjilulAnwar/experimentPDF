using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Rotativa.Models;
using Wkhtmltopdf.NetCore;


namespace Rotativa.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestDynamicViewsController : ControllerBase
    {
        readonly IGeneratePdf _generatePdf;
        readonly string htmlView = @"
                        <!DOCTYPE html>
                        <html>
                        <head>
                        </head>
                        <body>
<div>
 <table>
@{ int i = 1;}
    @foreach (var p in Model.ListValue)
    {
        <tr >
            <td>@(i++)</td>
         
            <td>@p.Text</td>
          
            <td>@p.Number</td>
         
           
        </tr>
    }

</table>

  </div>
                        </body>
                        </html>";


        readonly string formHtml =  "<!DOCTYPE html>"+
                                    "<html>"+
                                    "<body>"+
                                    "<h2>HTML Forms</h2>"+
                                    "<form>"+
                                    "  <label for=\"fname\">First name:</label><br>"+
                                    "  <input type =\"text\" id=\"fname\" name=\"fname\"><br>"+
                                    "  <label for=\"lname\">Last name:</label><br>"+
                                    "  <input type =\"text\" id=\"lname\" name=\"lname\"><br><br>"+
                                    "</form> "+
                                    "</body>"+
                                    "</html>";


        public TestDynamicViewsController(IGeneratePdf generatePdf)
        {
            _generatePdf = generatePdf;
        }

        /// <summary>
        /// String view pdf generation as ActionResult
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("LL")]
        public async Task<IActionResult> GetByRazorText()
        {
            var kv = new Dictionary<string, string>
            {
                { "username", "Veaer" },
                { "age", "20" },
                { "url", "google.com" }
            };

            var options = new ConvertOptions
            {
                HeaderHtml = "https://localhost:5002/header.html",
                PageOrientation = Wkhtmltopdf.NetCore.Options.Orientation.Portrait,
                FooterHtml = "https://localhost:5002/footer.html",
                Replacements = kv,
                PageMargins = new Wkhtmltopdf.NetCore.Options.Margins()
                {
                    Top = 15,
                    Left = 10,
                    Right = 10,
                    Bottom = 15
                }

            };
            _generatePdf.SetConvertOptions(options);
            var data = new TestData
            {
                Text = "Tanjilul Anwar",
                Number = 12345678
            };

            List<TestData> f = new List<TestData>();
            f.Add(new TestData
            {
                Text = "Tanjilul Anwar",
                Number = 12345678
            });
            f.Add(new TestData
            {
                Text = "Jakequ Sivan",
                Number = 98600
            });

            for(int i=0; i<30; i++) {
                f.Add(new TestData
                {
                    Text = "Pitaki Behkore",
                    Number = 5436
                });
            }
            
            var list = f;
            decimal totBalance = 10.0M;
            var model = ToExpando(new { ListValue = list, Balance = totBalance });
            string htmlViewX = await System.IO.File.ReadAllTextAsync("Views/dirty.cshtml");

             return await _generatePdf.GetPdfViewInHtml(htmlViewX, model);
        
        }

        public ExpandoObject ToExpando(object anonymousObject)
        {
            IDictionary<string, object> anonymousDictionary = new RouteValueDictionary(anonymousObject);
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var item in anonymousDictionary)
                expando.Add(item);
            return (ExpandoObject)expando;
        }

        /// <summary>
        /// string view pdf generation as ByteArray
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByteArray")]
        public async Task<IActionResult> GetByteArray()
        {
            var data = new TestData
            {
                Text = "This is not a test",
                Number = 12345678
            };

            
            var pdf = await _generatePdf.GetByteArrayViewInHtml(htmlView, data);
            var pdfStream = new System.IO.MemoryStream();
            pdfStream.Write(pdf, 0, pdf.Length);
            pdfStream.Position = 0;
            return new FileStreamResult(pdfStream, "application/pdf");
        }

        /// <summary>
        /// Cached view pdf generation as ActionResult
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetFromEngine")]
        public async Task<IActionResult> GetFromEngine()
        {
            var data = new TestData
            {
                Text = "This is a test",
                Number = 123456
            };

            if(!_generatePdf.ExistsView("notAView"))
            {
                //var html = await System.IO.File.ReadAllTextAsync("Views/Test.cshtml");
                var html = await System.IO.File.ReadAllTextAsync("Views/invoice.cshtml");
                _generatePdf.AddView("notAView", html);
            }

            return await _generatePdf.GetPdf("notAView", data);
        }

        /// <summary>
        /// Cached view and update view with string view pdf generation as ActionResult
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("UpdateAndGetFromEngine")]
        public async Task<IActionResult> UpdateAndGetFromEngine()
        {
            var data = new TestData
            {
                Text = "This is a test",
                Number = 123456
            };

            if (!_generatePdf.ExistsView("notAView"))
            {
                var html = await System.IO.File.ReadAllTextAsync("Views/Test.cshtml");
                _generatePdf.AddView("notAView", html);
            }
            else
            {
                var html = @"@model Rotativa.Models.TestData
                        <!DOCTYPE html>
                        <html>
                        <head>
                        </head>
                        <body>
                            <header>
                                <h1>@Model.Text</h1>
                            </header>
                            <div>
                                <h2>Repeat @Model.Text</h2>
                            </div>
                            <div>
                                <h5>@Model.Number</h2>
                            </div>
                        </body>
                        </html>";

                _generatePdf.UpdateView("notAView", html);
            }

            return await _generatePdf.GetPdf("notAView", data);
        }

        /// <summary>
        /// string form view pdf generation as ByteArray
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetFormByteArray")]
        public IActionResult GetFormByteArray()
        {
            var options = new ConvertOptions
            {
                EnableForms = true
            };

            _generatePdf.SetConvertOptions(options);

            var pdf = _generatePdf.GetPDF(formHtml);
            var pdfStream = new System.IO.MemoryStream();
            pdfStream.Write(pdf, 0, pdf.Length);
            pdfStream.Position = 0;
            return new FileStreamResult(pdfStream, "application/pdf");
        }
    }
}
