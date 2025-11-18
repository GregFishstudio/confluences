using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Syncfusion.EJ2.PdfViewer;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Nodes;
using System;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfViewerController : ControllerBase
    {
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        private IMemoryCache _cache;
        private ILogger<PdfViewerController> _logger;

        public PdfViewerController(Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, IMemoryCache cache, ILogger<PdfViewerController> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _cache = cache;
            _logger = logger;
        }

        public class jsonObjects
        {
            public string document { get; set; }
            public string password { get; set; }
            public string zoomFactor { get; set; }
            public string isFileName { get; set; }
            public string xCoordinate { get; set; }
            public string yCoordinate { get; set; }
            public string pageNumber { get; set; }
            public string documentId { get; set; }
            public string hashId { get; set; }
            public string sizeX { get; set; }
            public string sizeY { get; set; }
            public string startPage { get; set; }
            public string endPage { get; set; }
            public string stampAnnotations { get; set; }
            public string textMarkupAnnotations { get; set; }
            public string stickyNotesAnnotation { get; set; }
            public string shapeAnnotations { get; set; }
            public string measureShapeAnnotations { get; set; }
            public string action { get; set; }
            public string pageStartIndex { get; set; }
            public string pageEndIndex { get; set; }
            public string fileName { get; set; }
            public string elementId { get; set; }
            public string pdfAnnotation { get; set; }
            public string importPageList { get; set; }
            public string uniqueId { get; set; }
            public string data { get; set; }
            public string viewPortWidth { get; set; }
            public string viewPortHeight { get; set; }
            public string tilecount { get; set; }
            public bool isCompletePageSizeNotReceived { get; set; }
            public string freeTextAnnotation { get; set; }
            public string signatureData { get; set; }
            public string fieldsData { get; set; }
            public string FormDesigner { get; set; }
            public string inkSignatureData { get; set; }
            public bool hideEmptyDigitalSignatureFields { get; set; }
            public bool showDigitalSignatureAppearance { get; set; }
            public bool digitalSignaturePresent { get; set; }
            public string tileXCount { get; set; }
            public string tileYCount { get; set; }
            public string digitalSignaturePageList { get; set; }
            public string annotationCollection { get; set; }
            public string annotationsPageList { get; set; }
            public string formFieldsPageList { get; set; }
            public bool isAnnotationsExist { get; set; }
            public bool isFormFieldAnnotationsExist { get; set; }
            public string documentLiveCount { get; set; }
            public string annotationDataFormat { get; set; }
            public string importedData { get; set; }
        }

        [HttpPost("Load")]
        public IActionResult Load([FromBody] jsonObjects responseData)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            MemoryStream stream = new MemoryStream();
            var jsonObject = JsonConverterstring(responseData);
            object jsonResult = new object();
            if (jsonObject != null && jsonObject.ContainsKey("document"))
            {
                if (bool.Parse(jsonObject["isFileName"]))
                {
                    string documentPath = GetDocumentPath(jsonObject["document"]);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                        stream = new MemoryStream(bytes);
                    }
                    else
                    {
                        return this.Content(jsonObject["document"] + " is not found");
                    }
                }
                else
                {
                    byte[] bytes = Convert.FromBase64String(jsonObject["document"]);
                    stream = new MemoryStream(bytes);
                }
            }
            jsonResult = pdfviewer.Load(stream, jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [HttpPost("json-convert")]
        public Dictionary<string, string> JsonConverterstring(jsonObjects results)
        {
            Dictionary<string, object> resultObjects = new Dictionary<string, object>();
            resultObjects = results.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(results, null));
            var emptyObjects = (from kv in resultObjects
                                where kv.Value != null
                                select kv).ToDictionary(kv => kv.Key, kv => kv.Value);
            Dictionary<string, string> jsonResult = emptyObjects.ToDictionary(k => k.Key, k => k.Value.ToString());
            return jsonResult;
        }

        [HttpPost("RenderPdfPages")]
        //Post action for processing the PDF documents.
        public IActionResult RenderPdfPages([FromBody] jsonObjects responseData)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            var jsonObject = JsonConverterstring(responseData);
            object jsonResult = pdfviewer.GetPage(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [HttpPost("Unload")]
        //Post action for unloading and disposing the PDF document resources
        public IActionResult Unload([FromBody] jsonObjects responseData)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            var jsonObject = JsonConverterstring(responseData);
            pdfviewer.ClearCache(jsonObject);
            return this.Content("Document cache is cleared");
        }

        [HttpPost("RenderThumbnailImages")]
        //Post action for rendering the ThumbnailImages
        public IActionResult RenderThumbnailImages([FromBody] jsonObjects responseData)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            var jsonObject = JsonConverterstring(responseData);
            object result = pdfviewer.GetThumbnailImages(jsonObject);
            return Content(JsonConvert.SerializeObject(result));
        }

        [HttpPost("Bookmarks")]
        //Post action for processing the bookmarks from the PDF documents
        public IActionResult Bookmarks([FromBody] jsonObjects responseData)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            var jsonObject = JsonConverterstring(responseData);
            object jsonResult = pdfviewer.GetBookmarks(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [HttpPost("RenderAnnotationComments")]
        //Post action for rendering the annotation comments
        public IActionResult RenderAnnotationComments([FromBody] jsonObjects responseData)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            var jsonObject = JsonConverterstring(responseData);
            object jsonResult = pdfviewer.GetAnnotationComments(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [HttpPost("ExportAnnotations")]
        //Post action for exporting the annotations
        public IActionResult ExportAnnotations([FromBody] jsonObjects responseData)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            var jsonObject = JsonConverterstring(responseData);
            string jsonResult = pdfviewer.ExportAnnotation(jsonObject);
            return Content(jsonResult);
        }

        [HttpPost("ImportAnnotations")]
        //Post action for importing the annotations
        public IActionResult ImportAnnotations([FromBody] jsonObjects responseData)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            var jsonObject = JsonConverterstring(responseData);
            string jsonResult = string.Empty;
            object JsonResult;
            if (jsonObject != null && jsonObject.ContainsKey("fileName"))
            {
                string documentPath = GetDocumentPath(jsonObject["fileName"]);
                if (!string.IsNullOrEmpty(documentPath))
                {
                    jsonResult = System.IO.File.ReadAllText(documentPath);
                }
                else
                {
                    return this.Content(jsonObject["document"] + " is not found");
                }
            }
            else
            {
                string extension = Path.GetExtension(jsonObject["importedData"]);
                if (extension != ".xfdf")
                {
                    JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                    return Content(JsonConvert.SerializeObject(JsonResult));
                }
                else
                {
                    string documentPath = GetDocumentPath(jsonObject["importedData"]);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                        jsonObject["importedData"] = Convert.ToBase64String(bytes);
                        JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                        return Content(JsonConvert.SerializeObject(JsonResult));
                    }
                    else
                    {
                        return this.Content(jsonObject["document"] + " is not found");
                    }
                }
            }
            return Content(jsonResult);
        }

        [HttpPost("Download")]
        //Post action for downloading the PDF documents
        public IActionResult Download([FromBody] jsonObjects responseData)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            var jsonObject = JsonConverterstring(responseData);
            string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
            return Content(documentBase);
        }

        [HttpPost("PrintImages")]
        //Post action for printing the PDF documents
        public IActionResult PrintImages([FromBody] jsonObjects responseData)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            var jsonObject = JsonConverterstring(responseData);
            object pageImage = pdfviewer.GetPrintImage(jsonObject);
            return Content(JsonConvert.SerializeObject(pageImage));
        }

        //Gets the path of the PDF document
        private string GetDocumentPath(string document)
        {
            _logger.LogDebug("GetDocumentPath, document : {document}", document);
            string documentPath = string.Empty;
            if (!System.IO.File.Exists(document))
            {
                var splitPath = document.Split('/');
                var path = Path.Combine(_hostingEnvironment.WebRootPath, Path.Combine(splitPath));
                _logger.LogDebug("GetDocumentPath, path : {path}", path);
                if (System.IO.File.Exists(path))
                {
                    _logger.LogDebug("GetDocumentPath, File exists");
                    documentPath = path;
                }
                else
                {
                    _logger.LogDebug("GetDocumentPath, File doesn't exist");
                }
            }
            else
            {
                documentPath = document;
            }
            return documentPath;
        }
    }
}
