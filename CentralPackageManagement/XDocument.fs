namespace CentralPackageManagement

open System.IO
open System.Text
open System.Xml
open System.Xml.Linq

[<RequireQualifiedAccess>]
module XDocument =

    let saveXmlWithoutDeclaration (document: XDocument) (fileName: string) =
        let utf8Encoding = UTF8Encoding(encoderShouldEmitUTF8Identifier = true)
        let xmlSettings = XmlWriterSettings()
        xmlSettings.OmitXmlDeclaration <- true
        xmlSettings.Indent <- true
        xmlSettings.Encoding <- utf8Encoding

        use streamWriter = new StreamWriter(fileName, false, utf8Encoding)
        use xmlWriter = XmlWriter.Create(streamWriter, xmlSettings)

        document.Save(xmlWriter)
